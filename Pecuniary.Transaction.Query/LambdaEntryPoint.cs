using Microsoft.AspNetCore.Hosting;

namespace Pecuniary.Transaction.Query
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// Ama.Rewards.Loyalty.WebApi.Transaction::Ama.Rewards.Loyalty.WebApi.Transaction.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                //.ConfigureServices(services =>
                //{
                //    services.AddScoped<ISecretsRepository, SecretsRepository>();
                //})
                .UseStartup<Startup>();
        }
    }
}
