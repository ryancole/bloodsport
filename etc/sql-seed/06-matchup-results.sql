-- Populate a result for every matchup, randomly picking TeamOne or TeamTwo as the winner.
-- Assumes matchups already exist and no results are present (run reset-matchup-results.sql first if needed).

INSERT INTO [dbo].[SeasonWeekMatchupResults] ([SeasonWeekMatchupId], [WinnerTeamId])
SELECT
    m.Id,
    CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN m.TeamOneId ELSE m.TeamTwoId END
FROM [dbo].[SeasonWeekMatchups] m
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[SeasonWeekMatchupResults] r WHERE r.SeasonWeekMatchupId = m.Id
);
