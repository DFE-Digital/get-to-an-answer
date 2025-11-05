import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit';

export enum QuestionType {
    SingleSelectShort = 'SingleSelect',
    SingleSelectLong = 'SingleSelectLong',
    MultiSelect = 'MultiSelect'
}

export class AddQuestionPage extends BasePage {
    private readonly radioName = 'Type';
    // ===== Locators =====
    private readonly form = this.page.locator('form[action$="/questions/create"][method="post"]');
    private readonly questionInput = this.form.locator('input[name="content"][type="text"]');
    private readonly hintTextarea = this.form.locator('textarea[name="description"]');

    private readonly typeSingleShort = this.form
        .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.SingleSelectShort}"]`)
        .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(0));

    private readonly typeSingleLong = this.form
        .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.SingleSelectLong}"]`)
        .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));
    private readonly typeMulti = this.form
        .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.MultiSelect}"]`)
        .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).last());

    private readonly saveButton = this.form.locator('button.govuk-button[type="submit"]');
    private readonly radios = this.form.locator('input[type="radio"][name="Type"]');

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
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