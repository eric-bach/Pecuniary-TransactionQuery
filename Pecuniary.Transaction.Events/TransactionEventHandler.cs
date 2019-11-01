using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using EricBach.CQRS.QueryRepository;
using EricBach.CQRS.QueryRepository.Response;
using EricBach.LambdaLogger;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pecuniary.Transaction.Data.ViewModels;
using Pecuniary.Transaction.Events.ViewModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.Transaction.Events
{
    public class Function
    {
        private readonly TransactionQueryService _transactionQueryService;

        public Function()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _transactionQueryService = serviceProvider.GetService<TransactionQueryService>();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IReadRepository<TransactionViewModel>>(r =>
                new ElasticSearchRepository<TransactionViewModel>(Environment.GetEnvironmentVariable("ElasticSearchDomain")));
            serviceCollection.AddScoped<TransactionQueryService>();
        }

        public async Task TransactionEventHandlerAsync(SNSEvent message, ILambdaContext context)
        {
            Logger.Log($"Received {message.Records.Count} records");

            foreach (var record in message.Records)
            {
                Logger.Log($"Received message {record.Sns.Message}");
                
                Logger.Log("Adding Transaction document");
                var transactionResponse = await _transactionQueryService.AddTransactionModel(record.Sns.Message);
                Logger.Log("Update Account with Transaction Document");
                var accountTransactionResponse = await _transactionQueryService.UpdateAccountTransactionModel(record.Sns.Message);

                Logger.Log($"Add Transaction Result: {JsonConvert.SerializeObject(transactionResponse)}");
                Logger.Log($"Update Account Result: {JsonConvert.SerializeObject(accountTransactionResponse)}");
            }

            Logger.Log($"Completed processing {message.Records.Count} records");
        }

        public class TransactionQueryService
        {
            private readonly IReadRepository<TransactionViewModel> _repository;

            public TransactionQueryService(IReadRepository<TransactionViewModel> repository)
            {
                _repository = repository;
            }

            public async Task<ElasticSearchResponse> UpdateAccountTransactionModel(string message)
            {
                const string index = "account";

                Logger.Log("Updating document to ElasticSearch");

                // Get the event name from the message
                var eventName = Regex.Matches(message, @"EventName"":[\s]*""([a-zA-Z)]+)").First().Groups.Last().Value;
                Logger.Log($"Event Name: {eventName}");

                // Dynamically convert deserialized event to event type
                dynamic request = JsonConvert.DeserializeObject(message, Type.GetType(eventName));
                var accountId = request.Transaction.AccountId.ToString();
                Logger.Log($"Event Id: {accountId}");
                Logger.Log($"Event Index: {index}");

                var data = new AccountTransactionViewModel
                {
                    Doc = new AccountTransactionDocument
                    {
                        Transactions = new List<AccountTransaction>
                        {
                            new AccountTransaction
                            {
                                Transaction = request.Transaction.ToString()
                            }
                        }
                    }
                };
                Logger.Log($"Partial Document: {JsonConvert.SerializeObject(data)}");

                Logger.Log($"Sending document {accountId} to ElasticSearch");
                return await _repository.UpdateAsync(index, accountId, JsonConvert.SerializeObject(data));
            }

            public async Task<ElasticSearchResponse> AddTransactionModel(string message)
            {
                Logger.Log("Adding document to ElasticSearch");

                // Get the event name from the message
                var eventName = Regex.Matches(message, @"EventName"":[\s]*""([a-zA-Z)]+)").First().Groups.Last().Value;
                Logger.Log($"Event Name: {eventName}");

                // Dynamically convert deserialized event to event type
                dynamic request = JsonConvert.DeserializeObject(message, Type.GetType(eventName));
                Logger.Log($"Event Id: {request.Id}");

                // Use the domain model name (by parsing the first word in the event name) as the index
                var index = Regex.Matches(eventName, @"([A-Z][a-z]+)").Select(m => m.Value).First().ToLower();
                Logger.Log($"Event Index: {index}");

                Logger.Log($"Sending document {request.Id} to ElasticSearch");
                return await _repository.AddAsync(index, request.Id.ToString(), message);
            }
        }
    }
}
