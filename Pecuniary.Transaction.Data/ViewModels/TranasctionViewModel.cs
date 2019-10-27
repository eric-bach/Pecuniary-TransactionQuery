using System;
using Newtonsoft.Json;

namespace Pecuniary.Transaction.Data.ViewModels
{
    /// <summary>
    /// This has to match AccountViewModel in Pecuniary.ViewModels
    /// </summary>
    public class TransactionViewModel
    {
        public Guid AccountId { get; set; }
        public SecurityViewModel Security { get; set; }
    }

    public class SecurityViewModel
    {
        public Guid SecurityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExchangeTypeCode { get; set; }
    }
}

