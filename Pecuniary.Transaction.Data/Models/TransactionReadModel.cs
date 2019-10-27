using Newtonsoft.Json;
using Pecuniary.Transaction.Data.ViewModels;

namespace Pecuniary.Transaction.Data.Models
{
    public class TransactionReadModel : BaseReadModel
    {
        [JsonProperty("_source")]
        public TransactionSource Source { get; set; }
    }

    public class TransactionSource : ViewModel
    {
        public TransactionViewModel Transaction { get; set; }
    }
}