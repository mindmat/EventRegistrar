using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Spots;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class RegistrationProcessorDelegator
    {
        private readonly CoupleRegistrationProcessor _coupleRegistrationProcessor;
        private readonly SingleRegistrationProcessor _singleRegistrationProcessor;

        public RegistrationProcessorDelegator(SingleRegistrationProcessor singleRegistrationProcessor,
                                              CoupleRegistrationProcessor coupleRegistrationProcessor)
        {
            _singleRegistrationProcessor = singleRegistrationProcessor;
            _coupleRegistrationProcessor = coupleRegistrationProcessor;
        }

        public async Task<IEnumerable<Seat>> Process(Registration registration, RegistrationForm form)
        {
            var processConfiguration = form.ProcessConfigurationJson != null
                ? JsonConvert.DeserializeObject<IEnumerable<IRegistrationProcessConfiguration>>(form.ProcessConfigurationJson)
                : GetHardcodedConfiguration(form.Id);
            var spots = new List<Seat>();
            foreach (var registrationProcessConfiguration in processConfiguration)
            {
                if (registrationProcessConfiguration is SingleRegistrationProcessConfiguration singleConfig
                 && registration.Responses.Any(rsp => rsp.QuestionOptionId.HasValue &&
                                                      rsp.QuestionOptionId.Value == singleConfig.QuestionOptionId_Trigger))
                {
                    var newSpots = await _singleRegistrationProcessor.Process(registration, singleConfig);
                    spots.AddRange(newSpots);
                }
                else if (registrationProcessConfiguration is CoupleRegistrationProcessConfiguration coupleConfig
                      && registration.Responses.Any(rsp => rsp.QuestionOptionId.HasValue &&
                                                           rsp.QuestionOptionId.Value == coupleConfig.QuestionOptionId_Trigger))
                {
                    var newSpots = await _coupleRegistrationProcessor.Process(registration, coupleConfig);
                    spots.AddRange(newSpots);
                }
            }

            return spots;
        }

        private IEnumerable<IRegistrationProcessConfiguration> GetHardcodedConfiguration(Guid formId)
        {
            if (formId == Guid.Parse("954BE8A3-3FAB-4C9C-9C0B-4B9FFDD1FF3F"))
            {
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Balboa Einzelanmeldung",
                    QuestionOptionId_Trigger = Guid.Parse("1029CAAD-CE7E-4A36-9D30-AD01F0B5F11F"),
                    QuestionId_FirstName = Guid.Parse("543EBEFB-FC46-452B-814C-E209425F9D7A"),
                    QuestionId_LastName = Guid.Parse("313C4FFF-B58F-4B87-947F-CBCA74D7D912"),
                    QuestionId_Email = Guid.Parse("8CB73EA6-F663-4CDF-B6F0-77F560F2DDF6"),
                    QuestionId_Phone = Guid.Parse("3AFD336A-3C57-4A6A-A1BB-A0824B14431C"),
                    QuestionOptionId_Leader = Guid.Parse("F5DEF570-730B-4411-82DC-42959FF2E088"),
                    QuestionOptionId_Follower = Guid.Parse("AB8363D9-F816-4927-BFED-078E04201C50"),
                    LanguageMappings = new[]
                    {
                        (new Guid("46AD095F-A014-4321-BC5F-5C98F9060F1D"), Language.Deutsch ),
                        (new Guid("C77C269F-CCA1-4B42-89C5-06E5F1E2D3A4"), Language.English ),
                        (new Guid("509B56FD-5A99-42E5-9C8B-ED00DCA55288"), Language.Deutsch ),
                        (new Guid("9B78AFFF-CE9B-4EF1-AC95-957B869A13D8"), Language.English )
                    }
                };
                yield return new CoupleRegistrationProcessConfiguration
                {
                    Description = "Balboa Paaranmeldung",
                    QuestionOptionId_Trigger = Guid.Parse("FD9AAAA7-934D-4F43-B2A1-CBC2BA73324E"),
                    QuestionId_Leader_FirstName = Guid.Parse("6686F7BF-CF57-4390-87C0-C99BCC0D7260"),
                    QuestionId_Leader_LastName = Guid.Parse("18D3853F-6BFD-4D0D-B822-51E461A79B08"),
                    QuestionId_Leader_Email = Guid.Parse("00DCCB4B-D1BD-48D8-9F7C-7B2D113915E5"),
                    QuestionId_Leader_Phone = Guid.Parse("5CF1B588-3972-48DF-B1C9-38E494AB1C3F"),
                    QuestionId_Follower_FirstName = Guid.Parse("2F67DF2D-0027-4ABE-A291-68AAD32BD572"),
                    QuestionId_Follower_LastName = Guid.Parse("7E59DF8A-CC44-46C9-81C8-6A43DCF09E4D"),
                    QuestionId_Follower_Email = Guid.Parse("8E93164B-6403-4055-B966-1B14EC5297F3"),
                    QuestionId_Follower_Phone = Guid.Parse("C2D0D7E6-FFBE-49F1-9E83-1C155B92F95C"),
                    RoleSpecificMappings = new[]
                    {
                        // Helfer kurz
                        (new Guid("4C7951D1-3914-42F3-9A79-722E7F7EDD16"), Role.Leader,   new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        (new Guid("6CF2BFD2-04F5-42B9-8C56-D1242E1D0883"), Role.Follower, new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        (new Guid("929201E9-D012-4DE1-A53A-20D4407B4DC5"), Role.Leader,   new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        (new Guid("929201E9-D012-4DE1-A53A-20D4407B4DC5"), Role.Follower, new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        // Helfer lang
                        (new Guid("80648E55-BE44-40D9-86EF-1E91E247C05B"), Role.Leader,   new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                        (new Guid("6B95705D-1ECF-4434-B569-6138283FC594"), Role.Follower, new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                        (new Guid("44975F3A-9AEF-4C25-83FB-C934A942D4E4"), Role.Leader,   new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                        (new Guid("44975F3A-9AEF-4C25-83FB-C934A942D4E4"), Role.Follower, new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                    },
                    LanguageMappings = new[]
                    {
                        (new Guid("46AD095F-A014-4321-BC5F-5C98F9060F1D"), Language.Deutsch ),
                        (new Guid("C77C269F-CCA1-4B42-89C5-06E5F1E2D3A4"), Language.English ),
                        (new Guid("509B56FD-5A99-42E5-9C8B-ED00DCA55288"), Language.Deutsch ),
                        (new Guid("9B78AFFF-CE9B-4EF1-AC95-957B869A13D8"), Language.English )
                    }
                };
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Partypass",
                    QuestionOptionId_Trigger = Guid.Parse("6FC5F608-E3EF-4961-A2FC-A07A85F9BEB5"),
                    QuestionId_FirstName = Guid.Parse("94D91BD9-560D-4B3F-9AC8-9CEB24F64ABD"),
                    QuestionId_LastName = Guid.Parse("809AC411-A99C-40F1-96BE-356316918724"),
                    QuestionId_Email = Guid.Parse("C97B62D1-F883-4D67-830D-6355311EFDC9"),
                    QuestionId_Phone = Guid.Parse("FA6734EA-19EE-4E44-8F9E-5DB3BC286BD9"),
                    LanguageMappings = new[]
                    {
                        (new Guid("B1AB2D25-FEBE-431F-B1D1-F6574341C0B2"), Language.Deutsch ),
                        (new Guid("C8BFCFF1-CE38-4A37-AC54-B06489CB0484"), Language.English ),
                    }
                };
            }
        }
    }
}