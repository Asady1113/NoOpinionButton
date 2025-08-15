# データベース設計

## 概要

「意見ありませんボタン」システムは、AWS DynamoDBを使用したNoSQLデータベース設計です。

## テーブル構成

### Meeting テーブル

| カラム名 | 型 | PK | FK | NULL許可 | デフォルト値 | 説明 |
|---------|---|----|----|----------|-------------|-----|
| Id | VARCHAR(50) | ○ | | NO | `""` | ミーティングID（主キー） |
| Name | VARCHAR(50) | | | NO | `""` | ミーティング名 |
| FacilitatorPassword | VARCHAR(50) | | | NO | `""` | 司会者用パスワード |
| ParticipantPassword | VARCHAR(50) | | | NO | `""` | 参加者用パスワード |

### Participant テーブル

| カラム名 | 型 | PK | FK | NULL許可 | デフォルト値 | 説明 |
|---------|---|----|----|----------|-------------|-----|
| Id | VARCHAR(50) | ○ | | NO | `""` | 参加者ID（主キー） |
| Name | VARCHAR(50) | | | NO | `""` | 参加者名 |
| MeetingId | VARCHAR(50) | | ○ | NO | `""` | `Meeting` テーブルへの外部キー |
| NoOpinionPoint | INT | | | NO | `0` | 意見ありませんボタンを押せる回数 |
| HasOpinion | BOOLEAN | | | NO | `true` | 意見を持っているか |
| IsActive | BOOLEAN | | | NO | `true` | 会議に参加中かどうか |

### Message テーブル

| カラム名 | 型 | PK | FK | NULL許可 | デフォルト値 | 説明 |
|---------|---|----|----|----------|-------------|-----|
| Id | VARCHAR(50) | ○ | | NO | `""` | メッセージID（主キー） |
| MeetingId | VARCHAR(50) | | ○ | NO | `""` | `Meeting` テーブルの外部キー |
| ParticipantId | VARCHAR(50) | | ○ | NO | `""` | 作成者の参加者ID |
| CreatedAt | DATETIME | | | NO | CURRENT_TIMESTAMP | 作成日時 |
| Content | TEXT | | | NO | `""` | メッセージ内容 |
| LikeCount | INT | | | NO | `0` | 「いいね」の数 |
| ReportedCount | INT | | | NO | `0` | 通報された回数 |
| IsActive | BOOLEAN | | | NO | `true` | メッセージが有効かどうか |

### WebSocketConnection テーブル

| カラム名 | 型 | PK | FK | NULL許可 | デフォルト値 | 説明 |
|---------|---|----|----|----------|-------------|-----|
| ConnectionId | VARCHAR(50) | ○ | | NO | `""` | WebSocket接続ID（主キー） |
| ParticipantId | VARCHAR(50) | | ○ | NO | `""` | `Participant` テーブルへの外部キー |
| MeetingId | VARCHAR(50) | | ○ | NO | `""` | `Meeting` テーブルへの外部キー |
| ConnectedAt | DATETIME | | | NO | CURRENT_TIMESTAMP | 接続開始時刻 |
| IsActive | BOOLEAN | | | NO | `true` | 接続状態（true: 接続中, false: 切断済み） |