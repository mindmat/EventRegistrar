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
    QuestionId_FirstName uniqueidentifier NULL,
    [ExternalIdentifier] [varchar](50) NULL,
    [Title] [varchar](50) NULL,
    [State] [int] NOT NULL,
    [Language] nchar(2) NULL,
    [RowVersion] rowversion NULL,
 CONSTRAINT [PK_RegistrationForm] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[Questions](
    [Id] [uniqueidentifier] NOT NULL,
    [RegistrationFormId] [uniqueidentifier] NOT NULL,
    [ExternalId] [int] NULL,
    [Index] [int] NULL,
    [Title] [nvarchar](max) NULL,
    [TemplateKey] [nvarchar](200) NULL,
    [Type] [int] NULL,
    [RowVersion] [timestamp] NOT NULL,
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
    [TemplateKey] [nvarchar](200) NULL,
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
    [RespondentEmail] nvarchar(200) NULL,
    RespondentFirstName nvarchar(100) NULL,
    ReceivedAt datetime2(7) NOT NULL,
    ExternalTimestamp datetime2(7) NULL,
    [Language] nchar(2) NULL,
	[Price] [money] NULL,
	[IsWaitingList] [bit] NOT NULL DEFAULT(0),
	[SoldOutMessage] [nvarchar](max) NULL,
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
    MaximumAllowedImbalance int NULL,
    ShowInMailListOrder int NULL,
	Price money NULL,
	[HasWaitingList] [bit] NOT NULL,
	[IsCore] [bit] NOT NULL,
    [RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_LimitedResource] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[Reductions](
	[Id] [uniqueidentifier] NOT NULL,
	[RegistrableId] [uniqueidentifier] NOT NULL,
	[RegistrableId1_ReductionActivatedIfCombinedWith] [uniqueidentifier] NULL,
	[RegistrableId2_ReductionActivatedIfCombinedWith] [uniqueidentifier] NULL,
	[QuestionOptionId_ActivatesReduction] [uniqueidentifier] NULL,
	Amount money NOT NULL,
	[RowVersion] rowversion NOT NULL,
 CONSTRAINT [PK_Reductions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO



CREATE TABLE [dbo].[Seats](
    [Id] [uniqueidentifier] NOT NULL,
    [RegistrableId] [uniqueidentifier] NOT NULL,
    [RegistrationId] [uniqueidentifier] NULL,
    [RegistrationId_Follower] [uniqueidentifier] NULL,
    PartnerEmail NVARCHAR(200) NULL,
    [FirstPartnerJoined] [datetime2](7) NULL,
    [RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Seats] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO



CREATE TABLE [dbo].[QuestionOptionToRegistrableMappings](
    [Id] [uniqueidentifier] NOT NULL,
    [QuestionOptionId] [uniqueidentifier] NOT NULL,
    QuestionId_PartnerEmail [uniqueidentifier] NULL,
    [QuestionOptionId_Leader] [uniqueidentifier] NULL,
    [QuestionOptionId_Follower] [uniqueidentifier] NULL,
    [RegistrableId] [uniqueidentifier] NOT NULL,
    [RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_QuestionOptionToRegistrableMapping] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[MailTemplates](
    [Id] [uniqueidentifier] NOT NULL,
    [EventId] [uniqueidentifier] NOT NULL,
    [Language] nchar(2) NOT NULL,
    [Subject] nvarchar(max) NOT NULL,
    SenderMail nvarchar(200) NOT NULL,
    SenderName nvarchar(200) NOT NULL,
    [Type] [int] NOT NULL,
    [ContentType] [int] NOT NULL,
    [Template] [nvarchar](max) NOT NULL,
    [RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_MailTemplates] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO


CREATE TABLE [dbo].[Mails](
	[Id] [uniqueidentifier] NOT NULL,
	[SenderMail] [nvarchar](200) NULL,
	[SenderName] [nvarchar](200) NULL,
	[Recipients] [nvarchar](max) NULL,
	[Subject] [nvarchar](300) NULL,
	[ContentHtml] [nvarchar](max) NULL,
	[ContentPlainText] [nvarchar](max) NULL,
	[Type] [int] NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_Mails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [dbo].[MailToRegistrations](
	[Id] [uniqueidentifier] NOT NULL,
	[MailId] [uniqueidentifier] NOT NULL,
	[RegistrationId] [uniqueidentifier] NOT NULL,
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK_MailsToRegistrations] PRIMARY KEY CLUSTERED 
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