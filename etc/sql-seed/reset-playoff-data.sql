-- Deletes all playoff bracket data.
-- Safe to re-run. Does NOT affect regular season weeks, matchups, or standings.

DELETE FROM [dbo].[PlayoffMatchupResults];
DELETE FROM [dbo].[PlayoffMatchups];
