# API仕様

## 概要

「意見ありませんボタン」システムのREST API仕様書です。

## ベースURL

- **開発環境**: `http://localhost:3000`
- **本番環境**: `https://api.example.com` (デプロイ後に更新)

## 認証

現在は認証機能を実装していません。将来的にJWTトークンベースの認証を追加予定です。

## エンドポイント

### サインイン

#### POST /signin

会議への参加者認証を行います。

**リクエスト**

```http
POST /signin
Content-Type: application/json
```

```json
{
  "meetingId": "string",
  "password": "string"
}
```

| パラメータ | 型 | 必須 | 説明 |
|----------|---|-----|-----|
| meetingId | string | ✓ | 会議ID |
| password | string | ✓ | パスワード |

**リクエスト例**

```bash
curl -X POST http://localhost:3000/signin \
  -H "Content-Type: application/json" \
  -d '{
    "meetingId": "meeting-001",
    "password": "facilitator-password"
  }'
```

**レスポンス**

**成功時 (200 OK)**

```json
{
  "Data": {
    "id": "string",
    "meetingId": "string",
    "meetingName": "string",
    "isFacilitator": boolean
  }
}
```

| フィールド | 型 | 説明 |
|----------|---|-----|
| id | string | 参加者ID (UUID) |
| meetingId | string | 会議ID |
| meetingName | string | 会議名 |
| isFacilitator | boolean | 司会者フラグ |

**レスポンス例**

```json
{
  "Data": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "meetingId": "meeting-001",
    "meetingName": "週次定例会議",
    "isFacilitator": true
  }
}
```

**エラーレスポンス**

```json
{
  "Error": "string"
}
```

| ステータスコード | 説明 | メッセージ例 |
|---------------|-----|------------|
| 400 | リクエストパラメータ不正 | "会議IDが入力されていません" |
| 401 | 認証失敗 | "パスワードが間違っています" |
| 404 | 会議が見つからない | "指定された会議が見つかりません" |
| 500 | サーバーエラー | "サーバーエラーが発生しました" |

## エラーハンドリング

すべてのエラーレスポンスは以下の形式で返されます：

```json
{
  "Error": "エラーメッセージ"
}
```

### 一般的なエラー

- **400 Bad Request**: リクエストパラメータの不正
- **401 Unauthorized**: 認証失敗
- **404 Not Found**: リソースが見つからない
- **500 Internal Server Error**: サーバー内部エラー

## 今後追加予定の機能

### 意見表明機能

- `POST /opinion` - 意見なし状態の更新
- `GET /meeting/{meetingId}/opinions` - 会議の意見状況取得

### リアルタイム通信

- WebSocket接続による状態のリアルタイム同期