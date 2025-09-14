# Requirements Document

## Introduction

This feature implements a participant name registration popup that appears immediately after a participant logs into the participant page. The popup allows users to enter their display name and register it via the existing participant name registration API. The implementation should follow the existing frontend architecture patterns used in the NoOpinionButtonWeb application.

## Requirements

### Requirement 1

**User Story:** As a participant who has just logged into the participant page, I want to see a popup to enter my display name, so that I can register my name for the meeting.

#### Acceptance Criteria

1. WHEN a participant navigates to the participant page after successful sign-in THEN the system SHALL display a name registration popup immediately
2. WHEN the popup is displayed THEN the system SHALL show a text input field for entering the participant name
3. WHEN the popup is displayed THEN the system SHALL show a "決定" (Confirm) button to submit the name
4. WHEN the popup is displayed THEN the system SHALL prevent interaction with the underlying page content

### Requirement 2

**User Story:** As a participant entering my name, I want to input my display name in a text field, so that I can specify how I want to be identified in the meeting.

#### Acceptance Criteria

1. WHEN the participant clicks on the text input field THEN the system SHALL allow text input
2. WHEN the participant enters text THEN the system SHALL accept Unicode characters including Japanese characters and emojis
3. WHEN the participant enters text THEN the system SHALL enforce a maximum length of 50 characters
4. WHEN the participant enters only whitespace characters THEN the system SHALL treat this as invalid input
5. WHEN the participant leaves the field empty THEN the system SHALL treat this as invalid input

### Requirement 3

**User Story:** As a participant who has entered my name, I want to click a confirm button to register my name, so that my display name is saved and I can proceed to use the application.

#### Acceptance Criteria

1. WHEN the participant clicks the "決定" button with valid input THEN the system SHALL call the PUT /participants/{participantId}/name API endpoint
2. WHEN the API call is made THEN the system SHALL use the participant ID from the sign-in store
3. WHEN the API call is made THEN the system SHALL send the entered name in the request body
4. WHEN the API call is made THEN the system SHALL disable the button to prevent multiple submissions
5. WHEN the API call is in progress THEN the system SHALL show a loading indicator

### Requirement 4

**User Story:** As a participant whose name registration was successful, I want the popup to disappear automatically, so that I can proceed to use the main participant interface.

#### Acceptance Criteria

1. WHEN the API returns a successful response (200 OK) THEN the system SHALL close the popup
2. WHEN the popup closes THEN the system SHALL restore interaction with the underlying page content
3. WHEN the popup closes THEN the system SHALL not show the popup again during the current session

### Requirement 5

**User Story:** As a participant whose name registration failed, I want to see an error message and be able to retry, so that I can successfully register my name.

#### Acceptance Criteria

1. WHEN the API returns an error response THEN the system SHALL display an error message to the user
2. WHEN an error occurs THEN the system SHALL keep the popup open
3. WHEN an error occurs THEN the system SHALL re-enable the confirm button
4. WHEN an error occurs THEN the system SHALL allow the participant to modify their input and retry
5. WHEN a network error occurs THEN the system SHALL display an appropriate error message

### Requirement 6

**User Story:** As a developer maintaining the codebase, I want the popup implementation to follow existing architectural patterns, so that the code is consistent and maintainable.

#### Acceptance Criteria

1. WHEN implementing the popup THEN the system SHALL use Vue 3 Composition API consistent with existing components
2. WHEN implementing API calls THEN the system SHALL create a composable following the pattern of useSignInApi
3. WHEN implementing error handling THEN the system SHALL use the existing ApiError types and error handling patterns
4. WHEN implementing the popup THEN the system SHALL use Tailwind CSS classes consistent with the existing styling approach
5. WHEN implementing the popup THEN the system SHALL be responsive and work on mobile devices