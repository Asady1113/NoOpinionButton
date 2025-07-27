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

### 本番ビルド

```bash
# 本番用ビルド
npm run build

# プレビュー（本番ビルドをローカルで確認）
npm run preview
```

## 📁 プロジェクト構造

```
NoOpinionButtonWeb/
├── pages/                    # ページコンポーネント
│   └── signin.vue           # サインインページ
├── composables/             # コンポーザブル関数
│   └── signIn/
│       ├── useSignIn.ts     # 状態管理
│       └── useSignInApi.ts  # API通信
├── types/                   # TypeScript型定義
│   └── error.ts             # エラー型定義
├── tests/                   # テストスイート（25テストケース）
│   ├── composables/         # Composables単体テスト
│   ├── pages/               # コンポーネントテスト
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

- ✅ **サインインページ**: 完全実装・テスト完了
- ✅ **状態管理**: useState（Pinia風）
- ✅ **API通信**: 認証・エラーハンドリング
- ✅ **レスポンシブデザイン**: Tailwind CSS
- ✅ **包括的テスト**: 25テストケース
- ⏳ **司会者画面**: 未実装
- ⏳ **参加者画面**: 未実装

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

### テスト構成

- **単体テスト (10件)**: Composables機能テスト
- **コンポーネントテスト (9件)**: UI・イベントハンドリング
- **統合テスト (6件)**: エンドツーエンドフロー

## 🎨 スタイリング

### Tailwind CSS

ユーティリティクラスでレスポンシブデザインを実装：

```vue
<template>
  <div class="flex flex-col items-center justify-center min-h-screen">
    <input class="mb-6 border px-4 py-1 rounded-lg w-96" />
    <button class="px-6 py-2 bg-indigo-500 text-white rounded-full disabled:opacity-50 w-60">
      会議に参加
    </button>
  </div>
</template>
```

## 📚 関連ドキュメント

- **[Nuxt.js公式ドキュメント](https://nuxt.com/docs)**
- **[Vue.js公式ドキュメント](https://vuejs.org/)**
- **[Vitest公式ドキュメント](https://vitest.dev/)**
- **[Tailwind CSS公式ドキュメント](https://tailwindcss.com/docs)**
