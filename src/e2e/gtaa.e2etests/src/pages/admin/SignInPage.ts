import {Page, expect, Locator} from '@playwright/test';
import {BasePage} from "../BasePage";
import {JwtHelper} from "../../helpers/JwtHelper";

export class SignInPage extends BasePage {

    // ===== Locators =====
    readonly mainHeading: Locator;
    readonly subHeading: Locator;
    readonly signInButton: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.mainHeading = page.locator('h1.govuk-heading-l');
        this.subHeading = page.locator('h2.govuk-heading-m').first();
        this.signInButton = page.locator('a.govuk-button.govuk-button--start');
    }

    // ===== Actions =====
    async openSignInPage(bearerToken?: string) {
        await this.navigateTo(`/dev/login?jt=${bearerToken ?? JwtHelper.ValidToken}`)
            .catch(() => { /* ignore if it redirects instantly */
            });
        await this.waitForPageLoad();
    }

    async clickSignIn(): Promise<void> {
        await this.signInButton.click();
    }

    // ===== Validations =====
    async verifySignInButtonIsFocused() {
        await this.signInButton.focus();
        await expect(this.signInButton).toBeFocused();
        await this.signInButton.press('Enter');
    }
    
    async verifyOnSignInPage(): Promise<void> {
        await expect(this.signInButton).toBeVisible();
    }
    
    async assertPageElements() {
        await this.verifyPublicHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.mainHeading, '❌ Main heading not visible').toBeVisible();
        await expect(this.subHeading, '❌ Sub heading not visible').toBeVisible();
        await expect(this.signInButton, '❌ Sign in button not visible').toBeVisible();
    }
}