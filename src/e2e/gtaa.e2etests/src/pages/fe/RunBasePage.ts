import {Page, Locator, expect, BrowserContext, Cookie, FrameLocator} from '@playwright/test';
import {convertColorToHex} from "../../helpers/utils";

type LoadState = 'domcontentloaded' | 'load' | 'networkidle';

export class RunBasePage {
    protected readonly page: Page;
    protected readonly frame?: FrameLocator;
    protected readonly pageOrFrame: Page | FrameLocator;
    
    protected readonly mainContent: Locator;

    //Locators for web page Header
    public readonly dfeWebsiteLink: Locator;
    public readonly WebsiteNameLink: Locator;
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
    constructor(page: Page, frame?: FrameLocator) {
        this.page = page;
        this.frame = frame;
        
        this.pageOrFrame = frame ?? page;
        
        this.mainContent = this.pageOrFrame.locator('.govuk-template__body');
        
        // Locators for cookie banner and buttons - TBC
        this.cookieBanner = this.pageOrFrame.getByRole('region', {
            name: /cookies on gettoananswer/i,
        });
        this.acceptButton = this.pageOrFrame.locator('#accept-cookie');
        this.rejectButton = this.pageOrFrame.locator('#reject-cookie');
        
        // ===== Locators for web page header =====
        this.logoLink = this.pageOrFrame.locator('a.govuk-header__link--homepage');
        this.defaultLogo = this.pageOrFrame.locator('img.govuk-header__logotype');
        this.dfeWebsiteLink = this.pageOrFrame.locator('header.dfe-header a.dfe-header__link--service');
        this.WebsiteNameLink = this.pageOrFrame.locator('nav .govuk-service-navigation__link');

        // ===== Locators for web page Footers =====
        this.footer = this.pageOrFrame.locator('footer.govuk-footer');
        this.footerLinks = this.footer.locator('a.govuk-footer__link');
        this.cookiePolicyLinkInFooter = this.footer.getByRole('link', {name: /cookie policy/i});
        this.licenceLogo = this.footer.locator('svg.govuk-footer__licence-logo');
    }

    async navigateTo(url: string, waitUntil: LoadState = 'networkidle'): Promise<void> {
        await this.page.goto(url, {waitUntil});
    }

    // ===== Actions =====
    // Wait for the page to load
    async waitForPageLoad(waitState: LoadState = 'networkidle') {
        await this.page.waitForLoadState(waitState);
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
    async verifyPublicHeaderLinks() {
        if (!this.frame) {
            //await expect(this.WebsiteNameLink).toHaveText(/Support for/i); //TBC
            await expect(this.WebsiteNameLink, '❌ Website name link is not visible on the page').toBeVisible();

            await expect(this.logoLink, '❌ Logo link is not visible on the page').toBeVisible();
            await expect(this.defaultLogo, '❌ Default logo is not visible on the page').toBeVisible();
        }
    }
    async verifyHeaderLinks() {
        if (!this.frame) {
            await expect(this.logoLink, '❌ Logo link is not visible on the page').toBeVisible();
            await expect(this.defaultLogo, '❌ Default logo is not visible on the page').toBeVisible();
        }
    }

    // ===== Verify footer Links are visible =====
    async verifyFooterLinks() {
        if (!this.frame) {
            //Ensure the footer is visible 
            await expect(this.footer).toBeVisible();
            
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

    static async create<T extends RunBasePage>(
        this: new (page: Page, frame?: FrameLocator) => T,
        page: Page,
        frame?: FrameLocator
    ): Promise<T> {
        const instance = new this(page, frame);
        await instance.waitForPageLoad();
        if (frame) {
           await frame.locator('body').waitFor({ state: 'visible' });
        }
        return instance;
    }

    errorSummaryLink(href: string): Locator {
        return this.pageOrFrame.locator(`[href="${href}"].govuk-link.govuk-error-summary__link`)
    }

    inlineErrorLink(fieldId: string): Locator {
        return this.pageOrFrame.locator(`#${fieldId}.govuk-error-message`)
    }
    
    async assertBackgroundColor(expectedHexColor: string) {
        await expect(this.mainContent).toBeVisible();
        const backgroundColor = await this.mainContent.evaluate((el) =>
            window.getComputedStyle(el).getPropertyValue('background-color')
        );
        expect(backgroundColor.length).toBeGreaterThan(0);
        expect(convertColorToHex(backgroundColor)).toBe(expectedHexColor);
    }
}