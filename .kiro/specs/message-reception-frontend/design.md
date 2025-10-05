# 設計文書

## 概要

この設計文書は、participant.vueページにメッセージ受信機能を実装するための詳細な設計を定義します。WebSocket接続の管理、メッセージの受信・表示、UI更新を既存のアーキテクチャパターンに従って実装します。

## アーキテクチャ

### 全体構成

```
participant.vue
├── WebSocket接続管理 (useWebSocketConnection composable)
├── メッセージ状態管理 (useMessageReception composable)  
├── UI表示コンポーネント (MessageList component)
└── 既存の参加者名登録機能
```

### データフロー

1. 参加者名登録完了 → WebSocket接続開始
2. WebSocket接続確立 → 接続状態更新
3. メッセージ受信 → データ解析 → 状態更新 → UI更新
4. 接続エラー → エラーハンドリング → 再接続試行

## コンポーネントとインターフェース

### 1. WebSocket接続管理 Composable

**ファイル:** `composables/webSocket/useWebSocketConnection.ts`

```typescript
interface WebSocketConnectionState {
  isConnected: boolean
  isConnecting: boolean
  error: string | null
  reconnectAttempts: number
}

interface WebSocketConnection {
  state: Ref<WebSocketConnectionState>
  connect: (meetingId: string, participantId: string) => Promise<void>
  disconnect: () => void
  send: (message: any) => void
  onMessage: (callback: (data: any) => void) => void
  onError: (callback: (error: Event) => void) => void
}
```

**主要機能:**
- WebSocket接続の確立と管理
- 自動再接続機能（最大5回、指数バックオフ）
- 接続状態の監視
- エラーハンドリング

### 2. メッセージ受信管理 Composable

**ファイル:** `composables/message/useMessageReception.ts`

```typescript
interface MessageData {
  id: string
  meetingId: string
  participantId: string
  participantName: string
  content: string
  createdAt: string
  likeCount: number
  reportedCount: number
  isActive: boolean
}

interface MessageReceptionState {
  messages: MessageData[]
  isLoading: boolean
  error: string | null
}

interface MessageReception {
  state: Ref<MessageReceptionState>
  addMessage: (messageData: MessageData) => void
  clearMessages: () => void
  handleWebSocketMessage: (rawData: string) => void
}
```

**主要機能:**
- WebSocketから受信したメッセージの解析
- メッセージリストの管理
- MessageEntityからフロントエンド用データへの変換

### 3. メッセージ表示コンポーネント

**ファイル:** `components/organisms/MessageList.vue`

```typescript
interface Props {
  messages: MessageData[]
  isConnected: boolean
  connectionError?: string | null
}
```

**主要機能:**
- メッセージリストの表示
- 参加者IDアバターの表示
- 緑色吹き出しでのメッセージ表示
- 自動スクロール機能
- 接続状態インジケーター

### 4. メッセージアイテムコンポーネント

**ファイル:** `components/molecules/MessageItem.vue`

```typescript
interface Props {
  participantId: string
  participantName: string
  content: string
  createdAt: string
}
```

**主要機能:**
- 個別メッセージの表示
- 参加者IDアバターの表示
- メッセージ内容の吹き出し表示
- 時刻表示（オプション）

## データモデル

### WebSocketメッセージ形式

WebSocketから受信するメッセージは、MessageEntityのJSON形式です：

```json
{
  "id": "message-id",
  "meetingId": "meeting-id", 
  "participantId": "participant-id",
  "participantName": "参加者名",
  "content": "メッセージ内容",
  "createdAt": "2024-01-01T00:00:00Z",
  "likeCount": 0,
  "reportedCount": 0,
  "isActive": true
}
```

### フロントエンド内部データ形式

```typescript
interface MessageData {
  id: string
  meetingId: string
  participantId: string
  participantName: string
  content: string
  createdAt: string
  likeCount: number
  reportedCount: number
  isActive: boolean
}
```

## エラーハンドリング

### WebSocket接続エラー

