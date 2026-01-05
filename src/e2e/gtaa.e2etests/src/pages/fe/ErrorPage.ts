
import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {convertColorToHex} from "../../helpers/utils";
import {RunBasePage} from "./RunBasePage";

export class ErrorPage extends RunBasePage {
    // ===== Locators =====
    readonly outerHeading: Locator;            // Heading when no decorative image & not embedded
    readonly description: Locator;
    
    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.outerHeading = this.page.locator('#error-page-title');

        this.description = this.page.locator('#error-page-details').first();
    }

    // ===== Assertions =====

    async assertStructure(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();

        // One of: header with H1 OR decorative image
        const hasOuterHeading = await this.outerHeading.isVisible().catch(() => false);

        expect(hasOuterHeading).toBeTruthy();

        if (hasOuterHeading) {
            const text = (await this.outerHeading.textContent())?.trim() ?? '';
            expect(text.length).toBeGreaterThan(0);
        }

        await expect(this.description).toBeVisible();
    }
    
    async assertErrorText(expectedHeading: string, expectedMessage: string): Promise<void> {
        await expect(this.outerHeading).toBeVisible()
        await expect(this.outerHeading).toHaveText(expectedHeading);
        await expect(this.description).toBeVisible();
        await expect(this.description).toHaveText(expectedMessage);
    }
}