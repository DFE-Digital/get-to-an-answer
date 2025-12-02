import {expect, Locator, Page} from '@playwright/test';
import {Timeouts} from "../../../constants/timeouts";
import { formatTimestampToLongDate } from '../../../helpers/utils';

export class ViewQuestionnaireTable {
    // ===== Locators =====
    private readonly section: Locator;
    private readonly table: Locator;
    private readonly headers: Locator;
    private readonly rows: Locator;
    private readonly firstTitleLink: Locator;

    // ===== Constructor =====
    constructor(private readonly page: Page) {
        this.section = page.getByRole('region', {name: /questionnaires/i});
        this.table = this.page
            .getByRole('region', { name: /questionnaires/i })
            .locator('table.govuk-table');
        this.headers = this.table.getByRole('columnheader');
        this.rows = this.table.locator('tbody tr');
        this.firstTitleLink = this.table.locator('tbody tr td a').first();
    }

    // ===== Basic visibility =====
    async verifyVisible(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.table).toBeVisible();
    }

    async verifyFirstTitleIsLink() {
        await expect(this.firstTitleLink).toBeVisible();
    }

    async verifyHeaders(): Promise<void> {
        await expect(this.headers).toHaveCount(4);
        await expect(this.table.getByRole('columnheader', {name: 'Name'})).toBeVisible();
        await expect(this.table.getByRole('columnheader', {name: 'Created by'})).toBeVisible();
        await expect(this.table.getByRole('columnheader', {name: 'Last updated'})).toBeVisible();
        await expect(this.table.getByRole('columnheader', {name: 'Status'})).toBeVisible();
    }

    // ===== Actions =====
    async clickFirstQuestionnaireTitle() {
        await this.firstTitleLink.click();
    }

    // ===== Row helpers =====
    private rowByName(name: string): Locator {
        // Row that contains the questionnaire link with the given name
        const linkInRow = this.table.getByRole('link', {name});
        return this.rows.filter({has: linkInRow}).first();
    }

    async expectRowPresentByName(name: string): Promise<void> {
        await expect(this.rowByName(name)).toBeVisible();
    }

    async getRowCount(): Promise<number> {
        return this.rows.count();
    }

    private cleanText(text: string): string {
        return text.replace(/\s+/g, ' ').trim();
    }

    async verifyTableData(expectedRows: {
        title: string;
        createdBy: string;
        status: string;
        updatedAt: string
    }[]): Promise<void> {
        const rows = this.table.locator('tbody tr');

        // ✅ Wait for at least one row to appear
        await expect(rows.first()).toBeVisible();
        await expect(rows).toHaveCount(expectedRows.length);

        const rowCount = await rows.count();

        expect(rowCount, `Expected ${expectedRows.length} rows but found ${rowCount}`).toBe(expectedRows.length);

        for (let i = 0; i < rowCount; i++) {
            const row = rows.nth(i);
            await expect(row).toBeVisible({timeout: Timeouts.LONG});

            const titleText = this.cleanText(await row.locator('td').nth(0).innerText());
            const createdByText = this.cleanText(await row.locator('td').nth(1).innerText());
            const lastpdatedAtText = this.cleanText(await row.locator('td').nth(2).innerText());
            const statusText = this.cleanText(await row.locator('td').nth(3).innerText());

            const expected = expectedRows[i];

            expect(titleText, `Row ${i + 1}: title mismatch`).toBe(expected.title);
            const emailPrefix = expected.createdBy.split('@')[0];

            // Normalize both strings: replace hyphens with spaces for comparison
            const normalizedCreatedByText = createdByText.toLowerCase().replace(/-/g, ' ');
            const normalizedEmailPrefix = emailPrefix.toLowerCase().replace(/-/g, ' ');
            expect(normalizedCreatedByText).toContain(normalizedEmailPrefix);

            const date = formatTimestampToLongDate(expected.updatedAt);
            expect(lastpdatedAtText, `Row ${i + 1}: last updated mismatch`).toBe(date);
            expect(statusText, `Row ${i + 1}: status mismatch`).toBe(expected.status);
        }
    }
}