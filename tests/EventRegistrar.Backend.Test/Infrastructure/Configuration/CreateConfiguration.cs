using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Test.TestInfrastructure;
using Newtonsoft.Json;
using Xunit;

namespace EventRegistrar.Backend.Test.Infrastructure.Configuration
{
    public class CreateConfigurationTest : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _integrationTestEnvironment;

        public CreateConfigurationTest(IntegrationTestEnvironment integrationTestEnvironment)
        {
            _integrationTestEnvironment = integrationTestEnvironment;
        }

        [Fact]
        public void GenerateJson()
        {
            var configuration = new ExternalMailConfiguration
            {
            };
            var eventId = "B546D28C-97E8-4B94-BC07-D32633AF980C";

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