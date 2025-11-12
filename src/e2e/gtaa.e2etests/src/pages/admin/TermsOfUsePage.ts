import { expect, Locator, Page } from '@playwright/test';
import {BasePage} from "../BasePage";

export class TermsOfUsePage extends BasePage{
    readonly heading: Locator;
    readonly agreeCheckbox: Locator;
    readonly continueButton: Locator;

    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');
        this.agreeCheckbox = page.locator('input.govuk-checkboxes__input#Accepted');
        this.continueButton = page.locator('button.govuk-button');
    }

    // ===== Validations =====
    async verifyHeading() {
        await expect(this.heading).toBeVisible();
    }
    
    // ===== Actions =====
    async acceptTerms() {
        await this.agreeCheckbox.setChecked(true);
        await expect(this.agreeCheckbox).toBeChecked();
    }

    async submit() {
        await this.continueButton.click();
    }
    
    async agreeToTermsOfUse() {
        await this.verifyHeading();
        await this.acceptTerms();
        await this.submit();
    }
}