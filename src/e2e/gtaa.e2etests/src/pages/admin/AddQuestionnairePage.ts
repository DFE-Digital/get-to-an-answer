import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit' | 'clone';

export class AddQuestionnairePage extends BasePage {
    // ===== Locators =====
    private readonly form = this.page.locator(
        'form[action*="/questionnaires/"][action$="/edit"][method="post"]'
    );

    private readonly titleInput = this.form.locator(
        'input#forms-name-input-name-field[name="Title"][type="text"]'
    );
    private readonly saveAndContinueButton = this.form.locator(
        'button.govuk-button[type="submit"]'
    );

    private readonly titleLabel = this.page.locator(
        'label[for="forms-name-input-name-field"]'
    );
    private readonly supportiveHint = this.page.locator(
        '#forms-name-input-name-hint'
    );

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
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

    // Validations (structure only)
    async verifyOnAddQuestionnairePage(): Promise<void> {
        await expect(this.page).toHaveURL(/\/questionnaires\/[^/]+\/edit/);
        await expect(this.form).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }

    async verifyLabelAndHintPresent(): Promise<void> {
        await expect(this.titleLabel).toBeVisible();
        await expect(this.supportiveHint).toBeVisible();
    }
}