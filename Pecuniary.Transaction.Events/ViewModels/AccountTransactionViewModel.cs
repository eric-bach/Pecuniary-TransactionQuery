using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pecuniary.Transaction.Events.ViewModels
{
    public class AccountTransactionViewModel
    {
        [JsonProperty("doc")]
        public AccountTransactionDocument Doc { get; set; }
    }

    public class AccountTransactionDocument
    {
        public List<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
    }

    public class AccountTransaction
    {
        public string Transaction { get; set; }
    }
}
