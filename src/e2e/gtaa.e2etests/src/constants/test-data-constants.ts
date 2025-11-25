// ===== API related test data =====
export enum QuestionType {
    SingleSelect = 1,
    MultiSelect = 2,
    DropdownSelect = 3
}

export enum AnswerDestinationType {
    Question = 1,
    ExternalLink = 2,
    CustomContent
}

export enum EntityStatus {
    Draft = 1,
    Published = 2,
    Deleted = 3,
    Archived = 4,
    Private = 5
}

export enum QuestionnaireAction {
    Publish,
    Unpublish
}

export enum QuestionAction {
    MoveUp,
    MoveDown,
}

export const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

// ===== Admin related test data =====
export const ErrorMessages = {
    ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE: 'Enter a questionnaire title',
    ERROR_MESSAGE_INVALID_TITLE: 'Enter a valid title',
    
    ERROR_MESSAGE_TERMS_OF_USE: 'You need to accept the agreement to continue',
    ERROR_MESSAGE_MISSING_QUESTION_TEXT: 'Enter a question text',
    ERROR_MESSAGE_MISSING_QUESTION_TYPE: 'Enter a question type',
}