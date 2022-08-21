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

INSERT INTO [dbo].[Users]
           ([Id]
           ,[IdentityProvider]
           ,[IdentityProviderUserIdentifier]
           ,[Email]
           ,[FirstName]
           ,[LastName])
SELECT [Id]
      ,[IdentityProvider]
      ,[IdentityProviderUserIdentifier]
      ,[Email]
      ,[FirstName]
      ,[LastName]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Users]

INSERT INTO [dbo].[UsersInEvents]
           ([Id]
           ,[EventId]
           ,[UserId]
           ,[Role])
SELECT [Id]
      ,[EventId]
      ,[UserId]
      ,[Role]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[UsersInEvents]


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
           ,[CheckinListColumn]
           ,[Type])
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
      ,[Type] = CASE WHEN [MaximumDoubleSeats] IS NULL THEN 1 ELSE 2 END
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


INSERT INTO [dbo].[Spots]
           ([Id]
           ,[RegistrableId]
           ,[RegistrationId]
           ,[RegistrationId_Follower]
           ,[FirstPartnerJoined]
           ,[IsCancelled]
           ,[IsPartnerSpot]
           ,[IsWaitingList]
           ,[PartnerEmail])
SELECT [Id]
      ,[RegistrableId]
      ,[RegistrationId]
      ,[RegistrationId_Follower]
      ,[FirstPartnerJoined]
      ,[IsCancelled]
      ,[IsPartnerSpot]
      ,[IsWaitingList]
      ,[PartnerEmail]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Seats]

-- Payments
INSERT INTO [dbo].[PaymentsFiles]
           ([Id]
           ,[EventId]
           ,[AccountIban]
           ,[FileId]
           ,[Balance]
           ,[BookingsFrom]
           ,[BookingsTo]
           ,[Currency]
           ,[Content])
SELECT [Id]
      ,[EventId]
      ,[AccountIban]
      ,[FileId]
      ,[Balance]
      ,[BookingsFrom]
      ,[BookingsTo]
      ,[Currency]
      ,[Content]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PaymentFiles]

INSERT INTO [dbo].[PayoutRequests]
           ([Id]
           ,[RegistrationId]
           ,[Amount]
           ,[Reason]
           ,[Created]
           ,[State])
SELECT [Id]
      ,[RegistrationId]
      ,[Amount]
      ,[Reason]
      ,[Created]
      ,[State]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PayoutRequests]

INSERT INTO [dbo].[PaymentSlips]
           ([Id]
           ,[EventId]
           ,[ContentType]
           ,[FileBinary]
           ,[Filename]
           ,[Reference])
SELECT [Id]
      ,[EventId]
      ,[ContentType]
      ,[FileBinary]
      ,[Filename]
      ,[Reference]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PaymentSlips]
  WHERE EventId IN (SELECT Id FROM dbo.[Events])


INSERT INTO [dbo].Payments
           ([Id]
           ,PaymentsFileId
           ,[Amount]
           ,[BookingDate]
           ,[Charges]
           ,[Currency]
           ,[Info]
           ,[InstructionIdentification]
           ,[RawXml]
           ,[RecognizedEmail]
           ,[Reference]
           ,[Repaid]
           ,[Settled]
           ,[Ignore]
           ,[Message]
		   ,[Type])
SELECT [Id]
      ,[PaymentFileId]
      ,[Amount]
      ,[BookingDate]
      ,[Charges]
      ,[Currency]
      ,[Info]
      ,[InstructionIdentification]
      ,[RawXml]
      ,[RecognizedEmail]
      ,[Reference]
      ,[Repaid]
      ,[Settled]
      ,[Ignore]
      ,[Message]
	  ,[CreditDebitType]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[ReceivedPayments]
  WHERE [PaymentFileId] IN (SELECT Id FROM dbo.PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM dbo.PaymentSlips))

INSERT INTO [dbo].IncomingPayments
           ([Id]
           ,[PaymentSlipId]
           ,[DebitorIban]
           ,[DebitorName])
SELECT [Id]
      ,[PaymentSlipId]
      ,[DebitorIban]
      ,[DebitorName]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[ReceivedPayments]
  WHERE [PaymentFileId] IN (SELECT Id FROM dbo.PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM dbo.PaymentSlips))
	AND [CreditDebitType] = 1
INSERT INTO [dbo].OutgoingPayments
           ([Id]
           ,[CreditorName]
           ,[CreditorIban])
SELECT [Id]
      ,[CreditorName]
      ,[CreditorIban]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[ReceivedPayments]
  WHERE [PaymentFileId] IN (SELECT Id FROM dbo.PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM dbo.PaymentSlips))
	AND [CreditDebitType] = 2

