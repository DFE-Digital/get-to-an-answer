import type {AnswerModel} from '../models/api-models';
import {AnswerDestinationType} from '../constants/test-data-constants';

export class AnswerBuilder {
    private _questionId: string;
    private _questionnaireId: string;
    private _content: string;
    private _description: string;
    private _destinationUrl: string;
    private _destinationType: AnswerDestinationType;
    private _score: number;
    // private _answerPrefix: string

    constructor(questionId: string, questionnaireId: string) {
        const timestamp = Date.now();
        this._questionId = questionId;
        this._questionnaireId = questionnaireId;
        this._content = `Default answer content - ${timestamp}`;
        this._description = `Default answer description - ${timestamp}`;
        this._destinationUrl = `https://example.com/destination-url-${timestamp}`;
        this._destinationType = AnswerDestinationType.QUESTION; //set to default
        this._score = 0.0; //set to default
        // this._answerPrefix = 'Default answer prefix';
    }

    withContent(content?: string) {
        if (content !== undefined)
            this._content = content;
        return this;
    }

    // withContentPrefix(prefix?: string) {
    //     if (prefix && prefix.trim().length > 0)
    //         this._content = `${prefix} - ${this._content}`;
    //     else
    //         this._content = `${this._answerPrefix} - ${this._content}`;
    //     return this;
    // }
    
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

    withScore(score?: number) {
        if (score !== undefined)
            this._score = score;
        return this;
    }

    build(): AnswerModel {
        return {
            questionId: this._questionId,
            questionnaireId: this._questionnaireId,
            content: this._content,
            description: this._description,
            destinationUrl: this._destinationUrl,
            destinationType: this._destinationType,
            score: this._score
        };
    }
}