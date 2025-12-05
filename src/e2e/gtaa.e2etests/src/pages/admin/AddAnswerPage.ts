import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";
import {QuestionRadioLabel} from "./AddQuestionPage";

type Mode = 'create' | 'edit';
type Destination = 'NextQuestion' | 'SpecificQuestion' | 'ExternalResultsPage' | 'InternalResultsPage' | '0';

export class AddAnswerPage extends BasePage {
    private readonly mode: string;
    private readonly backLink: Locator;
    private readonly addAnswersHeading: Locator;
    private readonly statusTag: Locator;
    private readonly answerOptionsHeader: Locator;
    private readonly addAnotherOptionButton: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly removeButton: Locator;
    private readonly enterAllOptionsButton: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorLinkOptionContent: Locator;
    private readonly inlineOptionContentError: Locator;
    private readonly optionContentFormGroup: Locator;

    private optionContent: (i: number) => Locator;
    private optionHint: (i: number) => Locator;
    private optionScore: (i: number) => Locator;
    private destinationRadio: (i: number, value: Destination) => Locator;
    private selectSpecificQuestion: (i: number) => Locator;
    private selectInternalResultsPage: (i: number) => Locator;
    private externalLinkInput: (i: number) => Locator;
    private inlineError: (i: number) => Locator;

    constructor(page: Page, mode: Mode = 'create') {
        super(page);

        this.mode = mode;
        this.addAnswersHeading = this.page.getByRole('heading', {
            level: 1,
            name: /Add or edit answers/i
        });
        this.backLink = page.locator('a.govuk-back-link');
        this.statusTag = page.locator('strong.govuk-tag[data-status="Draft"]');
        this.answerOptionsHeader = page.getByRole('heading', { name: 'Answer options' });
        this.removeButton = page.getByRole('button', { name: 'Remove' });
        this.saveAndContinueButton = page.getByRole('button', {name: /Save and continue/i});
        this.enterAllOptionsButton = page.locator(
            'button[formaction*="RedirectToBulkEntry"]'
        ).first();
        
        this.optionContent = (i: number) =>
            page.locator(`input[name="Options[${i}].OptionContent"]`)
        
        this.inlineError = (i: number) =>
            page.locator(`#Options-${i}-OptionContent-error`)
        
        this.optionHint = (i: number) =>
            page.locator(`textarea[name="Options[${i}].OptionHint"]`);
        
        this.optionScore = (i: number) =>
            page.locator(`input[name="Answers[${i}].Priority"]`);
        
        this.destinationRadio = (
            i: number,
            value: Destination = 'NextQuestion'
        ) =>
            page.locator(`input[name="Options[${i}].AnswerDestination"][value="${value}"]`);
        
        this.selectSpecificQuestion = (i: number) =>
            page.locator(
                `select#Options-${i}-destination-specific-select`
            );

        this.selectInternalResultsPage = (i: number) =>
            page.locator(
                `select#Options-${i}-destination-internal-select`
            );
        
        this.externalLinkInput = (i: number) =>
            page.locator(`input[id="Options-${i}-destination-external-link"]`);

        this.addAnotherOptionButton = this.page.getByRole('button', {name: /add another option/i});
        this.errorSummary = this.page.locator('div.govuk-error-summary[role="alert"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');
        this.errorLinkOptionContent = this.page.locator('a[href="#Answers0__Content"]');
        this.optionContentFormGroup = page.locator('div.govuk-form-group:has(input[name="Answers[0].Content"])');
        this.inlineOptionContentError = this.optionContentFormGroup.locator('span.govuk-error-message');
    }

