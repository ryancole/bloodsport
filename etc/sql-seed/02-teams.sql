-- Seed 100 random teams for local dev, one per user seeded in 01-users.sql.
-- Only 100 teams so the remaining 400 users can fill out rosters (5 members each).
-- Each team is managed by the user in the same row position.
-- Assumes 01-users.sql has been run first.

DECLARE @Adjectives TABLE (Word NVARCHAR(50));
INSERT INTO @Adjectives VALUES
    ('Shadow'), ('Iron'), ('Crimson'), ('Neon'), ('Frost'),
    ('Thunder'), ('Silent'), ('Blaze'), ('Void'), ('Steel'),
    ('Arctic'), ('Dark'), ('Solar'), ('Echo'), ('Rogue'),
    ('Apex'), ('Quantum'), ('Midnight'), ('Orbital'), ('Hyper'),
    ('Savage'), ('Phantom'), ('Blazing'), ('Fierce'), ('Storm'),
    ('Radical'), ('Toxic'), ('Electric'), ('Cosmic'), ('Lunar');

DECLARE @Nouns TABLE (Word NVARCHAR(50));
INSERT INTO @Nouns VALUES
    ('Wolves'), ('Falcons'), ('Tide'), ('Vipers'), ('Giants'),
    ('Hawks'), ('Storm'), ('Squad'), ('Walkers'), ('Phantoms'),
    ('Foxes'), ('Matter'), ('Flare'), ('Force'), ('Wave'),
    ('Predators'), ('Shift'), ('Run'), ('Strike'), ('Drive'),
    ('Dragons'), ('Titans'), ('Legion'), ('Raiders'), ('Crew'),
    ('Syndicate'), ('Uprising'), ('Dynasty'), ('Outlaws'), ('Reapers');

DECLARE @Users TABLE (RowNum INT IDENTITY(1,1), UserId BIGINT);
INSERT INTO @Users (UserId)
SELECT Id FROM [dbo].[Users] ORDER BY DateCreated;

DECLARE @i INT = 1;
DECLARE @UserId BIGINT;
DECLARE @TeamName NVARCHAR(101);

WHILE @i <= 100
BEGIN
    SELECT @UserId = UserId FROM @Users WHERE RowNum = @i;

    SELECT @TeamName =
        (SELECT TOP 1 Word FROM @Adjectives ORDER BY NEWID()) + ' ' +
        (SELECT TOP 1 Word FROM @Nouns ORDER BY NEWID());

    INSERT INTO [dbo].[Teams] ([Name], [ManagerId])
    VALUES (@TeamName, @UserId);

    SET @i = @i + 1;
END
