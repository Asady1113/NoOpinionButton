# Participant名前登録API エンドポイント仕様

## 概要

参加者が自分の表示名を登録・変更するためのREST APIエンドポイント。既存のクリーンアーキテクチャパターンに従い、AWS Lambda + API Gatewayで実装。

## エンドポイント

### PUT /participants/{participantId}/name

参加者の表示名を更新する。

#### パス パラメーター

| パラメーター | 型 | 必須 | 説明 |
|------------|---|------|------|
| `participantId` | string | ○ | 更新対象の参加者ID |

#### リクエスト

**Content-Type**: `application/json`

**ボディ構造**:
```json
{
  "name": "string"
}
```

**フィールド詳細**:

| フィールド | 型 | 必須 | 制約 | 説明 |
|----------|---|------|------|------|
| `name` | string | ○ | 1-50文字、空白文字のみ不可 | 新しい参加者名 |

**リクエスト例**:
```json
{
  "name": "田中太郎"
}
```

#### レスポンス

##### 成功時（200 OK）

```json
{
    "Data": {
        "updatedName": "田中太郎"
    },
    "Error": null
}
```

##### エラー時

```json
{
    "Data": null,
    "Error": "エラーメッセージ"
}
```

## バリデーション仕様

### 参加者名バリデーション

1. **必須チェック**: 空文字列・null・undefined不可
2. **長さチェック**: 1文字以上50文字以内
3. **空白チェック**: 空白文字のみ不可
4. **文字制限**: Unicode文字対応（絵文字含む）

### パスパラメータバリデーション

1. **participantId**: 非空文字列であること

## セキュリティ

### 入力サニタイゼーション

- XSS対策: 出力時にエスケープ処理
- SQLインジェクション対策: DynamoDB使用のため該当なし
- JSON形式チェック: 不正なJSON構造を拒否

## パフォーマンス

### レスポンス時間

- **目標**: 3秒以内
- **平均予想**: 500ms以内

### スループット

- **想定RPS**: 10 requests/second
- **最大対応RPS**: 1000 requests/second（Lambda自動スケーリング）

## エラーハンドリング

### リトライポリシー

- クライアント側でのリトライ推奨条件:
  - 500 Internal Server Error
  - 503 Service Unavailable
  - ネットワークタイムアウト

### ログ出力

- リクエスト受信時: 参加者ID、リクエスト内容
- バリデーションエラー時: エラー詳細
- データベースエラー時: エラースタックトレース
- レスポンス送信時: ステータスコード、処理時間

## 使用例

### cURLコマンド例

```bash
# 正常ケース
curl -X PUT https://api.example.com/participants/12345/name \
  -H "Content-Type: application/json" \
  -d '{"name": "新しい名前"}'

# レスポンス
# {"success": true}
```