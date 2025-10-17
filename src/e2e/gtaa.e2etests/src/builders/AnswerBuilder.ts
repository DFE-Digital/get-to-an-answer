import type {AnswerModel, DestinationType} from '../models/api-models';

export class AnswerBuilder {
    private _questionId: string;
    private _content = 'Default answer content';
    private _description = 'Default answer description';
    private _destination = 'Default answer destination';
    private _destinationType: DestinationType = 'question';
    private _weight: number = 0.0;

    constructor(questionId: string) {
        this._questionId = questionId;
    }
    
    withContent(content: string) {
        this._content = content;
        return this;
    }

    withDescription(description: string) {
        this._description = description;
        return this;
    }

    withDestination(destination: string) {
        this._destination = destination;
        return this;
    }

    withDestinationType(destinationType: DestinationType) {
        this._destinationType = destinationType;
        return this;
    }

    withWeight(weight: number) {
        this._weight = weight;
        return this;
    }

    build(): AnswerModel {
        return {
            QuestionId: this._questionId,
            Content: this._content,
            Description: this._description,
            Destination: this._destination,
            DestinationType: this._destinationType,
            Weight: this._weight
        };
    }
}