1. **接続失敗:** 初回接続に失敗した場合
   - エラーメッセージを表示
   - 5秒後に再接続を試行
   - 最大5回まで再試行

2. **接続切断:** 接続が予期せず切断された場合
   - 切断状態を表示
   - 自動再接続を開始
   - 指数バックオフで再接続間隔を調整

3. **メッセージ解析エラー:** 受信データが不正な場合
   - エラーログを出力
   - 該当メッセージを無視して処理を継続

### エラー表示

既存のApiErrorタイプを拡張してWebSocketエラーに対応：

```typescript
export enum ApiErrorType {
  BadRequest = 'BadRequest',
  Unauthorized = 'Unauthorized', 
  NotFound = 'NotFound',
  Server = 'Server',
  WebSocketConnection = 'WebSocketConnection',
  WebSocketMessage = 'WebSocketMessage'
}
```

## テスト戦略

### 単体テスト

1. **useWebSocketConnection composable**
   - 接続確立のテスト
   - 再接続ロジックのテスト
   - エラーハンドリングのテスト

2. **useMessageReception composable**
   - メッセージ解析のテスト
   - メッセージリスト管理のテスト
   - 不正データ処理のテスト

3. **MessageList component**
   - メッセージ表示のテスト
   - 接続状態表示のテスト
   - 自動スクロールのテスト

### 統合テスト

1. **WebSocket接続フロー**
   - 参加者名登録完了からWebSocket接続まで
   - メッセージ受信から表示まで
   - エラー発生時の処理

2. **UI更新フロー**
   - 新しいメッセージ受信時のUI更新
   - 接続状態変更時のUI更新
   - エラー状態の表示

## UI/UXデザイン

### レイアウト構成

```
┌─────────────────────────────────────┐
│ 参加者ページ                          │
├─────────────────────────────────────┤
│ 接続状態インジケーター                  │
├─────────────────────────────────────┤
│ メッセージリスト                       │
│ ┌─────┐ ┌─────────────────────────┐ │
│ │ ID  │ │ メッセージ内容            │ │
│ └─────┘ └─────────────────────────┘ │
│ ┌─────┐ ┌─────────────────────────┐ │
│ │ ID  │ │ メッセージ内容            │ │
│ └─────┘ └─────────────────────────┘ │
│ ...                                 │
└─────────────────────────────────────┘
```

### スタイリング

**Tailwind CSSクラス:**

- 参加者IDアバター: `w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center text-sm font-medium`
- メッセージ吹き出し: `bg-green-200 rounded-lg px-4 py-2 ml-3 max-w-xs lg:max-w-md`
- メッセージコンテナ: `flex items-start space-x-2 mb-4`
- 接続状態インジケーター: `flex items-center space-x-2 p-2 bg-gray-100 rounded`

### レスポンシブデザイン

- モバイル: 最大幅制限、タッチフレンドリーなサイズ
- タブレット: 中間サイズでの最適化
- デスクトップ: 大画面での見やすいレイアウト

## パフォーマンス考慮事項

### メモリ管理

1. **メッセージリスト制限:** 最大100件まで保持、古いメッセージは自動削除
2. **WebSocketリスナー:** コンポーネントアンマウント時の適切なクリーンアップ
3. **再描画最適化:** Vue 3のReactivityを活用した効率的な更新

### ネットワーク最適化

1. **再接続戦略:** 指数バックオフによる負荷軽減
2. **メッセージバッファリング:** 接続切断中のメッセージ処理
3. **接続プール:** 単一WebSocket接続の再利用

## セキュリティ考慮事項

### データ検証

1. **受信メッセージ検証:** スキーマ検証による不正データ排除
2. **XSS対策:** メッセージ内容のサニタイゼーション
3. **接続認証:** meetingIdとparticipantIdによる認証

### プライバシー

1. **データ保持:** ブラウザメモリ内のみでの一時保存
2. **ログ出力:** 個人情報を含まないログ設計
3. **接続情報:** 最小限の接続情報のみ使用