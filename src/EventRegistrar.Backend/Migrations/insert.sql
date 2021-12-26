BEGIN TRAN

INSERT INTO [dbo].[RegistrableTags]
           ([Id]
           ,[EventId]
           ,[Tag]
           ,[FallbackText]
           ,[SortKey])
     VALUES
           ('0EC9FFC3-DF30-4356-BE6C-69BCD64CC547'
           ,'40EB7B32-696E-41D5-9A57-AE9A45344E2B'
           ,'workshop'
           ,'Workshop'
           ,1),     
           ('D6EF37EF-ED49-47B9-8AF0-D877194FA121'
           ,'40EB7B32-696E-41D5-9A57-AE9A45344E2B'
           ,'party'
           ,'Party'
           ,2),
           ('255F1C42-7BF8-4C94-BECD-06D8C2D44E09'
           ,'40EB7B32-696E-41D5-9A57-AE9A45344E2B'
           ,'hosting'
           ,'Hosting'
           ,3)

UPDATE dbo.Registrables
SET [Name] = 'Lindy Hop',
    NameSecondary = 'Beginner Intermediate',
    Tag = 'workshop'
WHERE Id = '11186E6A-18A3-46D8-ACCB-DD99A433823B'

UPDATE dbo.Registrables
SET [Name] = 'Lindy Hop',
    NameSecondary = 'Intermediate',
    Tag = 'workshop'
WHERE Id = '6398B760-0B79-41AA-9558-729A967D6463'

UPDATE dbo.Registrables
SET [Name] = 'Lindy Hop',
    NameSecondary = 'Intermediate Advanced',
    Tag = 'workshop'
WHERE Id = 'DAA2C9EA-8880-4204-B940-8B2355F83A1D'

UPDATE dbo.Registrables
SET [Name] = 'Lindy Hop',
    NameSecondary = 'Advanced/Advanced+',
    Tag = 'workshop'
WHERE Id = '52CCF468-3B27-4A6E-9CEB-6BED906F2E76'


UPDATE dbo.Registrables
SET [Name] = 'Shag',
    NameSecondary = 'Basics',
    Tag = 'workshop'
WHERE Id = '8700C7D3-9345-4F91-A6A6-AC26C0BDF24D'

UPDATE dbo.Registrables
SET [Name] = 'Shag',
    NameSecondary = 'Variations',
    Tag = 'workshop'
WHERE Id = '32BB78D9-2191-4586-A424-074082428A4A'


UPDATE dbo.Registrables
SET [Name] = 'Solo Jazz',
    NameSecondary = 'Relaxing',
    Tag = 'workshop'
WHERE Id = 'EAE1588B-854F-4123-B2A0-8B1D804D581D'

UPDATE dbo.Registrables
SET [Name] = 'Solo Jazz',
    NameSecondary = 'Challenging',
    Tag = 'workshop'
WHERE Id = '229904D4-860C-48A1-9B9C-3E673A47E21B'


UPDATE dbo.Registrables
SET Tag = 'party'
WHERE Id = '5E80A090-4F5A-4F7C-9405-727035680378'

UPDATE dbo.Registrables
SET Tag = 'party'
WHERE Id = 'F18565D1-EC2D-4298-9E74-85BA275C9CAD'

UPDATE dbo.Registrables
SET Tag = 'party'
WHERE Id = '52860E7F-DF06-4D32-BE56-C037C5B1405B'


UPDATE dbo.Registrables
SET Tag = 'hosting'
WHERE Id = 'D7EAA162-969D-4FFB-A156-067CCEB5C378'

UPDATE dbo.Registrables
SET Tag = 'hosting'
WHERE Id = '62DE4041-9640-49F6-AAFC-33271687F344'

--select *
--from registrables
--where eventid = '40EB7B32-696E-41D5-9A57-AE9A45344E2B'
--ORDER BY ShowInMailListOrder

ROLLBACK
--COMMIT

