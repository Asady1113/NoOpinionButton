# アーキテクチャ設計

## 概要

「意見ありませんボタン」APIは、クリーンアーキテクチャの原則に従って設計されたサーバーレスアプリケーションです。

## アーキテクチャ構成

```
┌─────────────────────────────────────────────────┐
│      API Gateway (REST + WebSocket)            │
├─────────────────────────────────────────────────┤
│                AWS Lambda                       │
│  ┌─────────────────────────────────────────────┐│
│  │           LambdaHandlers 層              ││
│  │  ┌─────────────────────────────────────────┐││
│  │  │              Core 層                ││││
│  │  │  ┌───────────────┬─────────────────┐│││
│  │  │  │   Domain      │   Application   ││││
│  │  │  │   Entities    │   Services      ││││
│  │  │  │   Ports       │   DTOs, Ports   ││││
│  │  │  └───────────────┴─────────────────┘│││
│  │  └─────────────────────────────────────────┘││
│  │  ┌─────────────────────────────────────────┐││
│  │  │           Infrastructure 層         ││││
│  │  │        Repositories, Entities       ││││
│  │  └─────────────────────────────────────────┘││
│  └─────────────────────────────────────────────┘│
└─────────────────────────────────────────────────┘
          │                    │
          ▼                    ▼
   ┌─────────────┐    ┌─────────────────┐
   │  DynamoDB   │◄───│ DynamoDB Streams │
   │             │    │                 │
   └─────────────┘    └─────────────────┘
```

## レイヤー構成

### Core層 (ビジネスロジック)

#### Domain層
- **Entities**: ビジネスエンティティ
  - `Meeting`: 会議情報
  - `Participant`: 参加者情報
  - `Message`: チャットメッセージ
  - `Connection`: WebSocket接続情報
- **Ports**: 外部依存のインターフェース
  - `IMeetingRepository`
  - `IParticipantRepository`
  - `IMessageRepository`
  - `IConnectionRepository`
  - `IBroadcastRepository`

#### Application層
- **Services**: ビジネスロジックの実装
  - `SignInService`: サインイン処理
  - `MessageService`: メッセージ送信・配信処理
  - `ConnectionService`: WebSocket接続管理
  - `BroadcastService`: リアルタイムメッセージ配信
- **Ports**: サービス層インターフェース
  - `ISignInService`, `IMessageService`, `IConnectionService`, `IBroadcastService`
- **DTOs**: データ転送オブジェクト
  - Request/Response モデル（SignIn, PostMessage, Connect, Disconnect）

### Infrastructure層 (データアクセス)
- **Repositories**: データ永続化の実装
  - `MeetingRepository`
  - `ParticipantRepository`
  - `MessageRepository`
  - `ConnectionRepository`
  - `BroadcastRepository`: WebSocketメッセージ配信
- **Entities**: DynamoDB用データモデル
  - `MeetingEntity`
  - `ParticipantEntity`
  - `MessageEntity`
  - `WebSocketConnectionEntity`

### LambdaHandlers層 (APIエントリーポイント)
- **Functions**: AWS Lambda関数
  - `SignInFunction`: サインインAPI
  - `PostMessageFunction`: メッセージ送信API
  - `WebSocketConnectFunction`: WebSocket接続管理
  - `WebSocketDisconnectFunction`: WebSocket切断管理
  - `MessageBroadcastFunction`: DynamoDB Streams経由リアルタイム配信

## 設計原則

### 依存関係の方向
```
LambdaHandlers → Core ← Infrastructure
```

- LambdaHandlers層は Core層に依存
- Infrastructure層は Core層に依存
- Core層は外部に依存しない（Dependency Inversion）

### 責務分離
- **Domain**: ビジネスルール・エンティティ
- **Application**: ユースケース・ビジネスロジック
- **Infrastructure**: 外部システムとの連携
- **LambdaHandlers**: HTTP リクエスト/レスポンス処理

## 技術選択

### サーバーレスアーキテクチャ
- **AWS Lambda**: 実行環境
- **API Gateway**: HTTPエンドポイント・WebSocket API
- **DynamoDB**: NoSQLデータベース
- **DynamoDB Streams**: リアルタイムデータ変更イベント

## リアルタイム機能アーキテクチャ

### WebSocket通信フロー
1. **接続確立**: WebSocketConnectFunction → ConnectionService → ConnectionRepository
2. **メッセージ送信**: PostMessageFunction → MessageService → MessageRepository (DynamoDB)
3. **リアルタイム配信**: DynamoDB Streams → MessageBroadcastFunction → BroadcastService → WebSocket API

### DynamoDB Streamsイベント処理
- **トリガー**: Messageテーブルの INSERT イベント
- **処理**: MessageBroadcastFunction が自動実行
- **配信**: 同じ会議の全接続者にリアルタイム配信

## テスト戦略

### 単体テスト
- **Core層**: ビジネスロジックのテスト（Message, Connection エンティティ含む）
- **Infrastructure層**: リポジトリのテスト（Broadcast, Connection リポジトリ含む）
- **LambdaHandlers層**: API関数のテスト（全5関数）

### テスト分離
- モックを使用してレイヤー間を分離
- 外部依存（DynamoDB、WebSocket API等）をモック化
- DynamoDB Streams処理の単体テスト