# Participant名前登録API データフロー設計

## システム相互作用フロー

```mermaid
flowchart TD
    A[フロントエンド] -->|PUT /participants/{id}/name| B[API Gateway]
    B --> C[UpdateParticipantNameFunction]
    C --> D[ParticipantUpdateService]
    D --> E[ParticipantRepository]
    E --> F[DynamoDB]
    
    F -->|結果| E
    E -->|結果| D
    D -->|結果| C
    C -->|HTTPレスポンス| B
    B -->|HTTPレスポンス| A
```

## 詳細な処理シーケンス

```mermaid
sequenceDiagram
    participant FE as フロントエンド
    participant AG as API Gateway
    participant LF as UpdateParticipantNameFunction
    participant SV as ParticipantUpdateService
    participant RP as ParticipantRepository
    participant DB as DynamoDB

    FE->>AG: PUT /participants/{participantId}/name
    note over FE,AG: リクエストボディ: {"name": "新しい名前"}
    
    AG->>LF: Lambda関数実行
    note over AG,LF: participantId, リクエストボディを渡す
    
    LF->>LF: リクエストバリデーション
    alt バリデーションエラー
        LF-->>AG: 400 Bad Request
        AG-->>FE: エラーレスポンス
    end
    
    LF->>SV: UpdateParticipantName()
    note over LF,SV: ParticipantId, ParticipantNameを渡す
    
    SV->>RP: GetByIdAsync(participantId)
    RP->>DB: DynamoDB GetItem
    DB-->>RP: 参加者データ
    
    alt 参加者が存在しない
        RP-->>SV: null
        SV-->>LF: ParticipantNotFound例外
        LF-->>AG: 404 Not Found
        AG-->>FE: エラーレスポンス
    end
    
    alt 参加者が非アクティブ
        SV->>SV: IsActiveチェック
        SV-->>LF: ParticipantInactive例外
        LF-->>AG: 403 Forbidden
        AG-->>FE: エラーレスポンス
    end
    
    RP-->>SV: 参加者エンティティ
    SV->>SV: 名前更新処理
    SV->>RP: UpdateAsync(updatedParticipant)
    RP->>DB: DynamoDB UpdateItem
    
    alt DynamoDB更新エラー
        DB-->>RP: エラー
        RP-->>SV: 例外
        SV-->>LF: DatabaseException
        LF-->>AG: 500 Internal Server Error
        AG-->>FE: エラーレスポンス
    end
    
    DB-->>RP: 更新成功
    RP-->>SV: 成功
    SV-->>LF: 成功レスポンス
    LF-->>AG: 200 OK
    note over LF,AG: {"success": true}
    AG-->>FE: 成功レスポンス
```

## データ処理フロー

### 1. 入力データの流れ
```mermaid
flowchart LR
    A[HTTPリクエスト] --> B[リクエストDTO]
    B --> C[ParticipantNameバリューオブジェクト]
    C --> D[Participantエンティティ更新]
    D --> E[DynamoDBエンティティ]
    E --> F[DynamoDB保存]
```

### 2. バリデーション処理フロー
```mermaid
flowchart TD
    A[リクエスト受信] --> B{HTTPバリデーション}
    B -->|失敗| C[400 Bad Request]
    B -->|成功| D{参加者存在確認}
    D -->|不存在| E[404 Not Found]
    D -->|存在| F{参加者状態確認}
    F -->|非アクティブ| G[403 Forbidden]
    F -->|アクティブ| H{名前バリデーション}
    H -->|失敗| I[400 Bad Request]
    H -->|成功| J[名前更新処理]
    J --> K[200 OK]
```

### 3. エラーハンドリングフロー
```mermaid
flowchart TD
    A[例外発生] --> B{例外タイプ判定}
    B -->|ArgumentException| C[400 Bad Request]
    B -->|ParticipantNotFoundException| D[404 Not Found]
    B -->|ParticipantInactiveException| E[403 Forbidden]
    B -->|DynamoDBException| F[500 Internal Server Error]
    B -->|その他| G[500 Internal Server Error]
    
    C --> H[エラーレスポンス生成]
    D --> H
    E --> H
    F --> H
    G --> H
    H --> I[クライアントへ返却]
```