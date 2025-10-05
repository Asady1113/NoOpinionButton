# メッセージ・WebSocket接続管理 単体テスト実装 要件定義書

## 概要

MessageBroadcast、PostMessage、WebSocketConnect、WebSocketDisconnect機能について、SignInの実装パターンを参考にしてLambdaHandler層、Infrastructure層、Core層（Domain層のValueObjectsを含む）の全層で単体テストを完全実装する。

## ユーザストーリー

### ストーリー1: メッセージ機能の品質保証
- **である** 開発者 **として**
- **私は** メッセージ送信・配信機能の単体テストを実装 **したい**
- **そうすることで** メッセージ機能の動作を保証し、DynamoDB Streamsによるリアルタイム配信の信頼性を確保できる

### ストーリー2: WebSocket接続管理の品質保証
- **である** 開発者 **として**
- **私は** WebSocket接続・切断機能の単体テストを実装 **したい**
- **そうすることで** リアルタイム通信の安定性とWebSocket接続の生存管理を保証できる

### ストーリー3: ValueObjectsの品質保証
- **である** 開発者 **として**
- **私は** Domain層のValueObjectsの単体テストを実装 **したい**
- **そうすることで** ビジネスルールの検証ロジック（バリデーション）と値変換の正確性を保証できる

## 機能要件（EARS記法）

### 通常要件

#### Lambda Handler層
- REQ-001: システムはMessageBroadcastFunctionの単体テストを実装しなければならない
- REQ-002: システムはPostMessageFunctionの単体テストを実装しなければならない
- REQ-003: システムはWebSocketConnectFunctionの単体テストを実装しなければならない
- REQ-004: システムはWebSocketDisconnectFunctionの単体テストを実装しなければならない

#### Infrastructure層
- REQ-005: システムはBroadcastRepositoryの単体テストを実装しなければならない
- REQ-006: システムはMessageRepositoryの単体テストを実装しなければならない
- REQ-007: システムはConnectionRepositoryの単体テストを実装しなければならない

#### Core層（Application Services）
- REQ-008: システムはMessageServiceの単体テストを実装しなければならない
- REQ-009: システムはConnectionServiceの単体テストを実装しなければならない
- REQ-010: システムはBroadcastServiceの単体テストを実装しなければならない

#### Core層（Domain ValueObjects）
- REQ-011: システムはMessageIdの単体テストを実装しなければならない
- REQ-012: システムはMessageContentの単体テストを実装しなければならない
- REQ-013: システムはLikeCountの単体テストを実装しなければならない
- REQ-014: システムはReportedCountの単体テストを実装しなければならない
- REQ-015: システムはParticipantIdの単体テストを実装しなければならない
- REQ-016: システムはParticipantNameの単体テストを実装しなければならない
- REQ-017: システムはNoOpinionPointの単体テストを実装しなければならない
- REQ-018: システムはMeetingIdの単体テストを実装しなければならない
- REQ-019: システムはMeetingNameの単体テストを実装しなければならない
- REQ-020: システムはFacilitatorPasswordの単体テストを実装しなければならない
- REQ-021: システムはParticipantPasswordの単体テストを実装しなければならない
- REQ-022: システムはConnectionIdの単体テストを実装しなければならない

### 条件付き要件

- REQ-101: SignInの実装パターンが参考として提供される場合、システムは同様のテスト構造・Mock化・リフレクションパターンを使用しなければならない
- REQ-102: DynamoDB Streamsイベントをテストする場合、システムは適切なDynamoDBEventオブジェクトを作成してINSERT/MODIFY/REMOVEイベントを検証しなければならない
- REQ-103: WebSocketイベントをテストする場合、システムは適切なAPIGatewayProxyRequestオブジェクトを作成して接続・切断を検証しなければならない
- REQ-104: ValueObjectsをテストする場合、システムはコンストラクタバリデーション・暗黙的変換・境界値を検証しなければならない

### 状態要件

- REQ-201: テスト実行時、システムは正常系・異常系・境界値の全てのケースを網羅しなければならない
- REQ-202: エラー処理テスト時、システムは例外の適切な再スローとログ出力を検証しなければならない
- REQ-203: ValueObjectsのテスト時、システムは無効値でのArgumentException発生を検証しなければならない

### 制約要件

- REQ-401: システムはxUnitテストフレームワークを使用しなければならない
- REQ-402: システムはMoqライブラリでモック化しなければならない
- REQ-403: システムは既存のSignInテストと同じディレクトリ構造を維持しなければならない

## 非機能要件

### 保守性
- NFR-101: テストメソッド名は日本語で動作を明確に表現しなければならない
- NFR-102: テストは実装クラスと1対1で対応しなければならない

### テスタビリティ
- NFR-201: 各テストは独立実行可能でなければならない
- NFR-202: テストは決定論的でなければならない

## Edgeケース

### エラー処理
- EDGE-001: 不正なDynamoDBイベントデータでの例外処理
- EDGE-002: 不正なJSONリクエストでの例外処理
- EDGE-003: WebSocket接続ID不正時の例外処理
- EDGE-004: AWS API呼び出し失敗時の例外処理
- EDGE-005: ValueObjects作成時の無効値での例外処理

### 境界値
- EDGE-101: 空のメッセージ内容
- EDGE-102: 大量の同時WebSocket接続
- EDGE-103: 既に切断されたWebSocket接続への送信
- EDGE-104: ValueObjectsの最大値・最小値境界
- EDGE-105: MessageContent.MaxLength（500文字）境界値
- EDGE-106: NoOpinionPoint.MaxPoint（2ポイント）境界値

## 受け入れ基準

### 機能テスト

#### LambdaHandler層
- [ ] MessageBroadcastFunctionTests.cs が実装される
- [ ] PostMessageFunctionTests.cs が実装される  
- [ ] WebSocketConnectFunctionTests.cs が実装される
- [ ] WebSocketDisconnectFunctionTests.cs が実装される

#### Infrastructure層
- [ ] BroadcastRepositoryTests.cs が実装される
- [ ] MessageRepositoryTests.cs が実装される
- [ ] ConnectionRepositoryTests.cs が実装される

#### Core層（Application Services）
- [ ] MessageServiceTests.cs が実装される
- [ ] ConnectionServiceTests.cs が実装される
- [ ] BroadcastServiceTests.cs が実装される

#### Core層（Domain ValueObjects）
- [ ] MessageIdTests.cs が実装される
- [ ] MessageContentTests.cs が実装される
- [ ] LikeCountTests.cs が実装される
- [ ] ReportedCountTests.cs が実装される
- [ ] ParticipantIdTests.cs が実装される
- [ ] ParticipantNameTests.cs が実装される
- [ ] NoOpinionPointTests.cs が実装される
- [ ] MeetingIdTests.cs が実装される
- [ ] MeetingNameTests.cs が実装される
- [ ] FacilitatorPasswordTests.cs が実装される
- [ ] ParticipantPasswordTests.cs が実装される
- [ ] ConnectionIdTests.cs が実装される

### 品質基準
- [ ] 全テストが `dotnet test` で正常実行される
- [ ] 正常系・異常系・境界値ケースが網羅される
- [ ] SignInテストと同じパターンでモック化される
- [ ] ValueObjectsのバリデーションロジックが完全にテストされる