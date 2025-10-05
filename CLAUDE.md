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
  - `Application/Ports/` - サービスインターフェース
  - `Application/Services/` - SignInService, MessageService, ConnectionService, BroadcastService, ParticipantUpdateService
  - `Application/DTOs/` - リクエスト/レスポンスDTO
  - `Domain/Entities/` - Meeting, Participant, Message, Connectionエンティティ
  - `Domain/Ports/` - リポジトリインターフェース
- `src/Api/Infrastructure/` - リポジトリ実装
  - `Repositories/` - DynamoDBリポジトリ, BroadcastRepository
- `src/Api/LambdaHandlers/` - Lambda関数
  - `SignInFunction/` - サインインAPI
  - `PostMessageFunction/` - メッセージ送信API
  - `UpdateParticipantNameFunction/` - 参加者名更新API
  - `WebSocketConnectFunction/` - WebSocket接続管理
  - `WebSocketDisconnectFunction/` - WebSocket切断管理
  - `MessageBroadcastFunction/` - メッセージリアルタイム配信
- `src/Api/DependencyInjection/` - DI設定
- `src/Api/Common/` - ユーティリティクラス
- `src/ApiInfra/` - CDKインフラ定義
- `tests/` - 単体テスト（20+テストケース）

### フロントエンド（NoOpinionButtonWeb/）
- `pages/` - ページコンポーネント
  - `signin.vue` - サインインページ
  - `facilitator.vue` - 司会者ページ（基本実装）
  - `participant.vue` - 参加者ページ（ほぼ完成）
- `components/` - UIコンポーネント（Atomic Design）
  - `atoms/` - 基本UI要素（Button, Input, ErrorMessage等）
  - `molecules/` - 複合コンポーネント（ModalHeader, MessageItem, MessageSendingForm等）
  - `organisms/` - 大型コンポーネント（ParticipantNameModal, MessageList等）
- `composables/` - コンポーザブル関数
  - `signIn/` - サインイン関連（useSignIn.ts, useSignInApi.ts）
  - `participantName/` - 参加者名関連（useParticipantNameApi.ts）
  - `message/` - メッセージ関連（useMessageSending.ts, useMessageReception.ts, useMessageSendingApi.ts）
  - `webSocket/` - WebSocket関連（useWebSocketConnection.ts）
- `types/` - TypeScript型定義
  - `error.ts` - エラー型定義
- `tests/` - テストスイート（25テストケース）
  - `composables/` - Composables単体テスト（各機能別）
  - `components/` - コンポーネントテスト（atoms/molecules/organisms別）
  - `pages/` - ページコンポーネントテスト
  - `integration/` - 統合テスト
- `vitest.config.ts` - テスト設定

## 現在の実装状況

### 完全実装済み（フルスタック）
- **サインイン機能**: フルスタック実装完了
  - バックエンドAPI（.NET Core + AWS Lambda）
  - フロントエンドUI（Nuxt.js + Vue.js）
  - 認証ロジック・エラーハンドリング
  - 司会者/参加者自動判定・ページ遷移

- **参加者名更新機能**: フルスタック実装完了
  - バックエンドAPI（UpdateParticipantNameFunction）
  - フロントエンドUI（ParticipantNameModal + composables）
  - 参加者名登録・更新機能

- **メッセージ機能**: フルスタック実装完了
  - バックエンドAPI（PostMessageFunction + MessageBroadcastFunction）
  - フロントエンドUI（MessageSendingForm + MessageList + composables）
  - リアルタイムメッセージ送受信・WebSocket接続管理

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
- **司会者画面の詳細UI**: 参加者一覧・状況表示機能
- **「意見なし」ボタン機能**: エンティティ・API・UI全て未実装

## 開発時の注意点
- ClaudeCodeは、思考は英語で、会話は日本語で行う
- ディレクトリ構造等、プロジェクトに変更があった場合、CLAUDE.md、README.md、docs内のドキュメントも更新する
- アーキテクチャを更新した場合は、architecture.mdを更新する
- DBに変更がある場合は、database.mdを更新する
- クリーンアーキテクチャに従って実装
- XMLドキュメントはインターフェース側に記述し、実装側には< /inheritdoc>を記述する
- テスト実行: `npm test`（フロント）、`dotnet test`（バック）