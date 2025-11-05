import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit';

export enum QuestionType {
    SingleSelectShort = 'SingleSelect',
    SingleSelectLong = 'SingleSelectLong',
    MultiSelect = 'MultiSelect'
}

export class AddQuestionPage extends BasePage {
    // ----- form scope -----
    private readonly form: Locator;

    // fields
    private readonly questionInput: Locator;
    private readonly hintTextarea: Locator;
    private readonly typeSingleShort: Locator;
    private readonly typeSingleLong: Locator;
    private readonly typeMulti: Locator;
    private readonly radios: Locator;

    // actions
    private readonly saveButton: Locator;


    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        
        // The add-question form
        this.form = page.locator('form[action$="/questions/create"][method="post"]');

        // Inputs
        this.questionInput = this.form.locator('input[name="content"][type="text"]');
        this.hintTextarea = this.form.locator('textarea[name="description"]');

        // Radios 
        const radioName = 'Type';
        this.typeSingleShort = this.form
            .locator(`input[type="radio"][name="${radioName}"][value="${QuestionType.SingleSelectShort}"]`)
            .or(this.form.locator(`input[type="radio"][name="${radioName}"]`).nth(0));

        this.typeSingleLong = this.form
            .locator(`input[type="radio"][name="${radioName}"][value="${QuestionType.SingleSelectLong}"]`)
            .or(this.form.locator(`input[type="radio"][name="${radioName}"]`).nth(1));

        this.typeMulti = this.form
            .locator(`input[type="radio"][name="${radioName}"][value="${QuestionType.MultiSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${radioName}"]`).last());

        // Save
        this.saveButton = this.form.locator('button.govuk-button[type="submit"]');

        // Radios count
        this.radios = this.form.locator('input[type="radio"][name="Type"]');
    }

    // ===== Validations (structure-only) =====
    async assertLoaded(): Promise<void> {
        await expect(this.form).toBeVisible();
        await expect(this.questionInput).toBeVisible();
        await expect(this.hintTextarea).toBeVisible();

        // Ensure at least two radios exist for the "Type" group
        expect(await this.radios.count()).toBeGreaterThan(1);
        await expect(this.saveButton).toBeVisible();
    }

    // ===== Actions =====
    async enterQuestion(text: string): Promise<void> {
        await this.questionInput.fill(text);
    }

    async enterHint(text: string): Promise<void> {
        await this.hintTextarea.fill(text);
    }

    async chooseType(type: QuestionType): Promise<void> {
        switch (type) {
            case QuestionType.SingleSelectShort:
                await this.typeSingleShort.check();
                break;
            case QuestionType.SingleSelectLong:
                await this.typeSingleLong.check();
                break;
            case QuestionType.MultiSelect:
                await this.typeMulti.check();
                break;
        }
    }

    async save(): Promise<void> {
        await this.saveButton.click();
    }

    // Convenience helper for typical flow
    async createQuestion(
        question: string,
        type: QuestionType,
        hint?: string
    ): Promise<void> {
        await this.enterQuestion(question);
        if (hint !== undefined) await this.enterHint(hint);
        await this.chooseType(type);
        await this.save();
    }
}