# アーキテクチャ設計

## 概要

「意見ありませんボタン」APIは、クリーンアーキテクチャの原則に従って設計されたサーバーレスアプリケーションです。

## アーキテクチャ構成

```
┌─────────────────────────────────────────────────┐
│                API Gateway                      │
├─────────────────────────────────────────────────┤
│                AWS Lambda                       │
│  ┌─────────────────────────────────────────────┐│
│  │           LambdaHandlers 層              ││
│  │  ┌─────────────────────────────────────────┐││
│  │  │              Core 層                ││││
│  │  │  ┌───────────────┬─────────────────┐│││
│  │  │  │   Domain      │   Application   ││││
│  │  │  │   Entities    │   Services      ││││
│  │  │  │   Ports       │   DTOs          ││││
│  │  │  └───────────────┴─────────────────┘│││
│  │  └─────────────────────────────────────────┘││
│  │  ┌─────────────────────────────────────────┐││
│  │  │           Infrastructure 層         ││││
│  │  │   Repositories, Entities            ││││
│  │  └─────────────────────────────────────────┘││
│  └─────────────────────────────────────────────┘│
└─────────────────────────────────────────────────┘
                     │
                     ▼
               ┌─────────────┐
               │  DynamoDB   │
               └─────────────┘
```

## レイヤー構成

### Core層 (ビジネスロジック)

#### Domain層
- **Entities**: ビジネスエンティティ
  - `Meeting`: 会議情報
  - `Participant`: 参加者情報
- **Ports**: 外部依存のインターフェース
  - `IMeetingRepository`
  - `IParticipantRepository`

#### Application層
- **Services**: ビジネスロジックの実装
  - `SignInService`: サインイン処理
- **DTOs**: データ転送オブジェクト
  - Request/Response モデル

### Infrastructure層 (データアクセス)
- **Repositories**: データ永続化の実装
  - `MeetingRepository`
  - `ParticipantRepository`
- **Entities**: DynamoDB用データモデル
  - `MeetingEntity`
  - `ParticipantEntity`

### LambdaHandlers層 (APIエントリーポイント)
- **Functions**: AWS Lambda関数
  - `SignInFunction`: サインインAPI

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
- **API Gateway**: HTTPエンドポイント
- **DynamoDB**: NoSQLデータベース

## テスト戦略

### 単体テスト
- **Core層**: ビジネスロジックのテスト
- **Infrastructure層**: リポジトリのテスト
- **LambdaHandlers層**: API関数のテスト

### テスト分離
- モックを使用してレイヤー間を分離
- 外部依存（DynamoDB等）をモック化