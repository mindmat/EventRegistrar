using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Hosting
{
    public class HardcodedLL19HostingConfiguration : HostingConfiguration, IDefaultConfigurationItem
    {
        public HardcodedLL19HostingConfiguration()
        {
            RegistrableId_HostingOffer = new Guid("46F2F019-BA6E-4480-8C06-82F8528E7ED5");
            RegistrableId_HostingSeeker = new Guid("0A90D0B1-0935-4237-B9FA-64C02DB687E5");
            ColumnsOffers = new Dictionary<string, Guid>
            {
                { "Location",    new Guid("A159C44B-06DF-400C-BD35-306A140AD265") },
                { "CountTotal",  new Guid("CE685D63-358F-4510-8CD6-8FDF19EC0735") },
                { "CountShared", new Guid("36AB4D23-3C04-466F-95C7-25C299C102A8") },
                { "Comment",     new Guid("F8BB28FD-429E-4E2A-AB3D-33088362448C") }
            };
            ColumnsRequests = new Dictionary<string, Guid>
            {
                { "Partner",            new Guid("02465933-5238-4D49-9F55-08DCC7B98C00") },
                { "ShareOkWithPartner", new Guid("CE685D63-358F-4510-8CD6-8FDF19EC0735") },
                { "ShareOkWithRandom",  new Guid("08B9350C-3A6A-40A0-BDF7-C50A24861040") },
                { "Travel",             new Guid("07586716-58FD-4A9F-A9E9-16CDB9F23690") },
                { "Comment",            new Guid("C3946039-630A-4235-BD94-FA50F27081F7") }
            };
        }
    }

    public class HostingConfiguration : IConfigurationItem
    {
        public IDictionary<string, Guid> ColumnsOffers { get; set; }
        public IDictionary<string, Guid> ColumnsRequests { get; set; }
        public Guid RegistrableId_HostingOffer { get; set; }
        public Guid RegistrableId_HostingSeeker { get; set; }
    }
}