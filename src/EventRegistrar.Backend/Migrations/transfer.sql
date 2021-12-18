BEGIN TRAN

INSERT INTO [dbo].[Events]
           ([Id]
           ,[PredecessorEventId]
           ,[Name]
           ,[State]
           ,[Acronym]
           ,[Currency]
           ,[AccountIban])
SELECT [Id]
      ,[PredecessorEventId]
	  ,[Name]
      ,[State]
      ,[Acronym]
      ,[Currency]
      ,[AccountIban]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Events]


INSERT INTO [dbo].[Registrables]
           ([Id]
           ,[EventId]
           ,[HasWaitingList]
           ,[AutomaticPromotionFromWaitingList]
           ,[IsCore]
           ,[MaximumAllowedImbalance]
           ,[MaximumDoubleSeats]
           ,[MaximumSingleSeats]
           ,[Name]
           ,[Price]
           ,[ReducedPrice]
           ,[ShowInMailListOrder]
           ,[CheckinListColumn])
SELECT [Id]
      ,[EventId]
      ,[HasWaitingList]
      ,[AutomaticPromotionFromWaitingList]
      ,[IsCore]
      ,[MaximumAllowedImbalance]
      ,[MaximumDoubleSeats]
      ,[MaximumSingleSeats]
      ,[Name]
      ,[Price]
      ,[ReducedPrice]
      ,[ShowInMailListOrder]
      ,[CheckinListColumn]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Registrables]

INSERT INTO [dbo].[RegistrationForms]
           ([Id]
           ,[EventId]
           ,[ExternalIdentifier]
           ,[State]
           ,[Title])
SELECT [Id]
      ,[EventId]
      ,[ExternalIdentifier]
      ,[State]
      ,[Title]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[RegistrationForms]


INSERT INTO [dbo].[Registrations]
           ([Id]
           ,[EventId]
           ,[RegistrationFormId]
           ,[RegistrationId_Partner]
           ,[AdmittedAt]
           ,[ExternalIdentifier]
           ,[ExternalTimestamp]
           ,[FallbackToPartyPass]
           ,[IsReduced]
           ,[IsWaitingList]
           ,[Language]
           ,[OriginalPrice]
           ,[PartnerNormalized]
           ,[PartnerOriginal]
           ,[Phone]
           ,[PhoneNormalized]
           ,[Price]
           ,[ReceivedAt]
           ,[Remarks]
           ,[RemarksProcessed]
           ,[ReminderLevel]
           ,[RespondentEmail]
           ,[RespondentFirstName]
           ,[RespondentLastName]
           ,[SoldOutMessage]
           ,[State]
           ,[WillPayAtCheckin])
SELECT [Id]
      ,[EventId]
      ,[RegistrationFormId]
	  ,[RegistrationId_Partner]
      ,[AdmittedAt]
      ,[ExternalIdentifier]
      ,[ExternalTimestamp]
      ,[FallbackToPartyPass]
      ,[IsReduced]
      ,[IsWaitingList]
      ,[Language]
      ,[OriginalPrice]
      ,[PartnerNormalized]
      ,[PartnerOriginal]
      ,[Phone]
      ,[PhoneNormalized]
      ,[Price]
      ,[ReceivedAt]
      ,[Remarks]
      ,[RemarksProcessed]
      ,[ReminderLevel]
      ,[RespondentEmail]
      ,[RespondentFirstName]
      ,[RespondentLastName]
      ,[SoldOutMessage]
      ,[State]
      ,[WillPayAtCheckin]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Registrations]

ROLLBACK
--COMMIT


