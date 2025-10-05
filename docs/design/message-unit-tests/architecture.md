# メッセージ・WebSocket接続管理 単体テスト アーキテクチャ設計

## システム概要

既存の「意見ありませんボタン」システムにおける、MessageBroadcast・PostMessage・WebSocket接続管理機能の包括的な単体テストスイートを構築する。既存のSignInテストパターンに準拠し、クリーンアーキテクチャの各層（Lambda Handler・Infrastructure・Core層）とDomain層のValueObjectsを完全にカバーする。

## アーキテクチャパターン

- **パターン**: レイヤードアーキテクチャに基づくテスト構造
- **理由**: 既存システムのクリーンアーキテクチャに対応し、各層の責務を明確に分離してテスト可能性を最大化

## テストアーキテクチャ構成

### レイヤー分離テスト戦略

```
┌─────────────────────────────────────────────────────┐
│                Unit Tests                           │
├─────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────┐ │
│  │        LambdaHandlers.Tests                   │ │
│  │  ┌──────────────────────────────────────────────┐││
│  │  │ MessageBroadcastFunctionTests            │││
│  │  │ PostMessageFunctionTests                 │││
│  │  │ WebSocketConnect/DisconnectFunctionTests │││
│  │  │ - DynamoDBEvent Mock化                   │││
│  │  │ - APIGatewayProxyRequest Mock化          │││
│  │  │ - Service層依存注入（Reflection）         │││
│  │  └──────────────────────────────────────────────┘││
│  └─────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────┐ │
│  │           Infrastructure.Tests                │ │
│  │  ┌──────────────────────────────────────────────┐││
│  │  │ BroadcastRepositoryTests                 │││
│  │  │ MessageRepositoryTests                   │││
│  │  │ ConnectionRepositoryTests                │││
│  │  │ - AWS SDK Mock化                         │││
│  │  │ - DynamoDB Context Mock化                │││
│  │  └──────────────────────────────────────────────┘││
│  └─────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────┐ │
│  │              Core.Tests                       │ │
│  │  ┌──────────────────────────────────────────────┐││
│  │  │ Application Services Tests               │││
│  │  │ - MessageServiceTests                    │││
│  │  │ - ConnectionServiceTests                 │││
│  │  │ - BroadcastServiceTests                  │││
│  │  └──────────────────────────────────────────────┘││
│  │  ┌──────────────────────────────────────────────┐││
│  │  │ Domain ValueObjects Tests                │││
│  │  │ - MessageId, MessageContent Tests        │││
│  │  │ - Participant関連 ValueObjects Tests     │││
│  │  │ - Meeting関連 ValueObjects Tests         │││
│  │  │ - Connection関連 ValueObjects Tests      │││
│  │  └──────────────────────────────────────────────┘││
│  └─────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

## コンポーネント構成

### テストフレームワーク
- **フレームワーク**: xUnit.net
- **Mocking**: Moq ライブラリ
- **テストランナー**: .NET Test SDK
- **カバレッジ**: 既存テスト実行環境に統合

### 依存関係管理
- **依存注入**: リフレクションによるプライベートフィールド置換（SignInパターン踏襲）
- **AWS SDK Mock**: Moq による AmazonApiGatewayManagementApiClient, IDynamoDBContext のモック化
- **イベント生成**: テスト専用 DynamoDBEvent, APIGatewayProxyRequest ファクトリ

### テスト分離戦略
- **レイヤー分離**: 各層のテストは独立実行可能
- **モック境界**: 外部依存（AWS SDK, DynamoDB）は完全モック化
- **データ分離**: 各テストは独立したテストデータを使用

## テスト対象システムの技術構成

### Lambda Handler層
- **MessageBroadcastFunction**: DynamoDB Streamsイベント処理
- **PostMessageFunction**: HTTP API Gateway経由メッセージ送信
- **WebSocketConnect/DisconnectFunction**: WebSocket API Gateway接続管理

### Infrastructure層  
- **BroadcastRepository**: API Gateway Management API経由WebSocket送信
- **MessageRepository**: DynamoDB メッセージデータ永続化
- **ConnectionRepository**: DynamoDB WebSocket接続データ管理

### Core層
- **Application Services**: ビジネスロジック実装（Message, Connection, Broadcast）
- **Domain ValueObjects**: ビジネスルール検証・型安全性保証

## テスト実行環境

### 実行コマンド
```bash
cd NoOpinionButtonApi
dotnet test  # 全テスト実行
dotnet test --filter "FullyQualifiedName~MessageBroadcast"  # 特定機能テスト
```

### 継続的インテグレーション対応
- **実行時間**: 各テストクラス個別実行可能
- **並列実行**: テスト間依存関係なし
- **決定論的**: 実行のたびに同じ結果を保証

## 品質保証戦略

### カバレッジ目標
- **正常系**: 全機能の成功パスをカバー
- **異常系**: 例外処理・エラーハンドリングをカバー
- **境界値**: ValueObjectsの制限値・AWS APIエラーをカバー

### テスト命名規則
- **形式**: `{状態}_{メソッド名}_{期待結果}`
- **例**: `正常系_BroadcastToConnectionAsync_配信成功`
- **言語**: 日本語（既存SignInテストと統一）