    // ===== Validations =====
    async expectAnswerHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.addAnswersHeading, '❌ Add answers page heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.addAnswersHeading,
                `❌ Add answers heading text mismatch: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async expectQuestionnaireStatusOnPage(expectedText?: string): Promise<void> {
        await expect(this.statusTag, '❌ Questionnaire status not visible').toBeVisible();

        if (expectedText) {
            await expect(this.statusTag, `❌ Questionnaire status text does not match: expected "${expectedText}"`).toHaveText(expectedText);
        }
    }

    async asserPageElementsUponLanding(i: number, removeButtonCount: number) {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await expect(this.answerOptionsHeader, '❌ Add another option button not visible').toBeVisible();
        await this.verifyAnswerOptionElements(i);
        await expect(this.removeButton).toHaveCount(removeButtonCount);
        for (let i = 0; i < removeButtonCount; i++) {
            await expect(this.removeButton.nth(i)).toBeVisible();    
        }
        await expect(this.addAnotherOptionButton, '❌ Add another option button not visible').toBeVisible();
        await expect(this.enterAllOptionsButton, '❌ Enter all options link not visible').toBeVisible();
        await expect(this.saveAndContinueButton, '❌ Continue button not visible').toBeVisible();
    }

    async verifyAnswerOptionElements(i: number): Promise<void> {
        await expect(this.optionContent(i), '❌ Option content input not visible').toBeVisible();
        await expect(this.optionHint(i), '❌ Option hint textarea not visible').toBeVisible();
        await expect(this.destinationRadio(i, 'NextQuestion'), '❌ Next question radio not visible').toBeVisible();
        await expect(this.destinationRadio(i, 'SpecificQuestion'), '❌ Specific question radio not visible').toBeVisible();
        await expect(this.destinationRadio(i, 'InternalResultsPage'), '❌ Internal results radio not visible').toBeVisible();
        await expect(this.destinationRadio(i, 'ExternalResultsPage'), '❌ External results radio not visible').toBeVisible();
    }

    async VerifyOptionContentAndHintTextarea(i: number): Promise<void> {
        await expect(this.optionContent(i), '❌ Question input not visible').toBeVisible();
        await expect(this.optionHint(i), '❌ Hint textarea not visible').toBeVisible();
    }

    async validateMissingAllFieldsErrorMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION1_CONTENT);
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION2_CONTENT);

        await this.clickAllLinksAndValidateFocus(browserName);
    }
    
    async validateInlineQuestionContentError(i:number): Promise<void> {
        await expect(this.inlineError(i), '❌ Inline option content error not visible').toBeVisible();
        if(i==0)
        {
            await expect(this.inlineError(i)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION1_CONTENT);    
        }else{
            await expect(this.inlineError(i)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION2_CONTENT);   
        }
    }
    
    // Accessibility
    async validateOptionContentFieldAriaDescribedBy(i: number) {
        // TBC, when update is available then check below lines are applicable or not
        // const errorElement = this.mode === 'update'
        //     ? this.inlineUpdateQuestionContentError
        //     : this.inlineQuestionContentError;

        const errorElement = this.inlineOptionContentError;

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

        const errorElement = this.inlineOptionContentError;

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

    async setSpecificQuestion(i: number, optionText: string) {
        await this.chooseDestination(i, 'SpecificQuestion');
        await this.selectSpecificQuestion(i).selectOption(optionText);
    }

    async setInternalLink(i: number, optionText: string) {
        await this.chooseDestination(i, 'InternalResultsPage');
        await this.selectInternalResultsPage(i).selectOption(optionText);
    }

    async setExternalLink(i: number, url: string) {
        await this.chooseDestination(i, 'ExternalResultsPage');
        await this.externalLinkInput(i).fill(url);
    }

    async clickBackLInk() {
        await this.backLink.click();
    }
    
    async removeOption(i: number) {
        await this.removeButton.nth(i).click();
    }

    async clickAddAnotherOptionButton() {
        await this.addAnotherOptionButton.click();
    }

    async clickSaveAndContinueButton() {
        await this.saveAndContinueButton.click();
    }

    async openBulkOptions() {
        await this.enterAllOptionsButton.click();
    }

    async getRemoveButtonCount(): Promise<number> {
        return await this.removeButton.count();
    }
}