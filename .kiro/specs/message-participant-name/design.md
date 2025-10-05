# 設計書

## 概要

この設計では、MessageエンティティにParticipantNameプロパティを追加し、メッセージの作成・配信時に参加者名を含めるように既存のAPIを拡張します。既存のClean Architectureパターンを維持しながら、ドメインエンティティ、インフラストラクチャ、アプリケーションサービス、Lambda関数の各層を適切に修正します。

## アーキテクチャ

### 現在のアーキテクチャ概要

```
┌─────────────────────────────────────────────────────────────┐
│                    Lambda Functions                         │
│  ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│  │ PostMessage     │  │ MessageBroadcast                    │ │
│  │ Function        │  │ Function                            │ │
│  └─────────────────┘  └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                Application Layer                            │
│  ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│  │ MessageService  │  │ BroadcastService                    │ │
│  └─────────────────┘  └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                  Domain Layer                               │
│  ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│  │ Message Entity  │  │ Participant Entity                  │ │
│  │ + ParticipantName│  │                                    │ │
│  └─────────────────┘  └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│              Infrastructure Layer                           │
│  ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│  │ MessageRepository│  │ ParticipantRepository               │ │
│  │ + ParticipantName│  │                                    │ │
│  └─────────────────┘  └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    DynamoDB                                 │
│  ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│  │ Message Table   │  │ Participant Table                   │ │
│  │ + ParticipantName│  │                                    │ │
│  └─────────────────┘  └─────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 変更対象コンポーネント

1. **Domain Layer**
   - `Message` エンティティ: `ParticipantName` プロパティ追加
   
2. **Application Layer**
   - `MessageService`: 参加者名取得ロジック追加
   
3. **Infrastructure Layer**
   - `MessageEntity`: `ParticipantName` フィールド追加
   - `MessageRepository`: マッピング処理更新
   
4. **Lambda Functions**
   - `MessageBroadcastFunction`: 参加者名配信ロジック追加

## コンポーネントとインターフェース

### ドメインエンティティの変更

**Message.cs**
```csharp
public class Message
{
    public MessageId Id { get; private set; }
    public MeetingId MeetingId { get; private set; }
    public ParticipantId ParticipantId { get; private set; }
    public ParticipantName ParticipantName { get; private set; } // 新規追加
    public DateTime CreatedAt { get; private set; }
    public MessageContent Content { get; private set; }
    public LikeCount LikeCount { get; private set; }
    public ReportedCount ReportedCount { get; private set; }
    public bool IsActive { get; private set; }

    // コンストラクタにParticipantNameパラメータを追加
    public Message(MessageId id, MeetingId meetingId, ParticipantId participantId, 
                  ParticipantName participantName, MessageContent content, 
                  DateTime? createdAt = null, LikeCount likeCount = default, 
                  ReportedCount reportedCount = default, bool isActive = true)
}
```

### アプリケーションサービスの変更

**MessageService.cs**
```csharp
public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IParticipantRepository _participantRepository; // 新規追加

    public async Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request)
    {
        // 参加者名を取得
        var participant = await _participantRepository.GetByIdAsync(request.ParticipantId);
        if (participant == null)
        {
            throw new ArgumentException($"参加者が見つかりません: {request.ParticipantId}");
        }

        // メッセージエンティティ作成（参加者名を含む）
        var message = new Message(
            IdGenerator.GenerateGuid(),
            request.MeetingId,
            request.ParticipantId,
            participant.Name, // 参加者名を設定
            request.Content,
            DateTime.UtcNow
        );

        var savedMessage = await _messageRepository.SaveAsync(message);
        return new PostMessageServiceResponse { MessageId = savedMessage.Id };
    }
}
```

### インフラストラクチャの変更

**MessageEntity.cs**
```csharp
[DynamoDBTable("Message")]
public class MessageEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string MeetingId { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string ParticipantId { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string ParticipantName { get; set; } = string.Empty; // 新規追加
    
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
    
