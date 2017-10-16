﻿BEGIN TRAN

DELETE FROM Registrables
DELETE FROM QuestionOptionToRegistrableMappings

INSERT INTO [dbo].[Registrables]([Id], [EventId], [Name], [MaximumSingleSeats], [MaximumDoubleSeats], MaximumAllowedImbalance)
     VALUES('118A9B4F-D14E-4FD4-9E1C-A6771416E088', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Lindy Hop Beginner Intermediate', NULL, 35,   5),
           ('2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Lindy Hop Intermediate',          NULL, 35,   5),
           ('9310B016-D70F-4EB1-8F04-57412132F4A8', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Lindy Hop Intermediate Advanced', NULL, 35,   5),
           ('373E0514-2F5F-4499-990A-A130B9D38142', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Lindy Hop Advanced',              NULL, 35,   5),
           ('11CB1EB9-D823-4AD3-BFA5-4D8440830226', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Lindy Hop Advanced+',             NULL, 35,   5),

           ('0488F651-A8B7-4369-8918-31EBABF7763B', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Solo Jazz Intermediate',          40,   NULL, NULL),
           ('57DE8C38-09EB-47EC-8697-A8D700CBDA44', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Solo Jazz Advanced',              40,   NULL, NULL),

           ('CC695010-41F4-4B6B-B4E5-32294284EDB5', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Solo Friday',                     100,  NULL, NULL),

           ('8720326D-D055-4E65-A1F9-A8CA14510652', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Mittagessen Fleisch',             NULL, NULL, NULL),
           ('B6A69D4B-9FB8-4E6B-89E5-4BCFEA0EAC31', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Mittagessen Vegi',                NULL, NULL, NULL),

           ('A425D0FD-DAF0-4783-8AE9-10029076063E', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Party Donnerstag',                NULL, NULL, NULL),
           ('88F013C5-5915-4513-98CE-E58D4DB9875E', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Party Freitag',                   NULL, NULL, NULL),
           ('C202D212-E69E-44DB-81AF-09045CD5E13B', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Party Samstag',                   NULL, NULL, NULL),

           ('1746CC39-7C95-4D03-B1BF-D599689B7B6A', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Bietet Hosting',                  NULL, NULL, NULL),
           ('EE10CE23-8219-44DF-9F2A-4FDEC3DE1867', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Sucht Hosting',                   NULL, NULL, NULL),

           ('16371CE3-8316-49E3-B791-AD38B03BD859', '762A93A4-56E0-402C-B700-1CFB3362B39D', 'Helfereinsatz',                   NULL, NULL, NULL)

INSERT INTO [dbo].[QuestionOptionToRegistrableMappings](Id, RegistrableId, QuestionOptionId, QuestionId_PartnerEmail, QuestionOptionId_Leader, QuestionOptionId_Follower)
  VALUES -- Lindy Hop Workshop
         (NEWID(), '118A9B4F-D14E-4FD4-9E1C-A6771416E088', '6CC551CC-E8D0-43A5-AA6A-937CEB611D41', 'B96DFED0-5D10-4EA1-AFAA-0F2E164FE4A5', '20ABE66F-9EAF-475E-9021-2C455055E6E1', '1562A341-FA98-4F27-A02F-E60207BC800D'),
		 (NEWID(), '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98', '9CC93D82-0D28-4A9D-8921-77935DD3B145', 'B96DFED0-5D10-4EA1-AFAA-0F2E164FE4A5', '20ABE66F-9EAF-475E-9021-2C455055E6E1', '1562A341-FA98-4F27-A02F-E60207BC800D'),
         (NEWID(), '9310B016-D70F-4EB1-8F04-57412132F4A8', 'EE307BC1-67A4-4574-A1E7-997AF010A5A0', 'B96DFED0-5D10-4EA1-AFAA-0F2E164FE4A5', '20ABE66F-9EAF-475E-9021-2C455055E6E1', '1562A341-FA98-4F27-A02F-E60207BC800D'),
         (NEWID(), '373E0514-2F5F-4499-990A-A130B9D38142', '30D30F2B-42B4-4A8C-9287-55C0D8350E90', 'B96DFED0-5D10-4EA1-AFAA-0F2E164FE4A5', '20ABE66F-9EAF-475E-9021-2C455055E6E1', '1562A341-FA98-4F27-A02F-E60207BC800D'),
         (NEWID(), '11CB1EB9-D823-4AD3-BFA5-4D8440830226', 'CA2035A3-437E-49A1-A0BC-0EF7C30D4527', 'B96DFED0-5D10-4EA1-AFAA-0F2E164FE4A5', '20ABE66F-9EAF-475E-9021-2C455055E6E1', '1562A341-FA98-4F27-A02F-E60207BC800D'),
         
		 -- Solo Jazz Workshop
         (NEWID(), '0488F651-A8B7-4369-8918-31EBABF7763B', '7BBB7580-1108-4503-A357-DE038D458058', NULL, NULL, NULL),
         (NEWID(), '57DE8C38-09EB-47EC-8697-A8D700CBDA44', '43BD5FEA-A45C-458A-A54E-F345C89D7009', NULL, NULL, NULL),
         
		 -- Solo Firday
         (NEWID(), 'CC695010-41F4-4B6B-B4E5-32294284EDB5', '54DA45AE-3F9C-4DBC-8D85-39ADA67CD9D6', NULL, NULL, NULL),
         
		 -- Essen
         (NEWID(), '8720326D-D055-4E65-A1F9-A8CA14510652', 'AA139C77-D8E2-4A87-BA6F-CC43B6B2C7C5', NULL, NULL, NULL),
         (NEWID(), '8720326D-D055-4E65-A1F9-A8CA14510652', 'F1746343-DEFF-489C-9F26-431B389FD632', NULL, NULL, NULL),
         (NEWID(), 'B6A69D4B-9FB8-4E6B-89E5-4BCFEA0EAC31', '2459C5E1-F548-4F81-A789-5FA308D469DC', NULL, NULL, NULL),
         (NEWID(), 'B6A69D4B-9FB8-4E6B-89E5-4BCFEA0EAC31', '359A7EA0-9C18-4E4F-9013-E55024B73CB0', NULL, NULL, NULL),
         
		 -- Parties
         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', 'F0E33B02-EEB7-46D5-B98C-5329CCF43F55', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '69820841-65C0-42D9-A569-196A16470FF1', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', 'E4C10FAB-BD0C-46B6-883F-5CF3F9DAA03B', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', '6CC551CC-E8D0-43A5-AA6A-937CEB611D41', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '6CC551CC-E8D0-43A5-AA6A-937CEB611D41', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', '6CC551CC-E8D0-43A5-AA6A-937CEB611D41', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', '9CC93D82-0D28-4A9D-8921-77935DD3B145', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '9CC93D82-0D28-4A9D-8921-77935DD3B145', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', '9CC93D82-0D28-4A9D-8921-77935DD3B145', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', 'EE307BC1-67A4-4574-A1E7-997AF010A5A0', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', 'EE307BC1-67A4-4574-A1E7-997AF010A5A0', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', 'EE307BC1-67A4-4574-A1E7-997AF010A5A0', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', '30D30F2B-42B4-4A8C-9287-55C0D8350E90', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '30D30F2B-42B4-4A8C-9287-55C0D8350E90', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', '30D30F2B-42B4-4A8C-9287-55C0D8350E90', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', 'CA2035A3-437E-49A1-A0BC-0EF7C30D4527', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', 'CA2035A3-437E-49A1-A0BC-0EF7C30D4527', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', 'CA2035A3-437E-49A1-A0BC-0EF7C30D4527', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', '7BBB7580-1108-4503-A357-DE038D458058', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '7BBB7580-1108-4503-A357-DE038D458058', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', '7BBB7580-1108-4503-A357-DE038D458058', NULL, NULL, NULL),

         (NEWID(), 'A425D0FD-DAF0-4783-8AE9-10029076063E', '43BD5FEA-A45C-458A-A54E-F345C89D7009', NULL, NULL, NULL),
         (NEWID(), '88F013C5-5915-4513-98CE-E58D4DB9875E', '43BD5FEA-A45C-458A-A54E-F345C89D7009', NULL, NULL, NULL),
         (NEWID(), 'C202D212-E69E-44DB-81AF-09045CD5E13B', '43BD5FEA-A45C-458A-A54E-F345C89D7009', NULL, NULL, NULL),
         
		 -- Hosting
         (NEWID(), '1746CC39-7C95-4D03-B1BF-D599689B7B6A', '0E311F67-FB0F-4241-936E-C8BE0BAB9EE0', NULL, NULL, NULL),
         (NEWID(), 'EE10CE23-8219-44DF-9F2A-4FDEC3DE1867', 'A2BAD46F-0512-437B-A2F8-FD3DF8739DEC', NULL, NULL, NULL),
         
		 -- Helfereinsatz
         (NEWID(), '16371CE3-8316-49E3-B791-AD38B03BD859', '948F6AF3-1596-4715-90AB-CE64B89A0F51', NULL, NULL, NULL)

SELECT * FROM [Registrables]
SELECT * FROM [QuestionOptionToRegistrableMappings]

ROLLBACK

-- COMMIT