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
    [RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_dbo.Events] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[RegistrationForms](
    [Id] [uniqueidentifier] NOT NULL,
    [EventId] [uniqueidentifier] NULL,
    [ExternalIdentifier] [varchar](50) NULL,
    [Title] [varchar](50) NULL,
    [State] [int] NOT NULL,
    [RowVersion] rowversion NULL,
 CONSTRAINT [PK_RegistrationForm] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[Question](
    [Id] [uniqueidentifier] NOT NULL,
    [RegistrationFormId] [uniqueidentifier] NOT NULL,
    [ExternalId] [int] NULL,
    [Index] [int] NULL,
    [Title] [varchar](max) NULL,
    [Type] [int] NULL,
    [RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_Question] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[QuestionOptions](
    [Id] [uniqueidentifier] NOT NULL,
    [QuestionId] [uniqueidentifier] NOT NULL,
    [Answer] [varchar](max) NOT NULL,
    [RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_QuestionOptions] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[Registrations](
    [Id] [uniqueidentifier] NOT NULL,
    [RegistrationFormId] [uniqueidentifier] NOT NULL,
    [ExternalIdentifier] [varchar](100) NULL,
    [RespondentEmail] [varchar](200) NULL,
    ReceivedAt datetime2(7) NOT NULL,
    ExternalTimestamp datetime2(7) NULL,
    [RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_Registration] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

CREATE TABLE [dbo].[Responses](
	Id uniqueidentifier NOT NULL,
	RegistrationId uniqueidentifier NULL,
	QuestionId uniqueidentifier NULL,
	ResponseString [varchar](max) NULL,
	QuestionOptionId uniqueidentifier NULL,
	[RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_Responses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[Registrables](
	Id uniqueidentifier NOT NULL,
	EventId uniqueidentifier NOT NULL,
	QuestionOptionId uniqueidentifier NULL,
	[Name] [nvarchar](200) NOT NULL,
	MaximumSingleSeats int NULL,
	MaximumDoubleSeats int NULL,
	[RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_LimitedResource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[Seats](
	[Id] [uniqueidentifier] NOT NULL,
	[RegistrableId] [uniqueidentifier] NOT NULL,
	[RegistrationId] [uniqueidentifier] NOT NULL,
	[RegistrationId_Partner] [uniqueidentifier] NULL,
	[RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_Seats] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO



CREATE TABLE [dbo].[LimitedResources](
	[Id] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[MaximumSeats] [int] NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_LimitedResource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[DomainEvents](
	[Id] [uniqueidentifier] NOT NULL,
	[AggregateId] [uniqueidentifier] NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Sequence] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_DomainEvents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO