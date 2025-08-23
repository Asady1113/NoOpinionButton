# ドメインモデル改善提案書

## 概要
現在のCore層ドメインモデルをDDD（ドメイン駆動設計）の観点から分析し、より堅牢で保守性の高いドメインモデルへの改善提案を行います。

## 現在の問題点分析

### 1. エンティティ設計の問題

#### 1.1 貧血ドメインモデル（Anemic Domain Model）

**問題の詳細:**
- 全エンティティがデータホルダーとして機能している
- Meetingエンティティ以外にドメインロジックが存在しない
- publicセッターによる状態の無制御な変更が可能

**影響:**
```csharp
// 現状：ドメインルールを無視した変更が可能
var participant = new Participant();
participant.NoOpinionPoint = -10; // 負の値設定可能
participant.IsActive = false;
participant.HasOpinion = true; // 矛盾した状態設定可能
```

#### 1.2 不変条件（Invariants）の欠如

**問題の詳細:**
- エンティティの状態が常に有効であることが保証されない
- ビジネスルールがドメイン層外に散在している

**具体例:**
```csharp
// Participantエンティティの潜在的な問題
public class Participant
{
    // これらの状態変更にビジネスルールが適用されない
    public int NoOpinionPoint { get; set; } = 0;  // 負数防止なし
    public bool HasOpinion { get; set; } = true;  // 状態遷移ルールなし
    public bool IsActive { get; set; } = true;    // ライフサイクル管理なし
}
```

#### 1.3 エンティティ識別子の弱い型付け

**問題の詳細:**
- 全てのIDが`string`型で統一されている
- 異なるエンティティのIDを誤って使用するリスクがある

**リスク例:**
```csharp
// 現状：型安全性の欠如
var meetingId = "meeting-123";
var participantId = "participant-456";

// 誤ったIDの使用を防げない
await messageService.PostMessageAsync(new PostMessageServiceRequest 
{
    MeetingId = participantId,  // 間違ったIDの使用
    ParticipantId = meetingId   // コンパイル時に検出不可
});
```

### 2. 集約境界の問題

#### 2.1 集約設計の不備

**問題の詳細:**
- 各エンティティが独立した集約として扱われている
- ビジネス的に関連の深いエンティティ間の整合性が保証されない
- トランザクション境界が不明確

**現在の集約設計:**
```
Meeting集約 ←→ Participant集約 ←→ Message集約 ←→ Connection集約
     ↓              ↓               ↓              ↓
   独立運用      独立運用        独立運用       独立運用
```

#### 2.2 不適切な集約境界

**問題の詳細:**
- 意見なしボタン機能におけるParticipantとMeetingの密結合
- メッセージ投稿時の参加者状態確認の複雑性

### 3. ドメインサービスの不在

#### 3.1 複雑なビジネスロジックの散在

**問題の詳細:**
- 複数エンティティにまたがるビジネスルールがアプリケーション層に流出
- 「意見なしボタン」の中核ロジックがドメイン層に存在しない

**具体例:**
```csharp
// 意見なし状態の変更ロジックが欠如
public class Participant
{
    // このメソッドが存在しない
    // public void PressNoOpinionButton() { /* ビジネスルール適用 */ }
    // public void ExpressOpinion() { /* 状態遷移ロジック */ }
}
```

### 4. 値オブジェクトの活用不足

#### 4.1 プリミティブ型の過度な使用

**問題の詳細:**
- 重要な概念がプリミティブ型で表現されている
- パスワード、メッセージ内容、参加者名などの制約が型で表現されない

## 改善提案

### 1. エンティティリファクタリング

#### 1.1 不変条件の強化

**提案内容:**
```csharp
public class Participant
{
    private string _id;
    private string _name;
    private string _meetingId;
    private int _noOpinionPoint;
    private bool _hasOpinion;
    private bool _isActive;

    // コンストラクタでの初期化強制
    public Participant(ParticipantId id, MeetingId meetingId, ParticipantName name)
    {
        _id = id.Value ?? throw new ArgumentNullException();
        _meetingId = meetingId.Value ?? throw new ArgumentNullException();
        _name = name.Value ?? throw new ArgumentNullException();
        _noOpinionPoint = 0;
        _hasOpinion = true;
        _isActive = true;
    }

    // 読み取り専用プロパティ
    public string Id => _id;
    public string Name => _name;
    public string MeetingId => _meetingId;
    public int NoOpinionPoint => _noOpinionPoint;
    public bool HasOpinion => _hasOpinion;
    public bool IsActive => _isActive;

    // ドメインメソッドによる状態変更
    public void PressNoOpinionButton()
    {
        if (!_isActive) throw new InvalidOperationException("非アクティブな参加者は操作できません");
        if (_noOpinionPoint <= 0) throw new InvalidOperationException("意見なしポイントが不足しています");
        if (!_hasOpinion) throw new InvalidOperationException("既に意見なし状態です");

        _hasOpinion = false;
        _noOpinionPoint--;
    }

    public void ExpressOpinion()
    {
        if (!_isActive) throw new InvalidOperationException("非アクティブな参加者は操作できません");
        _hasOpinion = true;
    }

    public void Deactivate()
    {
        _isActive = false;
    }
}
```

