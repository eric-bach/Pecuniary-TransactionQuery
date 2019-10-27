using System;
using System.Threading;
using System.Threading.Tasks;
using EricBach.CQRS.QueryRepository;
using EricBach.LambdaLogger;
using MediatR;
using Newtonsoft.Json;
using Pecuniary.Transaction.Data.Models;
using Pecuniary.Transaction.Query.Queries;

namespace Pecuniary.Transaction.Query.QueryHandlers
{
    public class TransactionQueryHandlers : IRequestHandler<GetTransactionQuery, TransactionReadModel>
    {
        private readonly IReadRepository<TransactionReadModel> _repository;

        public TransactionQueryHandlers(IReadRepository<TransactionReadModel> repository)
        {
            _repository = repository ?? throw new InvalidOperationException("Repository is not initialized.");
        }

        public async Task<TransactionReadModel> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
        {
            Logger.Log($"{nameof(GetTransactionQuery)} handler invoked");

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var response = await _repository.GetByIdAsync(query.Id);

            // Deserialize back to read model
            return JsonConvert.DeserializeObject<TransactionReadModel>(response);
        }
    }
}
