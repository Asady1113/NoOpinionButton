# 実装計画

- [x] 1. Messageエンティティの更新
  - MessageエンティティにParticipantNameプロパティを追加
  - コンストラクタにParticipantNameパラメータを追加
  - 既存のParticipantNameValueObjectを使用
  - _要件: 1.1, 1.2, 1.3, 3.1_

- [x] 2. MessageServiceの更新
  - MessageServiceにIParticipantRepositoryの依存性注入を追加
  - PostMessageAsyncメソッドで参加者名取得ロジックを実装
  - 参加者が存在しない場合のエラーハンドリングを追加
  - メッセージ作成時にParticipantNameを設定
  - _要件: 2.1, 2.2, 2.3, 2.4_

- [x] 3. MessageEntityとMessageRepositoryの更新
  - MessageEntityにParticipantNameプロパティを追加
  - DynamoDBPropertyアトリビュートを設定
  - MessageRepositoryのSaveAsyncメソッドでParticipantNameをマッピング
  - ドメインエンティティとの変換処理を更新
  - _要件: 4.1, 4.2, 4.3_

- [x] 4. MessageBroadcastFunctionの更新
  - ProcessMessageInsertメソッドでParticipantNameを取得
  - DynamoDBレコードからParticipantNameフィールドを読み取り
  - JSONシリアライズ時にParticipantNameを含める
  - ParticipantNameが存在しない場合のフォールバック処理を追加
  - _要件: 5.1, 5.2, 5.3, 5.4_

- [x] 5. Messageエンティティのテスト更新
  - MessageエンティティのコンストラクタテストにParticipantNameを追加
  - ParticipantNameプロパティの設定テストを作成
  - 無効なParticipantNameでの例外テストを作成
  - _要件: 6.1_

- [x] 6. MessageServiceのテスト更新
  - PostMessageAsyncテストで参加者名取得のモックを設定
  - 参加者が存在する場合のテストを更新
  - 参加者が存在しない場合の例外テストを作成
  - MessageRepositoryのSaveAsyncモックでParticipantNameを検証
  - _要件: 6.2_

- [x] 7. MessageRepositoryのテスト更新
  - SaveAsyncテストでParticipantNameのマッピングを検証
  - MessageEntityからMessageドメインエンティティへの変換テストを更新
  - ParticipantNameが正しく保存・取得されることを確認
  - _要件: 6.3_

- [x] 8. MessageBroadcastFunctionのテスト更新
  - ProcessMessageInsertテストでParticipantNameを含むDynamoDBレコードを作成
  - ParticipantNameが含まれるJSONの配信テストを作成
  - ParticipantNameが存在しない場合のフォールバック処理テストを作成
  - BroadcastServiceのモックでParticipantNameを含むJSONを検証
  - _要件: 6.4_