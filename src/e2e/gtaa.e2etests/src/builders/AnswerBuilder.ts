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
            questionId: this._questionId,
            content: this._content,
            description: this._description,
            destination: this._destination,
            destinationType: this._destinationType,
            weight: this._weight
        };
    }
}