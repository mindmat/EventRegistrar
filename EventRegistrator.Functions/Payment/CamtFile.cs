using System.Collections.Generic;

namespace EventRegistrator.Functions.Payment
{
    public class CamtFile
    {
        public IReadOnlyCollection<CamtEntry> Entries { get; set; }
        public string Account { get; set; }
        public string Owner { get; set; }
    }
}