import type {QuestionnaireModel} from '../models/api-models';

export class QuestionnaireBuilder {
    private _title: string;
    private _description: string;
    private _slug: string;
    private _prefix:string;
    constructor() {
        const timestamp = Date.now();
        this._title = `Default questionnaire title - ${timestamp}`;
        this._description = `Default questionnaire description - ${timestamp}`;
        this._slug = `Default questionnaire slug - ${timestamp}`;
        this._prefix = 'Default prefix';
    }

    withTitlePrefix(prefix?: string) {
        if (prefix && prefix.trim().length > 0) 
            this._title = `${prefix} - ${this._title}`;
        else
            this._title = `${this._prefix} - ${this._title}`;
        return this;
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
            Title: this._title,
            Description: this._description,
            Slug: this._slug
        };
    }
}