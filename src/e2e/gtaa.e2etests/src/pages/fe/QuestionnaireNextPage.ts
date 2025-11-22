import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {QuestionType} from "../admin/AddQuestionPage";

type DestinationType = 'Question' | 'CustomContent' | 'ExternalLink';

export class QuestionnaireNextPage extends BasePage {
    // ===== Locators =====
    private pageHeading(): Locator {
        return this.page.locator('#main-content-header h1.govuk-heading-xl');
    }

    private questionHeading(): Locator {
        return this.page.locator('fieldset.govuk-fieldset h1.govuk-fieldset__heading');
    }

    private questionDescription(): Locator {
        return this.page.locator('.govuk-hint');
    }

    private radioOption(answerId: string): Locator {
        return this.page.locator(`input[type="radio"][value="${answerId}"]`);
    }

    private checkboxOption(answerId: string): Locator {
        return this.page.locator(`input[type="checkbox"][value="${answerId}"]`);
    }

    private dropdownSelect(): Locator {
        return this.page.locator('select.govuk-select[name="NextStateRequest.SelectedAnswerIds"]');
    }

    private answerLabel(answerId: string): Locator {
        return this.page.locator(`label[for*="${answerId}"]`);
    }

    private answerHint(answerId: string): Locator {
        return this.page.locator(`#q-*-a-${answerId}-hint`);
    }

    private continueButton(): Locator {
        return this.page.locator('button[type="submit"].govuk-button');
    }

    private errorSummary(): Locator {
        return this.page.locator('.govuk-error-summary');
    }

    private fieldError(): Locator {
        return this.page.locator('.govuk-error-message');
    }

    private formGroupError(): Locator {
        return this.page.locator('.govuk-form-group--error');
    }

    private customContent(): Locator {
        return this.page.locator('.govuk-grid-column-full p');
    }

    private externalLinkInput(): Locator {
        return this.page.locator('#external-link-dest');
    }

    private hiddenField(name: string): Locator {
        return this.page.locator(`input[type="hidden"][name="${name}"]`);
    }

    constructor(page: Page) {
        super(page);
    }

    // ===== Navigation Actions =====
    async goto(questionnaireSlug: string, embed?: boolean) {
        const embedParam = embed ? `?embed=${embed}` : '';
        await this.page.goto(`/questionnaires/${questionnaireSlug}/next${embedParam}`);
    }

    async expectOnPage() {
        await expect(this.pageHeading()).toBeVisible();
    }

    async expectQuestionPage(expectedQuestion?: string) {
        if (expectedQuestion) {
            await expect(this.questionHeading()).toHaveText(expectedQuestion);
        } else {
            await expect(this.questionHeading()).toBeVisible();
        }
    }

    // ===== Question Content Actions =====
    async getPageHeading(): Promise<string> {
        return await this.pageHeading().textContent() || '';
    }

    async getQuestionText(): Promise<string> {
        return await this.questionHeading().textContent() || '';
    }

    async getQuestionDescription(): Promise<string> {
        return await this.questionDescription().textContent() || '';
    }

    async expectQuestionDescription() {
        await expect(this.questionDescription()).toBeVisible();
    }

    // ===== Single Select Actions =====
    async selectRadioOption(answerId: string) {
        await this.radioOption(answerId).check();
    }

    async expectRadioOptionSelected(answerId: string) {
        await expect(this.radioOption(answerId)).toBeChecked();
    }

    async expectRadioOptionNotSelected(answerId: string) {
        await expect(this.radioOption(answerId)).not.toBeChecked();
    }

    async isRadioOptionVisible(answerId: string): Promise<boolean> {
        return await this.radioOption(answerId).isVisible();
    }

    // ===== Multi Select Actions =====
    async checkCheckboxOption(answerId: string) {
        await this.checkboxOption(answerId).check();
    }

    async uncheckCheckboxOption(answerId: string) {
        await this.checkboxOption(answerId).uncheck();
    }

    async expectCheckboxOptionChecked(answerId: string) {
        await expect(this.checkboxOption(answerId)).toBeChecked();
    }

    async expectCheckboxOptionNotChecked(answerId: string) {
        await expect(this.checkboxOption(answerId)).not.toBeChecked();
    }

    async checkMultipleCheckboxes(answerIds: string[]) {
        for (const answerId of answerIds) {
            await this.checkCheckboxOption(answerId);
        }
    }

    // ===== Dropdown Select Actions =====
    async selectDropdownOption(answerId: string) {
        await this.dropdownSelect().selectOption(answerId);
    }

    async selectDropdownOptionByText(text: string) {
        await this.dropdownSelect().selectOption({label: text});
    }

