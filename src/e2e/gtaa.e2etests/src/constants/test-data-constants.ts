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

    ERROR_MESSAGE_DUPLICATE_QUESTIONNAIRE_SLUG: 'A questionnaire with this slug already exists',
    ERROR_MESSAGE_EMPTY_QUESTIONNAIRE_SLUG: 'Enter a questionnaire ID',
    ERROR_MESSAGE_INVALID_QUESTIONNAIRE_SLUG: 'Questionnaire IDs must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen',
    
    ERROR_MESSAGE_TERMS_OF_USE: 'You need to accept the agreement to continue',
    
    ERROR_MESSAGE_MISSING_QUESTION_CONTENT: 'Enter a question',
    ERROR_MESSAGE_MISSING_QUESTION_CONTENT_OnUpdate: 'The question is required',
    ERROR_MESSAGE_MISSING_QUESTION_TYPE: 'Select question type',

    ERROR_MESSAGE_MISSING_RESULTS_PAGE_TITLE: 'Enter a title',
    ERROR_MESSAGE_MISSING_RESULTS_PAGE_DETAILS: 'Enter some details',

    ERROR_MESSAGE_MISSING_OPTION_CONTENT: 'The OptionContent field is required.',

    ERROR_MESSAGE_TOP_QUESTION_UP: 'You cannot move this question further up',
    ERROR_MESSAGE_BOTTOM_QUESTION_DOWN: 'You cannot move this question further down',
    ERROR_MESSAGE_DISPLAY_TITLE_REQUIRED: 'Display title is required when you save',
    ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED: 'Description is required when you save',

    ERROR_MESSAGE_MISSING_ANSWER_OPTION1_CONTENT: 'Option 1 content is required',
    ERROR_MESSAGE_MISSING_ANSWER_OPTION2_CONTENT: 'Option 2 content is required',

    ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION1_CONTENT: 'Option 1 content is duplicated',
    ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION2_CONTENT: 'Option 2 content is duplicated',

    ERROR_MESSAGE_MISSING_RESULTS_PAGE_ANSWER_OPTION1_SUMMARY: 'Please select a results page for option 1',
    ERROR_MESSAGE_MISSING_RESULTS_PAGE_ANSWER_OPTION2_SUMMARY: 'Please select a results page for option 2',
    ERROR_MESSAGE_MISSING_RESULTS_PAGE_ANSWER_INLINE: 'Please select a results page',
}

export const PageHeadings = {
    DESIGN_QUESTIONNAIRE_PAGE_HEADING: 'Design a questionnaire',
    EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING: 'Add or edit questionnaire ID',
    VIEW_QUESTION_PAGE_HEADING: 'Add and edit your questions',
    ADD_QUESTION_PAGE_HEADING: 'Add a question',
    EDIT_QUESTION_PAGE_HEADING: 'Edit question',
    ADD_ANSWER_PAGE_HEADING: 'Create a list of answer options',
    
    VIEW_RESULTS_PAGES_PAGE_HEADING: 'Add or edit results pages',
    ADD_RESULTS_PAGE_PAGE_HEADING: 'Add a results page',
    EDIT_RESULTS_PAGE_PAGE_HEADING: 'Edit your results page',
    
    VIEW_VERSION_HISTORY_PAGE_HEADING: 'View version history',
}

export const SuccessBannerMessages = {
    DELETED_RESULTS_PAGE_SUCCESS_MESSAGE: 'Your results page has been deleted.',
    CREATED_RESULTS_PAGE_SUCCESS_MESSAGE: "The results page '**resultsPageTitle**' has been saved",
    UPDATED_RESULTS_PAGE_SUCCESS_MESSAGE: "Your changes to the results page '**resultsPageTitle**' has been saved",
    UPDATED_GENERIC_BANNER_SUCCESS_MESSAGE: 'Your changes have been saved.',

    DELETED_QUESTIONNAIRE_SUCCESS_MESSAGE: "Your questionnaire '**name**' has been deleted.",
}

export const TaskStatus = {
     CANNOT_VIEW_HISTORY: 'Cannot view until published'
}