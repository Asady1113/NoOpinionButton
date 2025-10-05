# 意見ありませんボタン フロントエンド

「意見ありませんボタン」システムのWebフロントエンドです。Nuxt.js + Vue.js + TypeScriptで構築されています。

## 🚀 クイックスタート

### 開発環境セットアップ

```bash
# 依存関係のインストール
npm install

# 開発サーバー起動
npm run dev

# テスト実行
npm test
```

開発サーバーは `http://localhost:3000` で起動します。

### 本番ビルド・デプロイ

本番用のビルド・デプロイ手順については [scripts/README.md](./scripts/README.md) を参照してください。

## 📁 プロジェクト構造

```
NoOpinionButtonWeb/
├── pages/                    # ページコンポーネント
│   ├── signin.vue           # サインインページ
│   ├── facilitator.vue      # 司会者ページ（基本実装）
│   └── participant.vue      # 参加者ページ（ほぼ完成）
├── components/              # UIコンポーネント（Atomic Design）
│   ├── atoms/               # 基本UI要素
│   ├── molecules/           # 複合コンポーネント
│   └── organisms/           # 大型コンポーネント
├── composables/             # コンポーザブル関数
│   ├── signIn/              # サインイン関連
│   ├── participantName/     # 参加者名関連
│   ├── message/             # メッセージ関連
│   └── webSocket/           # WebSocket関連
├── types/                   # TypeScript型定義
│   └── error.ts             # エラー型定義
├── tests/                   # テストスイート（25テストケース）
│   ├── composables/         # Composables単体テスト（各機能別）
│   ├── components/          # コンポーネントテスト（atoms/molecules/organisms別）
│   ├── pages/               # ページコンポーネントテスト
│   └── integration/         # 統合テスト
├── vitest.config.ts         # テスト設定
└── nuxt.config.ts           # Nuxt設定
```

## 🔧 技術スタック

- **Nuxt.js 3**: Vue.jsフルスタックフレームワーク
- **Vue.js 3**: Composition API
- **TypeScript**: 型安全性
- **Tailwind CSS**: ユーティリティファーストCSS
- **Vitest**: 高速テストランナー
- **Vue Test Utils**: Vue.jsコンポーネントテスト

## 📋 実装状況

### 完全実装済み
- ✅ **サインイン機能**: フルスタック実装・テスト完了
  - 認証ロジック・エラーハンドリング
  - 司会者/参加者自動判定・ページ遷移
- ✅ **参加者名更新機能**: フルスタック実装完了
  - ParticipantNameModal + composables
  - 参加者名登録・更新機能
- ✅ **メッセージ機能**: フルスタック実装完了
  - MessageSendingForm + MessageList + composables
  - リアルタイムメッセージ送受信・WebSocket接続管理

### 未実装
- ⏳ **司会者画面の詳細UI**: 参加者一覧・状況表示機能
- ⏳ **「意見なし」ボタン機能**: UI未実装

## 🧪 テスト

### テスト実行

```bash
# 全テスト実行
npm test

# テスト種別ごとの実行
npm test tests/composables   # 単体テスト
npm test tests/pages         # コンポーネントテスト
npm test tests/integration   # 統合テスト
```

## 📚 関連ドキュメント

### プロジェクトドキュメント
- **[機能仕様書](./docs/FunctionalSpecification.md)**: 画面仕様・機能詳細
- **[デプロイ手順](./scripts/README.md)**: 本番ビルド・デプロイ方法

### 技術ドキュメント
- **[Nuxt.js公式ドキュメント](https://nuxt.com/docs)**
- **[Vue.js公式ドキュメント](https://vuejs.org/)**
- **[Vitest公式ドキュメント](https://vitest.dev/)**
- **[Tailwind CSS公式ドキュメント](https://tailwindcss.com/docs)**