#### 1.2 強い型付けされたID

**提案内容:**
```csharp
// 基底クラス
public abstract class EntityId<T> : IEquatable<EntityId<T>>
{
    public string Value { get; }
    
    protected EntityId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IDは空にできません", nameof(value));
        Value = value;
    }

    public bool Equals(EntityId<T> other) => Value == other?.Value;
    public override bool Equals(object obj) => Equals(obj as EntityId<T>);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(EntityId<T> left, EntityId<T> right) => Equals(left, right);
    public static bool operator !=(EntityId<T> left, EntityId<T> right) => !Equals(left, right);
}

// 具象実装
public class MeetingId : EntityId<MeetingId>
{
    public MeetingId(string value) : base(value) { }
}

public class ParticipantId : EntityId<ParticipantId>  
{
    public ParticipantId(string value) : base(value) { }
}

public class MessageId : EntityId<MessageId>
{
    public MessageId(string value) : base(value) { }
}

public class ConnectionId : EntityId<ConnectionId>
{
    public ConnectionId(string value) : base(value) { }
}
```

### 2. 値オブジェクトの導入

#### 2.1 ドメイン概念の型安全な表現

**提案内容:**
```csharp
// 参加者名
public class ParticipantName : IEquatable<ParticipantName>
{
    public string Value { get; }
    
    public ParticipantName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参加者名は必須です");
        if (value.Length > 50)
            throw new ArgumentException("参加者名は50文字以内で入力してください");
        Value = value.Trim();
    }

    public bool Equals(ParticipantName other) => Value == other?.Value;
    public override bool Equals(object obj) => Equals(obj as ParticipantName);
    public override int GetHashCode() => Value.GetHashCode();
}

// メッセージ内容
public class MessageContent : IEquatable<MessageContent>
{
    public string Value { get; }
    
    public MessageContent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("メッセージ内容は必須です");
        if (value.Length > 1000)
            throw new ArgumentException("メッセージは1000文字以内で入力してください");
        Value = value.Trim();
    }

    public bool Equals(MessageContent other) => Value == other?.Value;
    public override bool Equals(object obj) => Equals(obj as MessageContent);
    public override int GetHashCode() => Value.GetHashCode();
}

// パスワード
public class Password : IEquatable<Password>
{
    public string Value { get; }
    
    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("パスワードは必須です");
        if (value.Length < 4 || value.Length > 20)
            throw new ArgumentException("パスワードは4文字以上20文字以内で入力してください");
        Value = value;
    }

    public bool Equals(Password other) => Value == other?.Value;
    public override bool Equals(object obj) => Equals(obj as Password);
    public override int GetHashCode() => Value.GetHashCode();
}
```

### 3. 集約再設計

#### 3.1 Meeting集約の拡張

**提案内容:**
```csharp
public class Meeting
{
    private readonly MeetingId _id;
    private readonly MeetingName _name;
    private readonly Password _facilitatorPassword;
    private readonly Password _participantPassword;
    private readonly List<Participant> _participants;
    
    public Meeting(MeetingId id, MeetingName name, Password facilitatorPassword, Password participantPassword)
    {
        _id = id ?? throw new ArgumentNullException();
        _name = name ?? throw new ArgumentNullException();
        _facilitatorPassword = facilitatorPassword ?? throw new ArgumentNullException();
        _participantPassword = participantPassword ?? throw new ArgumentNullException();
        _participants = new List<Participant>();
        
        if (_facilitatorPassword.Equals(_participantPassword))
            throw new ArgumentException("司会者パスワードと参加者パスワードは異なる必要があります");
    }

    // 読み取り専用プロパティ
    public MeetingId Id => _id;
    public MeetingName Name => _name;
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    // ドメインメソッド
    public PasswordType VerifyPassword(Password password)
    {
        if (password.Equals(_facilitatorPassword)) return PasswordType.Facilitator;
        if (password.Equals(_participantPassword)) return PasswordType.Participant;
        return PasswordType.InvalidPassword;
    }

    public Participant AddParticipant(ParticipantId participantId, ParticipantName name)
    {
        if (_participants.Any(p => p.Id == participantId.Value))
            throw new InvalidOperationException("既に存在する参加者IDです");
            
        var participant = new Participant(participantId, _id, name);
        _participants.Add(participant);
        return participant;
    }

    public int GetActiveParticipantCount()
    {
        return _participants.Count(p => p.IsActive);
    }

    public int GetNoOpinionParticipantCount()
    {
        return _participants.Count(p => p.IsActive && !p.HasOpinion);
    }
}
```

