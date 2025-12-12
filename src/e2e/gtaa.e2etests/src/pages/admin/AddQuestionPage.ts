import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {EditAnswerTable} from "./components/EditAnswerTable";
import {Timeouts} from '../../constants/timeouts'

type Mode = 'create' | 'update';

export enum QuestionRadioLabel {
    SingleSelectShort = 'SingleSelect',
    SingleSelectLong = 'DropdownSelect',
    MultiSelect = 'Multiselect'
}

export class AddQuestionPage extends BasePage {
    private readonly mode: string;
    private readonly radioName = 'QuestionType';

    // ===== Locators =====
    private readonly form: Locator;
    private readonly addQuestionHeading: Locator;
    private readonly backLink: Locator;
    private readonly questionInput: Locator;
    private readonly hintTextarea: Locator;
    private readonly typeSingleShort: Locator;
    private readonly typeSingleLong: Locator;
    private readonly typeMulti: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly radios: Locator;
    private readonly radiosFormGroup: Locator;
    private readonly errorSummary: Locator;
    private readonly errorSummaryLinks: Locator;
    private readonly questionInputFormGroup: Locator;
    private readonly questionInputError: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorLinkQuestionContent: Locator;
    private readonly errorLinkQuestionType: Locator;
    private readonly questionContentFormGroup: Locator;
    private readonly inlineQuestionContentError: Locator;
    private readonly inlineUpdateQuestionContentError: Locator;
    private readonly inlineQuestionTypeError: Locator;
    private readonly fieldset: Locator;
    private readonly saveQuestionButton: Locator;
    private readonly deleteQuestionButton: Locator;
    private readonly addQuestionButton: Locator;
    private readonly backToQuestionsLink: Locator;
    private readonly successBanner: Locator;
    private readonly successBannerHeading: Locator;

    readonly table: EditAnswerTable;

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        this.mode = mode;

        this.form = this.page.locator('#main-content form[method="post"]');
        this.addQuestionHeading = this.page.locator('main h1.govuk-heading-l');
        this.questionContentFormGroup = page.locator('.govuk-form-group:has(#QuestionContent)');
        this.backLink = this.page.locator('a.govuk-back-link');

        this.questionInput = this.form.locator('input[name="QuestionContent"][type="text"]');
        this.hintTextarea = this.form.locator('textarea[name="QuestionHintText"]');

