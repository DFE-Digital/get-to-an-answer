import type {QuestionnaireModel} from '../models/api-models';

export class QuestionnaireBuilder {
    private _title = 'Default questionnaire title';
    private _description = 'Default questionnaire description';
    private _slug = 'Default questionnaire slug';

    withTitle(title: string) {
        this._title = title;
        return this;
    }

    withDescription(description: string) {
        this._description = description;
        return this;
    }

    withSlug(slug: string) {
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