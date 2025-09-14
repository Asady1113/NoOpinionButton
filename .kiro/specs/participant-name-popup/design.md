# Design Document

## Overview

The participant name popup feature will be implemented as a modal component that appears on the participant page immediately after sign-in. The design follows the existing Vue 3 + Nuxt 3 architecture with Tailwind CSS styling, maintaining consistency with the current codebase patterns.

## Architecture

### Component Structure (Atomic Design)

```
participant.vue (page/template)
├── ParticipantNameModal.vue (organism)
│   ├── ModalOverlay.vue (molecule)
│   ├── NameInputForm.vue (molecule)
│   │   ├── TextInput.vue (atom)
│   │   ├── ErrorMessage.vue (atom)
│   │   └── SubmitButton.vue (atom)
│   └── ModalHeader.vue (molecule)
│       └── Title.vue (atom)
└── existing page content
```

### Composables Structure

```
composables/
├── signIn/ (existing)
│   ├── useSignIn.ts
│   └── useSignInApi.ts
└── participantName/ (new)
    └── useParticipantNameApi.ts
```

### State Management

The popup will use local component state for:
- Input field value
- Loading state
- Error state
- Popup visibility

The participant ID will be retrieved from the existing `useSignInStore()` composable.

## Components and Interfaces

### Atoms

**TextInput.vue**
```typescript
interface Props {
  modelValue: string
  placeholder?: string
  maxLength?: number
  disabled?: boolean
}
```

**SubmitButton.vue**
```typescript
interface Props {
  disabled?: boolean
  loading?: boolean
  text: string
}
```

**ErrorMessage.vue**
```typescript
interface Props {
  message?: string
  visible?: boolean
}
```

**Title.vue**
```typescript
interface Props {
  text: string
  level?: 'h1' | 'h2' | 'h3'
}
```

### Molecules

**NameInputForm.vue**
```typescript
interface Props {
  participantId: string
}

interface Emits {
  success: []
  error: [message: string]
}
```

**ModalHeader.vue**
```typescript
interface Props {
  title: string
}
```

**ModalOverlay.vue**
```typescript
interface Props {
  visible: boolean
}

interface Emits {
  close: []
}
```

### Organisms

**ParticipantNameModal.vue**
```typescript
interface Props {
  isVisible: boolean
  participantId: string
}

interface Emits {
  close: []
}
```

### useParticipantNameApi Composable

**Interface:**
```typescript
interface ParticipantNameApi {
  updateParticipantName: (participantId: string, name: string) => Promise<UpdateNameResponse>
}

interface UpdateNameRequest {
  name: string
}

interface UpdateNameResponse {
  Data: {
    updatedName: string
  } | null
  Error: string | null
}
```

## Data Models

### API Request/Response Models

```typescript
// Request payload for PUT /participants/{participantId}/name
interface UpdateParticipantNameRequest {
  name: string
}

// API response structure
interface UpdateParticipantNameResponse {
  Data: {
    updatedName: string
  } | null
  Error: string | null
}
```

### Validation Model

```typescript
interface NameValidation {
  isValid: boolean
  errorMessage?: string
}

// Validation rules:
// - Required: not empty, null, or undefined
// - Length: 1-50 characters
// - Content: not only whitespace characters
```

## Error Handling

### Validation Errors

Client-side validation will check:
1. **Empty input**: "名前を入力してください"
2. **Too long**: "名前は50文字以内で入力してください"
3. **Whitespace only**: "有効な名前を入力してください"

### API Errors

Following the existing error handling pattern:
- **400 Bad Request**: "入力内容に問題があります"
- **404 Not Found**: "参加者が見つかりません"
- **500 Server Error**: "サーバーエラーが発生しました"
- **Network Error**: "ネットワークエラーが発生しました"

### Error Display

Errors will be displayed:
- Below the input field for validation errors
- In a dedicated error section for API errors
- With appropriate styling (red text, error icons)

## Testing Strategy

### Unit Tests

**ParticipantNamePopup.vue:**
- Input field renders correctly
- Validation works for various input scenarios
- Loading state displays correctly
- Error messages display correctly
- Events are emitted properly

**useParticipantNameApi.ts:**
- API calls are made with correct parameters
- Response handling works correctly
- Error handling covers all scenarios
- Network errors are handled properly

### Integration Tests

**Participant Page Integration:**
- Popup appears on page load
- Popup closes after successful registration
- Sign-in store data is used correctly
- Page interaction is blocked while popup is open

### Test Scenarios

1. **Happy Path**: Enter valid name → API success → popup closes
2. **Validation Errors**: Empty input, too long input, whitespace only
3. **API Errors**: 400, 404, 500 responses
4. **Network Errors**: Timeout, connection failure
5. **Loading States**: Button disabled, loading indicator shown

## UI/UX Design

### Modal Layout

```
┌─────────────────────────────────────┐
│  Overlay (semi-transparent black)   │
│                                     │
│    ┌─────────────────────────────┐  │
│    │     名前を入力してください      │  │
│    │                             │  │
│    │  ┌─────────────────────────┐ │  │
│    │  │   [Text Input Field]    │ │  │
│    │  └─────────────────────────┘ │  │
│    │                             │  │
│    │     [Error Message Area]    │  │
│    │                             │  │
│    │         [決定 Button]        │  │
│    └─────────────────────────────┘  │
└─────────────────────────────────────┘
```

### Styling Specifications

**Modal Container:**
- Fixed positioning with z-index for overlay
- Centered horizontally and vertically
- Semi-transparent black background (bg-black bg-opacity-50)
- Backdrop blur effect

**Modal Content:**
- White background with rounded corners
- Drop shadow for depth
- Padding for comfortable spacing
- Maximum width with responsive design

**Input Field:**
- Border with focus states
- Placeholder text: "お名前を入力してください"
- Character counter (optional)

**Button:**
- Primary button styling consistent with existing design
- Disabled state when loading or invalid input
- Loading spinner when API call in progress

### Responsive Design

- **Desktop**: Modal width 400px, centered
- **Tablet**: Modal width 90% max 400px
- **Mobile**: Modal width 95% with adjusted padding

## Implementation Flow

### Page Load Sequence

1. Participant page loads
2. Check if participant ID exists in sign-in store
3. If participant ID exists, show popup immediately
4. Initialize popup component with participant ID

### User Interaction Flow

1. User sees popup with input field focused
2. User types name (real-time validation feedback)
3. User clicks "決定" button
4. Button becomes disabled, loading indicator appears
5. API call is made
6. On success: popup closes, user can interact with page
7. On error: error message shown, button re-enabled

### State Transitions

```
Initial → Input → Validating → Submitting → Success/Error
   ↓        ↓         ↓           ↓           ↓
 Show    Enable    Validate    Disable    Close/Show Error
 Popup   Input     Input       Button     
```

## Security Considerations

### Input Sanitization

- Client-side validation for length and content
- Server-side validation handled by existing API
- No XSS vulnerabilities (Vue's automatic escaping)

### API Security

- Use existing API endpoint with proper error handling
- No sensitive data stored in component state
- Participant ID validation on server side

## Performance Considerations

### Component Loading

- Popup component loaded only when needed
- Minimal bundle size impact
- No external dependencies required

### API Calls

- Single API call per registration attempt
- Proper error handling to avoid unnecessary retries
- Loading states to provide user feedback

### Memory Management

- Component cleanup when popup closes
- No memory leaks from event listeners
- Proper reactive state management