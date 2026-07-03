-- Assign every RiotAccount to exactly one Team as a TeamMembership.
-- 700 riot accounts / 100 teams = 7 members per team.
-- RiotAccounts are shuffled randomly then chunked into groups of 7;
-- each chunk maps to one team by row number.
-- Assumes 01-users.sql, 02-teams.sql, and 03-riot-accounts.sql have been run first.

WITH ShuffledAccounts AS (
    SELECT
        ra.Id AS RiotAccountId,
        ROW_NUMBER() OVER (ORDER BY NEWID()) AS RowNum
    FROM [dbo].[RiotAccounts] ra
),
NumberedTeams AS (
    SELECT
        t.Id AS TeamId,
        ROW_NUMBER() OVER (ORDER BY t.DateCreated) AS RowNum
    FROM [dbo].[Teams] t
),
Assignments AS (
    SELECT
        sa.RiotAccountId,
        -- Map each block of 7 accounts to one team (1-7 -> team 1, 8-14 -> team 2, ...)
        nt.TeamId
    FROM ShuffledAccounts sa
    JOIN NumberedTeams nt ON nt.RowNum = CEILING(sa.RowNum * 1.0 / 7)
)
INSERT INTO [dbo].[TeamMemberships] ([TeamId], [RiotAccountId], [Active])
SELECT TeamId, RiotAccountId, 1
FROM Assignments;
