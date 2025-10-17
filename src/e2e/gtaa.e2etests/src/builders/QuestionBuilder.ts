import type {QuestionModel, QuestionType} from '../models/api-models';

export class QuestionBuilder {
    private _questionnaireId: string;
    private _content = 'Default question';
    private _description = 'Default description';
    private _type: QuestionType = 'single';

    constructor(questionnaireId: string) {
        this._questionnaireId = questionnaireId;
    }
    
    withContent(content: string) {
        this._content = content;
        return this;
    }

    withDescription(description: string) {
        this._description = description;
        return this;
    }

    withType(questionType: QuestionType) {
        this._type = questionType;
        return this;
    }

    build(): QuestionModel {
        return {
            QuestionnaireId: this._questionnaireId,
            Content: this._content,
            Description: this._description,
            Type: this._type,
        };
    }
}