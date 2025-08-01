using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Core.Application;
using Core.Application.Services;
using Core.Domain.Ports;
using Infrastructure.Repository;
using Infrastructure.Clients;
using Infrastructure.Interfaces;

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

        // Application Services
        services.AddTransient<ISignInService, SignInService>();
        services.AddTransient<ChatService>();

        // Domain Ports -> Infrastructure implementations
        services.AddTransient<IParticipantRepository, ParticipantRepository>();
        services.AddTransient<IMeetingRepository, MeetingRepository>();
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<IMessageNotificationClient, MessageNotificationClient>();

        // Infrastructure internal interfaces
        services.AddTransient<IWebSocketConnectionRepository, WebSocketConnectionRepository>();

        return services.BuildServiceProvider();
    }
}