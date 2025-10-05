# 要件定義書

## 概要

この機能は、Messageエンティティに参加者名（ParticipantName）プロパティを追加し、メッセージの送信・配信時に参加者名を含めるように既存のAPIを拡張します。これにより、フロントエンドでメッセージ表示時に参加者IDから名前を別途取得する必要がなくなり、パフォーマンスとユーザビリティが向上します。

## 要件

### 要件1

**ユーザーストーリー:** 開発者として、メッセージエンティティに参加者名を含めたい。これにより、メッセージ表示時に参加者名を別途取得する必要がなくなる。

#### 受け入れ基準

1. WHEN Messageエンティティが定義される THEN システムはParticipantNameプロパティを含む必要がある
2. WHEN Messageエンティティが作成される THEN システムはParticipantNameを必須プロパティとして扱う必要がある
3. WHEN Messageエンティティが保存される THEN システムはParticipantNameをデータベースに永続化する必要がある
4. WHEN MessageエンティティのValueObjectが定義される THEN システムは適切な検証ルールを適用する必要がある

### 要件2

**ユーザーストーリー:** 開発者として、MessageServiceでメッセージ作成時に参加者名を自動的に取得したい。これにより、正確な参加者名がメッセージに含まれる。

#### 受け入れ基準

1. WHEN MessageServiceがメッセージを作成する THEN システムはParticipantIdから参加者名を取得する必要がある
2. WHEN 参加者名の取得が成功する THEN システムはその名前をMessageエンティティに設定する必要がある
3. WHEN 参加者名の取得が失敗する THEN システムは適切なエラーハンドリングを行う必要がある
4. WHEN 参加者が存在しない THEN システムはエラーを返す必要がある

### 要件3

**ユーザーストーリー:** 開発者として、既存のParticipantNameValueObjectを活用したい。これにより、型安全性と既存の検証ルールが適用される。

#### 受け入れ基準

1. WHEN MessageエンティティでParticipantNameを使用する THEN システムは既存のParticipantNameValueObjectを使用する必要がある
2. WHEN ParticipantNameが設定される THEN システムは既存の検証ルール（長さ制限1-50文字）を適用する必要がある
3. WHEN ParticipantNameが設定される THEN システムは既存の検証ルール（空文字列や空白のみの値を拒否）を適用する必要がある
4. WHEN 無効なParticipantNameが設定される THEN システムは既存の例外処理を使用する必要がある

### 要件4

**ユーザーストーリー:** 開発者として、データベーススキーマを更新してParticipantNameを保存したい。これにより、メッセージデータの完全性が保たれる。

#### 受け入れ基準

1. WHEN MessageEntityがDynamoDBに保存される THEN システムはParticipantNameフィールドを含む必要がある
2. WHEN MessageRepositoryがメッセージを保存する THEN システムはParticipantNameをマッピングする必要がある
3. WHEN MessageRepositoryがメッセージを取得する THEN システムはParticipantNameを含むエンティティを返す必要がある
4. WHEN データベーススキーマが更新される THEN システムは既存データとの互換性を保つ必要がある

### 要件5

**ユーザーストーリー:** 開発者として、MessageBroadcastFunctionでメッセージ配信時に参加者名を含めたい。これにより、WebSocketクライアントが参加者名を受信できる。

#### 受け入れ基準

1. WHEN MessageBroadcastFunctionがDynamoDBイベントを処理する THEN システムはParticipantNameを取得する必要がある
2. WHEN メッセージをJSONにシリアライズする THEN システムはParticipantNameを含める必要がある
3. WHEN WebSocketクライアントにメッセージを配信する THEN システムはParticipantNameを含むJSONを送信する必要がある
4. WHEN ParticipantNameが取得できない THEN システムは適切なエラーハンドリングを行う必要がある

### 要件6

**ユーザーストーリー:** 開発者として、既存のテストを修正して新しいParticipantNameプロパティに対応したい。これにより、コードの品質と信頼性が維持される。

#### 受け入れ基準

1. WHEN Messageエンティティのテストが実行される THEN システムはParticipantNameプロパティのテストを含む必要がある
2. WHEN MessageServiceのテストが実行される THEN システムは参加者名取得のテストを含む必要がある
3. WHEN MessageBroadcastFunctionのテストが実行される THEN システムはParticipantName配信のテストを含む必要がある
4. WHEN 既存のテストが実行される THEN システムは新しいプロパティに対応して正常に動作する必要がある