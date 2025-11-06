import {Page, Locator, expect, BrowserContext, Cookie} from '@playwright/test';

export class BasePage {
    protected readonly page: Page;

    //Locators for web page Header
    public readonly WebsiteNameLink: Locator;
    public readonly WebsiteAdminName: Locator;
    public readonly SignOutLink: Locator;
    public readonly defaultLogo: Locator;
    public readonly logoLink: Locator;
    
    // Locators for cookie banner and buttons - TBC
    public readonly cookieBanner: Locator;
    public readonly acceptButton: Locator;
    public readonly rejectButton: Locator;

    //Locators for web page Footers
    public readonly footer: Locator;
    public readonly footerLinks: Locator;
    public readonly cookiePolicyLinkInFooter: Locator;
    public readonly licenceLogo: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        this.page = page;
        
        // Locators for cookie banner and buttons - TBC
        this.cookieBanner = page.locator('');
        this.acceptButton = page.locator('');
        this.rejectButton = page.locator('');

        // ===== Locators for web page header =====
        this.logoLink = page.locator('');
        this.defaultLogo = page.locator('');

        // Title locator
        this.WebsiteNameLink = page.locator('');

        // Admin name locator
        this.WebsiteAdminName = page.locator('');

        // Sign out locator
        this.SignOutLink = page.locator('');

        // ===== Locators for web page Footers =====
        this.footer = page.locator('footer');
        this.footerLinks = page.locator('footer a');
        this.cookiePolicyLinkInFooter = page.locator('');
        this.licenceLogo = page.locator('');
    }

    async navigateTo(url: string) {
        await this.page.goto(url, {waitUntil: 'networkidle'});
    }

    protected escapeRegexExpression(s: string): string {
        return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    async validateUrlMatches(patternOrFragment: string | RegExp): Promise<void> {
        const pattern = typeof patternOrFragment === 'string'
            ? new RegExp(this.escapeRegexExpression(patternOrFragment))
            : patternOrFragment;
        await expect(this.page).toHaveURL(pattern);
    }
    
    // ===== Actions =====
    
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
    
    // ===== Verify header Links are visible =====
    async verifyHeaderLinks() {
        await expect(this.WebsiteNameLink).toHaveText(/Support for/i);
        await expect(this.WebsiteNameLink).toBeVisible();
        
        await expect(this.WebsiteAdminName).toBeVisible();
        
        await expect(this.logoLink).toBeVisible();
        await expect(this.defaultLogo).toBeVisible();

        await expect(this.SignOutLink).toHaveText(/Support for/i);
        await expect(this.SignOutLink).toBeVisible();
    }
    
    // ===== Verify footer Links are visible =====
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

    static async create<T extends BasePage>(
        this: new (page: Page) => T,
        page: Page
    ): Promise<T> {
        const instance = new this(page);
        await instance.waitForPageLoad();
        return instance;
    }
}