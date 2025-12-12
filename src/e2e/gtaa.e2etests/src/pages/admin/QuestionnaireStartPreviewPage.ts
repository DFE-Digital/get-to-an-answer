import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from '../BasePage';

export class QuestionnaireStartPreviewPage extends BasePage {
    // ===== Locators =====
    readonly outerHeaderSection: Locator;
    readonly outerHeading: Locator;            // Heading when no decorative image & not embedded
    readonly outerImage: Locator;              // Decorative image when present

    readonly innerFieldset: Locator;           // Fieldset when decorative image or embedded
    readonly innerHeading: Locator;            // Inner H1 inside fieldset
    readonly description: Locator;

    readonly errorSummary: Locator;

    readonly startButton: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.outerHeaderSection = this.page.locator('#main-content-header');
        this.outerHeading = this.outerHeaderSection.locator('h1.govuk-heading-xl');
        this.outerImage = this.outerHeaderSection.locator('img.gtaa-preview-image');

        this.innerFieldset = this.page.locator('fieldset.govuk-fieldset');
        this.innerHeading = this.innerFieldset.locator('#start-page-display-title');

        this.description = this.page.locator('#start-page-details').first();

        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');

        this.startButton = this.page.locator(
            '#start-page-start-now-btn'
        );
    }

    // ===== Assertions =====

    async assertStructure(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();

        // One of: header with H1 OR decorative image
        const hasOuterHeading = await this.outerHeading.isVisible().catch(() => false);
        const hasImage = await this.outerImage.isVisible().catch(() => false);
        const hasInnerFieldset = await this.innerFieldset.isVisible().catch(() => false);

        expect(hasOuterHeading || hasImage).toBeTruthy();

        if (hasOuterHeading) {
            const text = (await this.outerHeading.textContent())?.trim() ?? '';
            expect(text.length).toBeGreaterThan(0);
        }

        if (hasImage) {
            await expect(this.outerImage).toHaveAttribute('src', /\/admin\/questionnaires\/.+\/decorative-image/);
        }

        if (hasInnerFieldset) {
            await expect(this.innerHeading).toBeVisible();
            const innerText = (await this.innerHeading.textContent())?.trim() ?? '';
            expect(innerText.length).toBeGreaterThan(0);
        }

        await expect(this.description).toBeVisible();

        await expect(this.startButton).toBeVisible();
        await expect(this.startButton).toHaveText(/Start now/i);
    }

    async expectErrorSummaryIfPresent(): Promise<void> {
        const visible = await this.errorSummary.isVisible().catch(() => false);
        if (!visible) {
            return;
        }

        await expect(this.errorSummary).toHaveAttribute('role', 'alert');
        await expect(this.errorSummary).toHaveAttribute('tabindex', '-1');
    }

    // ===== Actions =====

    async clickStartNow(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.startButton.click(),
        ]);
    }
}