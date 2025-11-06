import {Page, Locator, expect} from '@playwright/test';
import {ViewContributorTable} from './components/ViewContributorTable';
import {BasePage} from "../BasePage";

export class AddContributorPage extends BasePage{
    // ===== Locators =====
    private readonly section=this.page.locator('main[role="main"]');
    private readonly backLink=this.section.locator('a.govuk-back-link');
    private readonly h1=this.section.locator('h1');
    private readonly addContributorLink=this.section.locator('a[role="button"][href$="/contributors/new"]');
    private readonly tableRegion=this.section.locator('table.govuk-table');

    // ===== Embedded component =====
    readonly table: ViewContributorTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
        this.table = new ViewContributorTable(page);
    }
    
    // ===== Actions =====
    async clickAddContributor(): Promise<void> {
        await this.addContributorLink.click();
    }

    // ===== Assertions (structure-only) =====
    async verifyBasicStructure(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.backLink).toBeVisible();
        await expect(this.h1).toBeVisible();

        await expect(this.addContributorLink).toBeVisible();
        await expect(this.addContributorLink).toHaveAttribute('role', 'button');
        await expect(this.addContributorLink).toHaveAttribute('href', /\/contributors\/new$/);
    }

    async verifyContributorsTablePresent(): Promise<void> {
        await expect(this.tableRegion).toBeVisible();

        const rowCount = await this.tableRegion.locator('tr').count();
        expect(rowCount).toBeGreaterThan(0);

        const cellCount = await this.tableRegion.locator('td, th').count();
        expect(cellCount).toBeGreaterThan(0);
    }
}