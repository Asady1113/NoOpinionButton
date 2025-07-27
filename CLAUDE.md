# 「意見ありませんボタン」システム

## プロジェクト概要
会議で「意見がない」参加者を可視化するシステム。参加者は「意見なし」を表明でき、司会者は参加者の状況を確認できる。

## 技術スタック
- **フロントエンド:** Nuxt.js（Vue.js）+ TypeScript + Tailwind CSS
- **バックエンド:** .NET Core + AWS Lambda + API Gateway
- **データベース:** DynamoDB
- **インフラ:** AWS CDK

## アーキテクチャ
- クリーンアーキテクチャ（Core/Infrastructure/Api層）
- サーバーレス構成
- 会議・参加者エンティティでデータ管理

## 開発環境セットアップ

### API（.NET）
```bash
cd NoOpinionButtonApi
dotnet build src
```

### フロントエンド（Nuxt.js）
```bash
cd NoOpinionButtonWeb
npm install
npm run dev
```

## デプロイ

### API
```bash
cd NoOpinionButtonApi/src
dotnet build
cdk bootstrap
cdk diff
cdk synth
cdk deploy
```

### ローカル実行（API）
```bash
cd NoOpinionButtonApi
sam build
sam local start-api
```

## プロジェクト構造

### API（NoOpinionButtonApi/）
- `src/Api/Core/` - ドメインロジック、サービス
- `src/Api/Infrastructure/` - リポジトリ、エンティティ
- `src/Api/LambdaHandlers/` - Lambda関数
- `src/ApiInfra/` - CDKインフラ定義

### フロントエンド（NoOpinionButtonWeb/）
- `pages/` - ページコンポーネント
- `composables/` - コンポーザブル関数
- `types/` - TypeScript型定義

## 現在の実装状況
- サインイン機能実装済み
- 会議・参加者エンティティ定義済み
- 基本的なUI画面（signin.vue, participant.vue, facilitator.vue）作成済み

## 開発時の注意点
- ClaudeCodeは、思考は英語で、会話は日本語で行う
- クリーンアーキテクチャに従って実装
- DynamoDBのパーティションキー設計に注意
- CORS設定が必要（Tips/CORS.md参照）
- 環境変数の設定（Tips/env.md参照）