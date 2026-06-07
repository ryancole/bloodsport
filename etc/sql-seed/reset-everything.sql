-- Deletes all data from every table in dependency order.
-- Safe to re-run. Use before re-running the full seed sequence.

DELETE FROM [dbo].[SeasonWeekMatchupResults];
DELETE FROM [dbo].[SeasonWeekMatchups];
DELETE FROM [dbo].[SeasonWeeks];
DELETE FROM [dbo].[TeamSeasonResults];
DELETE FROM [dbo].[TeamSeasonRosters];
DELETE FROM [dbo].[SeasonRegistrations];
DELETE FROM [dbo].[Seasons];
DELETE FROM [dbo].[TeamInvites];
DELETE FROM [dbo].[TeamMemberships];
DELETE FROM [dbo].[Teams];
DELETE FROM [dbo].[RiotAccounts];
DELETE FROM [dbo].[Users];
