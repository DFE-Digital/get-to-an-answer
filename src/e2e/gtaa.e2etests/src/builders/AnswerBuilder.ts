import type {AnswerModel} from '../models/api-models';
import {AnswerDestinationType} from '../constants/test-data-constants';

export class AnswerBuilder {
    private _questionId: string;
    private _content: string;
    private _description: string;
    private _destination: string;
    private _destinationType: AnswerDestinationType;
    private _weight: number;
    private _answerPrefix: string

    constructor(questionId: string) {
        const timestamp = Date.now();
        this._questionId = questionId;
        this._content = `Default answer content - ${timestamp}`;
        this._description = `Default answer description - ${timestamp}`;
        this._destination = `Default answer destination - ${timestamp}`;
        this._destinationType = AnswerDestinationType.QUESTION; //set to default
        this._weight = 0.0; //set to default
        this._answerPrefix = 'Default answer prefix';
    }

    withContent(content?: string) {
        if (content !== undefined)
            this._content = content;
        return this;
    }

    withContentPrefix(prefix?: string) {
        if (prefix && prefix.trim().length > 0)
            this._content = `${prefix} - ${this._content}`;
        else
            this._content = `${this._answerPrefix} - ${this._content}`;
        return this;
    }
    
    withDescription(description?: string) {
        if (description !== undefined)
            this._description = description;
        return this;
    }

    withDestination(destination?: string) {
        if (destination !== undefined)
            this._destination = destination;
        return this;
    }

    withDestinationType(destinationType?: AnswerDestinationType) {
        if (destinationType !== undefined)
            this._destinationType = destinationType;
        return this;
    }

    withWeight(weight?: number) {
        if (weight !== undefined)
            this._weight = weight;
        return this;
    }

    build(): AnswerModel {
        return {
            questionId: this._questionId,
            content: this._content,
            description: this._description,
            destination: this._destination,
            destinationType: this._destinationType,
            weight: this._weight
        };
    }
}