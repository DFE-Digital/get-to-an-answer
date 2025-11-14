import {QuestionType, AnswerDestinationType} from '../constants/test-data-constants'

export interface QuestionnaireModel {
    title:string;
    description:string;
    slug:string;
}

export interface QuestionnaireDtoModel {
    id: string;
    displayTitle: string;
    description: string;
    slug: string;
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
    destinationQuestionId?: string;
    content: string;
    description: string;
    destinationUrl: string;
    destinationType: AnswerDestinationType;
    priority: number;
}