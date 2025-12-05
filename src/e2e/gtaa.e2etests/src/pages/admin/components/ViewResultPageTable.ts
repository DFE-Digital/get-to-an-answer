import { expect, Locator, Page } from '@playwright/test';

export class ViewResultPageTable {
    // ===== Locators =====
    private readonly list: Locator;
    private readonly rows: Locator;
    private readonly keys: Locator;
    private readonly editLinks: Locator;
    private readonly page: Page;

    // ===== Constructor =====
    constructor(page: Page) {
        this.page = page;

        // Section that contains the "Your questions" heading + list
        const section = page
            .locator('#view-results-pages-heading')
            .locator('..');

        // GOV.UK summary list
        this.list = section.locator('dl.govuk-summary-list');

        // Each question is a summary-list row
        this.rows = this.list.locator('div.govuk-summary-list__row');

        // Left-hand column text (key)
        this.keys = this.rows.locator('dt.govuk-summary-list__key');

        // Convenience locators for actions in rows
        this.editLinks = this.rows.getByRole('link', { name: /Edit/i });
    }

    // ===== Validation =====
    async verifyVisible(): Promise<void> {
        await expect(this.list).toBeVisible();
    }

    async count(): Promise<number> {
        return this.rows.count();
    }

    async textByIndex(index: number): Promise<string> {
        return (await this.keys.nth(index - 1).innerText()).trim();
    }

    async allResultsPageTitles(): Promise<string[]> {
        const n = await this.count();
        const out: string[] = [];

        for (let i = 0; i < n; i++) {
            const raw = await this.keys.nth(i).innerText();
            const cleaned = raw.trim().replace(/^"|"$/g, ''); // remove surrounding quotes if present
            out.push(cleaned);
        }

        return out;
    }
    
    // ===== Row finders =====
    private rowByIndex(index: number): Locator {
        // 1-based index external API → 0-based nth()
        return this.rows.nth(index - 1);
    }

    private rowByName(partialText: string): Locator {
        // Match by any text in the row (key/action label/etc.)
        return this.rows.filter({ hasText: partialText }).first();
    }

    // ===== Actions =====
    async clickEditByIndex(index: number): Promise<void> {
        const link = this.rowByIndex(index).getByRole('link', { name: /Edit/i });
        await expect(link).toBeVisible();
        await link.click();
    }

    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    // ===== Getters =====
    async valueTextByIndex(index: number): Promise<string> {
        return (await this.keys.nth(index - 1).innerText()).trim();
    }

    async verifyListExistsWithTitleAndActions(): Promise<void> {
        // Verify the list is visible
        await expect(this.list, '❌ Results page list not visible').toBeVisible();

        // Verify there are results pages in the list
        const rowCount = await this.count();
        expect(rowCount, '❌ No results pages found in the list').toBeGreaterThan(0);

        // Verify each row has order number, content, Edit link, and appropriate Move actions
        for (let i = 1; i <= rowCount; i++) {
            const row = this.rowByIndex(i);

            // Verify row is visible
            await expect(row, `❌ Row ${i} not visible`).toBeVisible();

            // Verify row contains text content (question content)
            const fullRowText = await row.innerText();
            expect(fullRowText, `❌ Row ${i} has no visible content`).toBeTruthy();
            expect(fullRowText.length, `❌ Row ${i} content is empty`).toBeGreaterThan(0);

            // Verify Edit link exists and is visible
            const editLink = row.getByRole('link', { name: /Edit/i });
            await expect(editLink, `❌ Edit link not visible on row ${i}`).toBeVisible();
        }
    }
}