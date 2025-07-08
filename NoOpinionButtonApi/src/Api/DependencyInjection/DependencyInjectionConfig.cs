using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Core.Application;
using Core.Domain.Ports;
using Infrastructure.Repository;

namespace DependencyInjection;

public static class DependencyInjectionConfig
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // DynamoDB Client
        services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.APNortheast1 // 東京リージョンなど
            };
            return new AmazonDynamoDBClient(config);
        });

        // DynamoDB Context
        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

        services.AddTransient<ISignInService, SignInService>();
        services.AddTransient<IParticipantRepository, ParticipantRepository>();
        services.AddTransient<IMeetingRepository, MeetingRepository>();

        return services.BuildServiceProvider();
    }
}