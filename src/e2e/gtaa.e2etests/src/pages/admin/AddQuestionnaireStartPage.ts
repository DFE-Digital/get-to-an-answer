import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {EditAnswerTable} from "./components/EditAnswerTable";
import {Timeouts} from '../../constants/timeouts'

export class AddQuestionnaireStartPage extends BasePage {

    // ===== Locators =====
    private readonly form: Locator;
    private readonly addEditStartPageHeading: Locator;
    private readonly backLink: Locator;
    private readonly questionnaireDisplayTitleInput: Locator;
    private readonly questionnaireDescriptionText: Locator;
    private readonly questionnaireTitleCaption: Locator;
    private readonly saveStartPageButton: Locator;
    private readonly removeStartPageButton: Locator;

    private readonly errorSummaryDisplayTitle: Locator;
    private readonly errorSummaryDescription: Locator;
    private readonly errorInlineDisplayTitle: Locator;
    private readonly errorInlineDescription: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.form = this.page.locator('form[method="post"]');
        this.addEditStartPageHeading = this.page.locator('#questionnaire-start-page-heading');
        this.questionnaireTitleCaption = page.locator('#current-questionnaire-title');
        this.backLink = this.page.locator('a.govuk-back-link');

        this.questionnaireDisplayTitleInput = this.form.locator('#DisplayTitle');
        this.questionnaireDescriptionText = this.form.locator('#Description');

        // Save button
        this.saveStartPageButton = page.locator('#save-start-page');
        this.removeStartPageButton = page.locator('#remove-start-page');
        
        // Error summary
        this.errorSummaryDisplayTitle = this.errorSummaryLink('#DisplayTitle');
        this.errorSummaryDescription = this.errorSummaryLink('#Description');
        
        // Inline error messages
        this.errorInlineDisplayTitle = this.inlineErrorLink('DisplayTitle-error');
        this.errorInlineDescription = this.inlineErrorLink('Description-error');
    }
    
    // ===== Validation =====
    
    async assertStartPageFieldsEmpty(): Promise<void> {
        await this.questionnaireDisplayTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.questionnaireDisplayTitleInput).toHaveValue('');
        await this.questionnaireDescriptionText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.questionnaireDescriptionText).toHaveValue('');
    }

    async assertStartPageFields(expectedDisplayTitle: string, expectedDescription: string): Promise<void> {
        await this.questionnaireDisplayTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.questionnaireDisplayTitleInput).toHaveValue(expectedDisplayTitle);
        await this.questionnaireDescriptionText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.questionnaireDescriptionText).toHaveValue(expectedDescription);
    }

    async assertErrorSummaryLinkForDisplayTitle(): Promise<void> {
        await this.errorSummaryDisplayTitle.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorSummaryDisplayTitle).toHaveText(ErrorMessages.ERROR_MESSAGE_DISPLAY_TITLE_REQUIRED);
    }

    async assertErrorSummaryLinkForDescription(): Promise<void> {
        await this.errorSummaryDescription.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorSummaryDescription).toHaveText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    }

    async assertInlineErrorForDisplayTitle(): Promise<void> {
        await this.errorInlineDisplayTitle.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorInlineDisplayTitle).toContainText(ErrorMessages.ERROR_MESSAGE_DISPLAY_TITLE_REQUIRED);
    }

    async assertInlineErrorForDescription(): Promise<void> {
        await this.errorInlineDescription.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorInlineDescription).toContainText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    }
    
    // ===== Actions =====

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.backLink.click(),
            await this.waitForPageLoad(),
        ]);
    }

    async enterQuestionnaireDisplayTitleInput(text: string): Promise<void> {
        await this.questionnaireDisplayTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionnaireDisplayTitleInput.clear();
        await this.questionnaireDisplayTitleInput.fill(text);
    }

    async clearQuestionnaireDisplayTitleInput(): Promise<void> {
        await this.questionnaireDisplayTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionnaireDisplayTitleInput.clear();
    }

    async enterQuestionnaireDescriptionText(text: string): Promise<void> {
        await this.questionnaireDescriptionText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionnaireDescriptionText.clear();
        await this.questionnaireDescriptionText.fill(text);
    }

    async clearQuestionnaireDescriptionText(): Promise<void> {
        await this.questionnaireDescriptionText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.questionnaireDescriptionText.clear();
    }

    async enterInvalidContent(): Promise<void> {
        await this.questionnaireDisplayTitleInput.clear();
        await this.questionnaireDisplayTitleInput.fill(`${' '.repeat(10)}`);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveStartPageButton.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.saveStartPageButton.click();
        await this.waitForPageLoad();
    }

    async clickRemoveStartPage(): Promise<void> {
        await this.removeStartPageButton.click();
    }
}