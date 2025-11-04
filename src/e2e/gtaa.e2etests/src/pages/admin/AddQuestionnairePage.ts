import { expect, Page } from '@playwright/test';
import { BasePage } from '../BasePage';

export class AddQuestionnairePage extends BasePage {
    private mode: 'create' | 'edit' = 'create';
    
    // ===== Locators =====
    private titleInput = this.page.locator('[data-test="questionnaire-title"]');
    private saveAndContinueButton = this.page.locator('[data-test="continue-button"]');

    // These are for presence checks only (not validating text)
    private questionLabel = this.page.locator('[data-test="questionnaire-question-label"]');
    private supportiveText = this.page.locator('[data-test="questionnaire-supportive-text"]');

    constructor(page: Page, mode: 'create' | 'edit' = 'create') {
        super(page);
        this.mode = mode;
    }

    // ===== Actions =====
    async enterTitle(title: string): Promise<void> {
        await this.titleInput.fill(title);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    async createNewQuestionnaire(title: string): Promise<void> {
        await this.enterTitle(title);
        await this.clickSaveAndContinue();
    }

    // ===== Validations =====
    async verifyOnNewQuestionnairePage(): Promise<void> {
        await expect(this.page).toHaveURL(/.*new-questionnaire/i);
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }

    async verifyQuestionAndSupportiveTextPresent(): Promise<void> {
        await expect(this.questionLabel).toBeVisible();
        await expect(this.supportiveText).toBeVisible();
    }
}