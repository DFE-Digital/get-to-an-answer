import type {QuestionModel} from '../models/api-models';
import {QuestionType} from '../constants/test-data-constants'

export class QuestionBuilder {
    private _questionnaireId: string;
    private _content: string
    private _description: string
    private _type: QuestionType
    private _prefix: string

    constructor(questionnaireId: string) {
        const timestamp = Date.now();
        this._questionnaireId = questionnaireId;
        this._content = `Default question content - ${timestamp}`;
        this._description = `Default question description - ${timestamp}`;
        this._type = QuestionType.SINGLE; //set to default
        this._prefix = 'Default question prefix';
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
            this._content = `${this._prefix} - ${this._content}`;
        return this;
    }

    withDescription(description?: string) {
        if (description !== undefined)
            this._description = description;
        return this;
    }

    withType(questionType?: QuestionType) {
        if (questionType !== undefined)
            this._type = questionType;
        return this;
    }

    build(): QuestionModel {
        return {
            questionnaireId: this._questionnaireId,
            content: this._content,
            description: this._description,
            type: this._type,
        };
    }
}