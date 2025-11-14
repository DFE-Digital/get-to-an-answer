// ===== API related test data =====
export enum QuestionType {
    SINGLE = 1,
    MULTIPLE = 2,
    DROPDOWN = 3
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
    ERROR_MESSAGE_MISSING_TITLE: 'Enter a questionnaire title',
    ERROR_MESSAGE_INVALID_TITLE: 'Enter a valid title'
}