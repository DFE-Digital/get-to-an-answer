import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";
import {QuestionRadioLabel} from "./AddQuestionPage";

type Mode = 'create' | 'update';
type Destination = 'NextQuestion' | 'SpecificQuestion' | 'ExternalResultsPage' | 'InternalResultsPage' | '0';

export enum AnswerFieldName {
    Content = 'OptionContent',
    Hint = 'OptionHint',
    SpecificQuestionSelect = 'AnswerDestination-specific',
    ResultsPageSelect = 'AnswerDestination-internal',
    ExternalLinkInput = 'AnswerDestination-external'
}

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
    private optionHintDiv: (i: number) => Locator;
    private answerRank: (i: number) => Locator;
    private destinationRadio: (i: number, value: Destination) => Locator;
    private selectSpecificQuestion: (i: number) => Locator;
    private selectInternalResultsPage: (i: number) => Locator;
    private externalLinkInput: (i: number) => Locator;
    //private inlineError: (i: number) => Locator;
    private inlineErrorAnswerDestination: (i: number) => Locator;
    private inlineError: (i: number, fieldName: AnswerFieldName) => Locator;
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

        //this.inlineError = (i: number) =>
          //  page.locator(`#Options-${i}-OptionContent-error`)
        
        this.inlineError = (i: number, fieldName: string) =>
            page.locator(`#Options-${i}-${fieldName}-error`)

        this.inlineErrorAnswerDestination = (i: number) =>
            page.locator(`#Options-${i}-AnswerDestination-internal-error`)

        this.optionHint = (i: number) =>
            page.locator(`textarea[name="Options[${i}].OptionHint"]`);

        this.optionHintDiv = (i: number) =>
            page.locator(`textarea[name="Options[${i}].OptionHint"]`)
                .locator('xpath=preceding-sibling::div[contains(@class,"govuk-hint")]');

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
    async validateAllOptionContents(expectedValues: string[]): Promise<void> {
        // Locates all the input fields for option content
        const inputs = this.page.locator('input[name*="OptionContent"]');

        // Get the actual values currently in the browser
        const actualValues = await inputs.all();
        const actualTexts = await Promise.all(actualValues.map(input => input.inputValue()));

        // Verify the counts match first
        expect(actualTexts.length, '❌ Number of answer options does not match expected count').toBe(expectedValues.length);

        // Verify each individual string
        for (let i = 0; i < expectedValues.length; i++) {
            expect(actualTexts[i], `❌ Answer option at index ${i} mismatch`).toBe(expectedValues[i]);
        }
    }

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
        if (this.mode == 'update') {
            await expect(this.saveAnswersButton, '❌ Save answers button not visible').toBeVisible();
        } else {
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

    async validateDuplicateAnswerOptionsErrorMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION1_CONTENT);
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION2_CONTENT);

        await this.clickAllLinksAndValidateFocus(browserName);
    }

    async validateMissingResultsPageErrorMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_RESULTS_PAGE_ANSWER_OPTION1_SUMMARY);

        await this.clickAllVisibleLinksAndValidateFocus(browserName);
    }

    async validateInlineQuestionContentError(i: number): Promise<void> {
        await expect(this.inlineError(i, AnswerFieldName.Content), '❌ Inline option content error not visible').toBeVisible();
        if (i == 0) {
            await expect(this.inlineError(i, AnswerFieldName.Content)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION1_CONTENT);
        } else {
            await expect(this.inlineError(i, AnswerFieldName.Content)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_ANSWER_OPTION2_CONTENT);
        }
    }

    async validateInlineDuplicatedQuestionContentError(i: number): Promise<void> {
        await expect(this.inlineError(i, AnswerFieldName.Content), '❌ Inline option content error not visible').toBeVisible();
        if (i == 0) {
            await expect(this.inlineError(i, AnswerFieldName.Content)).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION1_CONTENT);
        } else {
            await expect(this.inlineError(i, AnswerFieldName.Content)).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_ANSWER_OPTION2_CONTENT);
        }
    }

    async validateInlineMissingResultsPageError(i: number): Promise<void> {
        await expect(this.inlineErrorAnswerDestination(i), '❌ Inline missing results page error not visible').toBeVisible();
        await expect(this.inlineErrorAnswerDestination(i)).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_RESULTS_PAGE_ANSWER_INLINE);
    }

    async validateInlineErrorNotVisible(i: number): Promise<void> {
        const errorElement = this.inlineError(i, AnswerFieldName.Content);
        const isVisible = await errorElement.isVisible().catch(() => false);
        expect(isVisible, `❌ Inline error should not be visible for option ${i}`).toBe(false);
    }

    async expectDestinationRadioSelected(optionIndex: number, destination: Destination = 'NextQuestion'): Promise<void> {
        const destinationRadioLocator = this.destinationRadio(optionIndex, destination);
        await expect(destinationRadioLocator, `❌ Destination radio ${destination} for option ${optionIndex} not selected`).toBeChecked();
    }

    async expectSpecificQuestionDropdownSelected(optionIndex: number, expectedQuestionContent: string): Promise<void> {
        const specificQuestionDropdown = this.selectSpecificQuestion(optionIndex);

        // Visibility check
        await expect(
            specificQuestionDropdown,
            `❌ Specific question dropdown for option ${optionIndex} not visible`
        ).toBeVisible();

        // Get the actual value
        const selectedOption = specificQuestionDropdown.locator('option:checked');
        const actualValue = await selectedOption.textContent();

        expect(
            actualValue,
            `❌ Specific question dropdown for option ${optionIndex} does not match expected question content. Expected: "${expectedQuestionContent}", Actual: "${actualValue}"`
        ).toBe(expectedQuestionContent);
    }


    async expectExternalLinkInputValue(optionIndex: number, expectedValue: string): Promise<void> {
        const externalLinkInput = this.externalLinkInput(optionIndex);
        await expect(externalLinkInput, `❌ External link input for option ${optionIndex} not visible`).toBeVisible();
        await expect(externalLinkInput, `❌ External link input value for option ${optionIndex} does not match expected value`).toHaveValue(expectedValue);
    }

    async expectResultsPageDropdownSelected(optionIndex: number, expectedResultsPageContent?: string): Promise<void> {
        const internalResultsDropdown = this.selectInternalResultsPage(optionIndex);

        // Visibility check
        await expect(
            internalResultsDropdown,
            `❌ Internal results dropdown for option ${optionIndex} not visible`
        ).toBeVisible();

        // Get the text of the selected option
        const selectedOption = internalResultsDropdown.locator('option:checked');
        const actualValue = await selectedOption.textContent();

        expect(
            actualValue?.trim(),
            `❌ Internal results dropdown for option ${optionIndex} does not match expected results page content. Expected: "${expectedResultsPageContent}", Actual: "${actualValue}"`
        ).toBe(expectedResultsPageContent);

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

    async validateAriaDescribedByWithError(optionIndex: number, fieldName: AnswerFieldName): Promise<void> {
        let ariaDescribedBy;

        switch (fieldName) {
            case  AnswerFieldName.Content :
                ariaDescribedBy = await this.optionContent(optionIndex).getAttribute('aria-describedby');
                break;
            case AnswerFieldName.Hint :
                ariaDescribedBy = await this.optionHintDiv(optionIndex).getAttribute('aria-describedby');
                break;
            case AnswerFieldName.SpecificQuestionSelect :
                ariaDescribedBy = await this.selectSpecificQuestion(optionIndex).getAttribute('aria-describedby');
                break;
            case AnswerFieldName.ResultsPageSelect :
                ariaDescribedBy = await this.selectInternalResultsPage(optionIndex).getAttribute('aria-describedby');
                break;
            case AnswerFieldName.ExternalLinkInput :
                ariaDescribedBy = await this.externalLinkInput(optionIndex).getAttribute('aria-describedby');
                break;

            default:
                throw new Error(`Invalid field name: ${fieldName}`);
        }
        expect(ariaDescribedBy, `❌ aria-describedby is missing for option ${optionIndex}`).not.toBeNull();

        const errorElement = this.inlineError(optionIndex, fieldName);
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

    async validateAriaDescribedByForHintOnly(optionIndex: number): Promise<void> {
        const ariaDescribedBy = await this.optionHint(optionIndex).getAttribute('aria-describedby');
        expect(ariaDescribedBy, `❌ aria-describedby is missing for hint ${optionIndex}`).not.toBeNull();

        const hintDivId = await this.optionHintDiv(optionIndex).getAttribute('id');
        expect(hintDivId, `❌ Option ${optionIndex} hint missing id attribute`).not.toBeNull();
        expect(ariaDescribedBy, `❌ aria-describedby should include hint id for option ${optionIndex}`)
            .toContain(hintDivId);

        // Verify error is NOT visible when no validation errors are present
        const errorElement = this.inlineError(optionIndex, AnswerFieldName.Hint);
        const errorIsVisible = await errorElement.isVisible().catch(() => false);
        expect(errorIsVisible, `❌ Error should not be visible for option ${optionIndex}`).toBe(false);
    }

    async validateRankPriorityAriaDescribedBy(optionIndex: number): Promise<void> {
        const rankInput = this.answerRank(optionIndex);
        const ariaDescribedBy = await rankInput.getAttribute('aria-describedby');
        const expectedHintId = `Options-${optionIndex}-PriorityHint`;

        expect(ariaDescribedBy, `❌ aria-describedby missing for Rank Priority at index ${optionIndex}`).toContain(expectedHintId);

        const hintVisible = await this.page.locator(`#${expectedHintId}`).isVisible();
        expect(hintVisible, `❌ Priority hint with id ${expectedHintId} is not visible`).toBe(true);
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

    async clickAllVisibleLinksAndValidateFocus(browserName: string): Promise<void> {
        const visibleErrorLinks = this.errorLinks.filter({hasText: /\S/});
        const linkCount = await visibleErrorLinks.count();

        for (let i = 0; i < linkCount; i++) {
            const link = visibleErrorLinks.nth(i);
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
        const radioLocator = this.destinationRadio(i, value);

        // Wait for the radio button to be present and enabled
        await radioLocator.waitFor({ state: 'attached' });

        // Ensure the radio button is visible and enabled before clicking
        await expect(radioLocator, `❌ Destination radio ${value} for option ${i} not interactable`).toBeVisible();
        await expect(radioLocator, `❌ Destination radio ${value} for option ${i} is disabled`).toBeEnabled();

        // Use force click to handle potential overlay or other interference
        await radioLocator.click({ force: true });

        // Verify the radio button is actually checked after clicking
        await expect(radioLocator, `❌ Destination radio ${value} for option ${i} not selected`).toBeChecked();
    }


    async setSpecificQuestion(i: number, optionText: string) {
        await this.chooseDestination(i, 'SpecificQuestion');

        if (optionText)
            await this.selectSpecificQuestion(i).selectOption(optionText);
    }

    async setInternalLink(i: number, optionText: string) {
        // Add an explicit wait before choosing destination
        await this.page.waitForFunction(() => document.readyState === 'complete');

        await this.chooseDestination(i, 'InternalResultsPage');

        // Add visibility and interaction wait
        await this.selectInternalResultsPage(i).waitFor({state: 'visible'});

        if (optionText) {
            await this.selectInternalResultsPage(i).selectOption(optionText);

            // Additional assertion to ensure selection
            const selectedOption = await this.selectInternalResultsPage(i).locator('option:checked').textContent();
            expect(selectedOption?.trim()).toBe(optionText);
        }
    }

    async setExternalLink(i: number, url: string) {
        // Add an explicit wait before choosing destination
        await this.page.waitForFunction(() => document.readyState === 'complete');

        await this.chooseDestination(i, 'ExternalResultsPage');

        // Add visibility and interaction wait
        await this.externalLinkInput(i).waitFor({state: 'visible'});

        if (url) {
            await this.externalLinkInput(i).fill(url);

            // Additional assertion to ensure input
            const inputValue = await this.externalLinkInput(i).inputValue();
            expect(inputValue).toBe(url);
        }
    }

    async clickBackLink() {
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