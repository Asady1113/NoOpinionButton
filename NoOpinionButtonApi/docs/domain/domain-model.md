# ドメインモデル（ER図）

## エンティティ関係図

```
┌─────────────────────┐
│      Meeting        │
├─────────────────────┤
│ Id: string          │
│ Name: string        │
│ FacilitatorPassword │
│ ParticipantPassword │
├─────────────────────┤
│ VerifyPassword()    │
└─────────────────────┘
           │
           │ 1:N
           ▼
┌─────────────────────┐
│     Participant     │
├─────────────────────┤
│ Id: string          │
│ Name: string        │
│ MeetingId: string   │
│ NoOpinionPoint: int │
│ HasOpinion: bool    │
│ IsActive: bool      │
├─────────────────────┤
│ (メソッドなし)      │
└─────────────────────┘
           │
           │ 1:N
           ▼
┌─────────────────────┐        ┌─────────────────────┐
│       Message       │        │     Connection      │
├─────────────────────┤        ├─────────────────────┤
│ Id: string          │        │ Id: string          │
│ MeetingId: string   │        │ ParticipantId: str  │
│ ParticipantId: str  │        │ MeetingId: string   │
│ CreatedAt: DateTime │        │ ConnectedAt: DateTime│
│ Content: string     │        │ IsActive: bool      │
│ LikeCount: int      │        └─────────────────────┘
│ ReportedCount: int  │                │
│ IsActive: bool      │                │
├─────────────────────┤                │ N:1
│ (メソッドなし)      │                │
└─────────────────────┘                │
           │                           │
           │ N:1                       │
           └───────────────────────────┘
```

## 関係性

- **Meeting → Participant**: 1:N
- **Participant → Message**: 1:N  
- **Participant → Connection**: 1:N
- **Meeting → Message**: 1:N (MeetingId経由)
- **Meeting → Connection**: 1:N (MeetingId経由)