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
dotnet test  # バックエンドテスト実行
```

### フロントエンド（Nuxt.js）
```bash
cd NoOpinionButtonWeb
npm install
npm run dev
npm test     # フロントエンドテスト実行（Vitest）
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
  - `Entities/` - Meeting, Participantエンティティ
  - `Services/` - SignInService（認証ロジック）
  - `Repositories/` - リポジトリインターフェース
- `src/Api/Infrastructure/` - リポジトリ実装
  - `Repositories/` - DynamoDBリポジトリ
- `src/Api/LambdaHandlers/` - Lambda関数
  - `SignInFunction.cs` - サインインAPI
- `src/ApiInfra/` - CDKインフラ定義
- `tests/` - 単体テスト（20+テストケース）

### フロントエンド（NoOpinionButtonWeb/）
- `pages/` - ページコンポーネント
  - `signin.vue` - サインインページ
- `composables/` - コンポーザブル関数
  - `signIn/useSignIn.ts` - 状態管理
  - `signIn/useSignInApi.ts` - API通信
- `types/` - TypeScript型定義
  - `error.ts` - エラー型定義
- `tests/` - テストスイート（25テストケース）
  - `composables/` - Composables単体テスト
  - `pages/` - コンポーネントテスト
  - `integration/` - 統合テスト
- `vitest.config.ts` - テスト設定

## 現在の実装状況

### 完全実装済み
- **サインイン機能**: フルスタック実装完了
  - バックエンドAPI（.NET Core + AWS Lambda）
  - フロントエンドUI（Nuxt.js + Vue.js）
  - 認証ロジック・エラーハンドリング
  - 司会者/参加者自動判定・ページ遷移

### テスト実装済み
- **バックエンドテスト**: 20+テストケース
  - Core層（エンティティ・サービス）単体テスト
  - Infrastructure層（リポジトリ）単体テスト
  - LambdaHandlers層（API）単体テスト
- **フロントエンドテスト**: 25テストケース（Vitest）
  - Composables単体テスト（10件）
  - コンポーネントテスト（9件）
  - 統合テスト（6件）

### 未実装
- facilitator.vue（司会者画面）
- participant.vue（参加者画面）
- 「意見なし」ボタン機能
- リアルタイム状態同期

## 開発時の注意点
- ClaudeCodeは、思考は英語で、会話は日本語で行う
- ディレクトリ構造等、プロジェクトに変更があった場合、CLAUDE.md、README.md、docs内のドキュメントも更新する
- クリーンアーキテクチャに従って実装
- DynamoDBのパーティションキー設計に注意
- テスト実行: `npm test`（フロント）、`dotnet test`（バック）