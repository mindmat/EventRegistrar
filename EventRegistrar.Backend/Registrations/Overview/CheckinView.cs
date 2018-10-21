using System.Collections.Generic;

namespace EventRegistrar.Backend.Registrations.Overview
{
    public class CheckinView
    {
        public IEnumerable<string> DynamicHeaders { get; set; }
        public IEnumerable<CheckinViewItem> Items { get; set; }
    }
}