    async getSelectedDropdownValue(): Promise<string> {
        return await this.dropdownSelect().inputValue();
    }

    async expectDropdownOptionSelected(answerId: string) {
        await expect(this.dropdownSelect()).toHaveValue(answerId);
    }

    async getDropdownOptions(): Promise<string[]> {
        const options = await this.dropdownSelect().locator('option').all();
        const optionTexts: string[] = [];
        for (const option of options) {
            const text = await option.textContent();
            if (text) optionTexts.push(text);
        }
        return optionTexts;
    }

    // ===== Answer Label & Hint Actions =====
    async getAnswerLabelText(answerId: string): Promise<string> {
        return await this.answerLabel(answerId).textContent() || '';
    }

    async expectAnswerLabel(answerId: string) {
        await expect(this.answerLabel(answerId)).toBeVisible();
    }

    async getAnswerHintText(answerId: string): Promise<string> {
        return await this.answerHint(answerId).textContent() || '';
    }

    async expectAnswerHint(answerId: string) {
        await expect(this.answerHint(answerId)).toBeVisible();
    }

    // ===== Form Submission Actions =====
    async clickContinue() {
        await this.continueButton().click();
    }

    async submitForm() {
        await this.clickContinue();
    }

    // ===== Error Handling Actions =====
    async expectErrorSummary() {
        await expect(this.errorSummary()).toBeVisible();
    }

    async expectNoErrors() {
        await expect(this.errorSummary()).not.toBeVisible();
    }

    async expectFieldError() {
        await expect(this.fieldError()).toBeVisible();
    }

    async getFieldErrorMessage(): Promise<string> {
        return await this.fieldError().textContent() || '';
    }

    async expectFormGroupError() {
        await expect(this.formGroupError()).toBeVisible();
    }

    async getErrorSummaryMessages(): Promise<string[]> {
        const errorLinks = await this.errorSummary().locator('a').all();
        const messages: string[] = [];
        for (const link of errorLinks) {
            const text = await link.textContent();
            if (text) messages.push(text);
        }
        return messages;
    }

    // ===== Custom Content Actions =====
    async expectCustomContent() {
        await expect(this.customContent()).toBeVisible();
    }

    async getCustomContentText(): Promise<string> {
        return await this.customContent().textContent() || '';
    }

    // ===== External Link Actions =====
    async getExternalLinkDestination(): Promise<string> {
        return await this.externalLinkInput().inputValue();
    }

    async expectExternalLinkDestination() {
        await expect(this.externalLinkInput()).toBeVisible();
    }

    // ===== Hidden Field Actions =====
    async getHiddenFieldValue(name: string): Promise<string> {
        return await this.hiddenField(name).inputValue();
    }

    async expectHiddenField(name: string, expectedValue: string) {
        await expect(this.hiddenField(name)).toHaveValue(expectedValue);
    }

    // ===== Combined Workflow Actions =====
    async answerSingleSelectQuestion(answerId: string) {
        await this.selectRadioOption(answerId);
        await this.clickContinue();
    }

    async answerMultiSelectQuestion(answerIds: string[]) {
        await this.checkMultipleCheckboxes(answerIds);
        await this.clickContinue();
    }

    async answerDropdownQuestion(answerId: string) {
        await this.selectDropdownOption(answerId);
        await this.clickContinue();
    }

    // ===== Validation Actions =====
    async expectQuestionType(type: QuestionType) {
        switch (type) {
            case 'SingleSelect':
                await expect(this.page.locator('.govuk-radios')).toBeVisible();
                break;
            case 'MultiSelect':
                await expect(this.page.locator('.govuk-checkboxes')).toBeVisible();
                break;
            case 'DropdownSelect':
                await expect(this.dropdownSelect()).toBeVisible();
                break;
        }
    }

    async countRadioOptions(): Promise<number> {
        return await this.page.locator('.govuk-radios__item').count();
    }

    async countCheckboxOptions(): Promise<number> {
        return await this.page.locator('.govuk-checkboxes__item').count();
    }

    async getAllRadioOptionIds(): Promise<string[]> {
        const radios = await this.page.locator('input[type="radio"][name="NextStateRequest.SelectedAnswerIds"]').all();
        const ids: string[] = [];
        for (const radio of radios) {
            const value = await radio.getAttribute('value');
            if (value) ids.push(value);
        }
        return ids;
    }

    async getAllCheckboxOptionIds(): Promise<string[]> {
        const checkboxes = await this.page.locator('input[type="checkbox"][name="NextStateRequest.SelectedAnswerIds"]').all();
        const ids: string[] = [];
        for (const checkbox of checkboxes) {
            const value = await checkbox.getAttribute('value');
            if (value) ids.push(value);
        }
        return ids;
    }
}