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
│ (メソッドなし)        │
└─────────────────────┘
           │
           │ 1:N (Message), 1:1 (Connection)
           ▼
┌─────────────────────┐        ┌─────────────────────┐
│       Message       │        │     Connection      │
├─────────────────────┤        ├─────────────────────┤
│ Id: string          │        │ Id: string          │
│ MeetingId: string   │        │ ParticipantId: str  │
│ ParticipantId: str  │        │ MeetingId: string   │
│ ParticipantName: str│        │ ConnectedAt: DateTime│
│ CreatedAt: DateTime │        │ IsActive: bool      │
│ Content: string     │        └─────────────────────┘
│ LikeCount: int      │                
│ ReportedCount: int  │                
│ IsActive: bool      │                
├─────────────────────┤                
│ (メソッドなし)        │                
└─────────────────────┘                

```

## 関係性

- **Meeting → Participant**: 1:N
- **Participant → Message**: 1:N  
- **Participant → Connection**: 1:1