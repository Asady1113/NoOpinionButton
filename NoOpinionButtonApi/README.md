# 意見ありませんボタン API

「意見ありませんボタン」システムのバックエンドAPIです。.NET Core + AWS Lambda + DynamoDBで構築されたサーバーレスアプリケーションです。

## 🚀 クイックスタート

### 開発環境セットアップ
```bash
# 依存関係のビルド
dotnet build src

# テスト実行
dotnet test

# ローカル実行
sam build
sam local start-api
```

### デプロイ手順
```bash
cd src
dotnet build
cdk bootstrap  # 初回のみ
cdk diff       # 変更内容確認
cdk deploy     # デプロイ実行
```

## 📁 プロジェクト構造

```
NoOpinionButtonApi/
├── src/
│   ├── Api/
│   │   ├── Core/           # ドメインロジック・サービス
│   │   ├── Infrastructure/ # リポジトリ実装
│   │   └── LambdaHandlers/ # AWS Lambda関数
│   └── ApiInfra/          # AWS CDKインフラ定義
├── tests/                 # 単体テスト（20+テストケース）
└── docs/                  # 詳細ドキュメント
```

## 🔧 技術スタック

- **.NET Core 8**: サーバーレスアプリケーション
- **AWS Lambda**: API実行環境
- **DynamoDB**: NoSQLデータベース
- **API Gateway**: RESTful API
- **AWS CDK**: Infrastructure as Code

## 📋 実装状況

- ✅ **サインイン機能**: 完全実装・テスト完了
- ✅ **認証ロジック**: 司会者/参加者判定
- ✅ **単体テスト**: Core/Infrastructure/LambdaHandlers層
- ⏳ **意見表明機能**: 未実装
- ⏳ **リアルタイム同期**: 未実装

## 🧪 テスト実行

```bash
# 全テスト実行
dotnet test

# 特定のプロジェクトのテスト
dotnet test tests/CoreTests
dotnet test tests/InfrastructureTests
dotnet test tests/LambdaHandlersTests
```

## 📚 詳細ドキュメント

- **[アーキテクチャ設計](./docs/architecture.md)**: システム設計・クリーンアーキテクチャ
- **[API仕様](./docs/api-specification.md)**: エンドポイント・リクエスト/レスポンス
- **[データベース設計](./docs/database.md)**: DynamoDB設計・テーブル構造