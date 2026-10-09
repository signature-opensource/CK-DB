-- Cleanup of the procedures that have been renamed when "Member" superseded "User" (v30).
-- The setup doesn't drop objects that are no more defined: these ones are left in upgraded databases.
if object_id('CK.sGroupUserAdd') is not null drop procedure CK.sGroupUserAdd;
if object_id('CK.sGroupUserRemove') is not null drop procedure CK.sGroupUserRemove;
if object_id('CK.sGroupRemoveAllUsers') is not null drop procedure CK.sGroupRemoveAllUsers;
if object_id('CK.sUserRemoveFromAllGroups') is not null drop procedure CK.sUserRemoveFromAllGroups;
