-- Deletes all season-related data in dependency order.
-- Safe to re-run. Does NOT affect Users, Teams, RiotAccounts, or TeamMemberships.

DELETE FROM [dbo].[SeasonWeekMatchups];
DELETE FROM [dbo].[SeasonWeeks];
DELETE FROM [dbo].[SeasonRegistrations];
DELETE FROM [dbo].[Seasons];
