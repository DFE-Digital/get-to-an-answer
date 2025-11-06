import { expect, Locator, Page } from '@playwright/test';

export class ViewQuestionTable {
    // ===== Locators =====
    private readonly list: Locator; 
    private readonly rows: Locator; 
    private readonly keys: Locator; 
    private readonly values: Locator;

    // ===== Constructor =====
    constructor(private readonly page: Page) {
        const section = page.getByRole('heading', { level: 2, name: 'Your questions' }).locator('..'); // parent
        this.list = section.locator('dl.govuk-summary-list');
        this.rows = this.list.locator('div.govuk-summary-list__row');
        this.keys = this.rows.locator('dt.govuk-summary-list__key');
        this.values = this.rows.locator('dd.govuk-summary-list__value');
    }

    // ===== Basic validation =====
    async verifyVisible(): Promise<void> {
        await expect(this.list).toBeVisible();
    }

    async count(): Promise<number> {
        return this.rows.count();
    }

    // ===== Text helpers =====
    async textByIndex(index: number): Promise<string> {
        // 1-based index, returns the key text (e.g. “question 1”)
        return (await this.keys.nth(index - 1).innerText()).trim();
    }

    async allText(): Promise<string[]> {
        const n = await this.count();
        const out: string[] = [];
        for (let i = 0; i < n; i++) {
            out.push((await this.keys.nth(i).innerText()).trim());
        }
        return out;
    }

    // ===== Row finders =====
    private rowByIndex(index: number): Locator {
        return this.rows.nth(index - 1);
    }

    private rowByName(partialText: string): Locator {
        // Matches by the key/value text contained in the row
        return this.rows.filter({ hasText: partialText }).first();
    }

    // ===== Actions =====
    async clickEditByIndex(index: number): Promise<void> {
        const link = this.rowByIndex(index).getByRole('link', { name: /Edit/i });
        await expect(link).toBeVisible();
        await link.click();
    }

    async moveUpByIndex(index: number): Promise<void> {
        const link = this.rowByIndex(index).getByRole('link', { name: /Move up/i });
        await expect(link).toBeVisible();
        await link.click();
    }

    async moveDownByIndex(index: number): Promise<void> {
        const link = this.rowByIndex(index).getByRole('link', { name: /Move down/i });
        await expect(link).toBeVisible();
        await link.click();
    }
    
    async moveByName(direction: 'up' | 'down', partialText: string): Promise<void> {
        const row = this.rowByName(partialText);
        await expect(row).toBeVisible();
        const link = row.getByRole('link', { name: direction === 'up' ? /Move up/i : /Move down/i });
        await expect(link).toBeVisible();
        await link.click();
    }

    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    // Optional getters if you need values
    async valueTextByIndex(index: number): Promise<string> {
        return (await this.values.nth(index - 1).innerText()).trim();
    }
}