# 設計文書

## 概要

この設計文書は、participant.vueページにメッセージ送信機能を実装するための詳細な設計を定義します。API通信の管理、フォーム状態管理、UI表示を既存のアーキテクチャパターンに従って実装します。

## アーキテクチャ

### 全体構成

```
participant.vue
├── メッセージ送信API管理 (useMessageSendingApi composable)
├── メッセージ送信フォーム管理 (useMessageSending composable)  
├── UI表示コンポーネント (MessageSendingForm component)
└── 既存のメッセージ受信機能
```

### データフロー

1. ユーザー入力 → フォーム状態更新
2. 送信ボタンクリック → API呼び出し → レスポンス処理
3. 送信成功 → フォームクリア → 通常状態に戻る
4. 送信失敗 → エラー表示 → 再送信可能状態

## コンポーネントとインターフェース

### 1. メッセージ送信API Composable

**ファイル:** `composables/message/useMessageSendingApi.ts`

```typescript
interface MessageSendingRequest {
  meetingId: string
  participantId: string
  content: string
}

interface MessageSendingResponse {
  messageId?: string
  error?: string
}

interface MessageSendingApi {
  sendMessage: (request: MessageSendingRequest) => Promise<MessageSendingResponse>
  isLoading: Ref<boolean>
  error: Ref<ApiError | null>
}
```

**主要機能:**
- POST /message APIの呼び出し
- リクエスト/レスポンスの型安全性
- エラーハンドリング
- ローディング状態管理

### 2. メッセージ送信フォーム管理 Composable

**ファイル:** `composables/message/useMessageSending.ts`

```typescript
interface MessageSendingState {
  content: string
  isSubmitting: boolean
  error: string | null
  canSubmit: boolean
}

interface MessageSending {
  state: Ref<MessageSendingState>
  updateContent: (content: string) => void
  submitMessage: (meetingId: string, participantId: string) => Promise<void>
  clearForm: () => void
  clearError: () => void
}
```

**主要機能:**
- フォーム入力状態の管理
- バリデーション（空文字チェック）
- 送信処理の制御
- エラー状態管理

### 3. メッセージ送信フォームコンポーネント

**ファイル:** `components/molecules/MessageSendingForm.vue`

```typescript
interface Props {
  meetingId: string
  participantId: string
  disabled?: boolean
}

interface Emits {
  messageSent: []
}
```

**主要機能:**
- メッセージ入力フィールド
- 送信ボタン（青色三角形アイコン）
- ローディング状態表示
- エラーメッセージ表示
- Enterキー送信対応

## データモデル

### API リクエスト形式

```typescript
interface MessageSendingRequest {
  meetingId: string    // 会議ID
  participantId: string // 参加者ID  
  content: string      // メッセージ内容
}
```

### API レスポンス形式

```typescript
interface MessageSendingResponse {
  messageId?: string
  error?: string
}
```

### フォーム状態

```typescript
interface MessageSendingState {
  content: string      // 入力中のメッセージ内容
  isSubmitting: boolean // 送信処理中フラグ
  error: string | null // エラーメッセージ
  canSubmit: boolean   // 送信可能フラグ
}
```

## エラーハンドリング

### API通信エラー

1. **ネットワークエラー:** 接続失敗、タイムアウト
   - 既存のApiErrorタイプを使用
   - 「ネットワークエラーが発生しました」メッセージ表示

2. **サーバーエラー:** 4xx, 5xxレスポンス
   - レスポンスステータスに応じたエラーメッセージ
   - 400: 「リクエストが無効です」
   - 500: 「サーバーエラーが発生しました」

3. **バリデーションエラー:** 空文字送信
   - クライアントサイドで事前チェック
   - 送信ボタン無効化で防止

### エラー表示

既存のApiErrorMessageコンポーネントを活用：

```vue
<ApiErrorMessage 
  v-if="error" 
  :error="error" 
  @dismiss="clearError" 
/>
```

## UI/UXデザイン

### レイアウト構成

```
┌─────────────────────────────────────┐
│ メッセージ受信エリア                  │
├─────────────────────────────────────┤
│ メッセージ送信フォーム                │
│ ┌─────────────────────────────┬───┐ │
│ │ 匿名意見を入力してください    │ ▶ │ │
│ └─────────────────────────────┴───┘ │
└─────────────────────────────────────┘
```

### スタイリング

**Tailwind CSSクラス:**

- フォームコンテナ: `flex items-center space-x-2 p-4 border-t bg-white`
- 入力フィールド: `flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500`
- 送信ボタン: `w-12 h-12 bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 rounded-lg flex items-center justify-center transition-colors`
- 送信アイコン: `w-6 h-6 text-white`
- エラーメッセージ: `text-red-500 text-sm mt-1`

### レスポンシブデザイン

- モバイル: フォーム要素の適切なサイズ調整
- タブレット: 中間サイズでの最適化  
- デスクトップ: 大画面での見やすいレイアウト

## パフォーマンス考慮事項

### フォーム最適化

1. **入力デバウンス:** バリデーション処理の最適化
2. **送信制御:** 重複送信防止
3. **メモリ管理:** 適切なクリーンアップ

### API通信最適化

1. **リクエスト制御:** 送信中の重複リクエスト防止
2. **エラーリトライ:** 必要に応じた再送信機能
3. **タイムアウト設定:** 適切なタイムアウト値

## セキュリティ考慮事項

### 入力検証

1. **XSS対策:** 入力内容のサニタイゼーション
2. **文字数制限:** 適切な文字数制限の実装
3. **特殊文字処理:** 安全な文字列処理

### API通信セキュリティ

1. **HTTPS通信:** 暗号化された通信の確保
2. **認証情報:** meetingIdとparticipantIdの適切な管理
3. **データ検証:** 送信前のデータ検証

## テスト戦略

### 単体テスト

1. **useMessageSendingApi composable**
   - API呼び出しのテスト
   - エラーハンドリングのテスト
   - ローディング状態のテスト

2. **useMessageSending composable**
   - フォーム状態管理のテスト
   - バリデーションのテスト
   - 送信処理のテスト

3. **MessageSendingForm component**
   - ユーザー入力のテスト
   - 送信ボタンの状態テスト
   - エラー表示のテスト