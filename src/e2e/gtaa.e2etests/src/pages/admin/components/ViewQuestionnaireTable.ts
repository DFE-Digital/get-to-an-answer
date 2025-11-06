import { expect, Locator, Page } from '@playwright/test';

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
        this.section = page.getByRole('region', { name: 'questionnaires' });
        this.table = this.section.getByRole('table', { name: 'questionnaires' });
        this.headers = this.table.getByRole('columnheader');
        this.rows = this.table.locator('tbody tr');
        this.tableHeading = this.table.locator('table.govuk-table > caption.govuk-table__caption');
        this.firstTitleLink = this.table.locator('').first();
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
        await expect(this.table.getByRole('columnheader', { name: 'Name' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Created by' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Status' })).toBeVisible();
    }

    // ===== Actions =====
    async clickFirstQuestionnaireTitle() {
        await this.firstTitleLink.click();
    }
    
    // ===== Row helpers =====
    private rowByName(name: string): Locator {
        // Row that contains the questionnaire link with the given name
        const linkInRow = this.table.getByRole('link', { name });
        return this.rows.filter({ has: linkInRow }).first();
    }
    
    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }
    
    async getRowCount(): Promise<number> {
        return this.rows.count();
    }

    async getStatus(name: string): Promise<string> {
        return this.rowByName(name).locator('td').nth(2).innerText();
    }
    
    // Optional: read fields from a row
    async getCreatedBy(name: string): Promise<string> {
        return this.rowByName(name).locator('td').nth(1).innerText();
    }
}