import { Page, Locator, expect } from '@playwright/test';

export class QuestionnaireVersionHistoryPage {
    readonly heading: Locator;
    readonly stepNav: Locator;
    readonly toggleAllVersionsButton: Locator;

    constructor(page: Page) {
        this.heading = page.locator('h1.govuk-heading-l');
        this.stepNav = page.locator('#step-by-step-navigation');
        this.toggleAllVersionsButton = page.getByRole('button', {name: /versions/i});
    }

    // ---- internal helpers ----
    private getStepHeader(level: number): Locator {
        return this.stepNav.locator(`.app-step-nav__header[data-position="${level}"]`);
    }

    private getStepPanel(level: number): Locator {
        return this.getStepHeader(level)
            .locator('xpath=../div[contains(@class,"app-step-nav__panel")]');
    }

    // ---- heading ----
    async expectHeading(expected: string): Promise<void> {
        await expect(this.heading).toContainText(expected);
    }

    // ---- show/hide all versions ----
    async clickHideAllVersions(): Promise<void> {
        await this.toggleAllVersionsButton.click();
    }

    async clickShowAllVersions(): Promise<void> {
        await this.toggleAllVersionsButton.click();
    }

    async expectAllVersionsToggleText(expectedText: string): Promise<void> {
        await expect(
            this.toggleAllVersionsButton,
            `❌ Expected "Show/Hide all versions" toggle text to be "${expectedText}"`
        ).toHaveText(expectedText);
    }

    // ---- show/hide changes per level ----
    async clickShowChanges(level: number): Promise<void> {
        const button = this.getStepHeader(level).getByRole('button', { name: /Show Changes/i });
        await button.click();
    }

    async clickHideChanges(level: number): Promise<void> {
        const button = this.getStepHeader(level).getByRole('button', { name: /Hide Changes/i });
        await button.click();
    }

    async expectToggleButtonText(level: number, expectedText: string): Promise<void> {
        const toggleButton = this.getStepHeader(level)
            .locator('.app-step-nav__button-text');

        await expect(toggleButton, `❌ Toggle button text mismatch at level ${level}`)
            .toHaveText(expectedText);
    }

    // ---- title / email / timestamp for any level ----
    async getVersionTitle(level: number): Promise<string> {
        const titleSpan = this.getStepHeader(level).locator('.app-step-nav__title-text');
        const text = await titleSpan.innerText();
        return text.trim();
    }

    async getVersionUserEmail(level: number): Promise<string> {
        const emailLocator = this.getStepHeader(level).locator('.govuk-hint').first();
        const text = await emailLocator.innerText();
        return text.replace(/^by\s+/i, '').trim();
    }

    async getVersionTimestamp(level: number): Promise<string> {
        const tsLocator = this.getStepHeader(level).locator('.govuk-hint').nth(1);
        const text = await tsLocator.innerText();
        return text.trim();
    }

    // ---- level 1 shortcuts ----
    async getLevel1Title(): Promise<string> {
        return this.getVersionTitle(1);
    }

    async getLevel1UserEmail(): Promise<string> {
        return this.getVersionUserEmail(1);
    }

    async getLevel1Timestamp(): Promise<string> {
        return this.getVersionTimestamp(1);
    }

    // ---- changes text inside expanded panel ----
    async getChangesTextForLevel(level: number): Promise<string> {
        const text = await this.getStepPanel(level).innerText();
        return text.trim();
    }
}