        // Radio buttons
        this.typeSingleShort = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionRadioLabel.SingleSelectShort}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(0));

        this.typeSingleLong = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionRadioLabel.SingleSelectLong}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));

        this.typeMulti = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionRadioLabel.MultiSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).last());

        // Save button
        this.saveAndContinueButton = this.form.locator('button.govuk-button[type="submit"]');

        // All radios in the QuestionType group
        this.radios = this.form.locator('input[type="radio"][name="QuestionType"]');

        // Form group around the radios
        this.radiosFormGroup = this.form.locator(
            '.govuk-form-group:has(#QuestionType-error)'
        );

        // Error summary
        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');
        this.errorLinkQuestionContent = this.page.locator('a[href="#QuestionContent"]');
        this.errorLinkQuestionType = this.page.locator('a[href="#QuestionType"]');
        this.errorSummaryLinks = this.errorSummary.locator('.govuk-error-summary__list a');

        this.questionInputFormGroup = this.form.locator(
            '.govuk-form-group:has(input[name="QuestionContent"])'
        );
        this.questionInputError = this.questionInputFormGroup.locator('.govuk-error-message');

        this.inlineQuestionContentError = this.questionContentFormGroup.locator(
            '#QuestionContent-error'
        );
        this.inlineUpdateQuestionContentError = this.questionContentFormGroup.locator(
            '#QuestionContent-error'
        );

        this.inlineQuestionTypeError = this.radiosFormGroup.locator('#QuestionType-error');
        this.fieldset = this.form.locator('fieldset[aria-describedby*="QuestionType-hint"]');

        this.saveQuestionButton = page.getByRole('button', {name: /save question/i});
        this.deleteQuestionButton = page.getByRole('button', {name: /delete question/i});

        // success banner
        this.successBanner = page.locator('.govuk-notification-banner--success');
        this.successBannerHeading = this.successBanner.getByRole('heading', {name: /your changes have been saved/i});
        this.addQuestionButton = page.getByRole('button', {name: /add a question/i});
        this.backToQuestionsLink = page.getByRole('link', {name: /back to your questions/i});

        this.table = new EditAnswerTable(page);
    }

    // ===== Validations =====
    async expectAddQuestionHeadingOnPage(expectedText?: string): Promise<void> {
        await this.addQuestionHeading.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.addQuestionHeading, '❌ Add question heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.addQuestionHeading,
                `❌ Add question heading text mismatch: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async VerifyQuestionInputAndHintTextarea(): Promise<void> {
        await expect(this.questionInput, '❌ Question input not visible').toBeVisible();
        await expect(this.hintTextarea, '❌ Hint textarea not visible').toBeVisible();
    }

    async verifyBackLinkPresent(): Promise<void> {
        await expect(this.backLink, '❌ Back link not visible').toBeVisible();
    }

    async validateMissingAllFieldsErrorMessageSummary(browserName: string) {
        
        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        if (this.mode === 'update') {
            await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_CONTENT_OnUpdate);
        }else{
            await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_TYPE);    
        }
        await this.page.waitForTimeout(200);  // Increase to 200ms for Firefox
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();
        
        await this.clickAllLinksAndValidateFocus(browserName);
    }

    async validateMissingQuestionContentErrorMessageSummary(browserName: string) {
        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_CONTENT);

        await this.page.waitForTimeout(200);
        await this.clickErrorLinkAndValidateFocus(this.errorLinkQuestionContent, browserName);
    }

    async validateMissingQuestionTypeErrorMessageSummary(browserName: string) {
        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_TYPE);

        await this.page.waitForTimeout(200);
        await this.clickErrorLinkAndValidateFocus(this.errorLinkQuestionType, browserName);
    }

    async validateInlineQuestionContentError(): Promise<void> {
        await this.inlineQuestionContentError.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await expect(this.inlineQuestionContentError, '❌ Inline question content error not visible').toBeVisible();
    }

    async validateInlineQuestionTypeError(): Promise<void> {
        await expect(this.errorSummary).toBeVisible();
        await expect(this.inlineQuestionTypeError, '❌ Inline question type error not visible').toBeVisible();
    }
    
    async assertPageElements() {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await this.verifyBackLinkPresent();
        await this.VerifyQuestionInputAndHintTextarea();
        await expect(this.form, '❌ Form not visible').toBeVisible();
        expect(await this.radios.count()).toBeGreaterThan(1);
        if (this.mode === 'update') {
            await this.table.verifyVisible();
            await expect(this.saveQuestionButton, '❌ Save question not visible').toBeVisible();
            await expect(this.deleteQuestionButton, '❌ Delete question not visible').toBeVisible();
        } else {
            await expect(this.saveAndContinueButton, '❌ Save button not visible').toBeVisible();
        }
    }

    async validateSuccessBanner(): Promise<void> {
        await expect(this.successBanner, '❌ Success banner not visible').toBeVisible();
        await expect(this.successBanner, '❌ Banner role attribute missing').toHaveAttribute('role', 'alert');
        await expect(this.successBannerHeading, '❌ Banner heading incorrect').toBeVisible();
        await expect(this.successBannerHeading).toHaveText(/your changes have been saved/i);
    }

    // Accessibility
    async validateQuestionContentFieldAriaDescribedBy() {
        const errorElement = this.mode === 'update'
            ? this.inlineUpdateQuestionContentError
            : this.inlineQuestionContentError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.questionInput.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        expect(ariaValue, '❌ aria-describedby missing error id')
            .toContain('QuestionContent-error');
    }

    async validateQuestionTypeErrorAriaDescribedBy(): Promise<void> {
        const errorElement = this.inlineQuestionTypeError;
        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        await expect(this.fieldset).toHaveAttribute('aria-describedby');

        const ariaValue = await this.fieldset.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby missing').not.toBeNull();

        expect(ariaValue, '❌ aria-describedby missing error id')
            .toContain('QuestionType-error');
    }

    // ===== Actions =====
    async clickAllLinksAndValidateFocus(browserName: string): Promise<void> {
        const linkCount = await this.errorLinks.count();

        for (let i = 0; i < linkCount; i++) {
            const link = this.errorLinks.nth(i);
            const href = await link.getAttribute('href');
            const targetId = href?.replace('#', '');

            await link.click();

            if (browserName !== 'webkit' && targetId) {
                const targetInput = this.page.locator(`#${targetId}`);
                await expect(targetInput, `❌ Target input for link ${i + 1} not focused`).toBeFocused();
            }
        }
    }

    async clickErrorLinkAndValidateFocus(link: Locator, browserName: string): Promise<void> {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        const href = await link.getAttribute('href');
        const targetId = href?.replace('#', '');

        await link.click();

        if (browserName !== 'webkit' && targetId) {
            const targetInput = this.page.locator(`#${targetId}`);
            await expect(targetInput, '❌ Target input not focused').toBeFocused();
        }
    }

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.backLink.click(),
            await this.waitForPageLoad(),
        ]);
    }

    async enterQuestionContent(text: string): Promise<void> {
        await this.questionInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionInput.clear();
        await this.questionInput.fill(text);
    }

    async getQuestionContent(): Promise<string> {
        await this.questionInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        const content = await this.questionInput.evaluate(el => (el as HTMLInputElement).value);
        return content.trim();
    }

    async clearQuestionContent(): Promise<void> {
        await this.questionInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionInput.clear();
    }

    async enterQuestionHintText(text: string): Promise<void> {
        await this.hintTextarea.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.hintTextarea.clear();
        await this.hintTextarea.fill(text);
    }

    async getQuestionHintText(): Promise<string> {
        await this.hintTextarea.waitFor({state: 'visible', timeout: Timeouts.LONG});
        const hintText = await this.hintTextarea.evaluate(el => (el as HTMLTextAreaElement).value);
        return hintText.trim();
    }

    async clearQuestionHintText(): Promise<void> {
        await this.hintTextarea.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.hintTextarea.clear();
    }

    async enterInvalidContent(): Promise<void> {
        await this.questionInput.clear();
        await this.questionInput.fill(`${' '.repeat(10)}`);
    }

    async chooseQuestionType(type: QuestionRadioLabel): Promise<void> {
        switch (type) {
            case QuestionRadioLabel.SingleSelectShort:
                await this.typeSingleShort.check();
                break;
            case QuestionRadioLabel.SingleSelectLong:
                await this.typeSingleLong.check();
                break;
            case QuestionRadioLabel.MultiSelect:
                await this.typeMulti.check();
                break;
        }
    }

    async isQuestionTypeSelected(type: QuestionRadioLabel): Promise<boolean> {
        switch (type) {
            case QuestionRadioLabel.SingleSelectShort:
                return await this.typeSingleShort.isChecked();
            case QuestionRadioLabel.SingleSelectLong:
                return await this.typeSingleLong.isChecked();
            case QuestionRadioLabel.MultiSelect:
                return await this.typeMulti.isChecked();
            default:
                throw new Error(`Unknown question type: ${type}`);
        }
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    async clickSaveQuestion(): Promise<void> {
        await this.saveQuestionButton.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.saveQuestionButton.click();
        await this.waitForPageLoad();
    }

    async clickDeleteQuestion(): Promise<void> {
        await this.deleteQuestionButton.click();
    }

    async clickAddQuestionInSuccessBanner(): Promise<void> {
        await expect(this.addQuestionButton).toBeVisible();
        await this.addQuestionButton.click();
    }

    async clickBackToYourQuestionsInSuccessBanner(): Promise<void> {
        await expect(this.backToQuestionsLink).toBeVisible();
        await this.backToQuestionsLink.click();
    }
}