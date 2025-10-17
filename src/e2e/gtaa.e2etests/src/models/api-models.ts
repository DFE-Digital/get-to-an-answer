export type QuestionType = 'single' | 'multiple';
export type DestinationType = 'page' | 'question' | 'external';

export interface QuestionnaireModel {
    title:string;
    description:string;
    slug:string;
}

export interface QuestionModel {
    questionnaireId:string;
    content:string;
    description:string;
    type:QuestionType;
}

export interface AnswerModel {
    questionId: string;
    content: string;
    description: string;
    destination: string;
    destinationType: DestinationType;
    weight: number;
}