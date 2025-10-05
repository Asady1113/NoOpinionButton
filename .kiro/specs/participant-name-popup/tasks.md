# Implementation Plan

- [x] 1. Create API composable for participant name registration
  - Create useParticipantNameApi.ts composable following existing API patterns
  - Implement updateParticipantName function with proper error handling
  - Add TypeScript interfaces for request/response types
  - _Requirements: 3.1, 3.2, 3.3, 5.1, 5.5_

- [x] 2. Create atomic components for the modal
- [x] 2.1 Create TextInput atom component
  - Implement reusable text input with v-model support
  - Add props for placeholder, maxLength, and disabled state
  - Include Tailwind CSS styling consistent with existing design
  - _Requirements: 2.1, 2.2, 2.3, 6.4_

- [x] 2.2 Create SubmitButton atom component
  - Implement button with loading and disabled states
  - Add loading spinner functionality
  - Include proper accessibility attributes
  - _Requirements: 3.4, 3.5, 6.4_

- [x] 2.3 Create ErrorMessage atom component
  - Implement error message display with conditional visibility
  - Add proper styling for error states
  - Include accessibility considerations for screen readers
  - _Requirements: 5.1, 5.4, 6.4_

- [x] 2.4 Create Title atom component
  - Implement reusable title component with heading levels
  - Add proper semantic HTML structure
  - Include responsive typography styling
  - _Requirements: 6.4, 6.5_

- [x] 3. Create molecular components
- [x] 3.1 Create NameInputForm molecule component
  - Combine TextInput, ErrorMessage, and SubmitButton atoms
  - Implement form validation logic for name input
  - Add API integration using useParticipantNameApi composable
  - Handle form submission and loading states
  - _Requirements: 2.4, 2.5, 3.1, 3.2, 3.3, 3.4, 3.5, 5.2, 5.3, 5.4_

- [x] 3.2 Create ModalHeader molecule component
  - Combine Title atom for modal header
  - Add consistent spacing and styling
  - Ensure responsive design
  - _Requirements: 6.4, 6.5_

- [x] 3.3 Create ModalOverlay molecule component
  - Implement modal backdrop with proper z-index
  - Add click-outside handling (optional for this use case)
  - Include backdrop blur and opacity effects
  - _Requirements: 1.4, 6.4, 6.5_

- [x] 4. Create ParticipantNameModal organism component
- [x] 4.1 Implement main modal organism
  - Combine ModalOverlay, ModalHeader, and NameInputForm molecules
  - Implement modal visibility logic and animations
  - Add proper modal accessibility (focus management, ARIA attributes)
  - Handle success and error events from child components
  - _Requirements: 1.1, 1.4, 4.1, 4.2, 4.3, 6.4, 6.5_

- [x] 5. Integrate modal into participant page
- [x] 5.1 Update participant.vue page component
  - Import and use ParticipantNameModal organism
  - Implement popup visibility logic based on sign-in state
  - Add participant ID retrieval from useSignInStore
  - Handle modal close events
  - _Requirements: 1.1, 3.2, 4.1, 4.2, 4.3_

- [x] 6. Add comprehensive unit tests
- [x] 6.1 Create tests for API composable
  - Test successful API calls with correct parameters
  - Test error handling for various HTTP status codes
  - Test network error scenarios
  - Mock fetch API and verify request structure
  - _Requirements: 3.1, 3.2, 3.3, 5.1, 5.5_

- [x] 6.2 Create tests for atomic components
  - Test TextInput component props, events, and validation
  - Test SubmitButton component states and interactions
  - Test ErrorMessage component visibility and content
  - Test Title component rendering and accessibility
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 3.4, 3.5, 5.1, 5.4_

- [x] 6.3 Create tests for molecular components
  - Test NameInputForm validation and API integration
  - Test ModalHeader rendering and styling
  - Test ModalOverlay behavior and events
  - Mock child components and verify interactions
  - _Requirements: 1.4, 2.4, 2.5, 3.1, 3.2, 3.3, 3.4, 3.5, 5.2, 5.3, 5.4_

- [x] 6.4 Create tests for organism component
  - Test ParticipantNameModal integration and state management
  - Test modal visibility and animation behavior
  - Test event handling and data flow
  - Test accessibility features and focus management
  - _Requirements: 1.1, 1.4, 4.1, 4.2, 4.3, 6.4, 6.5_

- [x] 6.5 Create integration tests for participant page
  - Test popup appearance on page load
  - Test complete user flow from input to success
  - Test error scenarios and recovery
  - Test interaction blocking while popup is open
  - _Requirements: 1.1, 1.4, 3.2, 4.1, 4.2, 4.3_

- [x] 7. Add validation and error handling
- [x] 7.1 Implement client-side validation
  - Add name length validation (1-50 characters)
  - Add empty input validation
  - Add whitespace-only validation
  - Provide real-time validation feedback
  - _Requirements: 2.3, 2.4, 2.5, 5.2, 5.4_

- [x] 7.2 Implement comprehensive error handling
  - Handle API error responses with appropriate messages
  - Handle network errors and timeouts
  - Provide user-friendly error messages in Japanese
  - Ensure error recovery and retry capability
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 8. Ensure responsive design and accessibility
- [ ] 8.1 Implement responsive modal design
  - Test modal layout on desktop, tablet, and mobile
  - Ensure proper spacing and sizing across screen sizes
  - Verify touch interactions work correctly on mobile
  - _Requirements: 6.5_

- [ ] 8.2 Add accessibility features
  - Implement proper ARIA labels and roles
  - Ensure keyboard navigation works correctly
  - Add focus management for modal interactions
  - Test with screen readers
  - _Requirements: 6.4, 6.5_