    [DynamoDBProperty]
    public string Content { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public int LikeCount { get; set; } = 0;
    
    [DynamoDBProperty]
    public int ReportedCount { get; set; } = 0;
    
    [DynamoDBProperty]
    public bool IsActive { get; set; } = true;
}
```

**MessageRepository.cs**
```csharp
public class MessageRepository : IMessageRepository
{
    public async Task<Message> SaveAsync(Message message)
    {
        var messageEntity = new MessageEntity
        {
            Id = message.Id,
            MeetingId = message.MeetingId,
            ParticipantId = message.ParticipantId,
            ParticipantName = message.ParticipantName, // 新規追加
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            LikeCount = message.LikeCount,
            ReportedCount = message.ReportedCount,
            IsActive = message.IsActive
        };

        await _context.SaveAsync(messageEntity);

        return new Message(
            messageEntity.Id,
            messageEntity.MeetingId,
            messageEntity.ParticipantId,
            messageEntity.ParticipantName, // 新規追加
            messageEntity.Content,
            messageEntity.CreatedAt,
            messageEntity.LikeCount,
            messageEntity.ReportedCount,
            messageEntity.IsActive
        );
    }
}
```

### Lambda関数の変更

**MessageBroadcastFunction.cs**
```csharp
private async Task ProcessMessageInsert(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
{
    try
    {
        var newImage = record.Dynamodb.NewImage;
        
        var messageId = newImage["Id"].S;
        var meetingId = newImage["MeetingId"].S;
        var participantId = newImage["ParticipantId"].S;
        var participantName = newImage["ParticipantName"].S; // 新規追加
        var content = newImage["Content"].S;
        var createdAt = DateTime.Parse(newImage["CreatedAt"].S);
        var likeCount = int.Parse(newImage["LikeCount"].N);
        var reportedCount = int.Parse(newImage["ReportedCount"].N);
        var isActive = int.Parse(newImage["IsActive"].N);

        // メッセージをJSON形式にシリアライズ（参加者名を含む）
        var messageJson = JsonSerializer.Serialize(new
        {
            messageId,
            meetingId,
            participantId,
            participantName, // 新規追加
            content,
            createdAt,
            likeCount,
            reportedCount,
            isActive
        });

        await _broadcastService.BroadcastMessageToMeetingAsync(meetingId, messageJson);
    }
    catch (Exception ex)
    {
        context.Logger.LogLine($"Error processing message insert: {ex}");
        throw;
    }
}
```

## データモデル

### DynamoDBスキーマ変更

**Message テーブル**
```json
{
  "TableName": "Message",
  "KeySchema": [
    {
      "AttributeName": "Id",
      "KeyType": "HASH"
    }
  ],
  "AttributeDefinitions": [
    {
      "AttributeName": "Id",
      "AttributeType": "S"
    }
  ],
  "StreamSpecification": {
    "StreamEnabled": true,
    "StreamViewType": "NEW_AND_OLD_IMAGES"
  }
}
```

**新規フィールド**
- `ParticipantName` (String): 参加者の表示名

### データ互換性

既存のMessageレコードには`ParticipantName`フィールドが存在しないため、後方互換性を考慮：

- 既存レコードの`ParticipantName`が存在しない場合は空文字列として扱う
- 新しいメッセージから`ParticipantName`を含める

## エラーハンドリング

### MessageServiceでのエラーハンドリング

```csharp
public async Task<PostMessageServiceResponse> PostMessageAsync(PostMessageServiceRequest request)
{
    try
    {
        // 参加者名を取得
        var participant = await _participantRepository.GetByIdAsync(request.ParticipantId);
        if (participant == null)
        {
            throw new ArgumentException($"参加者が見つかりません: {request.ParticipantId}");
        }

        // メッセージ作成処理...
    }
    catch (ArgumentException)
    {
        // 参加者が見つからない場合は再スロー
        throw;
    }
    catch (Exception ex)
    {
        // その他のエラーはログ出力して再スロー
        // ログ処理...
        throw;
    }
}
```

### MessageBroadcastFunctionでのエラーハンドリング

```csharp
private async Task ProcessMessageInsert(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
{
    try
    {
        var newImage = record.Dynamodb.NewImage;
        
        // ParticipantNameが存在しない場合のフォールバック
        var participantName = newImage.ContainsKey("ParticipantName") 
            ? newImage["ParticipantName"].S 
            : "不明なユーザー";

        // 処理続行...
    }
    catch (KeyNotFoundException ex)
    {
        context.Logger.LogLine($"Required field missing in DynamoDB record: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        context.Logger.LogLine($"Error processing message insert: {ex}");
        throw;
    }
}
```

## テスト戦略

### 単体テスト

**MessageEntityTests.cs**
```csharp
[Test]
public void Message_Constructor_WithParticipantName_ShouldSetProperty()
{
    // Arrange
    var participantName = new ParticipantName("テストユーザー");
    
    // Act
    var message = new Message(
        messageId, meetingId, participantId, participantName, content);
    
    // Assert
    Assert.AreEqual(participantName, message.ParticipantName);
}

[Test]
public void Message_Constructor_WithInvalidParticipantName_ShouldThrowException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentException>(() => 
        new Message(messageId, meetingId, participantId, "", content));
}
```

**MessageServiceTests.cs**
```csharp
[Test]
public async Task PostMessageAsync_WithValidParticipant_ShouldIncludeParticipantName()
{
    // Arrange
    var participant = new Participant(participantId, "テストユーザー", meetingId, 0);
    _participantRepository.Setup(x => x.GetByIdAsync(participantId))
                         .ReturnsAsync(participant);
    
    // Act
    var result = await _messageService.PostMessageAsync(request);
    
    // Assert
    _messageRepository.Verify(x => x.SaveAsync(
        It.Is<Message>(m => m.ParticipantName == "テストユーザー")), Times.Once);
}

[Test]
public async Task PostMessageAsync_WithNonExistentParticipant_ShouldThrowException()
{
    // Arrange
    _participantRepository.Setup(x => x.GetByIdAsync(participantId))
                         .ReturnsAsync((Participant?)null);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => _messageService.PostMessageAsync(request));
    Assert.Contains("参加者が見つかりません", exception.Message);
}
```

**MessageBroadcastFunctionTests.cs**
```csharp
[Test]
public async Task ProcessMessageInsert_WithParticipantName_ShouldIncludeInBroadcast()
{
    // Arrange
    var record = CreateDynamoDBRecord(includeParticipantName: true);
    
    // Act
    await _function.FunctionHandler(new DynamoDBEvent { Records = { record } }, _context);
    
    // Assert
    _broadcastService.Verify(x => x.BroadcastMessageToMeetingAsync(
        It.IsAny<string>(), 
        It.Is<string>(json => json.Contains("participantName"))), Times.Once);
}

[Test]
public async Task ProcessMessageInsert_WithoutParticipantName_ShouldHandleGracefully()
{
    // Arrange
    var record = CreateDynamoDBRecord(includeParticipantName: false);
    
    // Act & Assert
    await _function.FunctionHandler(new DynamoDBEvent { Records = { record } }, _context);
    
    // 例外が発生しないことを確認
    _broadcastService.Verify(x => x.BroadcastMessageToMeetingAsync(
        It.IsAny<string>(), It.IsAny<string>()), Times.Once);
}
```



## パフォーマンス考慮事項

### データベースアクセス最適化

1. **参加者名取得**: MessageService内で参加者情報を取得する際のパフォーマンス影響を最小化
2. **キャッシュ戦略**: 頻繁にアクセスされる参加者情報のキャッシュを検討
3. **バッチ処理**: 複数メッセージの一括処理時の最適化

### DynamoDB最適化

1. **読み取り容量**: 参加者名取得による追加の読み取り操作
2. **書き込み容量**: ParticipantNameフィールド追加による書き込みサイズ増加
3. **ストリーム処理**: MessageBroadcastFunctionでの追加フィールド処理

## セキュリティ考慮事項

### データ検証

1. **ParticipantName検証**: 既存のParticipantNameValueObjectの検証ルールを活用
2. **入力サニタイゼーション**: XSS攻撃防止のための適切なエスケープ処理
3. **権限チェック**: 参加者が存在し、アクティブであることの確認

### データ保護

1. **個人情報**: 参加者名の適切な取り扱い
2. **ログ出力**: 個人情報を含むログの適切な管理
3. **エラーメッセージ**: 機密情報の漏洩防止

## 実装フロー

### 実装順序

1. **ドメインエンティティの更新**: MessageエンティティにParticipantNameプロパティを追加
2. **インフラストラクチャ層の更新**: MessageEntity、MessageRepositoryの更新
3. **アプリケーションサービスの更新**: MessageServiceに参加者名取得ロジックを追加
4. **Lambda関数の更新**: MessageBroadcastFunctionに参加者名配信ロジックを追加
5. **テストの更新**: 各層のテストを新しいプロパティに対応