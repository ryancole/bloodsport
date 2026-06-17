-- Seed one RiotAccount per user for local dev.
-- Puuid is a fake 78-char string prefixed with 'FAKE-' so it can be identified and filtered out.
-- GameName is derived from the user's DisplayName (first 16 chars, no spaces).
-- TagLine is a random 4-digit number prefixed with '#' stripped (stored without #).
-- Assumes 01-users.sql has been run first.

DECLARE @UserId    BIGINT;
DECLARE @GameName  NVARCHAR(16);
DECLARE @TagLine   NVARCHAR(5);
DECLARE @Puuid     NVARCHAR(78);

DECLARE user_cursor CURSOR FAST_FORWARD FOR
    SELECT u.Id, u.DisplayName
    FROM [dbo].[Users] u
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[RiotAccounts] ra WHERE ra.UserId = u.Id)
    ORDER BY u.DateCreated;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId, @GameName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Strip spaces from display name and truncate to 16 chars for GameName
    SET @GameName = LEFT(REPLACE(@GameName, ' ', ''), 16);

    -- Random 4-digit tag (1000-9999)
    SET @TagLine = CAST(FLOOR(RAND() * 9000) + 1000 AS NVARCHAR(4));

    -- Fake 78-char Puuid: prefixed with 'FAKE-' so it can be identified and filtered out
    SET @Puuid = LEFT(
        'FAKE-' +
        REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', '') +
        REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', '') +
        REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''),
        78
    );

    INSERT INTO [dbo].[RiotAccounts] ([UserId], [Puuid], [GameName], [TagLine])
    VALUES (@UserId, @Puuid, @GameName, @TagLine);

    FETCH NEXT FROM user_cursor INTO @UserId, @GameName;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;
