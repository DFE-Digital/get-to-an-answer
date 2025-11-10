import {expect, Locator, Page} from '@playwright/test';

export class ViewQuestionnaireTable {
    // ===== Locators =====
    private readonly section: Locator;
    private readonly table: Locator;
    private readonly headers: Locator;
    private readonly rows: Locator;
    private readonly tableHeading: Locator;
    private readonly firstTitleLink: Locator;

    // ===== Constructor =====
    constructor(private readonly page: Page) {
        this.section = page.getByRole('region', {name: /questionnaires/i});
        this.table = this.section.getByRole('table', {name: /questionnaires/i});
        this.headers = this.table.getByRole('columnheader');
        this.rows = this.table.locator('tbody tr');
        this.tableHeading = this.page.locator('table.govuk-table > caption.govuk-table__caption');
        this.firstTitleLink = this.table.locator('tbody tr td a').first();
    }

    // ===== Basic visibility =====
    async verifyVisible(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.tableHeading).toBeVisible();
        await expect(this.table).toBeVisible();
    }

    async verifyFirstTitleIsLink() {
        await expect(this.firstTitleLink).toBeVisible();
    }

    async verifyHeaders(): Promise<void> {
        await expect(this.headers).toHaveCount(3);
        await expect(this.table.getByRole('columnheader', {name: 'Name'})).toBeVisible();
        await expect(this.table.getByRole('columnheader', {name: 'Created by'})).toBeVisible();
        await expect(this.table.getByRole('columnheader', {name: 'Status'})).toBeVisible();
    }

    // ===== Actions =====
    async clickFirstQuestionnaireTitle() {
        await this.firstTitleLink.click();
    }

    // ===== Row helpers =====
    private rowByName(name: string): Locator {
        // Row that contains the questionnaire link with the given name
        const linkInRow = this.table.getByRole('link', {name});
        return this.rows.filter({has: linkInRow}).first();
    }

    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    async getRowCount(): Promise<number> {
        return this.rows.count();
    }

    private escapeCss(s: string): string {
        return s.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
    }

    async getStatus(title: string): Promise<string> {
        const row = this.page.locator(
            `table.govuk-table tr:has(a:has-text("${this.escapeCss(title)}"))`
        );
        await row.waitFor({state: 'attached', timeout: 15000});
        const cell = row.locator('td:last-child');
        const raw = (await cell.textContent()) ?? '';
        return raw.replace(/\s+/g, ' ').trim();
    }
}