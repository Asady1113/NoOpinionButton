# Participant名前登録API アーキテクチャ設計

## システム概要

会議参加者が自分の表示名を登録・変更できるAPI機能を既存の「意見ありませんボタン」システムに追加する。既存のクリーンアーキテクチャパターンを踏襲し、サーバーレスアーキテクチャで実装する。

## アーキテクチャパターン

- **パターン**: クリーンアーキテクチャ + サーバーレスアーキテクチャ
- **理由**: 既存システムとの整合性を保ちながら、AWS Lambda環境で最適に動作するため

## コンポーネント構成

### Lambda関数レイヤー
- **フレームワーク**: AWS Lambda (.NET 8)
- **関数名**: UpdateParticipantNameFunction
- **HTTPメソッド**: PUT
- **エンドポイント**: `/participants/{participantId}/name`

### Core層（ビジネスロジック）

#### Domain層
- **既存エンティティ**: 
  - `Participant` - 既存エンティティ
  - `ParticipantName` - 既存バリューオブジェクト（バリデーション機能付き）
  - `ParticipantId` - 既存バリューオブジェクト
- **新規ポート**:
  - `IParticipantUpdateService` - 参加者情報更新サービスインターフェース

#### Application層
- **新規サービス**: 
  - `ParticipantUpdateService` - 参加者名前更新のビジネスロジック
- **新規DTOs**:
  - `UpdateParticipantNameRequest` - 名前更新リクエスト
  - `UpdateParticipantNameResponse` - 名前更新レスポンス

### Infrastructure層（データアクセス）
- **既存リポジトリ拡張**: 
  - `ParticipantRepository` - 名前更新メソッドを追加
- **データベース**: DynamoDB Participant テーブル

### 依存関係の方向
```
UpdateParticipantNameFunction → Core ← Infrastructure
```

## セキュリティ設計

### バリデーション層
1. **Lambda関数レベル**: HTTPリクエストの基本バリデーション
2. **Application層**: ビジネスルール適用（参加者の存在確認・状態確認）
3. **Domain層**: ParticipantNameバリューオブジェクトによる名前形式バリデーション

### エラーハンドリング
- **400 Bad Request**: バリデーションエラー
- **404 Not Found**: 参加者が存在しない
- **403 Forbidden**: 非アクティブな参加者
- **500 Internal Server Error**: システムエラー

## パフォーマンス設計

### レスポンス時間要件
- **目標**: 3秒以内
- **実装方針**: 
  - DynamoDB読み取り/書き込み操作の最適化
  - 適切なタイムアウト設定

### スケーラビリティ
- **AWS Lambda**: 自動スケーリング
- **DynamoDB**: オンデマンドキャパシティモード