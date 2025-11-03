import { expect, Locator, Page } from '@playwright/test';

export class ViewQuestionnaireTable {
    private root: Locator;
    private headers: Locator;
    private rows: Locator;

    constructor(private page: Page) {
        this.root = page.locator('table'); 
        this.headers = this.root.locator('thead th');
        this.rows = this.root.locator('tbody tr');
    }

    async verifyVisible(): Promise<void> {
        await expect(this.root).toBeVisible();
    }

    async verifyHeaders(): Promise<void> {
        // just verifies header presence, not text
        await expect(this.headers).toHaveCount(3);
        for (let i = 0; i < 3; i++) {
            await expect(this.headers.nth(i)).toBeVisible();
        }
    }

    async expectRowPresentByName(name: string): Promise<void> {
        const row = this.rows.filter({hasText: name}).first();
        await expect(row).toBeVisible();
    }

    async getRowCount(): Promise<number> {
        return this.rows.count();
    }
}