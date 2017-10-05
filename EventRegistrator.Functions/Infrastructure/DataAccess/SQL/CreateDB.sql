-- ======================================================================================
-- Create SQL Login template for Azure SQL Database and Azure SQL Data Warehouse Database
-- ======================================================================================

CREATE LOGIN eventregistrator
	WITH PASSWORD = 'palmate-liege-clink1' 
GO


-- ========================================================================================
-- Create User as DBO template for Azure SQL Database and Azure SQL Data Warehouse Database
-- ========================================================================================
-- For login <login_name, sysname, login_name>, create a user in the database
CREATE USER eventregistrator
	FOR LOGIN eventregistrator
	WITH DEFAULT_SCHEMA = dbo
GO

-- Add user to the database owner role
EXEC sp_addrolemember N'db_datareader', N'eventregistrator'
EXEC sp_addrolemember N'db_datawriter', N'eventregistrator'
GO


CREATE TABLE [dbo].[Events](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_dbo.Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[RegistrationForm](
	[Id] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NULL,
	[ExternalIdentifier] [varchar](50) NULL,
	[Title] [varchar](50) NULL,
	[RowVersion] [timestamp] NULL,
 CONSTRAINT [PK_RegistrationForm] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[RegistrationForm](
	[Id] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NULL,
	[ExternalIdentifier] [varchar](50) NULL,
	[Title] [varchar](50) NULL,
	[RowVersion] [timestamp] NULL,
 CONSTRAINT [PK_RegistrationForm] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO