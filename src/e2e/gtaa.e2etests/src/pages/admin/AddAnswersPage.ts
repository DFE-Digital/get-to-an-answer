import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";
import {QuestionType} from "./AddQuestionPage";

type Mode = 'create' | 'edit';
type Destination = 'NextQuestion' |'SpecificQuestion' | 'ExternalLink' | 'InternalPage' | '0';

export class AddAnswersPage extends BasePage {
    private readonly mode: string;
    private readonly form: Locator;
    private readonly addAnswersHeading: Locator;
    private readonly addAnotherOptionButton: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly radios: Locator;
    private readonly enterAllOptionsLink: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorLinkOptionContent: Locator;
    private readonly inlineOptionContentError: Locator;
    private readonly optionContentFormGroup: Locator;
    
    private optionContent: (i: number) => Locator;
    private optionHint: (i: number) => Locator;
    private optionScore: (i: number) => Locator;
    private destinationRadio: (i: number, value: Destination)  => Locator;
    private externalLinkInput: (i: number) => Locator;
    private removeButtonFor: (i: number) => Locator;

    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        
        this.mode = mode;
        this.form = this.page.locator('#main-content form[method="post"]');
        this.addAnswersHeading = page.locator('main h1.govuk-heading-l');
        this.saveAndContinueButton = page.getByRole('button', { name: 'Save and continue' });
        this.enterAllOptionsLink = page.locator('a.govuk-link[href$="/bulk-options"]');
        
        this.optionContent = (i: number) =>
            page.locator(`input[name="Answers[${i}].Content"]`);

        this.optionHint = (i: number) =>
            page.locator(`textarea[name="Answers[${i}].Description"]`);

        this.optionScore = (i: number) =>
            page.locator(`input[name="Answers[${i}].Priority"]`);

        this.destinationRadio = (
            i: number,
            value: Destination = 'NextQuestion'
        ) =>
            page.locator(`input[name="Answers[${i}].DestinationType"][value="${value}"]`);

        this.externalLinkInput = (i: number) =>
            page.locator(
                `#conditional-e-answer-${i} input[type="text"], #conditional-e-answer-${i} input[type="url"]`
            ).first();

        this.removeButtonFor = (i: number) =>
            page.locator(`ul.app-select-options__list > li.app-select-options__item >> nth=${i}`)
                .locator(`button[name="remove"]`);

        //this.radios = this.form.locator('input[type="radio"][name="QuestionType"]'); //if form available in DOM
        this.radios = this.page.locator('input[type="radio"][name="QuestionType"]');
        this.addAnotherOptionButton = this.page.getByRole('button', { name: /add another option/i });

        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');

        this.errorLinkOptionContent = this.page.locator('a[href="#QuestionContent"]');
        this.optionContentFormGroup = page.locator('.govuk-form-group:has(#OptionContent)');
        this.inlineOptionContentError = this.optionContentFormGroup.locator('#OptionContent-error');
    }

    // ===== Validations =====
    async expectAnswerHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.addAnswersHeading, '❌ Add answers page heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.addAnswersHeading,
                `❌ Add answers heading text mismatch: expected "${expectedText}"`
            ).toHaveText(expectedText);
        }
    }

    async VerifyOptionContentAndHintTextarea(i: number): Promise<void> {
        await expect(this.optionContent(i), '❌ Question input not visible').toBeVisible();
        await expect(this.optionHint(i), '❌ Hint textarea not visible').toBeVisible();
    }
    
    async assertPageElements(i: number) {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await this.VerifyOptionContentAndHintTextarea(i);
        await expect(this.form, '❌ Form not visible').toBeVisible();
        expect(await this.radios.count()).toBeGreaterThan(1);
        await expect(this.saveAndContinueButton, '❌ Save button not visible').toBeVisible();
    }

    async validateMissingAllFieldsErrorMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_OPTION_CONTENT);
        // TBC, we may need to capture error for missing radio selection

        await this.clickAllLinksAndValidateFocus(browserName);
    }

    async validateMissingOptionContentErrorMessageSummary(browserName: string) {
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_OPTION_CONTENT);
        await this.clickErrorLinkAndValidateFocus(this.errorLinkOptionContent, browserName);
    }

    async validateMissingDestinationTypeErrorMessageSummary(browserName: string) {
        // TBC, not available yet on front-end
    }

    async validateInlineQuestionContentError(): Promise<void> {
        if (this.mode === 'update') {
            //await expect(this.inlineUpdateOptionContentError, '❌ Inline option content error not visible').toBeVisible();
        } else {

            await expect(this.inlineOptionContentError, '❌ Inline option content error not visible').toBeVisible();
        }
    }

    async validateInlineDestinationTypeError(): Promise<void> {
        //TBC, not available yet on front-end
    }

    // Accessibility
    async validateOptionContentFieldAriaDescribedBy(i: number) {
        // TBC, when update is available then check below lines are applicable or not
        // const errorElement = this.mode === 'update'
        //     ? this.inlineUpdateQuestionContentError
        //     : this.inlineQuestionContentError;

        const errorElement =this.inlineOptionContentError; 
            
        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.optionContent(i).getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        if (this.mode === 'update') {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('optioncontent-field-error');
        } else {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('optioncontent-field-error');
        }
    }

    async validateDestinationTypeFieldAriaDescribedBy(i: number) {
        // TBC, when update is available then check below lines are applicable or not
        // const errorElement = this.mode === 'update'
        //     ? this.inlineUpdateQuestionContentError
        //     : this.inlineQuestionContentError;

        const errorElement =this.inlineOptionContentError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.optionContent(i).getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        if (this.mode === 'update') {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('destinationtype-field-error');
        } else {
            expect(ariaValue, '❌ aria-describedby missing error id')
                .toContain('destinationtype-field-error');
        }
    }
    
    // ===== Actions =====
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
    
    async setOptionContent(i: number, text: string) {
        await this.optionContent(i).fill(text);
    }

    async setOptionHint(i: number, text: string) {
        await this.optionHint(i).fill(text);
    }

    async setOptionScore(i: number, priority: number) {
        await this.optionScore(i).fill(String(priority));
    }

    /** Next Question - nothing is set (set to null in backend)
     Specific question - Question (set to 1 in backend)
     Results page (inside Get to an answer) - CustomContent (set to 3 in backend)
     Results page (outside Get to an answer) - ExternalLink (set to 2 in backend) */
    async chooseDestination(i: number, value: Destination = 'NextQuestion') {
        await this.destinationRadio(i, value).check();
    }

    async setExternalLink(i: number, url: string) {
        await this.chooseDestination(i, 'ExternalLink');
        await this.externalLinkInput(i).fill(url);
    }
    
    // TBC, we may need more methods here just like 'setExternalLink'

    async removeOption(i: number) {
        await this.removeButtonFor(i).click();
    }

    async addAnotherOption() {
        await this.addAnotherOptionButton.click();
    }

    async saveAndContinue() {
        await this.saveAndContinueButton.click();
    }

    async openBulkOptions() {
        await this.enterAllOptionsLink.click();
    }
}