using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace ApiInfra
{
    public class ApiInfraStack : Stack
    {
        internal ApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // DynamoDBのテーブル定義
            var administratorTable = new Table(this, "Administrator", new TableProps {
                TableName = "Administrator",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var meetingTable = new Table(this, "Meeting", new TableProps {
                TableName = "Meeting",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var participantTable = new Table(this, "Participant", new TableProps {
                TableName = "Participant",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var messageTable = new Table(this, "Message", new TableProps {
                TableName = "Message",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var buttonActivityTable = new Table(this, "ButtonActivity", new TableProps {
                TableName = "ButtonActivity",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var webSocketConnectionTable = new Table(this, "WebSocketConnection", new TableProps {
                TableName = "WebSocketConnection",
                // 主キーを設定
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Lambda関数の定義
            var signInLambda = new Function(this, "SignInFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-SignInFunction",
                Runtime = Runtime.DOTNET_8,  // .NET 8 を使用（Lambda関数をC#で記述）
                // cdk.jsonから見たパス
                Code = Code.FromAsset("src/Api/LambdaHandlers/SignInFunction/bin/Release/net8.0"), // C#コードがあるディレクトリを指定
                Handler = "SignInFunction::SignInFunction.Function::FunctionHandler",  // C#のエントリーポイント
                Environment = new Dictionary<string, string>
                {
                    // Lambda関数内で参照する環境変数
                    { "ADMINISTRATOR_TABLE_NAME", "Administrator" },
                    { "MEETING_TABLE_NAME", "Meeting" },
                    { "PARTICIPANT_TABLE_NAME", "Participant" }
                }
            });

            var postMessageLambda = new Function(this, "PostMessageFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-PostMessageFunction",
                Runtime = Runtime.DOTNET_8,  // .NET 8 を使用（Lambda関数をC#で記述）
                // cdk.jsonから見たパス
                Code = Code.FromAsset("src/Api/LambdaHandlers/PostMessageFunction/bin/Release/net8.0"), // C#コードがあるディレクトリを指定
                Handler = "PostMessageFunction::PostMessageFunction.Function::FunctionHandler",  // C#のエントリーポイント
                Environment = new Dictionary<string, string>
                {
                    // Lambda関数内で参照する環境変数
                    { "ADMINISTRATOR_TABLE_NAME", "Administrator" },
                    { "MEETING_TABLE_NAME", "Meeting" },
                    { "PARTICIPANT_TABLE_NAME", "Participant" },
                    { "MESSAGE_TABLE_NAME", "Message" },
                    { "BUTTONACTIVITY_TABLE_NAME", "ButtonActivity" },
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection"}
                }
            });

            var webSocketConnectLambda = new Function(this, "WebSocketConnectFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-WebSocketConnectFunction",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("src/Api/LambdaHandlers/WebSocketConnectFunction/bin/Release/net8.0"),
                Handler = "WebSocketConnectFunction::WebSocketConnectFunction.Function::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection"}
                }
            });

            var webSocketDisconnectLambda = new Function(this, "WebSocketDisconnectFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-WebSocketDisconnectFunction",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("src/Api/LambdaHandlers/WebSocketDisconnectFunction/bin/Release/net8.0"),
                Handler = "WebSocketDisconnectFunction::WebSocketDisconnectFunction.Function::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection"}
                }
            });

            // Lambda関数にDynamoDBアクセス権を付与（データの取得・挿入）
            administratorTable.GrantReadWriteData(signInLambda);
            meetingTable.GrantReadWriteData(signInLambda);
            participantTable.GrantReadWriteData(signInLambda);

            // PostMessageFunction にもDynamoDBアクセス権を付与
            administratorTable.GrantReadWriteData(postMessageLambda);
            meetingTable.GrantReadWriteData(postMessageLambda);
            participantTable.GrantReadWriteData(postMessageLambda);
            messageTable.GrantReadWriteData(postMessageLambda);
            buttonActivityTable.GrantReadWriteData(postMessageLambda);
            webSocketConnectionTable.GrantReadWriteData(postMessageLambda);

            // WebSocketConnect/DisconnectFunction にDynamoDBアクセス権を付与
            webSocketConnectionTable.GrantReadWriteData(webSocketConnectLambda);
            webSocketConnectionTable.GrantReadWriteData(webSocketDisconnectLambda);

            // RestApi（REST API v1）を作成
            var api = new RestApi(this, "NoOpinionButtonApi", new RestApiProps
            {
                RestApiName = "NoOpinionButton API",
                Description = "This Service is backend for NoOpinionButton",
                DeployOptions = new StageOptions
                {
                    StageName = "prod"
                }
            });

            // signin エンドポイント
            var signinResource = api.Root.AddResource("signin");
            signinResource.AddMethod("POST", new LambdaIntegration(signInLambda));
            signinResource.AddMethod("OPTIONS", new LambdaIntegration(signInLambda)); // CORS対応

            // messages エンドポイント
            var messagesResource = api.Root.AddResource("message");
            messagesResource.AddMethod("POST", new LambdaIntegration(postMessageLambda));
            messagesResource.AddMethod("OPTIONS", new LambdaIntegration(postMessageLambda)); // CORS対応

            // WebSocket接続管理エンドポイント
            var wsConnectResource = api.Root.AddResource("ws-connect");
            wsConnectResource.AddMethod("POST", new LambdaIntegration(webSocketConnectLambda));
            
            var wsDisconnectResource = api.Root.AddResource("ws-disconnect");
            wsDisconnectResource.AddMethod("POST", new LambdaIntegration(webSocketDisconnectLambda));
        }
    }
}
