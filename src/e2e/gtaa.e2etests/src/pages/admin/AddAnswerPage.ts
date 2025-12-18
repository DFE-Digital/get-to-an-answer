import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";
import {QuestionRadioLabel} from "./AddQuestionPage";

type Mode = 'create' | 'update';
type Destination = 'NextQuestion' | 'SpecificQuestion' | 'ExternalResultsPage' | 'InternalResultsPage' | '0';

export class AddAnswerPage extends BasePage {
    private readonly mode: string;
    private readonly backLink: Locator;
    private readonly addAnswersHeading: Locator;
    private readonly answerOptionsHeader: Locator;
    private readonly addAnotherOptionButton: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly saveAnswersButton: Locator;
    private readonly removeButton: Locator;
    private readonly enterAllOptionsButton: Locator;
    private readonly addBulkQuestionsButton: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorLinkOptionContent: Locator;
    private readonly inlineOptionContentError: Locator;
    private readonly optionContentFormGroup: Locator;
    
    private optionContent: (i: number) => Locator;
    private optionHint: (i: number) => Locator;
    private answerRank: (i: number) => Locator;
    private destinationRadio: (i: number, value: Destination) => Locator;
    private selectSpecificQuestion: (i: number) => Locator;
    private selectInternalResultsPage: (i: number) => Locator;
    private externalLinkInput: (i: number) => Locator;
    private inlineError: (i: number) => Locator;
    private optionNumber: (index: number) => Locator;

    constructor(page: Page, mode: Mode = 'create') {
        super(page);

        this.mode = mode;
        this.addAnswersHeading = this.page.getByRole('heading', {
            level: 1,
            name: /Add or edit answers/i
        });
        this.backLink = page.locator('a.govuk-back-link');
        this.answerOptionsHeader = page.getByRole('heading', {name: 'Answer options'});
        this.removeButton = page.getByRole('button', {name: 'Remove'});
        this.saveAndContinueButton = page.getByRole('button', {name: /Save and continue/i});
        this.saveAnswersButton = page.getByRole('button', {name: /Save answers/i});
        this.enterAllOptionsButton = page.locator(
            'button[formaction*="RedirectToBulkEntry"]'
        ).first();
        this.addBulkQuestionsButton = this.page.locator('button.govuk-button[type="submit"][formaction*="handler=RedirectToBulkEntry"]');

        this.optionNumber = (index: number) =>
            this.page.locator(`label[for="Options-${index}-OptionContent"]`);
        
        this.optionContent = (i: number) =>
            page.locator(`input[name="Options[${i}].OptionContent"]`)

        this.inlineError = (i: number) =>
            page.locator(`#Options-${i}-OptionContent-error`)

        this.optionHint = (i: number) =>
            page.locator(`textarea[name="Options[${i}].OptionHint"]`);

        this.answerRank = (i: number) =>
            page.locator(`input[id=Options-${i}-RankPriority]`);

        this.destinationRadio = (
            i: number,
            value: Destination = 'NextQuestion'
        ) =>
            page.locator(`input[name="Options[${i}].AnswerDestination"][value="${value}"]`);

        this.selectSpecificQuestion = (i: number) =>
            page.locator(
                `select#Options-${i}-destination-specific-select`
            ); this.selectInternalResultsPage = (i: number) =>
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
    async assertAllOptionNumberLabelsInOrder(): Promise<void> {
        const labels = this.page.locator('label.govuk-label.govuk-label--s[for*="OptionContent"]');
        const count = await labels.count();
        const seenLabels = new Set<string>();
        let expectedTextIndex = 0;

        for (let i = 0; i < count; i++) {
            const label = labels.nth(i);
            const textContent = (await label.textContent())?.trim() ?? '';
            
            if (!textContent.startsWith('Option '))
                continue;
            
            expectedTextIndex++;
            const expectedText = `Option ${expectedTextIndex}`;

            expect(textContent, `❌ Label ${expectedTextIndex} text mismatch`).toBe(expectedText);
            expect(seenLabels.has(textContent), `❌ Duplicate label detected: ${textContent}`).toBe(false);
            seenLabels.add(textContent);
        }
    }
    
    async expectAnswerHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.addAnswersHeading).toBeVisible({
            timeout: Timeouts.MEDIUM,  
            visible: true   // Explicitly ensure visibility
        });

