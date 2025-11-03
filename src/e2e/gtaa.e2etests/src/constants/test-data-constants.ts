// Define question types
export enum QuestionType {
    SINGLE = 1,
    MULTIPLE = 2,
    DROPDOWN = 3
}

// Define destination types
export enum AnswerDestinationType {
    Question = 1,
    ExternalLink = 2
}

export enum EntityStatus {
    Draft = 1,
    Published = 2,
    Deleted = 3,
    Archived = 4,
}

export const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;