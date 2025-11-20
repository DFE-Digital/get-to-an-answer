import { expect, Locator, Page } from '@playwright/test';
import { BasePage } from '../BasePage';
import { ErrorMessages } from '../../constants/test-data-constants';
import {Timeouts} from "../../constants/timeouts";

type Mode = 'create' | 'update';

export enum QuestionType {
    SingleSelectShort = 'SingleSelect',
    SingleSelectLong  = 'DropdownSelect',
    MultiSelect       = 'Multiselect'
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
    private readonly saveButton: Locator;
    private readonly radios: Locator;
    private readonly radiosFormGroup: Locator;
    private readonly errorSummary: Locator;
    private readonly errorSummaryLinks: Locator;
    private readonly questionInputFormGroup: Locator;
    private readonly questionInputError: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorLinkQuestionContent: Locator;
    private readonly errorLinkQuestionText: Locator;
    private readonly errorLinkQuestionType: Locator;
    private readonly acceptedErrorLink: Locator;
    private readonly questionContentFormGroup: Locator;
    private readonly inlineQuestionContentError: Locator;
    private readonly inlineUpdateQuestionContentError: Locator;
    private readonly inlineQuestionTypeError: Locator;
    private readonly inlineUpdateQuestionTypeError: Locator;
    private readonly fieldset: Locator;
    
    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        this.mode = mode;

        // The only form under #main-content – method="post", accept-charset="UTF-8"
        this.form = this.page.locator('#main-content form[method="post"]');
        this.addQuestionHeading = this.page.locator('main h1.govuk-heading-l');
        this.questionContentFormGroup = page.locator('.govuk-form-group:has(#QuestionContent)');
        this.backLink = this.page.locator('a.govuk-back-link');

        // Question text input
        this.questionInput = this.form.locator('input[name="QuestionContent"][type="text"]');

        // Hint textarea
        this.hintTextarea = this.form.locator('textarea[name="QuestionHintText"]');

        // Radio buttons
        this.typeSingleShort = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.SingleSelectShort}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(0));

        this.typeSingleLong = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.SingleSelectLong}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));

        this.typeMulti = this.form
            .locator(`input[type="radio"][name="${this.radioName}"][value="${QuestionType.MultiSelect}"]`)
            .or(this.form.locator(`input[type="radio"][name="${this.radioName}"]`).last());

        // Save button
        this.saveButton = this.form.locator('button.govuk-button[type="submit"]');

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
        this.errorLinkQuestionText = this.page.locator('a[href="#QuestionHintText"]');
        this.errorLinkQuestionType = this.page.locator('a[href="#QuestionType"]');
        this.acceptedErrorLink = this.errorList.locator('a[href="#Accepted"]');
        this.errorSummaryLinks = this.errorSummary.locator('.govuk-error-summary__list a');
        
        this.questionInputFormGroup = this.form.locator(
            '.govuk-form-group:has(input[name="QuestionContent"])'
        );
        this.questionInputError = this.questionInputFormGroup.locator('.govuk-error-message');
        
        this.inlineQuestionContentError = this.questionContentFormGroup.locator(
            '#QuestionContent-error'
        );
        this.inlineUpdateQuestionContentError = this.questionContentFormGroup.locator(
            '#QuestionContent-error-xxx'
        );
        
        this.inlineQuestionTypeError = this.radiosFormGroup.locator('#QuestionType-error');
        this.inlineUpdateQuestionTypeError = this.radiosFormGroup.locator('#QuestionType-error-xxx');
        this.fieldset =  this.form.locator('fieldset[aria-describedby*="QuestionType-hint"]');
    }

    // ===== Validations =====
    async expectQuestionHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.addQuestionHeading, '❌ Add question heading not visible').toBeVisible();

        if (expectedText) {
            await expect(this.addQuestionHeading, `❌ Add question heading text does not match: expected "${expectedText}"`).toHaveText(expectedText);
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
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_TYPE);
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_CONTENT);

        await this.clickAllLinksAndValidateFocus(browserName);
    }

    async validateMissingQuestionContentErrorMessageSummary(browserName: string) {
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_CONTENT);
        await this.clickErrorLinkAndValidateFocus(this.errorLinkQuestionContent, browserName);
    }
    
    async validateMissingQuestionTypeErrorMessageSummary(browserName: string) {
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTION_TYPE);
        await this.clickErrorLinkAndValidateFocus(this.errorLinkQuestionType, browserName);
    }
    
    async validateInlineQuestionContentError(): Promise<void> {
        if (this.mode === 'update') {
            await expect(this.inlineUpdateQuestionContentError, '❌ Inline question content error not visible').toBeVisible();
        } else {
            
            await expect(this.inlineQuestionContentError, '❌ Inline question content error not visible').toBeVisible();
        }
    }

    async validateInlineQuestionTypeError(): Promise<void> {
        if (this.mode === 'update') {
            //await expect(this.inlineUpdateQuestionTypeError, '❌ Inline question type error not visible').toBeVisible();
        } else {
            await expect(this.errorSummary).toBeVisible();
            await expect(this.inlineQuestionTypeError, '❌ Inline question type error not visible').toBeVisible();
        }
    }
    
    async validateQuestionTextFormGroup() {
        // TODO: implement when requirements are known
    }

    async validateQuestionTypeFormGroup() {
        // TODO: implement when requirements are known
    }
    
    async assertPageElements() {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await this.VerifyQuestionInputAndHintTextarea();
        await expect(this.form, '❌ Form not visible').toBeVisible();
        expect(await this.radios.count()).toBeGreaterThan(1);
        await expect(this.saveButton, '❌ Save button not visible').toBeVisible();
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
            this.page.waitForLoadState('networkidle'),
            this.backLink.click()
        ]);
    }

    async enterQuestionContent(text: string): Promise<void> {
        await this.questionInput.clear();
        await this.questionInput.fill(text);
    }

    async clearQuestionContent(): Promise<void> {
        await this.questionInput.clear();
    }

    async enterQuestionHintText(text: string): Promise<void> {
        await this.hintTextarea.clear();
        await this.hintTextarea.fill(text);
    }

    async clearQuestionHintText(): Promise<void> {
        await this.hintTextarea.clear();
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

    // Accessibility
    async validateQuestionContentFieldAriaDescribedBy() {
        const errorElement = this.mode === 'update'
            ? this.inlineUpdateQuestionContentError
            : this.inlineQuestionContentError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.questionInput.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        if (this.mode === 'update') {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('questioncontent-field-error');
        } else {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('questioncontent-field-error');
        }
    }

    async validateQuestionTypeErrorAriaDescribedBy(): Promise<void> {
        const errorElement = this.mode === 'update'
            ? this.inlineQuestionTypeError
            : this.inlineUpdateQuestionTypeError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});
        
        await expect(this.fieldset).toHaveAttribute('aria-describedby');
        
        const ariaValue = await this.fieldset.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby missing').not.toBeNull();

        if (this.mode === 'update') {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('QuestionType-error');
        } else {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('QuestionType-error');
        }
    }
    
    // Convenience helper for typical flow
    async createQuestion(
        question: string,
        type: QuestionType,
        hint?: string
    ): Promise<void> {
        await this.enterQuestionContent(question);
        if (hint !== undefined) await this.enterQuestionHintText(hint);
        await this.chooseType(type);
        await this.save();
    }
}