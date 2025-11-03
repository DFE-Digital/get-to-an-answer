import {Page, Locator, expect, BrowserContext, Cookie} from '@playwright/test';

export class BasePage {
    protected readonly page: Page;

    // Locator for the Website Title navigation Link
    public readonly WebsiteNameLink: Locator;

    // Locator for the admin name Link
    public readonly AdminNameLink: Locator;

    // Locator for the sign-out Link
    public readonly SignOutLink: Locator;

    // Locators for the logo link and logo images
    public logoLink: Locator;
    public defaultLogo: Locator;

    // Locators for cookie banner and buttons - TBC
    public readonly cookieBanner: Locator;
    public readonly acceptButton: Locator;
    public readonly rejectButton: Locator;

    //Locators for web page Footers
    public readonly footer: Locator;
    public readonly footerLinks: Locator;
    public readonly cookiePolicyLinkInFooter: Locator;
    public readonly licenceLogo: Locator;

    constructor(page: Page) {
        this.page = page;

        // Title locator
        this.WebsiteNameLink = page.locator('');

        // Admin name locator
        this.AdminNameLink = page.locator('');

        // Sign out locator
        this.SignOutLink = page.locator('');
        
        // Logo locators
        this.logoLink = page.locator('');
        this.defaultLogo = page.locator('');

        // Locators for cookie banner and buttons - TBC
        this.cookieBanner = page.locator('');
        this.acceptButton = page.locator('');
        this.rejectButton = page.locator('');

        // Locators for web page Footers
        this.footer = page.locator('footer');
        this.footerLinks = page.locator('footer a');
        this.cookiePolicyLinkInFooter = page.locator('');
        this.licenceLogo = page.locator('');
    }

    async navigateTo(url: string) {
        await this.page.goto(url, {waitUntil: 'networkidle'});
    }
    
    //Validate URL matches with expected-pattern (RegExp characters replaced in expected Url)  
    async validateURLMatches(pattern: RegExp) {
        await expect(this.page).toHaveURL(pattern);
    }

    // Validate URL contains a specific path
    async validateURLContains(path: string) {
        await expect(this.page).toHaveURL(new RegExp(path));
    }
    
    // Wait for the page to load
    async waitForPageLoad() {
        await this.page.waitForLoadState('load');
    }
    
    // Cookie banner functionality
    async acceptCookies() {
        await this.acceptButton.click();
        await expect(this.cookieBanner).not.toBeVisible();
    }

    async rejectCookies() {
        await expect(this.rejectButton).toBeVisible();
        await this.rejectButton.click();
        // Ensure the banner disappears
        await expect(this.cookieBanner).not.toBeVisible();
    }
    
    // Verify logo presence
    async verifyLogoPresence() {
        await expect(this.logoLink).toBeVisible();
        await expect(this.defaultLogo).toBeVisible();
    }

    // Verify footer Links are visible
    async verifyFooterLinks() {
        //Ensure the footer is visible 
        await expect(this.footer).toBeVisible();

        // Verify the "Cookie policy" link(in Footer)
        await expect(this.cookiePolicyLinkInFooter).toBeVisible();
        await expect(this.cookiePolicyLinkInFooter).toContainText('Cookie policy');
        await expect(this.cookiePolicyLinkInFooter).toHaveAttribute('href', '/en/cookie-policy');

        // Verify the footer logo and licence description
        await expect(this.licenceLogo).toBeVisible();
        const licenceDescription = this.footer.locator('.govuk-footer__licence-description');
        await expect(licenceDescription).toBeVisible();
        await expect(licenceDescription).toContainText(/All content is available under the/);

        // Verify the Footer links-Crown copyright link and Open Government Licence link
        const footerLinksCount = await this.footerLinks.count();
        expect(footerLinksCount).toBeGreaterThan(0);

        for (let i = 0; i < footerLinksCount; i++) {
            const link = this.footerLinks.nth(i);
            await expect(link).toBeVisible();
            const href = await link.getAttribute('href');
            expect(href).not.toBeNull();
        }
    }
}