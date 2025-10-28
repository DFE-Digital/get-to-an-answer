import {QuestionType, AnswerDestinationType} from '../constants/test-data-constants'

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
    questionnaireId: string;
    content: string;
    description: string;
    destinationUrl: string;
    destinationType: AnswerDestinationType;
    score: number;
}