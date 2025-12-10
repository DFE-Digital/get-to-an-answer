import { expect, Locator, Page } from '@playwright/test';

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

        // Section that contains the "Your questions" heading + list
        const section = page
            .getByRole('heading', { level: 2, name: 'Your questions' })
            .locator('..');

        // GOV.UK summary list
        this.list = section.locator('dl.govuk-summary-list');

        // Each question is a summary-list row
        this.rows = this.list.locator('div.govuk-summary-list__row');

        // Left-hand column text (key)
        this.keys = this.rows.locator('dt.govuk-summary-list__key');

        // Value column (currently empty in your screenshot but keep for future)
        this.values = this.rows.locator('dd.govuk-summary-list__value');

        // Convenience locators for actions in rows
        this.moveUpLinks = this.rows.getByRole('button', { name: /Move up/i });
        this.moveDownLinks = this.rows.getByRole('button', { name: /Move down/i });
        this.editLinks = this.rows.getByRole('link', { name: /Edit/i });
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

        await expect(
            moveUpLink,
            `❌ Move up button not visible on row ${index}`
        ).toBeVisible();

        await expect(
            moveDownLink,
            `❌ Move down button not visible on row ${index}`
        ).toBeVisible();
    }

    private getMoveUpLinkForRow(row: Locator): Locator {
        return row.getByRole('button', { name: /Move up/i });
    }

    private getMoveDownLinkForRow(row: Locator): Locator {
        return row.getByRole('button', { name: /Move down/i });
    }

    async textByIndex(index: number): Promise<string> {
        return (await this.keys.nth(index - 1).innerText()).trim();
    }

    async allQuestionContent(): Promise<string[]> {
        const n = await this.count();
        const out: string[] = [];

        for (let i = 0; i < n; i++) {
            const raw = await this.values.nth(i).innerText();
            const cleaned = raw.trim().replace(/^"|"$/g, ''); // remove surrounding quotes if present
            out.push(cleaned);
        }

        return out;
    }

    async verifyMoveUpLinkNotVisibleForFirstRow(): Promise<void> {
        const firstRow = this.rowByIndex(1);
        const moveUpLink = this.getMoveUpLinkForRow(firstRow);
        await expect(
            moveUpLink,
            '❌ Move up link should not be visible on the first row'
        ).not.toBeVisible();
    }

    async verifyMoveDownLinkNotVisibleForLastRow(): Promise<void> {
        const rowCount = await this.count();
        const lastRow = this.rowByIndex(rowCount);
        const moveDownLink = this.getMoveDownLinkForRow(lastRow);
        await expect(
            moveDownLink,
            '❌ Move down link should not be visible on the last row'
        ).not.toBeVisible();
    }

    async verifyListExistsWithOrderNumbersContentAndActions(): Promise<void> {
        // Verify the list is visible
        await expect(this.list, '❌ Questions list not visible').toBeVisible();

        // Verify there are questions in the list
        const rowCount = await this.count();
        expect(rowCount, '❌ No questions found in the list').toBeGreaterThan(0);

        // Verify each row has order number, content, Edit link, and appropriate Move actions
        for (let i = 1; i <= rowCount; i++) {
            const row = this.rowByIndex(i);

            // Verify row is visible
            await expect(row, `❌ Row ${i} not visible`).toBeVisible();

            // Verify row contains text content (both order number and question content)
            const fullRowText = await row.innerText();
            expect(fullRowText, `❌ Row ${i} has no visible content`).toBeTruthy();
            expect(fullRowText.length, `❌ Row ${i} content is empty`).toBeGreaterThan(0);

            // Verify Edit link exists and is visible
            const editLink = row.getByRole('link', { name: /Edit/i });
            await expect(editLink, `❌ Edit link not visible on row ${i}`).toBeVisible();

            // Verify Move up/down actions where applicable
            const moveUpLink = this.getMoveUpLinkForRow(row);
            const moveDownLink = this.getMoveDownLinkForRow(row);

            if (i === 1) {
                // First row should not have Move up
                await expect(
                    moveUpLink,
                    `❌ Move up link should not be visible on first row`
                ).not.toBeVisible();
                // First row should have Move down if more than 1 question
                if (rowCount > 1) {
                    await expect(
                        moveDownLink,
                        `❌ Move down link not visible on first row when multiple questions exist`
                    ).toBeVisible();
                }
            } else if (i === rowCount) {
                // Last row should not have Move down
                await expect(
                    moveDownLink,
                    `❌ Move down link should not be visible on last row`
                ).not.toBeVisible();
                // Last row should have Move up
                await expect(
                    moveUpLink,
                    `❌ Move up link not visible on last row`
                ).toBeVisible();
            } else {
                // Middle rows should have both Move up and Move down
                await expect(
                    moveUpLink,
                    `❌ Move up link not visible on row ${i}`
                ).toBeVisible();
                await expect(
                    moveDownLink,
                    `❌ Move down link not visible on row ${i}`
                ).toBeVisible();
            }
        }
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

    async clickEditByQuestionContent(questionContent: string): Promise<void> {
        const row = this.rows.filter({ hasText: questionContent }).first();
        await expect(row, `❌ Row with question content "${questionContent}" not found`).toBeVisible();

        const editLink = row.getByRole('link', { name: /Edit/i });
        await expect(editLink, `❌ Edit link not visible for question "${questionContent}"`).toBeVisible();
        await editLink.click();
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

        const link =
            direction === 'up'
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