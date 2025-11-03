import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

export class QuestionnaireSlugPage extends BasePage {
    private mode: 'create' | 'edit' = 'create';

    // ===== Locators =====
    private slugInput = this.page.locator('[data-test="questionnaire-slug"]');
    private saveAndContinueButton = this.page.locator('[data-test="continue-button"]');

    // These are for presence checks only (not validating text)
    private alugLabel = this.page.locator('[data-test="questionnaire-slug-label"]');
    private supportiveText = this.page.locator('[data-test="questionnaire-supportive-text"]');

    constructor(page: Page, mode: 'create' | 'edit' = 'create') {
        super(page);
        this.mode = mode;
    }

    // ===== Actions =====
    async enterSlug(slug: string): Promise<void> {
        await this.slugInput.fill(slug);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    async createSlug(title: string): Promise<void> {
        await this.enterSlug(title);
        await this.clickSaveAndContinue();
    }

    // ===== Validations =====
    async verifyOnQuestionnaireSlugPage(): Promise<void> {
        await expect(this.page).toHaveURL(/new-questionnaire/);
        await expect(this.slugInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }

    async verifySlugLabelAndSupportiveTextPresent(): Promise<void> {
        await expect(this.alugLabel).toBeVisible();
        await expect(this.supportiveText).toBeVisible();
    }
}