INSERT INTO [dbo].[PaymentAssignments]
           ([Id]
           ,[RegistrationId]
           ,IncomingPaymentId
           ,[PaymentAssignmentId_Counter]
           ,OutgoingPaymentId
           ,[PayoutRequestId]
           ,[Amount]
           ,[Created])
SELECT [Id]
      ,[RegistrationId]
      ,[ReceivedPaymentId]
      ,[PaymentAssignmentId_Counter]
      ,[PaymentId_Repayment]
      ,[PayoutRequestId]
      ,[Amount]
      ,[Created]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PaymentAssignments]
  WHERE [ReceivedPaymentId] IN (SELECT Id FROM dbo.Payments)
    AND [ReceivedPaymentId] IN (SELECT Id FROM dbo.IncomingPayments)

INSERT INTO [dbo].[PaymentAssignments]
           ([Id]
           ,[RegistrationId]
           ,IncomingPaymentId
           ,[PaymentAssignmentId_Counter]
           ,OutgoingPaymentId
           ,[PayoutRequestId]
           ,[Amount]
           ,[Created])
SELECT [Id]
      ,[RegistrationId]
      ,null
      ,[PaymentAssignmentId_Counter]
      ,[ReceivedPaymentId]
      ,[PayoutRequestId]
      ,[Amount]
      ,[Created]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PaymentAssignments]
  WHERE [ReceivedPaymentId] IN (SELECT Id FROM dbo.Payments)
    AND [ReceivedPaymentId] IN (SELECT Id FROM dbo.OutgoingPayments)


INSERT INTO [dbo].[MailTemplates]
           ([Id]
           ,[EventId]
           ,[RegistrableId]
           ,[BulkMailKey]
           ,[ContentType]
           ,[Language]
           ,[MailingAudience]
           ,[SenderMail]
           ,[SenderName]
           ,[Subject]
           ,[Template]
           ,[Type]
           ,[IsDeleted]
           ,[ReleaseImmediately])
     SELECT [Id]
           ,[EventId]
           ,[RegistrableId]
           ,[BulkMailKey]
           ,[ContentType]
           ,[Language]
           ,[MailingAudience]
           ,[SenderMail]
           ,[SenderName]
           ,[Subject]
           ,[Template]
           ,[Type]
           ,[IsDeleted]
           ,[ReleaseImmediately]
FROM [AZURE_ER].[EventRegistrator].[dbo].[MailTemplates]




INSERT INTO [dbo].[Mails]
           ([Id]
           ,[EventId]
           ,[MailTemplateId]
           ,[SenderMail]
           ,[SenderName]
           ,[Subject]
           ,[Recipients]
           ,[ContentHtml]
           ,[ContentPlainText]
           ,[Created]
           ,[SendGridMessageId]
           ,[Sent]
           ,[State]
           ,[Type]
           ,[Withhold]
           ,[Discarded]
           ,[BulkMailKey]
           ,[DataTypeFullName]
           ,[DataJson])
SELECT [Id]
           ,[EventId]
           ,[MailTemplateId]
           ,[SenderMail]
           ,[SenderName]
           ,[Subject]
           ,[Recipients]
           ,[ContentHtml]
           ,[ContentPlainText]
           ,[Created]
           ,[SendGridMessageId]
           ,[Sent]
           ,[State]
           ,[Type]
           ,[Withhold]
           ,[Discarded]
           ,[BulkMailKey]
           ,[DataTypeFullName]
           ,[DataJson]
FROM [AZURE_ER].[EventRegistrator].[dbo].[Mails]


INSERT INTO [dbo].[MailsToRegistrations]
           ([Id]
           ,[MailId]
           ,[RegistrationId]
           ,[State])
SELECT [Id]
           ,[MailId]
           ,[RegistrationId]
           ,[State]
FROM [AZURE_ER].[EventRegistrator].[dbo].[MailToRegistrations]
  WHERE MailId IN (SELECT Id FROM Mails)

INSERT INTO [dbo].[Sms]
           ([Id]
           ,[RegistrationId]
           ,[AccountSid]
           ,[Body]
           ,[Error]
           ,[ErrorCode]
           ,[From]
           ,[Price]
           ,[RawData]
           ,[Received]
           ,[Sent]
           ,[SmsSid]
           ,[SmsStatus]
           ,[To]
           ,[Type])
SELECT [Id]
           ,[RegistrationId]
           ,[AccountSid]
           ,[Body]
           ,[Error]
           ,[ErrorCode]
           ,[From]
           ,[Price]
           ,[RawData]
           ,[Received]
           ,[Sent]
           ,[SmsSid]
           ,[SmsStatus]
           ,[To]
           ,[Type]
FROM [AZURE_ER].[EventRegistrator].[dbo].[Sms]

--UPDATE BankAccountbookings
--SET Charges = 12
--WHERE Id = '4B15D3EA-3317-4921-AE6A-1DD373E6DB9E'

ROLLBACK
--COMMIT


