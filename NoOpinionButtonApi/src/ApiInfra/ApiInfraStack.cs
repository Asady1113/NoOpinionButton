using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
// RestAPIを作成するための名前空間
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.IAM;
// WebSocketを操作するための名前空間
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AwsApigatewayv2Integrations;
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
                // DynamoDB Streamsを有効化
                Stream = StreamViewType.NEW_AND_OLD_IMAGES,
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
                PartitionKey = new Attribute { Name = "ConnectionId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Meetingで検索をかけるので、インデックスを作成する（DynamoDBでは 主キー or インデックス以外では直接検索できない）
            webSocketConnectionTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps {
                IndexName = "MeetingId-Index",
                PartitionKey = new Attribute { Name = "MeetingId", Type = AttributeType.STRING }
            });

            // Lambda関数の定義
            var signInLambda = new Function(this, "SignInFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-SignInFunction",
                Runtime = Runtime.DOTNET_8,  // .NET 8 を使用（Lambda関数をC#で記述）
                // cdk.jsonから見たパス
                Code = Code.FromAsset("src/Api/LambdaHandlers/SignInFunction/bin/Release/net8.0/publish"), // C#コードがあるディレクトリを指定
                Handler = "SignInFunction::SignInFunction.Function::FunctionHandler",  // C#のエントリーポイント
                Timeout = Duration.Seconds(30), // タイムアウト設定を追加
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
                Code = Code.FromAsset("src/Api/LambdaHandlers/PostMessageFunction/bin/Release/net8.0/publish"), // C#コードがあるディレクトリを指定
                Handler = "PostMessageFunction::PostMessageFunction.Function::FunctionHandler",  // C#のエントリーポイント
                Timeout = Duration.Seconds(30), // タイムアウト設定を追加
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
                Code = Code.FromAsset("src/Api/LambdaHandlers/WebSocketConnectFunction/bin/Release/net8.0/publish"),
                Handler = "WebSocketConnectFunction::WebSocketConnectFunction.Function::FunctionHandler",
                Timeout = Duration.Seconds(30), // タイムアウト設定を追加
                Environment = new Dictionary<string, string>
                {
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection"}
                }
            });

            var webSocketDisconnectLambda = new Function(this, "WebSocketDisconnectFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-WebSocketDisconnectFunction",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("src/Api/LambdaHandlers/WebSocketDisconnectFunction/bin/Release/net8.0/publish"),
                Handler = "WebSocketDisconnectFunction::WebSocketDisconnectFunction.Function::FunctionHandler",
                Timeout = Duration.Seconds(30), // タイムアウト設定を追加
                Environment = new Dictionary<string, string>
                {
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection"}
                }
            });

            var messageBroadcastLambda = new Function(this, "MessageBroadcastFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-MessageBroadcastFunction",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("src/Api/LambdaHandlers/MessageBroadcastFunction/bin/Release/net8.0/publish"),
                Handler = "MessageBroadcastFunction::MessageBroadcastFunction.Function::FunctionHandler",
                Timeout = Duration.Seconds(30), // タイムアウト設定を追加
                Environment = new Dictionary<string, string>
                {
                    { "WEBSOCKETCONNECTION_TABLE_NAME", "WebSocketConnection" },
                    { "WEBSOCKET_API_ENDPOINT", "" } // WebSocket APIエンドポイント（後で設定）
                }
            });

            var updateParticipantNameLambda = new Function(this, "UpdateParticipantNameFunction", new FunctionProps
            {
                FunctionName = "NoOpinionButton-UpdateParticipantNameFunction",
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("src/Api/LambdaHandlers/UpdateParticipantNameFunction/bin/Release/net8.0/publish"),
                Handler = "UpdateParticipantNameFunction::UpdateParticipantNameFunction.Function::FunctionHandler",
                Timeout = Duration.Seconds(30),
                Environment = new Dictionary<string, string>
                {
                    { "PARTICIPANT_TABLE_NAME", "Participant" }
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

            // MessageBroadcastFunction にDynamoDBアクセス権とDynamoDB Streamsトリガーを設定
            webSocketConnectionTable.GrantReadData(messageBroadcastLambda);
            // .AddEventSource(...) で「このLambdaの起動条件（イベントソース）を追加」
            // DynamoDB Streams をイベントソース
            messageBroadcastLambda.AddEventSource(new DynamoEventSource(messageTable, new DynamoEventSourceProps
            {
                // Lambdaがイベントを受け取るときの開始位置。
                StartingPosition = StartingPosition.LATEST,
                // 最大で10件のイベントをまとめてLambdaに渡します。
                BatchSize = 10,
                // イベントをまとめるために最大5秒まで待つ。
                MaxBatchingWindow = Duration.Seconds(5)
            }));

            // UpdateParticipantNameFunction にDynamoDBアクセス権を付与
            participantTable.GrantReadWriteData(updateParticipantNameLambda);

            // RestApi（REST API v1）を作成
            var api = new RestApi(this, "NoOpinionButtonApi", new RestApiProps
            {
                RestApiName = "NoOpinionButton API",
                Description = "This Service is backend for NoOpinionButton",
                DeployOptions = new Amazon.CDK.AWS.APIGateway.StageOptions
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

            // participants エンドポイント: PUT /participants/{participantId}/name
            var participantsResource = api.Root.AddResource("participants");
            var participantResource = participantsResource.AddResource("{participantId}");
            var participantNameResource = participantResource.AddResource("name");
            participantNameResource.AddMethod("PUT", new LambdaIntegration(updateParticipantNameLambda));
            participantNameResource.AddMethod("OPTIONS", new LambdaIntegration(updateParticipantNameLambda)); // CORS対応

            // WebSocket API Gateway V2 を作成
            var webSocketApi = new WebSocketApi(this, "NoOpinionButtonWebSocketApi", new WebSocketApiProps
            {
                ApiName = "NoOpinionButton WebSocket API",
                Description = "WebSocket API for real-time messaging in NoOpinionButton",
                RouteSelectionExpression = "$request.body.action"
            });

            // WebSocket接続・切断ルート
            // $connect → クライアントが接続した時に呼ばれるLambda（webSocketConnectLambda）
            webSocketApi.AddRoute("$connect", new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("ConnectIntegration", webSocketConnectLambda)
            });
            // $disconnect → 切断時に呼ばれるLambda（webSocketDisconnectLambda）
            webSocketApi.AddRoute("$disconnect", new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("DisconnectIntegration", webSocketDisconnectLambda)
            });

            // WebSocket APIのデプロイ設定
            // 作ったWebSocket APIをインターネットで使える形にするための最後の工程
            // エンドポイントをコード内で渡す必要があるので、コードで宣言
            var webSocketStage = new WebSocketStage(this, "NoOpinionButtonWebSocketStage", new WebSocketStageProps
            {
                WebSocketApi = webSocketApi,
                StageName = "prod",
                AutoDeploy = true
            });

            // WebSocket API Management のエンドポイントURLを環境変数として設定
            // Lambdaから他のクライアントへ逆方向にメッセージを送るためのエンドポイントURL。
            var webSocketManagementEndpoint = $"https://{webSocketApi.ApiId}.execute-api.{this.Region}.amazonaws.com/{webSocketStage.StageName}";

            // MessageBroadcastFunction の環境変数を更新
            // messageBroadcastLambda がWebSocket経由で全員にメッセージを送れるように、エンドポイントを環境変数に入れる。
            messageBroadcastLambda.AddEnvironment("WEBSOCKET_API_ENDPOINT", webSocketManagementEndpoint);

            // MessageBroadcastFunction に WebSocket API Management の権限を付与
            messageBroadcastLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                //  Lambdaが「接続中のクライアントにメッセージ送信」や「接続を切断」できる権限。
                Actions = new[] { "execute-api:ManageConnections" },
                // webSocketApiのステージ（prod）に限定
                Resources = new[] { $"arn:aws:execute-api:{this.Region}:{this.Account}:{webSocketApi.ApiId}/prod/POST/@connections/*" }
            }));
        }
    }
}
