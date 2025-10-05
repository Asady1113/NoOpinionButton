# 意見ありませんボタン

会議で「意見がない」参加者を可視化するシステムです。参加者は「意見なし」を表明でき、司会者は参加者の状況をリアルタイムで確認できます。

## プロジェクト構成

```
NoOpinionButton/
├── NoOpinionButtonApi/     # バックエンドAPI (.NET Core + AWS)
├── NoOpinionButtonWeb/     # フロントエンド (Nuxt.js)
├── CLAUDE.md              # 開発者向け詳細ドキュメント
└── README.md              # このファイル
```

## 技術スタック

- **フロントエンド**: Nuxt.js (Vue.js) + TypeScript + Tailwind CSS
- **バックエンド**: .NET Core + AWS Lambda + API Gateway
- **データベース**: DynamoDB
- **インフラ**: AWS CDK

## クイックスタート

### フロントエンド起動
```bash
cd NoOpinionButtonWeb
npm install
npm run dev
```

### バックエンド起動（ローカル）
```bash
cd NoOpinionButtonApi
sam build
sam local start-api
```

## テスト実行

```bash
# フロントエンドテスト
cd NoOpinionButtonWeb
npm test

# バックエンドテスト  
cd NoOpinionButtonApi
dotnet test
```

## 現在の実装状況

### 完全実装済み（フルスタック）
- ✅ **サインイン機能**: バックエンドAPI + フロントエンドUI完了・テスト完了
  - 認証ロジック・エラーハンドリング
  - 司会者/参加者自動判定・ページ遷移

### バックエンド実装済み
- ✅ **メッセージ機能**: API完了・フロントエンド未実装
  - メッセージ送信API・リアルタイム配信（DynamoDB Streams）
- ✅ **WebSocket接続管理**: API完了
  - 接続/切断管理・一括配信機能

### テスト実装済み
- ✅ **バックエンドテスト**: 20+テストケース（Core/Infrastructure/LambdaHandlers層）
- ✅ **フロントエンドテスト**: 25テストケース（Composables/コンポーネント/統合テスト）

### 未実装
- ⏳ **司会者画面**: facilitator.vue未実装
- ⏳ **参加者画面**: participant.vue未実装
- ⏳ **意見表明機能**: 「意見なし」ボタン未実装

## 詳細情報

各ディレクトリの詳細な設定・開発手順については、以下を参照してください：

- **全体の開発情報**: [CLAUDE.md](./CLAUDE.md)
- **フロントエンド**: [NoOpinionButtonWeb/README.md](./NoOpinionButtonWeb/README.md)
- **バックエンド**: [NoOpinionButtonApi/README.md](./NoOpinionButtonApi/README.md)