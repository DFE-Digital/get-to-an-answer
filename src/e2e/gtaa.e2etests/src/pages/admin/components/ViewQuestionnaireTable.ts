import { expect, Locator, Page } from '@playwright/test';

export class ViewQuestionnaireTable {
    private readonly section: Locator;
    private readonly table: Locator;
    private readonly headers: Locator;
    private readonly rows: Locator;

    constructor(private readonly page: Page) {
        this.section = page.getByRole('region', { name: 'questionnaires' });
        this.table = this.section.getByRole('table', { name: 'questionnaires' });
        this.headers = this.table.getByRole('columnheader');
        this.rows = this.table.locator('tbody tr');
    }

    // --- Basic visibility ---
    async verifyVisible(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.table).toBeVisible();
    }

    // --- Header checks ---
    async verifyHeaders(): Promise<void> {
        await expect(this.headers).toHaveCount(3);
        await expect(this.table.getByRole('columnheader', { name: 'Name' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Created by' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Status' })).toBeVisible();
    }

    // --- Row helpers ---
    private rowByName(name: string): Locator {
        // Row that contains the questionnaire link with the given name
        const linkInRow = this.table.getByRole('link', { name });
        return this.rows.filter({ has: linkInRow }).first();
    }

    // Presence / visibility of a row
    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    // Count of data rows
    async getRowCount(): Promise<number> {
        return this.rows.count();
    }

    // Optional: read fields from a row
    async getCreatedBy(name: string): Promise<string> {
        return this.rowByName(name).locator('td').nth(1).innerText();
    }

    async getStatus(name: string): Promise<string> {
        return this.rowByName(name).locator('td').nth(2).innerText();
    }
}