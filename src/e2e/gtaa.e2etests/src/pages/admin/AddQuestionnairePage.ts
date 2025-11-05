import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit' | 'clone';

export class AddQuestionnairePage extends BasePage {
    private static readonly ADD_URL: RegExp =
        /\/admin\/questionnaires\/[0-9a-f-]+\/edit\/?$/i;
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

    async addQuestionnaire(title?: string): Promise<void> {
        const finalTitle = title ?? `Auto questionnaire title - ${Date.now()}`;
        await this.enterTitle(finalTitle);
        await this.enterTitle(`title - ${Date.now()}`);
        await this.clickSaveAndContinue();
    }

    // Validations (structure only)
    async expectUrlOnPage(): Promise<void> {
        await this.validateUrlMatches(AddQuestionnairePage.ADD_URL);
    }
    
    async verifyLabelAndHintPresent(): Promise<void> {
        await expect(this.titleLabel).toBeVisible();
        await expect(this.supportiveHint).toBeVisible();
    }

    async assertPageElements() {
        await this.expectUrlOnPage();
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyLabelAndHintPresent();

        await expect(this.form).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }
}