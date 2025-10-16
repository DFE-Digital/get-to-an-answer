export type QuestionType = 'single' | 'multiple';
export type DestinationType = 'page' | 'question' | 'external';

export interface QuestionnaireModel {
    Title:string;
    Description:string;
    Slug:string;
}

export interface QuestionModel {
    QuestionnaireId:string;
    Content:string;
    Description:string;
    Type:QuestionType;
}

export interface AnswerModel {
    QuestionId: string;
    Content: string;
    Description: string;
    Destination: string;
    DestinationType: DestinationType;
    Weight: number;
}