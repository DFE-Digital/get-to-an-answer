import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";

type Mode = 'create' | 'edit';

export enum QuestionType {
    SingleSelect = 'SingleSelect',
    DropdownSelect = 'DropdownSelect',
    MultiSelect = 'MultiSelect'
}

export class AddQuestionPage extends BasePage {
    private readonly radioName = 'Type';

    // ===== Locators =====
    private readonly form: Locator;
    private readonly backLink: Locator;
    private readonly questionInput: Locator;
    private readonly hintTextarea: Locator;
    private readonly typeSingleShort: Locator;
    private readonly typeSingleLong: Locator;
    private readonly typeMulti: Locator;
    private readonly saveButton: Locator;
    private readonly radios: Locator;
    private readonly radiosFormGroup: Locator;
    private readonly errorSummary: Locator;
    private readonly errorSummaryLinks: Locator;
    private readonly questionInputFormGroup: Locator;
    private readonly questionInputError: Locator;
    private readonly descriptionFormGroup: Locator;
    private readonly descriptionError: Locator;
    private readonly radiosError: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly acceptedErrorLink: Locator;
    private readonly titleFormGroup: Locator;

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        
        this.form = this.page.locator('form[action$="/questions/create"][method="post"]');
        this.titleFormGroup = page.locator('.govuk-form-group:has(#forms-name-input-name-field)');
        this.backLink = this.page.locator('a.govuk-back-link');
        this.questionInput = this.form.locator('input[name="content"][type="text"]');
        this.hintTextarea = this.form.locator('textarea[name="description"]');

        this.typeSingleShort = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.SingleSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(0));

        this.typeSingleLong = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.DropdownSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));

        this.typeMulti = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.MultiSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).last());

        this.saveButton = this.form.locator('button.govuk-button[type="submit"]');
        this.radios = this.form.locator('input[type="radio"][name="Type"]');
        this.radiosFormGroup = this.form.locator('.govuk-form-group:has(input[type="radio"][name="Type"])');
        
        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');
        this.acceptedErrorLink = this.errorList.locator('a[href="#Accepted"]');
        this.errorSummaryLinks = this.errorSummary.locator('.govuk-error-summary__list a');
        this.questionInputFormGroup = this.form.locator('.govuk-form-group:has(input[name="content"])');
        this.questionInputError = this.questionInputFormGroup.locator('.govuk-error-message');
        this.descriptionFormGroup = this.form.locator('.govuk-form-group:has(textarea[name="description"])');
        this.descriptionError = this.descriptionFormGroup.locator('.govuk-error-message');
        this.radiosError = this.radiosFormGroup.locator('.govuk-error-message');
    }

    // ===== Validations =====
    async assertLoaded(): Promise<void> {
        await expect(this.form, '❌ Form not visible').toBeVisible();
        await expect(this.questionInput, '❌ Question input not visible').toBeVisible();
        await expect(this.hintTextarea, '❌ Hint textarea not visible').toBeVisible();

        // Ensure at least two radios exist for the "Type" group
        expect(await this.radios.count()).toBeGreaterThan(1);
        await expect(this.saveButton, '❌ Save button not visible').toBeVisible();
    }

    // ===== Validations =====
    async verifyBackLinkPresent(): Promise<void> {
        await expect(this.backLink, '❌ Back link not visible').toBeVisible();
    }

    async verifyLabelAndHintPresent(): Promise<void> {
        await expect(this.backLink, '❌ Back link not visible').toBeVisible();
        await expect(this.radios, '❌ Type radios not visible').toBeVisible();
    }

    // TBC, for multiple errors, need to check the error summary and the error links
    async validateMissingfAllErrorMessageSummary() {
    }
    
    async validateMissingQuestionTextErrorMessageSummary() {
        await expect(this.errorSummary, '❌ Error summary not visible').toBeVisible();
        await expect(this.errorSummary, '❌ Error summary missing role="alert"').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Error summary missing tabindex="-1"').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList, '❌ Missing error in the error summary list')
            .toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_TEXT);
        await this.acceptedErrorLink.click();

        //TBC, not sure about DOM yet
        // await expect(this.questionInput, '❌ Agree checkbox not focused')
        //     .toBeFocused();
    }
    
    async validateMissingQuestionTypeErrorMessageSummary() {
    }

    async validateInlineQuestionTextError(): Promise<void> {
    }

    async validateInlineQuestionTypeError(): Promise<void> {
    }

    async validateQuestionTextErrorAriaDescribedBy() {
    }
    async validateQuestionTypeErrorAriaDescribedBy() {
    }

    async validateQuestionTextFormGroup() {
    }

    async validateQuestionTypeFormGroup() {

    }

    async validateRadiosError(): Promise<void> {
        await expect(this.radiosError, '❌ Radios error not visible').toBeVisible();
        const ariaDescribedBy = await this.radios.first().getAttribute('aria-describedby');
        expect(ariaDescribedBy, '❌ Radios missing aria-describedby attribute').not.toBeNull();
    }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyLabelAndHintPresent();

        await expect(this.form, '❌ Form not visible').toBeVisible();
        await expect(this.questionInput, '❌ Question input not visible').toBeVisible();
        await expect(this.hintTextarea, '❌ Hint textarea not visible').toBeVisible();
        await expect(this.saveButton, '❌ Save button not visible').toBeVisible();
    }

    // ===== Actions =====
    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backLink.click()
        ]);
    }

    async enterQuestion(text: string): Promise<void> {
        await this.questionInput.fill(text);
    }

    async clearQuestion(): Promise<void> {
        await this.questionInput.clear();
    }

    async enterHint(text: string): Promise<void> {
        await this.hintTextarea.fill(text);
    }

    async clearHint(): Promise<void> {
        await this.hintTextarea.clear();
    }

    async chooseType(type: QuestionType): Promise<void> {
        switch (type) {
            case QuestionType.SingleSelect:
                await this.typeSingleShort.check();
                break;
            case QuestionType.DropdownSelect:
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