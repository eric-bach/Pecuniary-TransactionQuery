using System;
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
using static Pecuniary.Transaction.Events.Function;

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
            serviceCollection.AddScoped<IReadRepository<TransactionViewModel>, ElasticSearchRepository<TransactionViewModel>>();
            serviceCollection.AddScoped<TransactionQueryService>();
        }

        public async Task TransactionEventHandlerAsync(SNSEvent message, ILambdaContext context)
        {
            Logger.Log($"Received {message.Records.Count} records");

            var elasticSearchDomain = Environment.GetEnvironmentVariable("ElasticSearchDomain");
            Logger.Log("Found ElasticSearchDomain");

            foreach (var record in message.Records)
            {
                Logger.Log($"Received message {record.Sns.Message}");

                // Denormalize event based on EventType
                //dynamic obj = DeserializeMessage(record.Sns.Message);

                var response = await _transactionQueryService.AddOrUpdateAsync(record.Sns.Message);

                Logger.Log($"Result: {response.Result}");
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

            public async Task<ElasticSearchResponse> AddOrUpdateAsync(string message)
            {
                // Get the event name from the message
                var eventName = Regex.Matches(message, @"EventName"":[\s]*""([a-zA-Z)]+)").First().Groups.Last().Value;
                Logger.Log($"Event Name: {eventName}");

                // Dynamically convert deserialized event to event type
                dynamic request = JsonConvert.DeserializeObject(message, Type.GetType(eventName));
                Logger.Log($"Event Id: {request.Id}");

                // Get the domain model name by parsing the first word in the event name
                var model = Regex.Matches(eventName, @"([A-Z][a-z]+)").Select(m => m.Value).First().ToLower();
                Logger.Log($"Event Model: {model}");

                Logger.Log($"Sending event to ElasticSearch");

                return await _repository.AddOrUpdateAsync(message, model, request.Id.ToString());
            }
        }
    }
}
