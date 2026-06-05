-- Register every team in every season.
-- Assumes 02-teams.sql has been run and at least one season exists.

INSERT INTO [dbo].[SeasonRegistrations] ([TeamId], [SeasonId])
SELECT t.Id, s.Id
FROM [dbo].[Teams] t
CROSS JOIN [dbo].[Seasons] s;
