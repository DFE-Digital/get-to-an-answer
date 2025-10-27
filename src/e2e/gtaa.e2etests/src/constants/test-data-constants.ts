// Define prefixes to categorize questionnaire topics
export enum QuestionnairePrefix {
    SERVICE_ELIGIBILITY = 'Service eligibility questionnaire',
}

// Define prefixes to categorize question topics
export enum QuestionPrefix {
    AGE = 'Age',
    GENDER = 'Gender',
}

// Define question types
export enum QuestionType {
    SINGLE = 1,
    MULTIPLE = 2,
    DROPDOWN = 3
}

// Define destination types
export enum AnswerDestinationType {
    QUESTION = 'question',
    PAGE = 'page',
    EXTERNAL = 'external'
}

export const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;