import {Page, Locator, expect} from '@playwright/test';

export class ViewContributorTable {
    // ===== Locators =====
    private readonly page: Page;
    private readonly table: Locator;
    private readonly bodyRows: Locator;
    private readonly removeButtons: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        this.page = page;
        this.table = this.page.locator('table.govuk-table');
        this.bodyRows = this.table.locator('tbody > tr');
        this.removeButtons = this.table.locator('button.govuk-button.govuk-button--warning[type="submit"]');
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

    async expectedRemoveContributorButtonsDisabled() {
        const buttons = await this.removeButtons.all();
        for (const button of buttons) {
            await expect(button).toBeDisabled();
        }
    }
    
    async expectedRemoveContributorButtonsEnabled() {
        const buttons = await this.removeButtons.all();
        for (const button of buttons) {
            await expect(button).toBeEnabled();
        }
    }

    // ===== Helpers =====
    public rowByEmail(email: string): Locator {
        return this.bodyRows.filter({hasText: email}).first();
    }

    public async hasPerson(emailOrId: string): Promise<boolean> {
        const count = await this.bodyRows.filter({hasText: emailOrId}).count()
        return count > 0;
    }

    public rowByIndex(index: number): Locator {
        // 1-based index external API → 0-based nth()
        return this.bodyRows.nth(index - 1);
    }
}