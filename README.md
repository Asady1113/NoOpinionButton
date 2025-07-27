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

- ✅ **サインイン機能**: 完全実装・テスト完了
- ⏳ **司会者画面**: 未実装
- ⏳ **参加者画面**: 未実装
- ⏳ **意見表明機能**: 未実装

## 詳細情報

各ディレクトリの詳細な設定・開発手順については、以下を参照してください：

- **全体の開発情報**: [CLAUDE.md](./CLAUDE.md)
- **フロントエンド**: [NoOpinionButtonWeb/README.md](./NoOpinionButtonWeb/README.md)
- **バックエンド**: [NoOpinionButtonApi/README.md](./NoOpinionButtonApi/README.md)

## ライセンス

このプロジェクトはプライベートプロジェクトです。