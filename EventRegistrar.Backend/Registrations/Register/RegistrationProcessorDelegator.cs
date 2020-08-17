using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.RegistrationForms.FormPaths;
using EventRegistrar.Backend.Spots;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class RegistrationProcessorDelegator
    {
        private readonly PartnerRegistrationProcessor _partnerRegistrationProcessor;
        private readonly IQueryable<FormPath> _formPaths;
        private readonly JsonHelper _jsonHelper;
        private readonly SingleRegistrationProcessor _singleRegistrationProcessor;

        public RegistrationProcessorDelegator(SingleRegistrationProcessor singleRegistrationProcessor,
                                              PartnerRegistrationProcessor partnerRegistrationProcessor,
                                              IQueryable<FormPath> formPaths,
                                              JsonHelper jsonHelper)
        {
            _singleRegistrationProcessor = singleRegistrationProcessor;
            _partnerRegistrationProcessor = partnerRegistrationProcessor;
            _formPaths = formPaths;
            _jsonHelper = jsonHelper;
        }

        public Task<IEnumerable<Seat>> Process(Registration registration)
        {
            return _singleRegistrationProcessor.Process(registration);

            //var processConfigurations = await GetConfigurations(registrationFormId);
            //var spots = new List<Seat>();
            //foreach (var registrationProcessConfiguration in processConfigurations)
            //{
            //    if (registrationProcessConfiguration is SingleRegistrationProcessConfiguration singleConfig
            //     && (!singleConfig.QuestionOptionId_Trigger.HasValue // no trigger -> process all registrations
            //      || registration.Responses.Any(rsp => rsp.QuestionOptionId.HasValue &&
            //                                           rsp.QuestionOptionId.Value == singleConfig.QuestionOptionId_Trigger)))
            //    {
            //        var newSpots = await _singleRegistrationProcessor.Process(registration);
            //        spots.AddRange(newSpots);
            //    }
            //    else if (registrationProcessConfiguration is PartnerRegistrationProcessConfiguration coupleConfig
            //          && registration.Responses.Any(rsp => rsp.QuestionOptionId.HasValue &&
            //                                               rsp.QuestionOptionId.Value == coupleConfig.QuestionOptionId_Trigger))
            //    {
            //        var newSpots = await _partnerRegistrationProcessor.Process(registration, coupleConfig);
            //        spots.AddRange(newSpots);
            //    }
            //}

            //return spots;
        }

        private async Task<IEnumerable<IRegistrationProcessConfiguration>> GetConfigurations(Guid registrationFormId)
        {
            // ToDo: multiple configs per form
            var configs = await _formPaths.Where(fpt => fpt.RegistrationFormId == registrationFormId)
                                          .ToListAsync();
            if (configs.Any())
            {
                return configs.Select(fpt => _jsonHelper.Deserialize<IRegistrationProcessConfiguration>(fpt.ConfigurationJson));
            }
            //IRegistrationProcessConfiguration? config = null;
            //if (form.ProcessConfigurationJson != null)
            //{
            //    if (form.Type == FormPathType.Single)
            //    {
            //        config = _jsonHelper.TryDeserialize<SingleRegistrationProcessConfiguration>(form.ProcessConfigurationJson);
            //    }
            //}

            //if (config != null)
            //{
            //    return new[] { config };
            //}
            return GetHardcodedConfiguration(registrationFormId);
        }

        private IEnumerable<IRegistrationProcessConfiguration> GetHardcodedConfiguration(Guid registrationFormId)
        {
            // HACK: hardcoded
            if (registrationFormId == Guid.Parse("954BE8A3-3FAB-4C9C-9C0B-4B9FFDD1FF3F")) // rb18
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
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("46AD095F-A014-4321-BC5F-5C98F9060F1D"), Language.Deutsch ),
                    //    (new Guid("C77C269F-CCA1-4B42-89C5-06E5F1E2D3A4"), Language.English ),
                    //    (new Guid("509B56FD-5A99-42E5-9C8B-ED00DCA55288"), Language.Deutsch ),
                    //    (new Guid("9B78AFFF-CE9B-4EF1-AC95-957B869A13D8"), Language.English )
                    //}
                };
                yield return new PartnerRegistrationProcessConfiguration
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
                    QuestionId_Follower_Phone = Guid.Parse("0D95253E-782A-416D-B5F0-52BFFFC2E7C9"),
                    RoleSpecificMappings = new[]
                    {
                        // Helfer kurz
                        (new Guid("B7ACEA87-ED95-468A-8760-8439FE541496"), Role.Leader,   new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        (new Guid("907E3564-E89D-4F7C-9223-EE1BB23013A4"), Role.Follower, new Guid("2303AC02-480D-4EA4-B861-F01DEB7A2A5F" )),
                        // Helfer lang
                        (new Guid("D23C4941-49EE-48E3-8FBC-5E6BCCE119C8"), Role.Leader,   new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                        (new Guid("81291E5A-CB01-4453-8DF1-4F5B88724BA1"), Role.Follower, new Guid("6BFCCC2A-5C03-42EB-8642-81921701CD48" )),
                        // T-Shirt Follower
                        (new Guid("A3EC7651-293A-40B3-9844-738AEF10D931"), Role.Follower, new Guid("747C0FE9-1575-4E0B-80CB-DB229D3E00C2" )),  // Damenmodell Grösse L
                        (new Guid("1C257D0D-3808-4A19-B3D5-D2D58A9DAC00"), Role.Follower, new Guid("AC8D4748-F196-44F6-9807-E1CA1EFE29C7" )),  // Damenmodell Grösse M
                        (new Guid("B5556D7A-8188-4CF9-94F8-364993AEA89E"), Role.Follower, new Guid("3BAAE211-9B27-47B4-835F-4C9B9052DACF" )),  // Damenmodell Grösse S
                        (new Guid("9BFBDC41-9B8D-43D6-8647-CA496969CB8A"), Role.Follower, new Guid("0F185525-FF38-4CB7-8B15-0F81ACF0069F" )),  // Herrenmodell Grösse L
                        (new Guid("82C51368-BF74-43C2-80C1-F85D0A996359"), Role.Follower, new Guid("2148016C-BB24-4055-B5C2-14F0E2333ABA" )),  // Herrenmodell Grösse M
                        (new Guid("C21720AF-94A9-4B45-917D-306934F00868"), Role.Follower, new Guid("F9F46BE4-4E57-4748-897A-3708D312A8EC" )),  // Herrenmodell Grösse S
                    },
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("46AD095F-A014-4321-BC5F-5C98F9060F1D"), Language.Deutsch ),
                    //    (new Guid("C77C269F-CCA1-4B42-89C5-06E5F1E2D3A4"), Language.English ),
                    //    (new Guid("509B56FD-5A99-42E5-9C8B-ED00DCA55288"), Language.Deutsch ),
                    //    (new Guid("9B78AFFF-CE9B-4EF1-AC95-957B869A13D8"), Language.English )
                    //}
                };
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Partypass",
                    QuestionOptionId_Trigger = Guid.Parse("6FC5F608-E3EF-4961-A2FC-A07A85F9BEB5"),
                    QuestionId_FirstName = Guid.Parse("94D91BD9-560D-4B3F-9AC8-9CEB24F64ABD"),
                    QuestionId_LastName = Guid.Parse("809AC411-A99C-40F1-96BE-356316918724"),
                    QuestionId_Email = Guid.Parse("C97B62D1-F883-4D67-830D-6355311EFDC9"),
                    QuestionId_Phone = Guid.Parse("FA6734EA-19EE-4E44-8F9E-5DB3BC286BD9"),
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("B1AB2D25-FEBE-431F-B1D1-F6574341C0B2"), Language.Deutsch ),
                    //    (new Guid("C8BFCFF1-CE38-4A37-AC54-B06489CB0484"), Language.English ),
                    //}
                };
            }
            else if (registrationFormId == Guid.Parse("BD14FB5C-EC31-48F0-9DDA-E7D1BB2781C0")) // ll19
            {
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Leapin'",
                    QuestionOptionId_Trigger = null,
                    QuestionId_FirstName = Guid.Parse("6067B9FC-9421-40F8-BDBE-372E172B7A16"),
                    QuestionId_LastName = Guid.Parse("36C46DCB-178A-4300-ADAE-5890A8B2C733"),
                    QuestionId_Email = Guid.Parse("C1291FB3-61AB-4C09-8E62-2841603C10AC"),
                    QuestionId_Phone = Guid.Parse("EB488435-4313-4E51-9F7C-B4AED78F7ABC"),
                    QuestionOptionId_Leader = Guid.Parse("F5DEF570-730B-4411-82DC-42959FF2E088"),
                    QuestionOptionId_Follower = Guid.Parse("AB8363D9-F816-4927-BFED-078E04201C50"),
                    QuestionId_Remarks = Guid.Parse("59B34ABE-E14A-4E33-B829-E50D365B4C12"),
                    QuestionOptionId_Reduction = Guid.Parse("25D7532F-8850-4653-B4A9-A0D07D5E9BEE")
                };
            }
            else if (registrationFormId == Guid.Parse("196B878A-510E-49D9-86D1-9E41665D0544")) // ll19 party passes
            {
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Leapin' Partypässe",
                    QuestionOptionId_Trigger = null,
                    QuestionId_FirstName = Guid.Parse("FCFF7C75-3016-4A56-8BD0-560F3C6CA6D8"),
                    QuestionId_LastName = Guid.Parse("3C7268D7-F4BD-4302-AFC3-C09DB53E822A"),
                    QuestionId_Email = Guid.Parse("4419F2F5-4F02-462D-8CD4-86B3D0008E3B"),
                    QuestionId_Phone = Guid.Parse("4632EF7D-1D88-48B2-A528-FC21E13F94D6"),
                    QuestionId_Remarks = Guid.Parse("DBE4F7E8-CDF7-489B-A853-A8F063B669BA"),
                    QuestionOptionId_Reduction = Guid.Parse("CD825A86-78A7-45B3-A354-8A14BEAB68A6")
                };
            }
            else if (registrationFormId == Guid.Parse("8A39B7CD-D97F-4438-8604-D85E90925DF5")) // rb19
            {
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Balboa Einzelanmeldung Basic",
                    QuestionOptionId_Trigger = Guid.Parse("049062B2-C366-4A4B-86EC-412C0C0F3BE8"),
                    QuestionId_FirstName = Guid.Parse("8A45197C-8A01-4680-A172-E7B7794A6908"),
                    QuestionId_LastName = Guid.Parse("DDC31F78-5056-4388-AFBB-2BA61D389A74"),
                    QuestionId_Email = Guid.Parse("5BF3C34A-30E3-460D-AC75-FEADFC3072D7"),
                    QuestionId_Phone = Guid.Parse("7EA90B1D-160C-43BB-95B6-065FE82CE988"),
                    QuestionOptionId_Leader = Guid.Parse("2F9EB3BF-5772-417E-922A-E0CC6706F289"),
                    QuestionOptionId_Follower = Guid.Parse("C7EC7438-A03E-4EEA-8224-CF9DFF467DE4"),
                    QuestionOptionId_Reduction = Guid.Parse("BDB67C48-096A-4143-B1C4-4E546368E34F"),
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("09C78885-1414-485D-9B79-9804410020BC"), Language.Deutsch ),
                    //    (new Guid("D82315E1-90B3-4D34-AF56-625A57A96D8F"), Language.English )
                    //}
                };
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Balboa Einzelanmeldung INT/ADV",
                    QuestionOptionId_Trigger = Guid.Parse("4990F043-DACD-49B3-8C19-D8E7DF6B8053"),
                    QuestionId_FirstName = Guid.Parse("98859C38-112C-441A-AEFC-38DFDB49841A"),
                    QuestionId_LastName = Guid.Parse("DAED7E0F-03D0-4928-BC5D-8F042B4E7F16"),
                    QuestionId_Email = Guid.Parse("CC30792D-4388-4E71-9F31-24B3910314E1"),
                    QuestionId_Phone = Guid.Parse("D6AA5E29-104D-46A4-BD8B-79351595FE87"),
                    QuestionOptionId_Leader = Guid.Parse("3E0C3286-2531-4505-BEB2-45EB3807A8D0"),
                    QuestionOptionId_Follower = Guid.Parse("2DC7461D-7440-4B2E-AE1A-1BBE1255511A"),
                    QuestionOptionId_Reduction = Guid.Parse("D15EDBAF-24F2-4B09-AF1B-CD02298708B9"),
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("08D1F924-BBC4-4CAD-83E8-92890BB1C39C"), Language.Deutsch ),
                    //    (new Guid("A9BA2C11-F67A-4D31-8E4A-323682C706CF"), Language.English )
                    //}
                };
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Partypass",
                    QuestionOptionId_Trigger = Guid.Parse("BC74AB4A-9475-467B-9017-987F48E2C7B1"),
                    QuestionId_FirstName = Guid.Parse("EDAA4003-C85E-44D4-AFDA-DAE2A83FC20D"),
                    QuestionId_LastName = Guid.Parse("966F1966-A135-40E7-BD7C-C7B1FDC8245F"),
                    QuestionId_Email = Guid.Parse("490F5556-B79C-4AF3-84AA-1D0B4F820F93"),
                    QuestionId_Phone = Guid.Parse("735AE310-01E5-4279-8E7A-96325FBA3328"),
                    //LanguageMappings = new[]
                    //{
                    //    (new Guid("A3D304E2-7E5C-4B77-B6F0-36EFF6E8192C"), Language.Deutsch ),
                    //    (new Guid("73AC9B2F-1109-446A-946E-7AA50F09D649"), Language.English )
                    //}
                };

            }
            else if (registrationFormId == Guid.Parse("E03006B4-4DA6-40FE-8DF8-0D551CB39786")) // ll20
            {
                yield return new SingleRegistrationProcessConfiguration
                {
                    Description = "Leapin'",
                    QuestionId_FirstName = Guid.Parse("A56611DC-F858-4E25-B8DB-A621453DCD92"),
                    QuestionId_LastName = Guid.Parse("61863101-81A6-43CE-83AE-685D8527166D"),
                    QuestionId_Email = Guid.Parse("4AC1A1C6-1F0B-4AAE-AD27-A85847B32CD4"),
                    QuestionId_Phone = Guid.Parse("420D6986-C0B4-4D31-A5E5-E8A05DA2A45E"),
                    QuestionId_Remarks = Guid.Parse("53E0BE8C-CAE5-4950-91E0-0690FB8FDF7B"),
                    QuestionOptionId_Reduction = Guid.Parse("75B965CA-E4CD-4FE1-BE46-3C63CBDA7E0E")
                };
            }

        }
    }
}