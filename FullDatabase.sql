IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [CatId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CatId])
);
GO

CREATE TABLE [Cities] (
    [CityId] int NOT NULL IDENTITY,
    [SId] int NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([CityId])
);
GO

CREATE TABLE [Companies] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Password] nvarchar(max) NULL,
    [Comment] nvarchar(max) NULL,
    [JobProfileId] int NOT NULL,
    [Status] bit NOT NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Countries] (
    [CId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([CId])
);
GO

CREATE TABLE [Employees] (
    [EId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Gender] int NULL,
    [JobProfileId] int NOT NULL,
    [SkillsId] int NOT NULL,
    [Email] nvarchar(max) NULL,
    [Password] nvarchar(max) NULL,
    [Mobno] bigint NOT NULL,
    [Age] int NOT NULL,
    [CountryId] int NOT NULL,
    [StateId] int NOT NULL,
    [CityId] int NOT NULL,
    [ImagePath] nvarchar(max) NULL,
    [Comment] nvarchar(max) NULL,
    [Status] bit NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([EId])
);
GO

CREATE TABLE [JobPosts] (
    [JobPostId] int NOT NULL IDENTITY,
    [JobProfileId] int NOT NULL,
    [MinExp] int NOT NULL,
    [MaxExp] int NOT NULL,
    [MinSal] int NOT NULL,
    [MaxSal] int NOT NULL,
    [NoOfVac] int NOT NULL,
    [NoticePeriod] int NOT NULL,
    [Comment] nvarchar(max) NULL,
    [InsertedDate] datetime2 NOT NULL,
    [Status] bit NOT NULL,
    CONSTRAINT [PK_JobPosts] PRIMARY KEY ([JobPostId])
);
GO

CREATE TABLE [JobProfile] (
    [JPId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_JobProfile] PRIMARY KEY ([JPId])
);
GO

CREATE TABLE [Skills] (
    [SklId] int NOT NULL IDENTITY,
    [JPId] int NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Skills] PRIMARY KEY ([SklId])
);
GO

CREATE TABLE [States] (
    [SId] int NOT NULL IDENTITY,
    [CId] int NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_States] PRIMARY KEY ([SId])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240721064537_Init', N'8.0.3');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'NoticePeriod');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [NoticePeriod] int NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'NoOfVac');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [NoOfVac] int NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'MinSal');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [MinSal] int NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'MinExp');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [MinExp] int NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'MaxSal');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [MaxSal] int NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobPosts]') AND [c].[name] = N'MaxExp');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [JobPosts] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [JobPosts] ALTER COLUMN [MaxExp] int NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'Mobno');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Employees] ALTER COLUMN [Mobno] bigint NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'Age');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Employees] ALTER COLUMN [Age] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250125033336_New', N'8.0.3');
GO

COMMIT;
GO

