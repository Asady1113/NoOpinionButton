# テストデータフロー図

## テスト実行フロー全体像

```mermaid
flowchart TD
    A[dotnet test 実行] --> B[xUnit Test Runner]
    B --> C[各テストクラス初期化]
    C --> D[Moq によるモック生成]
    D --> E[リフレクションによる依存注入]
    E --> F[テストメソッド実行]
    F --> G[アサーション検証]
    G --> H[Verify でモック呼び出し確認]
    H --> I[テスト結果出力]
```

## レイヤー別テストフロー

### Lambda Handler層テストフロー

```mermaid
sequenceDiagram
    participant T as Test Method
    participant F as Lambda Function
    participant M as Mocked Service
    participant A as Assertions
    
    T->>F: CreateFunctionWithMockedService()
    Note over F: リフレクションで依存注入
    T->>F: FunctionHandler(event, context)
    F->>M: Service.Method()
    M-->>F: Mocked Response
    F-->>T: APIGatewayProxyResponse
    T->>A: Assert.Equal(200, response.StatusCode)
    T->>M: Verify(service called once)
```

### Infrastructure層テストフロー

```mermaid
sequenceDiagram
    participant T as Test Method
    participant R as Repository
    participant M as Mocked AWS SDK
    participant A as Assertions
    
    T->>R: new Repository(mockedContext)
    T->>R: SaveAsync(entity)
    R->>M: _context.SaveAsync()
    M-->>R: Task.CompletedTask
    R-->>T: Saved Entity
    T->>A: Assert.NotNull(result)
    T->>M: Verify(SaveAsync called once)
```

### Core層テストフロー

```mermaid
sequenceDiagram
    participant T as Test Method
    participant S as Service
    participant M as Mocked Repository
    participant A as Assertions
    
    T->>S: new Service(mockedRepo)
    T->>S: BusinessMethod(request)
    S->>M: Repository.Method()
    M-->>S: Domain Entity
    S-->>T: Service Response
    T->>A: Assert.Equal(expected, actual)
    T->>M: Verify(repo called correctly)
```

## ValueObjects テストフロー

### 正常系テストパターン

```mermaid
flowchart LR
    A[テストデータ準備] --> B[ValueObject 作成]
    B --> C[値の検証]
    C --> D[暗黙的変換テスト]
    D --> E[ToString() テスト]
    E --> F[Equality テスト]
```

### 異常系テストパターン

```mermaid
flowchart LR
    A[無効データ準備] --> B[ValueObject 作成試行]
    B --> C[ArgumentException 発生]
    C --> D[例外メッセージ検証]
    D --> E[例外パラメータ名検証]
```

## DynamoDB Streams イベントテストフロー

```mermaid
sequenceDiagram
    participant T as Test Method
    participant F as MessageBroadcastFunction
    participant B as BroadcastService Mock
    participant E as DynamoDBEvent
    
    T->>E: CreateDynamoDBEventWithInsertRecord()
    T->>F: FunctionHandler(dynamoEvent, context)
    loop Each Record
        F->>F: Check EventName == "INSERT"
        F->>F: Extract Message Data
        F->>F: Serialize to JSON
        F->>B: BroadcastMessageToMeetingAsync()
        B-->>F: Task.CompletedTask
    end
    F-->>T: Task.CompletedTask
    T->>B: Verify(broadcast called once)
```

## WebSocket 接続テストフロー

```mermaid
sequenceDiagram
    participant T as Test Method
    participant F as WebSocketConnectFunction
    participant C as ConnectionService Mock
    participant R as APIGatewayProxyRequest
    
    T->>R: Create WebSocket Connect Request
    R->>R: Set connectionId, meetingId
    T->>F: FunctionHandler(request, context)
    F->>C: SaveConnectionAsync()
    C-->>F: Connection Entity
    F-->>T: 200 OK Response
    T->>C: Verify(connection saved)
```

## エラー処理テストフロー

```mermaid
flowchart TD
    A[異常系テスト開始] --> B[Mock で例外設定]
    B --> C[対象メソッド実行]
    C --> D{例外発生?}
    D -->|Yes| E[適切な例外タイプ確認]
    D -->|No| F[期待したエラーレスポンス確認]
    E --> G[ログ出力確認]
    F --> G
    G --> H[テスト完了]
```

## 境界値テストフロー

```mermaid
flowchart LR
    A[境界値データセット] --> B[最小値テスト]
    B --> C[最大値テスト]
    C --> D[最大値+1テスト]
    D --> E[空文字/null テスト]
    E --> F[結果検証]
```

## テスト並列実行フロー

```mermaid
graph TB
    A[Test Runner] --> B[LambdaHandlers.Tests]
    A --> C[Infrastructure.Tests]
    A --> D[Core.Tests]
    
    B --> B1[MessageBroadcast]
    B --> B2[PostMessage]
    B --> B3[WebSocketConnect]
    B --> B4[WebSocketDisconnect]
    
    C --> C1[BroadcastRepository]
    C --> C2[MessageRepository]
    C --> C3[ConnectionRepository]
    
    D --> D1[Services Tests]
    D --> D2[ValueObjects Tests]
    
    B1 --> E[Test Results]
    B2 --> E
    B3 --> E
    B4 --> E
    C1 --> E
    C2 --> E
    C3 --> E
    D1 --> E
    D2 --> E
```