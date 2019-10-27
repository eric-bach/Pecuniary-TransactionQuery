using System;
using MediatR;
using Pecuniary.Transaction.Data.Models;

namespace Pecuniary.Transaction.Query.Queries
{
    public class GetTransactionQuery : IRequest<TransactionReadModel>
    {
        public Guid Id { get; set; }
    }
}
