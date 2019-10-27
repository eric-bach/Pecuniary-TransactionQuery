using System;

namespace Pecuniary.Transaction.Data.ViewModels
{
    // TODO Move this to nuget
    public class ViewModel
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string EventName { get; set; }
        public string EventVersion { get; set; }
        public DateTime Timestamp { get; set; }
    }
}