### 4. ドメインサービスの導入

#### 4.1 意見なしボタンドメインサービス

**提案内容:**
```csharp
public interface INoOpinionDomainService
{
    /// <summary>
    /// 参加者が意見なしボタンを押下する
    /// </summary>
    Task ProcessNoOpinionButtonPress(MeetingId meetingId, ParticipantId participantId);
    
    /// <summary>
    /// 会議の意見なし状況を取得する
    /// </summary>
    Task<NoOpinionStatus> GetNoOpinionStatus(MeetingId meetingId);
}

public class NoOpinionDomainService : INoOpinionDomainService
{
    private readonly IMeetingRepository _meetingRepository;
    
    public NoOpinionDomainService(IMeetingRepository meetingRepository)
    {
        _meetingRepository = meetingRepository;
    }
    
    public async Task ProcessNoOpinionButtonPress(MeetingId meetingId, ParticipantId participantId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        var participant = meeting.Participants.FirstOrDefault(p => p.Id == participantId.Value);
        
        if (participant == null)
            throw new InvalidOperationException("参加者が見つかりません");
            
        participant.PressNoOpinionButton();
        
        // ドメインイベント発行
        participant.AddDomainEvent(new NoOpinionButtonPressedEvent(meetingId, participantId));
        
        await _meetingRepository.SaveAsync(meeting);
    }
    
    public async Task<NoOpinionStatus> GetNoOpinionStatus(MeetingId meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        
        return new NoOpinionStatus(
            meeting.GetActiveParticipantCount(),
            meeting.GetNoOpinionParticipantCount()
        );
    }
}
```

### 5. ドメインイベントの導入

#### 5.1 ドメインイベント基盤

**提案内容:**
```csharp
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class NoOpinionButtonPressedEvent : DomainEvent
{
    public MeetingId MeetingId { get; }
    public ParticipantId ParticipantId { get; }
    
    public NoOpinionButtonPressedEvent(MeetingId meetingId, ParticipantId participantId)
    {
        MeetingId = meetingId;
        ParticipantId = participantId;
    }
}

public class ParticipantJoinedEvent : DomainEvent
{
    public MeetingId MeetingId { get; }
    public ParticipantId ParticipantId { get; }
    public ParticipantName ParticipantName { get; }
    
    public ParticipantJoinedEvent(MeetingId meetingId, ParticipantId participantId, ParticipantName participantName)
    {
        MeetingId = meetingId;
        ParticipantId = participantId;
        ParticipantName = participantName;
    }
}
```

## リファクタリング計画

### フェーズ1: 基盤整備（2-3日）
1. 強い型付けされたIDクラスの導入
2. 値オブジェクトの実装
3. エンティティ基底クラスの作成

### フェーズ2: エンティティリファクタリング（3-4日）
1. Participantエンティティの不変条件強化
2. Meetingエンティティの集約化
3. ドメインメソッドの移行

### フェーズ3: ドメインサービス導入（2-3日）
1. NoOpinionDomainServiceの実装
2. 既存アプリケーションサービスの簡素化
3. ドメインイベントの統合

### フェーズ4: テスト強化（2-3日）
1. ドメインロジックの単体テスト追加
2. 不変条件のテストケース作成
3. 統合テストの更新

## 期待される効果

### 1. 保守性の向上
- ビジネスルールがドメイン層に集約される
- 型安全性によるバグの早期発見
- コードの意図が明確になる

### 2. テスタビリティの向上
- ドメインロジックの独立したテストが可能
- モック依存の削減
- より細かい粒度でのテスト作成

### 3. 拡張性の向上
- 新機能追加時の影響範囲の限定
- ビジネスルール変更への柔軟な対応
- ドメインイベントによる疎結合化

### 4. 可読性の向上
- ユビキタス言語の型による表現
- ビジネス意図の明確化
- 新メンバーのオンボーディング効率化

## 注意事項

### 1. 段階的な移行
- 既存の動作を保ちながら段階的にリファクタリング
- 後方互換性の維持
- 十分なテストによる安全性確保

### 2. パフォーマンス考慮
- 値オブジェクト生成のオーバーヘッド監視
- 必要に応じたキャッシュ戦略の検討
- メモリ使用量の最適化

### 3. チーム教育
- DDDの概念とパターンの共有
- 新しいコーディング規約の策定
- レビュープロセスの更新