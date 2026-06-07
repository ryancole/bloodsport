-- Deletes all team membership and invite data.
-- Safe to re-run. Does NOT affect Users, Teams, RiotAccounts, or season data.

DELETE FROM [dbo].[TeamInvites];
DELETE FROM [dbo].[TeamMemberships];
