using System;
using System.Collections.Generic;
using System.Linq;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations.Register;

using Shouldly;

using Xunit;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public class JsonHelperTests
    {
        [Fact]
        public void TestFormMappingSerializationRoundtrip()
        {
            // Arrange
            var singleConfig = new SingleRegistrationProcessConfiguration
            {
                Description = "SingleTest",
                QuestionId_FirstName = new Guid("67243B20-79D8-4EDA-9868-E46D71FAB80E"),
                QuestionOptionId_Leader = new Guid("6EB8702D-6C67-41BB-BC5C-8CEB4C2FE53B")
            };
            var partnerConfig = new CoupleRegistrationProcessConfiguration
            {
                Description = "PartnerTest",
                QuestionId_Leader_FirstName = new Guid("F7E809C0-A339-40D0-8832-BFB88BD755F9"),
            };
            var config = new IRegistrationProcessConfiguration[]
            {
                singleConfig,
                partnerConfig
            };

            // Act
            var helper = new JsonHelper();
            var serialized = helper.Serialize(config);
            var roundtrip = helper.TryDeserialize<IEnumerable<IRegistrationProcessConfiguration>>(serialized)
                                  .ToList();

            // Assert
            var singleConfigRoundtrip = roundtrip.Single(cfg => cfg.Description == singleConfig.Description)
                                                 .ShouldBeOfType<SingleRegistrationProcessConfiguration>();
            singleConfigRoundtrip.QuestionId_FirstName.ShouldBe(singleConfig.QuestionId_FirstName);
            var partnerConfigRoundtrip = roundtrip.Single(cfg => cfg.Description == partnerConfig.Description)
                                                  .ShouldBeOfType<CoupleRegistrationProcessConfiguration>();
            partnerConfigRoundtrip.QuestionId_Leader_FirstName.ShouldBe(partnerConfig.QuestionId_Leader_FirstName);
        }

    }
}