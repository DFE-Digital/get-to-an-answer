import {expect, Locator, Page} from '@playwright/test';

export class ViewQuestionTable {
    // ===== Locators =====
    private readonly list: Locator;
    private readonly rows: Locator;
    private readonly keys: Locator;
    private readonly values: Locator;
    private readonly moveUpLinks: Locator;
    private readonly moveDownLinks: Locator;
    private readonly editLinks: Locator;
    private readonly page: Page;


    // ===== Constructor =====
    constructor(page: Page) {
        this.page = page;

        const section = page
            .getByRole('heading', {level: 2, name: 'Your questions'})
            .locator('..');
        this.list = section.locator('ul.govuk-list');
        this.rows = this.list.locator('li');
        this.keys = this.rows.locator('div.govuk-grid-column-two-thirds p.govuk-body');
        this.values = this.rows.locator('div.govuk-grid-column-one-third');
        this.moveUpLinks = this.rows.locator('a, button').filter({hasText: /Move up/i});
        this.moveDownLinks = this.rows.locator('a, button').filter({hasText: /Move down/i});
        this.editLinks = this.rows.getByRole('link', {name: /Edit/i});
    }

    // ===== Validation =====
    async verifyVisible(): Promise<void> {
        await expect(this.list).toBeVisible();
    }

    async count(): Promise<number> {
        return this.rows.count();
    }

    async expectReorderControlsOnRowByIndex(index: number): Promise<void> {
        const row = this.rowByIndex(index);
        await expect(row).toBeVisible();

        const rowContent = await row.innerHTML();
        console.log(`Row ${index} HTML:`, rowContent);

        const moveUpLink = this.getMoveUpLinkForRow(row);
        const moveDownLink = this.getMoveDownLinkForRow(row);

        await expect(moveUpLink, `❌ Move up button not visible on row ${index}`).toBeVisible();
        await expect(moveDownLink, `❌ Move down button not visible on row ${index}`).toBeVisible();
    }

    private getMoveUpLinkForRow(row: Locator): Locator {
        return row.locator('a, button').filter({hasText: /Move up/i}).first();
    }

    private getMoveDownLinkForRow(row: Locator): Locator {
        return row.locator('a, button').filter({hasText: /Move down/i}).first();
    }

    async textByIndex(index: number): Promise<string> {
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
        return this.rows.filter({hasText: partialText}).first();
    }

    // ===== Actions =====
    async clickEditByIndex(index: number): Promise<void> {
        const link = this.rowByIndex(index).getByRole('link', {name: /Edit/i});
        await expect(link).toBeVisible();
        await link.click();
    }

    async moveUpByIndex(index: number): Promise<void> {
        const row = this.rowByIndex(index);
        const link = this.getMoveUpLinkForRow(row);
        await expect(link).toBeVisible();
        await link.click();
    }
    
    async moveDownByIndex(index: number): Promise<void> {
        const row = this.rowByIndex(index);
        const link = this.getMoveDownLinkForRow(row);
        await expect(link).toBeVisible();
        await link.click();
    }


    async moveByName(direction: 'up' | 'down', partialText: string): Promise<void> {
        const row = this.rowByName(partialText);
        await expect(row).toBeVisible();
        const link = direction === 'up'
            ? this.getMoveUpLinkForRow(row)
            : this.getMoveDownLinkForRow(row);
        await expect(link).toBeVisible();
        await link.click();
    }

    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    // ===== Getters =====
    async valueTextByIndex(index: number): Promise<string> {
        return (await this.values.nth(index - 1).innerText()).trim();
    }
}