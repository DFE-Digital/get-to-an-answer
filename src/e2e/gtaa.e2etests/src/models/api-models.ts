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
    content: string;
    description: string;
    destination: string;
    destinationType: AnswerDestinationType;
    weight: number;
}