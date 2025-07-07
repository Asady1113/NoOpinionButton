# 「意見ありませんボタン」システムのAPI
### DB定義所
Database.md

### ローカル実行
Docker起動
* sam build
* sam local start-api

### デプロイ実行
* cd /src
* dotnet build
* cdk bootstrap
* cdk diff
* cdk synth
* cdk deploy

# Welcome to your CDK C# project!

This is a blank project for CDK development with C#.

The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Useful commands

* `dotnet build src` compile this app
* `cdk deploy`       deploy this stack to your default AWS account/region
* `cdk diff`         compare deployed stack with current state
* `cdk synth`        emits the synthesized CloudFormation template