-- Deletes all playoff bracket data.
-- Safe to re-run. Does NOT affect regular season weeks, matchups, or standings.

DELETE FROM [dbo].[PlayoffRoundMatchups];
DELETE FROM [dbo].[PlayoffTeams];
DELETE FROM [dbo].[Playoffs];
