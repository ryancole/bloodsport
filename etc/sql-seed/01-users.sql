-- Seed 500 random users for local dev.
-- EntraObjectId is a unique GUID per user (fake — not real Entra accounts).
-- Run this before 02-teams.sql.

DECLARE @FirstNames TABLE (Name NVARCHAR(50));
INSERT INTO @FirstNames VALUES
    ('Alice'), ('Ben'), ('Carmen'), ('Dmitri'), ('Elena'),
    ('Finn'), ('Grace'), ('Hassan'), ('Ingrid'), ('Javier'),
    ('Kira'), ('Liam'), ('Maya'), ('Noel'), ('Olivia'),
    ('Priya'), ('Quinn'), ('Ravi'), ('Sofia'), ('Tyler'),
    ('Amber'), ('Blake'), ('Chloe'), ('Derek'), ('Eva'),
    ('Felix'), ('Gina'), ('Hugo'), ('Isla'), ('Jake');

DECLARE @LastNames TABLE (Name NVARCHAR(50));
INSERT INTO @LastNames VALUES
    ('Thornton'), ('Nakamura'), ('Reyes'), ('Volkov'), ('Park'),
    ('O''Brien'), ('Liu'), ('Ibrahim'), ('Svensson'), ('Morales'),
    ('Petrov'), ('Okonkwo'), ('Chen'), ('Dupont'), ('Santos'),
    ('Kapoor'), ('Walsh'), ('Sharma'), ('Andersson'), ('Brooks'),
    ('Hale'), ('Cruz'), ('Jensen'), ('Novak'), ('Tanaka'),
    ('Muller'), ('Diaz'), ('Kwan'), ('Fischer'), ('Patel');

DECLARE @FirstCount INT = (SELECT COUNT(*) FROM @FirstNames);
DECLARE @LastCount  INT = (SELECT COUNT(*) FROM @LastNames);

DECLARE @i INT = 1;
DECLARE @DisplayName NVARCHAR(101);

WHILE @i <= 500
BEGIN
    SELECT @DisplayName =
        (SELECT TOP 1 Name FROM @FirstNames ORDER BY NEWID()) + ' ' +
        (SELECT TOP 1 Name FROM @LastNames  ORDER BY NEWID());

    INSERT INTO [dbo].[Users] ([DisplayName], [EntraObjectId])
    VALUES (@DisplayName, CONVERT(NVARCHAR(36), NEWID()));

    SET @i = @i + 1;
END
