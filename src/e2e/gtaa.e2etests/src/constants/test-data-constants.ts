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
    ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE: 'Enter a questionnaire title',
    ERROR_MESSAGE_INVALID_TITLE: 'Enter a valid title',
    
    ERROR_MESSAGE_TERMS_OF_USE: 'You need to accept the agreement to continue',

    ERROR_MESSAGE_MISSING_QUESTION_TYPE: 'The QuestionType field is required.',
    ERROR_MESSAGE_MISSING_QUESTION_CONTENT: 'The QuestionContent field is required.',

    ERROR_MESSAGE_MISSING_OPTION_CONTENT: 'The OptionContent field is required.',
}

export const PageHeadings = {
    VIEW_QUESTION_PAGE_HEADING: 'Add and edit your questions',
    ADD_QUESTION_PAGE_HEADING: 'Add a question',
    ADD_ANSWER_PAGE_HEADING: 'Create a list of answer options',
}