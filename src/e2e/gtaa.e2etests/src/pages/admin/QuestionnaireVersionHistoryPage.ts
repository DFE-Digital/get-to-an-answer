import {Page, Locator, expect} from '@playwright/test';

export class QuestionnaireVersionHistoryPage {
    readonly heading: Locator;
    readonly backLink: Locator;
    readonly stepNav: Locator;
    readonly toggleAllVersionsButton: Locator;
    readonly getVersionList: (level: number) => Locator;

    constructor(page: Page) {
        this.heading = page.locator('h1.govuk-heading-l');
        this.backLink = page.getByRole('link', { name: 'Back' });
        this.stepNav = page.locator('#step-by-step-navigation');
        this.toggleAllVersionsButton = page.getByRole('button', {name: /versions/i});
        this.getVersionList = (level: number) =>
            page.locator(`.js-step [data-position="${level}"]`)
                .locator('..') // move up to the <li>
                .locator('.app-step-nav__panel .app-step-nav__list');

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
        const button = this.getStepHeader(level).getByRole('button', {name: /Show Changes/i});
        await button.click();
    }

    async clickBackLink(): Promise<void> {
        await this.backLink.click();
    }
    
    async clickHideChanges(level: number): Promise<void> {
        const button = this.getStepHeader(level).getByRole('button', {name: /Hide Changes/i});
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
    
    // ---- changes text inside expanded panel ----
    async getChangesTextForLevel(level: number): Promise<string> {
        const text = await this.getStepPanel(level).innerText();
        return text.trim();
    }

    async expectVersionListHasItems(level: number): Promise<void> {
        const list = this.getVersionList(level);

        await expect(list, `❌ Change list <ol> for level ${level} is not visible`).toBeVisible();

        const dataLength = await list.getAttribute('data-length');
        expect(dataLength, `❌ data-length attribute missing for level ${level}`).not.toBeNull();

        const lengthNum = Number(dataLength?.split(' ')[0] ?? 0);
        expect(lengthNum, `❌ Expected data-length > 0 for level ${level}, but received ${lengthNum}`).toBeGreaterThan(0);
    }
}