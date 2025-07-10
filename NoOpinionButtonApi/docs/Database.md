# DB設計書

## テーブル: Participant

| カラム名      | 型          | PK | FK | NULL許可 | デフォルト値 | 説明                          |
|---------------|-------------|-----|-----|----------|--------------|-------------------------------|
| Id            | VARCHAR(50) | ○   |     | NO       |              | 参加者ID（主キー）            |
| Name          | VARCHAR(50) |     |     | NO       | `""`           | 参加者名                      |
| MeetingId     | VARCHAR(50) |     | ○   | NO       |              | `Meeting` テーブルへの外部キー |
| NoOpinionPoint| INT         |     |     | NO       | `0`          | 意見ありませんボタンを押せる回数 |
| HasOpinion    | BOOLEAN     |     |     | NO       | `true`       | 意見を持っているか            |
| IsActive      | BOOLEAN     |     |     | NO       | `true`       | 会議に参加中かどうか   |

---

# テーブル：Meeting

| カラム名                 | 型           | PK | FK | NULL許可 | デフォルト値 | 説明                  |
| -------------------- | ----------- | -- | -- | ------ | ------ | ------------------- |
| Id                   | VARCHAR(50) | ○  |    | NO     |        | ミーティングID（主キー） |
| Name                 | VARCHAR(50) |    |    | NO     | `""`   | ミーティング名             |
| FacilitatorPassword  | VARCHAR(50) |    |    | NO     | `""`   | 司会者用パスワード           |
| ParticipantsPassword | VARCHAR(50) |    |    | NO     | `""`   | 参加者用パスワード           |
