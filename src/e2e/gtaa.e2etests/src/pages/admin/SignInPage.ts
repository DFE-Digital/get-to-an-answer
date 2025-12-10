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
        this.navigateTo(`/dev/login?jt=${bearerToken ?? JwtHelper.ValidToken}`)
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

    async headingHierarchyIsCorrect() {
        const headings = await this.page.locator('h1, h2').all();

        let foundH1 = false;
        for (const heading of headings) {
            const tagName = await heading.evaluate(el => el.tagName);

            if (tagName === 'H1') {
                foundH1 = true;
            } else if (tagName === 'H2' && !foundH1) {
                // H2 found before H1 - hierarchy is broken
                throw new Error('H2 heading found before H1 heading');
            }
        }

        await expect(foundH1).toBeTruthy();
    }

    async verifyOnSignInPage(): Promise<void> {
        await expect(this.signInButton).toBeVisible();
    }

    // async verifyErrorMessage(expectedMessage: string): Promise<void> {
    //     await expect(this.errorMessage).toHaveText(expectedMessage);
    // }

    async assertPageElements() {
        await this.verifyPublicHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.mainHeading, '❌ Main heading not visible').toBeVisible();
        await expect(this.subHeading, '❌ Sub heading not visible').toBeVisible();
        await expect(this.signInButton, '❌ Sign in button not visible').toBeVisible();
    }
}