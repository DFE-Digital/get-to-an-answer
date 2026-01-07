import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {Timeouts} from "../../constants/timeouts";

export class AddBulkAnswerOptionsPage extends BasePage {
    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly bulkTextArea: Locator;
    private readonly continueButton: Locator;
    private readonly hint: Locator;

    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLink: Locator;
    private readonly inlineError: Locator;
    private readonly formGroup: Locator;
    private readonly backButton: Locator;

    private readonly enteredBulkOptions: string[] = [];
    
    public get getEnteredBulkOptions(): string[] {
        return this.enteredBulkOptions;
    }

    public async getCurrentBulkOptions(): Promise<string[]> {
        // Get the current content of the textarea
        const currentContent = await this.bulkTextArea.inputValue();

        // Split the content into lines and filter out empty lines
        return currentContent.split(/\r?\n/).filter(line => line.length > 0);
    }
    
    public clearEnterBulkOptions(): void {
        this.enteredBulkOptions.length = 0;
    }
    
    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');
        this.caption = page.locator('.govuk-caption-l');
        this.bulkTextArea = page.locator('textarea#BulkAnswerOptionsRawText');
        this.continueButton = page.getByRole('button', {name: 'Continue'});
        this.hint = page.locator('#BulksAnswerOptions-hint');

