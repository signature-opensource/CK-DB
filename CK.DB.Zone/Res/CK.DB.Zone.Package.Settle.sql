-- Cleanup of the procedures that have been renamed when "Member" superseded "User" (v30).
-- The setup doesn't drop objects that are no more defined: these ones are left in upgraded databases.
if object_id('CK.sZoneUserAdd') is not null drop procedure CK.sZoneUserAdd;
if object_id('CK.sZoneUserRemove') is not null drop procedure CK.sZoneUserRemove;
