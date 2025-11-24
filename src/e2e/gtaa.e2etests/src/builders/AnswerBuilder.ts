import type {AnswerModel} from '../models/api-models';
import {AnswerDestinationType} from '../constants/test-data-constants';

export class AnswerBuilder {
    private _questionId: string;
    private _questionnaireId: string;
    private _content: string;
    private _description: string;
    private _destinationUrl: string;
    private _destinationType?: AnswerDestinationType;
    private _destinationQuestionId: string;
    private _destinationContentId: string;
    private _score: number;

    constructor(questionId: string, questionnaireId: string) {
        const timestamp = Date.now();
        this._questionId = questionId;
        this._questionnaireId = questionnaireId;
        this._content = `Default answer content - ${timestamp}`;
        this._description = `Default answer description - ${timestamp}`;
        this._destinationUrl = `https://example.com/destination-url-${timestamp}`;
        this._destinationType = undefined; // this and '_destinationQuestionId' need to both be defined
        this._destinationContentId = '';
        this._destinationQuestionId = '';
        this._score = 0.0; //set to default
    }
    
    withDestinationQuestionId(destinationQuestionId?: string) {
        if (destinationQuestionId !== undefined)
            this._destinationQuestionId = destinationQuestionId;
        return this;
    }

    withDestinationContentId(destinationContentId: string | undefined) {
        if (destinationContentId !== undefined)
            this._destinationContentId = destinationContentId;
        return this;
    }
    withContent(content?: string) {
        if (content !== undefined)
            this._content = content;
        return this;
    }

    withDescription(description?: string) {
        if (description !== undefined)
            this._description = description;
        return this;
    }

    withDestinationUrl(destinationUrl?: string) {
        if (destinationUrl !== undefined)
            this._destinationUrl = destinationUrl;
        return this;
    }

    withDestinationType(destinationType?: AnswerDestinationType) {
        if (destinationType !== undefined)
            this._destinationType = destinationType;
        return this;
    }
    
    withScore(priority?: number) {
        if (priority !== undefined)
            this._score = priority;
        return this;
    }

    build(): AnswerModel {
        const answer: AnswerModel = {
            questionId: this._questionId,
            questionnaireId: this._questionnaireId,
            content: this._content,
            description: this._description,
            destinationUrl: this._destinationUrl,
            destinationType: this._destinationType!,
            priority: this._score
        };

        if (this._destinationQuestionId && this._destinationQuestionId.trim() !== '') {
            answer.destinationQuestionId = this._destinationQuestionId;
        }

        if (this._destinationContentId && this._destinationContentId.trim() !== '') {
            answer.destinationContentId = this._destinationContentId;
        }
        return answer;
    }
}