using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Seats;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class RegistrationProcessorDelegator
    {
        private readonly SingleRegistrationProcessor _singleRegistrationProcessor;

        public RegistrationProcessorDelegator(SingleRegistrationProcessor singleRegistrationProcessor)
        {
            _singleRegistrationProcessor = singleRegistrationProcessor;
        }

        public async Task<IEnumerable<Seat>> Process(Registration registration, RegistrationForm form)
        {
            var processConfiguration = form.ProcessConfigurationJson != null
                ? JsonConvert.DeserializeObject<IEnumerable<IRegistrationProcessConfiguration>>(form.ProcessConfigurationJson)
                : GetHardcodedConfiguration(form.Id);
            var spots = new List<Seat>();
            foreach (var registrationProcessConfiguration in processConfiguration)
            {
                if (registrationProcessConfiguration is SingleRegistrationProcessConfiguration singleConfig)
                {
                    var newSpots = await _singleRegistrationProcessor.Process(registration, singleConfig);
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
                    QuestionOptionId_Follower = Guid.Parse("AB8363D9-F816-4927-BFED-078E04201C50")
                };
            }
        }
    }
}