        if (expectedText) {
            // Use toHaveText instead of toContainText for more precise matching
            await expect(this.addAnswersHeading).toHaveText(
                new RegExp(expectedText, 'i'),  // Case-insensitive regex match
                {
                    timeout: Timeouts.MEDIUM,
                    ignoreCase: true  // Additional case-insensitive matching
                }
            );
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
        if(this.mode=='update') {
            await expect(this.saveAnswersButton, '❌ Save answers button not visible').toBeVisible();
        }else{
            await expect(this.saveAndContinueButton, '❌ Save and continue button not visible').toBeVisible();    
        }
        
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

    async validateInlineQuestionContentError(i: number): Promise<void> {
        await expect(this.inlineError(i), '❌ Inline option content error not visible').toBeVisible();
        if (i == 0) {
            await expect(this.inlineError(i)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION1_CONTENT);
        } else {
            await expect(this.inlineError(i)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION2_CONTENT);
        }
    }
    
    async validateInlineErrorNotVisible(i: number): Promise<void> {
        const errorElement = this.inlineError(i);
        const isVisible = await errorElement.isVisible().catch(() => false);
        expect(isVisible, `❌ Inline error should not be visible for option ${i}`).toBe(false);
    }


    // Accessibility
    async validateUniqueIdsForMultipleOptions(optionCount: number): Promise<void> {
        for (let i = 0; i < optionCount; i++) {
            const optionContentId = await this.optionContent(i).getAttribute('id');
            expect(optionContentId, `❌ Option ${i} content input missing unique id`).not.toBeNull();
            expect(optionContentId, `❌ Option ${i} content id should incorporate index ${i}`).toContain(`Options-${i}`);
            
            const optionHintId = await this.optionHint(i).getAttribute('id');
            expect(optionHintId, `❌ Option ${i} hint textarea missing unique id`).not.toBeNull();
            expect(optionHintId, `❌ Option ${i} hint id should incorporate index ${i}`).toContain(`Options-${i}`);
        }
    }

    async validateAriaDescribedByWithHintAndError(optionIndex: number): Promise<void> {
        const ariaDescribedBy = await this.optionContent(optionIndex).getAttribute('aria-describedby');
        expect(ariaDescribedBy, `❌ aria-describedby is missing for option ${optionIndex}`).not.toBeNull();
        
        const hintId = await this.optionHint(optionIndex).getAttribute('id');
        expect(hintId, `❌ Option ${optionIndex} hint missing id attribute`).not.toBeNull();
        expect(ariaDescribedBy, `❌ aria-describedby should include hint id for option ${optionIndex}`)
            .toContain(hintId);
        
        const errorElement = this.inlineError(optionIndex);
        const errorIsVisible = await errorElement.isVisible().catch(() => false);

        if (errorIsVisible) {
            const errorId = await errorElement.getAttribute('id');
            expect(errorId, `❌ Option ${optionIndex} error missing id attribute`).not.toBeNull();
            expect(ariaDescribedBy, `❌ aria-describedby should include error id for option ${optionIndex}`)
                .toContain(errorId);
        } else {
            expect(false, `❌ Error should be visible for option ${optionIndex}`).toBe(true);
        }
    }

    async validateAriaDescribedByWithHintOnly(optionIndex: number): Promise<void> {
        const ariaDescribedBy = await this.optionContent(optionIndex).getAttribute('aria-describedby');
        expect(ariaDescribedBy, `❌ aria-describedby is missing for option ${optionIndex}`).not.toBeNull();
        
        const hintId = await this.optionHint(optionIndex).getAttribute('id');
        expect(hintId, `❌ Option ${optionIndex} hint missing id attribute`).not.toBeNull();
        expect(ariaDescribedBy, `❌ aria-describedby should include hint id for option ${optionIndex}`)
            .toContain(hintId);

        // Verify error is NOT visible when no validation errors are present
        const errorElement = this.inlineError(optionIndex);
        const errorIsVisible = await errorElement.isVisible().catch(() => false);
        expect(errorIsVisible, `❌ Error should not be visible for option ${optionIndex}`).toBe(false);

        // Verify error id is NOT included in aria-describedby
        const errorId = await errorElement.getAttribute('id').catch(() => null);
        if (errorId) {
            expect(ariaDescribedBy, `❌ aria-describedby should not include error id for option ${optionIndex}`)
                .not.toContain(errorId);
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

    async clearOptionContent(i: number) {
        await this.optionContent(i).clear();
    }

    async setOptionContent(i: number, text: string) {
        await this.optionContent(i).fill(text);
    }

    async clearOptionHint(i: number) {
        await this.optionHint(i).clear();
    }

    async setOptionHint(i: number, text: string) {
        await this.optionHint(i).fill(text);
    }

    async clearAnswerRank(i: number) {
        await this.answerRank(i).clear();
    }

    async setAnswerRank(i: number, priority: number) {
        await this.answerRank(i).fill(String(priority));
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
    
    async clickSaveAnswersButton() {
        await this.saveAnswersButton.click();
    }
    async clickEnterAllOptionsButton() {
        await this.enterAllOptionsButton.click();
    }

    async getRemoveButtonCount(): Promise<number> {
        return await this.removeButton.count();
    }
}