        this.errorSummary = page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLink = page.locator('a[href="#BulkAnswerOptionsRawText"]');
        this.inlineError = page.locator('#bulk-options-error');
        this.formGroup = page.locator('.govuk-form-group:has(#BulkAnswerOptionsRawText)');
        this.backButton = page.locator('a.govuk-back-link');
    }
    
    // ===== Actions =====
    
    async enterBulkOptions(text: string): Promise<void> {
        await this.bulkTextArea.fill(text);
    }

    async enterNumberOfValidBulkOptions(numberOfOptions : number): Promise<void> {
        let bulkText = '';
        
        for (let i = 0; i < numberOfOptions; i++) {
            const randomString = Math.random().toString(36).substring(2, 8);
            const inputString = `Option ${randomString}`;
            this.enteredBulkOptions.push(inputString);
            bulkText += `${inputString}\n`;
        }
        await this.bulkTextArea.fill(bulkText);
    }

    async clearBulkOptions(): Promise<void> {
        await this.bulkTextArea.clear();
    }

    async clickContinue(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.continueButton.click()
        ]);
    }

    async clickBackButton(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backButton.click()
        ]);
    }



    async moveBottomOptionToTop(): Promise<void> {
        // Get the current content of the textarea
        const currentContent = await this.bulkTextArea.inputValue();

        // Split the content into lines
        const lines = currentContent.trim().split(/\r?\n/).filter(line => line.trim() !== '');

        if (lines.length < 2) {
            // If there are fewer than 2 lines, no need to move
            return;
        }

        // Remove and store the last line
        const bottomLine = lines.pop()!;  // Use non-null assertion after checking length

        // Prepend the bottom line to the beginning of the lines array
        lines.unshift(bottomLine);

        // Reconstruct the textarea content
        const newContent = lines.join('\n');

        // Fill the textarea with the new content
        await this.bulkTextArea.fill(newContent);
    }
    
    async removeRandomEntry(): Promise<string | undefined> {
        // Get the current content of the textarea
        const currentContent = await this.bulkTextArea.inputValue();

        // Split the content into lines
        const lines = currentContent.trim().split(/\r?\n/)
            .filter(line => line.trim() !== '');

        // Check if there are any entries to remove
        if (lines.length === 0) {
            return undefined;  // No entries to remove
        }

        // Select a random index
        const randomIndex = Math.floor(Math.random() * lines.length);

        // Remove the randomly selected line
        const removedEntry = lines[randomIndex];
        const filteredLines = lines.filter((_, index) => index !== randomIndex);

        // Reconstruct the textarea content
        const newContent = filteredLines.join('\n');

        // Fill the textarea with the new content
        await this.bulkTextArea.fill(newContent);

        // Update the enteredBulkOptions array to reflect the removal
        const optionIndex = this.enteredBulkOptions.findIndex(option =>
            option.trim() === removedEntry.trim()
        );

        if (optionIndex !== -1) {
            this.enteredBulkOptions.splice(optionIndex, 1);
        }

        return removedEntry;
    }

    async addRandomEntryAtBottom(): Promise<string> {
        // Generate a random option
        const randomString = Math.random().toString(36).substring(2, 8);
        const newOption = `Option ${randomString}`;

        // Get the current content of the textarea
        const currentContent = await this.bulkTextArea.inputValue();

        // Determine the new content (add a newline if the textarea is not empty)
        const updatedContent = currentContent
            ? `${currentContent}\n${newOption}`
            : newOption;

        // Fill the textarea with the updated content
        await this.bulkTextArea.fill(updatedContent);

        // Update the enteredBulkOptions array
        this.enteredBulkOptions.push(newOption);

        return newOption;
    }

    async addDuplicateEntryAtBottom(): Promise<string | undefined> {
        // Get the current content of the textarea
        const currentContent = await this.bulkTextArea.inputValue();

        // Split the content into lines
        const lines = currentContent.trim().split(/\r?\n/)
            .filter(line => line.length > 0);

        // Check if there are any existing entries to duplicate
        if (lines.length === 0) {
            return undefined;
        }

        // Select a random existing entry to duplicate
        const randomIndex = Math.floor(Math.random() * lines.length);
        const duplicateEntry = lines[randomIndex];

        // Determine the new content
        const updatedContent = `${currentContent}\n${duplicateEntry}`;

        // Fill the textarea with the updated content
        await this.bulkTextArea.fill(updatedContent);

        // Add to enteredBulkOptions array
        this.enteredBulkOptions.push(duplicateEntry);

        return duplicateEntry;
    }

    // ===== Validations =====

    async expectOnPage(): Promise<void> {
        await expect(this.heading).toContainText(/Enter all answer options/i);
        await expect(this.bulkTextArea).toBeVisible();
        await expect(this.continueButton).toBeVisible();
    }

    async validateDuplicateEntriesError(browserName: string) {
        const expectedMessage = 'Duplicate entries found';
        await expect(this.errorSummary).toBeVisible();
        await expect(this.errorList).toContainText(expectedMessage);
        await expect(this.inlineError).toContainText(expectedMessage);

        if (browserName !== 'webkit') {
            await this.errorLink.click();
            await expect(this.bulkTextArea).toBeFocused();
        }
    }

    async validateAllOptionContents(
        expectedValues: string[],
        opts?: { stripSuffix?: boolean }
    ): Promise<void> {
        const { stripSuffix = false } = opts ?? {};
        
        const raw = (await this.bulkTextArea.inputValue()).trim();

        const lines = raw
            .split(/\r?\n/)
            .map(l => l.trim())
            .filter(Boolean);

        const actualValues = stripSuffix
            ? lines.map(line => {
                const idx = line.indexOf(' - ');
                return (idx >= 0 ? line.slice(0, idx) : line).trim();
            })
            : lines;

        expect(
            actualValues.length,
            '❌ Number of answer options does not match expected count'
        ).toBe(expectedValues.length);

        for (let i = 0; i < expectedValues.length; i++) {
            expect(
                actualValues[i],
                `❌ Answer option at index ${i} mismatch`
            ).toBe(expectedValues[i]);
        }
    }
    
    async assertAllOptionNumberLabelsInOrder(expectedOptionCount?: number): Promise<void> {
        if (await this.bulkTextArea.count()) {
            const raw = (await this.bulkTextArea.inputValue()).trim();

            const lines = raw
                .split(/\r?\n/)
                .map(l => l.trim())
                .filter(Boolean);

            if (expectedOptionCount !== undefined) {
                expect(
                    lines.length,
                    '❌ Number of bulk answer option lines does not match expected count'
                ).toBe(expectedOptionCount);
            } else {
                // At least ensure there's something there if caller didn’t pass a count
                expect(lines.length, '❌ No bulk answer options found in textarea').toBeGreaterThan(0);
            }

            return;
        }

        // Otherwise fall back to the original UI (per-option inputs with labels)
        const labels = this.page.locator('label.govuk-label.govuk-label--s[for*="OptionContent"]');
        const count = await labels.count();
        const seenLabels = new Set<string>();
        let expectedTextIndex = 0;

        for (let i = 0; i < count; i++) {
            const label = labels.nth(i);
            const textContent = (await label.textContent())?.trim() ?? '';

            if (!textContent.startsWith('Option ')) continue;

            expectedTextIndex++;
            const expectedText = `Option ${expectedTextIndex}`;

            expect(textContent, `❌ Label ${expectedTextIndex} text mismatch`).toBe(expectedText);
            expect(seenLabels.has(textContent), `❌ Duplicate label detected: ${textContent}`).toBe(false);
            seenLabels.add(textContent);
        }

        if (expectedOptionCount !== undefined) {
            expect(
                expectedTextIndex,
                '❌ Number of option labels found does not match expected count'
            ).toBe(expectedOptionCount);
        }
    }


    async validateAriaDescribedBy() {
        await this.bulkTextArea.waitFor({state: 'visible', timeout: Timeouts.LONG});
        const ariaValue = await this.bulkTextArea.getAttribute('aria-describedby');

        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();
        expect(ariaValue).toContain('BulksAnswerOptions-hint');
    }
}