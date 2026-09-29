-- SetupConfig: { "Requires": [ "CK.vGroup", "CK.vUser" ]}
create view CK.vGroupMember
as 
    select g.GroupId,
		   g.GroupName,
           MemberId = p.ActorId,
           MemberType = case when u.UserId is not null then 'User' else '?' end,
           MemberName = coalesce( u.UserName, '?' ),
           CreationDate = coalesce( u.CreationDate, '0001-01-01' )
        from CK.vGroup g
        inner join CK.tActorProfile p on p.GroupId = g.GroupId and p.GroupId <> p.ActorId
        left outer join CK.vUser u on u.UserId = p.ActorId;
    

