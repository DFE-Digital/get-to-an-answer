import {Locator, expect, Page} from '@playwright/test';

export class ViewQuestionTable {
    readonly root: Locator;
    private headers: Locator;
    //private rows: Locator;
    
    constructor(private page: Page) {
        this.root = page.locator('table');
        this.headers = this.root.locator('thead th');
        //this.rows = this.root.locator('tbody tr');
        
    }
    
    private rows(): Locator {
        return this.root.locator('ol li, ul li, .govuk-summary-list__row');
    }
    
    async count(): Promise<number> {
        return this.rows().count();
    }

    async textByIndex(index: number): Promise<string> {
        return (await this.rows().nth(index - 1).innerText()).trim();
    }

    async allTexts(): Promise<string[]> {
        const rows = this.rows();
        const n = await rows.count();
        const out: string[] = [];
        for (let i = 0; i < n; i++) out.push((await rows.nth(i).innerText()).trim());
        return out;
    }

    // ---------- Row-scoped actions ----------
    async clickEditByIndex(index: number) {
        await this.rows().nth(index - 1).getByRole('link', {name: /^Edit$/}).click();
    }

    async clickEditByText(partialText: string) {
        await this.rows()
            .filter({hasText: partialText})
            .first()
            .getByRole('link', {name: /^Edit$/})
            .click();
    }

    async moveUpByIndex(index: number) {
        const link = this.rows().nth(index - 1).getByRole('link', {name: /^Move up$/});
        await expect(link).toBeVisible();
        await link.click();
    }

    async moveDownByIndex(index: number) {
        const link = this.rows().nth(index - 1).getByRole('link', {name: /^Move down$/});
        await expect(link).toBeVisible();
        await link.click();
    }

    async moveByText(direction: 'up' | 'down', partialText: string) {
        const row = this.rows().filter({hasText: partialText}).first();
        const name = direction === 'up' ? /^Move up$/ : /^Move down$/;
        const link = row.getByRole('link', {name});
        await expect(link).toBeVisible();
        await link.click();
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
        const row = this.rows().filter({hasText: name}).first();
        await expect(row).toBeVisible();
    }

    async getRowCount(): Promise<number> {
        return this.rows().count();
    }
}