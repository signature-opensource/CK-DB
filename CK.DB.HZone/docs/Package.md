Makes the Zones of CK.DB.Zone hierarchical: a Zone can have a parent Zone, and Zones can be moved in the hierarchy.

`ZoneTable` specializes the CK.DB.Zone one (create with a parent, destroy with the sub Zones, move); the views
`CK.vZoneAllChildren` and `CK.vZoneDirectChildren` list the children of a Zone.
