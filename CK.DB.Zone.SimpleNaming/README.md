# CK.DB.Zone.SimpleNaming

## 1. Package Role

This package is an excellent example of how CK-DB features can be combined and modified. It changes a fundamental business rule introduced by `CK.DB.Group.SimpleNaming`.

-   **Without this package**: A group's name (`GroupName`) must be unique across the entire database.
-   **With this package**: A group's name must only be unique **within its parent zone (`ZoneId`)**. Two groups can have the same name if they are in different zones.

## 2. Extension Mechanism

This package modifies the database objects from `CK.DB.Group.SimpleNaming` and `CK.DB.Zone` to implement this new rule.

### 2.1. Modification of the Uniqueness Constraint

The first action is to replace the uniqueness constraint on the `CK.tGroup` table.

```sql
-- File: Res/Model.CK.DB.Zone.SimpleNaming.Package.Install.1.0.0.sql
-- 1. Drops the old constraint (uniqueness on GroupName alone).
alter table CK.tGroup drop UK_CK_tGroup_GroupName;
-- 2. Adds the new constraint (uniqueness on the ZoneId, GroupName pair).
alter table CK.tGroup add constraint UK_CK_tGroup_GroupName unique( ZoneId, GroupName );
```

### 2.2. Adaptation of the Renaming Logic

The `CK.fGroupGroupNameComputeUnique` function, which is used to find a unique group name (e.g., by adding a numeric suffix), must be adapted to consider the zone.

The package transforms this function:
```sql
-- File: Res/fGroupGroupNameComputeUnique.tql
create transformer on CK.fGroupGroupNameComputeUnique
as 
begin
	-- 1. Adds the @ZoneId parameter.
	add parameter @ZoneId int = -1;
	
    -- ... logic to infer ZoneId if not provided ...

	-- 2. Modifies the query to filter by ZoneId.
	in all 2 part {select '?'}
	begin
		replace all 2 {and g.GroupName =} with "and (@ZoneId < 0 or g.ZoneId = @ZoneId) and g.GroupName ="
	end
end
```

### 2.3. Handling Group Moves

When a group is moved from one zone to another (`CK.sGroupMove`), its name might conflict with an existing group in the new zone. This package transforms the move procedure to handle this case.

```sql
-- File: Res/sGroupMove.tql
create transformer on CK.sGroupMove
as 
begin
	-- 1. Before moving, calculate what the new name should be in the destination zone.
	inject "
		declare @GroupName nvarchar(128);
		select @GroupName = GroupName from CK.tGroup where GroupId = @GroupId;
		declare @GroupNameCorrected nvarchar(128);
		exec @GroupNameCorrected = CK.fGroupGroupNameComputeUnique -1, @GroupName, @NewZoneId;
		if @GroupNameCorrected is null throw 50000, 'GroupName.NameClash', 1;
	" into "PreGroupMove";

	-- 2. Updates the group's name at the same time as its ZoneId.
	inject ", GroupName = @GroupNameCorrected"
	after single {update CK.tGroup set ZoneId = @NewZoneId};
end
```

## 3. Dependencies

-   **`CK.DB.Zone`**: Provides the concept of a Zone and the `tGroup.ZoneId` table.
-   **`CK.DB.Group.SimpleNaming`**: Provides the concept of `GroupName` and the base objects (`fGroupGroupNameComputeUnique`) that are transformed.