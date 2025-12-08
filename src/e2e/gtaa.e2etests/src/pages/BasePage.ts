import {Page, Locator, expect, BrowserContext, Cookie} from '@playwright/test';

type LoadState = 'domcontentloaded' | 'load' | 'networkidle';

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
        this.cookieBanner = page.getByRole('region', {
            name: /cookies on gettoananswer/i,
        });
        this.acceptButton = this.cookieBanner.locator('#accept-cookie');
        this.rejectButton = this.cookieBanner.locator('#reject-cookie');

        // ===== Locators for web page header =====
        this.logoLink = page.locator('a.govuk-header__link--homepage');
        this.defaultLogo = page.locator('img.govuk-header__logotype');
        this.WebsiteNameLink = page.locator('header.dfe-header a.dfe-header__link--service');
        this.WebsiteAdminName = page.locator('nav .govuk-service-navigation__text');
        this.SignOutLink = page.getByRole('link', {name: /sign out/i});

        // ===== Locators for web page Footers =====
        this.footer = page.locator('footer.govuk-footer');
        this.footerLinks = this.footer.locator('a.govuk-footer__link');
        this.cookiePolicyLinkInFooter = this.footer.getByRole('link', {name: /cookie policy/i});
        this.licenceLogo = this.footer.locator('svg.govuk-footer__licence-logo');
    }

    async navigateTo(url: string, waitUntil: LoadState = 'networkidle'): Promise<void> {
        await this.page.goto(url, {waitUntil});
    }
    
    // ===== Actions =====
    // Wait for the page to load
    async waitForPageLoad() {
        await this.page.waitForLoadState('networkidle');
    }

    // Cookie banner functionality
    async acceptCookiesIfVisible(): Promise<void> {
        if (await this.cookieBanner.isVisible()) {
            await this.acceptButton.click();
            await expect(this.cookieBanner).toBeHidden();
        }
    }

    async rejectCookiesIfVisible(): Promise<void> {
        if (await this.cookieBanner.isVisible()) {
            await this.rejectButton.click();
            await expect(this.cookieBanner).toBeHidden();
        }
    }
    
    // ===== Verify header Links are visible =====
    async verifyHeaderLinks() {
        //await expect(this.WebsiteNameLink).toHaveText(/Support for/i); //TBC
        //await expect(this.WebsiteNameLink, '❌ Website name link is not visible on the page').toBeVisible();

        await expect(this.WebsiteAdminName, '❌ Admin name is not visible on the page').toBeVisible();

        await expect(this.logoLink,'❌ Logo link is not visible on the page').toBeVisible();
        await expect(this.defaultLogo, '❌ Default logo is not visible on the page').toBeVisible();

        //await expect(this.SignOutLink).toHaveText(/Support for/i); TBC
        await expect(this.SignOutLink, '❌ Sign out link is not visible on the page').toBeVisible();
    }

    // ===== Verify footer Links are visible =====
    async verifyFooterLinks() {
        //Ensure the footer is visible 
        await expect(this.footer).toBeVisible();

        // Verify the "Cookie policy" link(in Footer)
        await expect(this.cookiePolicyLinkInFooter).toBeVisible();
        await expect(this.cookiePolicyLinkInFooter).toContainText('Cookie policy');
        //await expect(this.cookiePolicyLinkInFooter).toHaveAttribute('href', 'Cookie policy'); //TBC

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
    
    errorSummaryLink(href: string): Locator {
        return this.page.locator(`[href="${href}"].govuk-link.govuk-error-summary__link`)
    }

    inlineErrorLink(fieldId: string): Locator {
        return this.page.locator(`#${fieldId}.govuk-error-message`)
    }
}