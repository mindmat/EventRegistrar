using System.Collections.Generic;

namespace EventRegistrator.Functions.Payments
{
    public class CamtFile
    {
        public IReadOnlyCollection<CamtEntry> Entries { get; set; }
        public string Account { get; set; }
        public string Owner { get; set; }
        public string FileId { get; set; }
    }
}