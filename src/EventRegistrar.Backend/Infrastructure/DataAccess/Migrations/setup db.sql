CREATE USER [eventregistrator] FOR LOGIN [eventregistrator] WITH DEFAULT_SCHEMA=[dbo]
GO
sys.sp_addrolemember @rolename = N'db_owner', @membername = N'eventregistrator'
GO
sys.sp_addrolemember @rolename = N'db_datareader', @membername = N'eventregistrator'
GO
sys.sp_addrolemember @rolename = N'db_datawriter', @membername = N'eventregistrator'
GO
grant select, update, delete, insert to eventregistrator 