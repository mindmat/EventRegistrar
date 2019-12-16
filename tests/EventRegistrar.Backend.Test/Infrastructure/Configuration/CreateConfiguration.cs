using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Hosting;
using EventRegistrar.Backend.Infrastructure.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace EventRegistrar.Backend.Test.Infrastructure.Configuration
{
    public class CreateConfigurationTest
    {
        [Fact]
        public void GenerateJson()
        {
            var configuration = new HostingConfiguration
            {
                RegistrableId_HostingOffer = new Guid("E6D93456-8995-4087-BA60-B8064C3E3E25"),
                RegistrableId_HostingRequest = new Guid("06B0AE33-7015-472D-9DA8-99CEEE7EAFA9"),
                ColumnsOffers = new Dictionary<string, Guid>
                {
                    { "Location", new Guid("AC536E7D-767B-4B58-B337-8D3FD2960078") },
                    { "CountTotal", new Guid("3E7B68F0-D8AB-4EDB-946C-F01DBDCEDBCD") },
                    { "CountShared", new Guid("1C995737-7434-4C9A-9A05-846D229E6F22") },
                    { "Comment", new Guid("9C5E48F3-C3C6-4A4A-897F-CE0B76361220") }
                },
                ColumnsRequests = new Dictionary<string, Guid>
                {
                    { "Partner", new Guid("54368C3B-8FAB-49B6-83F8-E275F7C54C0E") },
                    { "ShareOkWithPartner", new Guid("6F4DF29A-02C6-487C-B1D2-F47150C6420A") },
                    { "ShareOkWithRandom", new Guid("301906BD-D2D0-4DD9-A36D-FAAB0203776F") },
                    { "Travel", new Guid("1E168E81-FD41-4659-8766-00933CE92547") },
                    { "Comment", new Guid("6A1C3340-092D-4738-905D-1850338BBE95") }
                },
            };

            var eventId = "8FE9F4AD-42C3-447D-ABD6-66E07D3FA848";
            //"7A84C480-C587-4476-AE78-53E054CB586D";

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            };
            var json = JsonConvert.SerializeObject(configuration, settings);
            var configType = configuration.GetType();

            var key = configType.FullName;
            if (configType.IsAssignableFrom(typeof(IDefaultConfigurationItem)))
            {
                key = configType.BaseType?.FullName ?? key;
            }

            var sql = $@"IF NOT EXISTS( SELECT TOP 1 1 FROM dbo.EventConfiguration WHERE [Type] = '{configType}' AND EventId = '{eventId}' )
BEGIN
  INSERT INTO dbo.EventConfiguration(Id, EventId, [Type], ValueJson)
  VALUES(NEWID(), '{eventId}', '{configType}', '{json}')

  PRINT('Info: EventConfiguration ""{configType}"" wurde erstellt');
END
ELSE
BEGIN
  PRINT('Warning: EventConfiguration ""{key}"" existiert bereits');
END;
";
        }
    }
}