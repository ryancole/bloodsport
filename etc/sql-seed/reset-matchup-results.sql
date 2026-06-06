-- Deletes all matchup result data.
-- Safe to re-run. Does NOT affect SeasonWeekMatchups or any other season data.

DELETE FROM [dbo].[SeasonWeekMatchupResults];
