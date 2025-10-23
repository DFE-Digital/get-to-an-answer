import type {QuestionnaireModel} from '../models/api-models';

export class QuestionnaireBuilder {
    private _title: string;
    private _description: string;
    private _slug: string;
    
    constructor() {
        const timestamp = Date.now();
        this._title = `Default questionnaire title - ${timestamp}`;
        this._description = `Default questionnaire description - ${timestamp}`;
        this._slug = `Default questionnaire slug - ${timestamp}`;
    }
    
    withTitle(title?: string) {
        if (title !== undefined)
            this._title = title;
        return this;
    }

    withDescription(description?: string) {
        if (description !== undefined)
            this._description = description;
        return this;
    }

    withSlug(slug?: string) {
        if (slug !== undefined)
            this._slug = slug;
        return this;
    }

    build(): QuestionnaireModel {
        return {
            title: this._title,
            description: this._description,
            slug: this._slug
        };
    }
}