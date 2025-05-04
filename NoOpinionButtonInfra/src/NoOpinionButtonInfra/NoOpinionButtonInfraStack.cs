using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace NoOpinionButtonInfra
{
    public class NoOpinionButtonInfraStack : Stack
    {
        internal NoOpinionButtonInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // DynamoDBのテーブル定義
            var administratorTable = new Table(this, "Administrator", new TableProps {
                // 主キーを設定
                PartitionKey = new Attribute { Name = "adminId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var meetingTable = new Table(this, "Meeting", new TableProps {
                // 主キーを設定
                PartitionKey = new Attribute { Name = "meetingId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var participantTable = new Table(this, "Participant", new TableProps {
                // 主キーを設定
                PartitionKey = new Attribute { Name = "participantId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var messageTable = new Table(this, "Message", new TableProps {
                // 主キーを設定
                PartitionKey = new Attribute { Name = "messageId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var buttonActivityTable = new Table(this, "ButtonActivity", new TableProps {
                // 主キーを設定
                PartitionKey = new Attribute { Name = "activityId", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST, // 従量課金
                // TODO: 本番環境では、データを消したくないので RETAINにする
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Lambda関数の定義
            var noOpinionButtonLambda = new Function(this, "NoOpinionButtonFunction", new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,  // .NET 8 を使用（Lambda関数をC#で記述）
                Code = Code.FromAsset("src/LambdaFunctions/NoOpinionButtonFunction/src/NoOpinionButtonFunction/bin/Release/net8.0"), // C#コードがあるディレクトリを指定
                Handler = "NoOpinionButtonFunction::NoOpinionButtonFunction.Function::FunctionHandler",  // C#のエントリーポイント
                Environment = new Dictionary<string, string>
                {
                    // Lambda関数内で参照する環境変数
                    { "ADMINISTRATOR_TABLE_NAME", "Administrator" },
                    { "MEETING_TABLE_NAME", "Meeting" },
                    { "PARTICIPANT_TABLE_NAME", "Participant" },
                    { "MESSAGE_TABLE_NAME", "Message" },
                    { "BUTTONACTIVITY_TABLE_NAME", "ButtonActivity" }
                }
            });

            // Lambda関数にDynamoDBアクセス権を付与（データの取得・挿入）
            administratorTable.GrantReadWriteData(noOpinionButtonLambda);
            meetingTable.GrantReadWriteData(noOpinionButtonLambda);
            participantTable.GrantReadWriteData(noOpinionButtonLambda);
            messageTable.GrantReadWriteData(noOpinionButtonLambda);
            buttonActivityTable.GrantReadWriteData(noOpinionButtonLambda);

            // APIGatewayとの統合
            var api = new RestApi(this, "NoOpinionButtonApi", new RestApiProps
            {
                RestApiName = "NoOpinionButton API",
                Description = "This Service is backend for NoOpnionButton"
            });

            // Topページ用のリソースを作成
            var topResource = api.Root.AddResource("top");  // "/top"というリソース（エンドポイント）を作成
            topResource.AddMethod("GET", new LambdaIntegration(noOpinionButtonLambda));  // Lambdaと紐付ける
            // TODO; どんどんエンドポイントを追加していく
        }
    }
}
