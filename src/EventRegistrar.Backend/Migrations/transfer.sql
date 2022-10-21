BEGIN TRAN

DECLARE @Watchdog INT = 20;
WHILE @Watchdog > 0
BEGIN
  INSERT INTO [AZURE_EA].[event-admin].[dbo].[Events]
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
  WHERE ([PredecessorEventId] IS NULL OR [PredecessorEventId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[Events]))
    AND Id NOT IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[Events]);

  IF @@ROWCOUNT = 0 BREAK;
  SET @Watchdog = @Watchdog - 1
END

INSERT INTO [AZURE_EA].[event-admin].[dbo].[Users]
           ([Id]
           ,[IdentityProvider]
           ,[IdentityProviderUserIdentifier]
           ,[Email]
           ,[FirstName]
           ,[LastName])
SELECT [Id]
      ,3
      ,'google-oauth2|' + [IdentityProviderUserIdentifier]
      ,[Email]
      ,[FirstName]
      ,[LastName]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Users]

INSERT INTO [AZURE_EA].[event-admin].[dbo].[UsersInEvents]
           ([Id]
           ,[EventId]
           ,[UserId]
           ,[Role])
SELECT [Id]
      ,[EventId]
      ,[UserId]
      ,[Role]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[UsersInEvents]


INSERT INTO [AZURE_EA].[event-admin].[dbo].[AccessToEventRequests]
           ([Id]
           ,[EventId]
           ,[UserId_Requestor]
           ,[UserId_Responder]
           ,[IdentityProvider]
           ,[Identifier]
           ,[FirstName]
           ,[LastName]
           ,[Email]
           ,[AvatarUrl]
           ,[RequestReceived]
           ,[RequestText]
           ,[Response]
           ,[ResponseText])
SELECT [Id]
      ,[EventId]
      ,[UserId_Requestor]
      ,[UserId_Responder]
      ,3
      ,'google-oauth2|' + [Identifier]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,null
      ,[RequestReceived]
      ,[RequestText]
      ,[Response]
      ,[ResponseText]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[AccessToEventRequests]





INSERT INTO [AZURE_EA].[event-admin].[dbo].[Registrables]
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

INSERT INTO [AZURE_EA].[event-admin].[dbo].[RegistrationForms]
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

INSERT INTO [AZURE_EA].[event-admin].[dbo].[Questions]
           ([Id]
           ,[RegistrationFormId]
           ,[ExternalId]
           ,[Section]
           ,[Index]
           ,[Title]
           ,[Type]
           ,[Mapping]
           ,[TemplateKey])
SELECT [Id]
      ,[RegistrationFormId]
      ,[ExternalId]
      ,[Section]
      ,[Index]
      ,[Title]
      ,[Type]
      ,[Mapping]
      ,[TemplateKey]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[Questions]

INSERT INTO [AZURE_EA].[event-admin].[dbo].[QuestionOptions]
           ([Id]
           ,[QuestionId]
           ,[Answer])
SELECT [Id]
      ,[QuestionId]
      ,[Answer]
FROM [AZURE_ER].[EventRegistrator].[dbo].[QuestionOptions]
WHERE QuestionId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[Questions])

INSERT INTO [AZURE_EA].[event-admin].[dbo].[QuestionOptionMappings]
           ([Id]
           ,[QuestionOptionId]
           ,[RegistrableId]
           ,[Type]
           ,[Language])
SELECT [Id]
      ,[QuestionOptionId]
      ,[RegistrableId]
      ,[Type]
      ,[Language]
FROM [AZURE_ER].[EventRegistrator].[dbo].[QuestionOptionMappings]
WHERE [QuestionOptionId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[QuestionOptions])
  AND (RegistrableId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[Registrables]) OR RegistrableId IS NULL)


INSERT INTO [AZURE_EA].[event-admin].[dbo].[Registrations]
           ([Id]
           ,[EventId]
           ,[RegistrationFormId]
           --,[RegistrationId_Partner]
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
      --,[RegistrationId_Partner]
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

UPDATE NEW
SET RegistrationId_Partner = OLD.RegistrationId_Partner
FROM [AZURE_EA].[event-admin].[dbo].[Registrations]              NEW 
  INNER JOIN [AZURE_ER].[EventRegistrator].[dbo].[Registrations] OLD ON OLD.Id = NEW.Id


INSERT INTO [AZURE_EA].[event-admin].[dbo].[Spots]
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
INSERT INTO [AZURE_EA].[event-admin].[dbo].[PaymentsFiles]
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

INSERT INTO [AZURE_EA].[event-admin].[dbo].[PayoutRequests]
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

INSERT INTO [AZURE_EA].[event-admin].[dbo].[PaymentSlips]
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
  WHERE EventId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].[Events])


