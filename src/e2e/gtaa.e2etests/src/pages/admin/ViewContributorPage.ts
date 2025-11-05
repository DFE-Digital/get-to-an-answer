import {Page, Locator, expect} from '@playwright/test';
import {ViewContributorTable} from './components/ViewContributorTable';

export class AddContributorPage {
    // ===== Embedded component =====
    readonly table: ViewContributorTable;

    // ===== Locators (declared) =====
    private readonly page: Page;
    private readonly section: Locator;
    private readonly backLink: Locator;
    private readonly h1: Locator;
    private readonly addContributorLink: Locator;
    private readonly tableRegion: Locator;

    // ===== Constructor (init locators & component) =====
    constructor(protected readonly p: Page) {
        this.page = p;

        this.section = this.page.locator('main[role="main"]');
        this.backLink = this.section.locator('a.govuk-back-link');
        this.h1 = this.section.locator('h1');
        this.addContributorLink = this.section.locator('a[role="button"][href$="/contributors/new"]');
        this.tableRegion = this.section.locator('table.govuk-table');

        this.table = new ViewContributorTable(this.page);
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