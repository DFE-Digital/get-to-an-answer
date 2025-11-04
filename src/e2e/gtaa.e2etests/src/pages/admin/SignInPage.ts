import { Page, expect } from '@playwright/test';
import {BasePage} from "../BasePage";

export class SignInPage extends BasePage {
    
    // ===== Locators =====
    private usernameInput = this.page.locator('#username'); 
    private passwordInput = this.page.locator('#password');
    private signInButton = this.page.locator('[data-test="sign-in-button"]');
    private errorMessage = this.page.locator('[data-test="error-message"]');
    private forgotPasswordLink = this.page.locator('[data-test="forgot-password-link"]');

    constructor(page: Page) {
        super(page);
    }
    
    // ===== Actions =====
    async openSignInPage() {
        await this.navigateTo('/');
        await this.waitForPageLoad();
    }
    async enterUsername(username: string): Promise<void> {
        await this.usernameInput.fill(username);
    }

    async enterPassword(password: string): Promise<void> {
        await this.passwordInput.fill(password);
    }

    async clickSignIn(): Promise<void> {
        await this.signInButton.click();
    }

    async signIn(username: string, password: string): Promise<void> {
        await this.enterUsername(username);
        await this.enterPassword(password);
        await this.clickSignIn();
    }

    // ===== Validations =====
    async verifyOnSignInPage(): Promise<void> {
        await expect(this.page).toHaveURL(/.*sign-in/i);
        await expect(this.usernameInput).toBeVisible();
        await expect(this.passwordInput).toBeVisible();
        await expect(this.signInButton).toBeVisible();
    }

    async verifyErrorMessage(expectedMessage: string): Promise<void> {
        await expect(this.errorMessage).toHaveText(expectedMessage);
    }

    async verifyForgotPasswordLinkVisible(): Promise<void> {
        await expect(this.forgotPasswordLink).toBeVisible();
    }
}