INSERT INTO [AZURE_EA].[event-admin].[dbo].Payments
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
  WHERE [PaymentFileId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentSlips))

INSERT INTO [AZURE_EA].[event-admin].[dbo].IncomingPayments
           ([Id]
           ,[PaymentSlipId]
           ,[DebitorIban]
           ,[DebitorName])
SELECT [Id]
      ,[PaymentSlipId]
      ,[DebitorIban]
      ,[DebitorName]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[ReceivedPayments]
  WHERE [PaymentFileId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentSlips))
	AND [CreditDebitType] = 1

INSERT INTO [AZURE_EA].[event-admin].[dbo].OutgoingPayments
           ([Id]
           ,[CreditorName]
           ,[CreditorIban])
SELECT [Id]
      ,[CreditorName]
      ,[CreditorIban]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[ReceivedPayments]
  WHERE [PaymentFileId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentsFiles)
    AND (PaymentSlipId IS NULL OR PaymentSlipId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].PaymentSlips))
	AND [CreditDebitType] = 2

INSERT INTO [AZURE_EA].[event-admin].[dbo].[PaymentAssignments]
           ([Id]
           ,[RegistrationId]
           ,IncomingPaymentId
           --,[PaymentAssignmentId_Counter]
           ,OutgoingPaymentId
           ,[PayoutRequestId]
           ,[Amount]
           ,[Created])
SELECT [Id]
      ,[RegistrationId]
      ,[ReceivedPaymentId]
      --,[PaymentAssignmentId_Counter]
      ,[PaymentId_Repayment]
      ,[PayoutRequestId]
      ,[Amount]
      ,[Created]
  FROM [AZURE_ER].[EventRegistrator].[dbo].[PaymentAssignments]
  WHERE [ReceivedPaymentId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].Payments)
    AND [ReceivedPaymentId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].IncomingPayments)

INSERT INTO [AZURE_EA].[event-admin].[dbo].[PaymentAssignments]
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
  WHERE [ReceivedPaymentId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].Payments)
    AND [ReceivedPaymentId] IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].OutgoingPayments)

UPDATE NEW
SET PaymentAssignmentId_Counter = OLD.PaymentAssignmentId_Counter
FROM [AZURE_EA].[event-admin].[dbo].[PaymentAssignments]              NEW 
  INNER JOIN [AZURE_ER].[EventRegistrator].[dbo].[PaymentAssignments] OLD ON OLD.Id = NEW.Id


INSERT INTO [AZURE_EA].[event-admin].[dbo].[MailTemplates]
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

INSERT INTO [AZURE_EA].[event-admin].[dbo].AutoMailTemplates
           ([Id]
           ,[EventId]
           ,[Language]
           ,[Subject]
           ,[ContentHtml]
           ,[Type]
           ,[ReleaseImmediately])
     SELECT [Id]
           ,[EventId]
           ,[Language]
           ,[Subject]
           ,[Template]
           ,[Type]
           ,[ReleaseImmediately]
FROM [AZURE_ER].[EventRegistrator].[dbo].[MailTemplates]
WHERE [Type] <> 0



INSERT INTO [AZURE_EA].[event-admin].[dbo].[Mails]
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


INSERT INTO [AZURE_EA].[event-admin].[dbo].[MailsToRegistrations]
           ([Id]
           ,[MailId]
           ,[RegistrationId]
           ,[State])
SELECT [Id]
           ,[MailId]
           ,[RegistrationId]
           ,[State]
FROM [AZURE_ER].[EventRegistrator].[dbo].[MailToRegistrations]
WHERE MailId IN (SELECT Id FROM [AZURE_EA].[event-admin].[dbo].Mails)


INSERT INTO [AZURE_EA].[event-admin].[dbo].[Sms]
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


INSERT INTO [AZURE_EA].[event-admin].[dbo].[RawRegistrationForms]
           ([Id]
           ,[FormExternalIdentifier]
           ,[ReceivedMessage]
           ,[Created]
           ,[EventAcronym]
           ,[Processed])
SELECT [Id]
      ,[FormExternalIdentifier]
      ,[ReceivedMessage]
      ,[Created]
      ,[EventAcronym]
      ,CASE WHEN Processed THEN [Created] ELSE NULL END
  FROM [AZURE_ER].[EventRegistrator].[dbo].[RawRegistrationForms]
   
INSERT INTO [AZURE_EA].[event-admin].[dbo].[RegistrationForms]
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

--UPDATE BankAccountbookings
--SET Charges = 12
--WHERE Id = '4B15D3EA-3317-4921-AE6A-1DD373E6DB9E'

ROLLBACK
--COMMIT


