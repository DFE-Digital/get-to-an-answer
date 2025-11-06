import {Page, Locator, expect} from '@playwright/test';

export class ViewContributorTable {
    // ===== Locators =====
    private readonly page: Page;
    private readonly table: Locator;
    private readonly bodyRows: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        this.page = page;
        this.table = this.page.locator('main[role="main"] table.govuk-table');
        this.bodyRows = this.table.locator('tbody > tr');
    }

    // ===== Actions =====
    async clickRemoveByEmail(email: string): Promise<void> {
        const row = this.rowByEmail(email);
        await expect(row).toBeVisible();
        await row.locator('button.govuk-button.govuk-button--warning[type="submit"]').click();
    }

    async count(): Promise<number> {
        return this.bodyRows.count();
    }

    // ===== Assertions (structure-only) =====
    async expectVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
    }

    async expectStructure(): Promise<void> {
        await expect(this.table).toBeVisible();

        const sectionCount = await this.table.locator('thead, tbody').count();
        expect(sectionCount).toBeGreaterThan(0);

        const rowCount = await this.bodyRows.count();
        expect(rowCount).toBeGreaterThan(0);

        const firstRowCellCount = await this.bodyRows.first().locator('td').count();
        expect(firstRowCellCount).toBeGreaterThan(2); // >=3 cols (Name, Email, Role)
    }

    // ===== Helpers =====
    private rowByEmail(email: string): Locator {
        return this.bodyRows.filter({hasText: email}).first();
    }
}