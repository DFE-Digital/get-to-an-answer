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
    SINGLE = 'single',
    MULTIPLE = 'multiple'
}

// Define destination types
export enum AnswerDestinationType {
    QUESTION = 'question',
    PAGE = 'page',
    EXTERNAL = 'external'
}