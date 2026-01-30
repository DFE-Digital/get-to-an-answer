import {Page, Locator, expect} from '@playwright/test';
import {ViewContributorTable} from './components/ViewContributorTable';
import {BasePage} from "../BasePage";

export class ViewContributorPage extends BasePage{
    // ===== Locators =====
    private readonly backLink: Locator;
    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly hint: Locator;

    private readonly notificationBanner: Locator;
    private readonly notificationBannerTitle: Locator;
    private readonly notificationBannerHeading: Locator;
    private readonly contributorAddedSuccessBannerText: Locator;

    private readonly addPersonButton: Locator;
    private readonly contributorsTableRegion: Locator;

    private readonly errorSummary: Locator;

    // ===== Embedded component =====
    readonly table: ViewContributorTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.backLink = this.page.locator('a.govuk-back-link');
        this.heading = this.page.locator('#manage-access-title');
        this.caption = this.heading.locator('#current-questionnaire-title');
        this.hint = this.page.locator('#manage-access-hint');

        this.notificationBanner = this.page.locator(
            '.govuk-notification-banner.govuk-notification-banner--success[role="alert"]'
        );
        this.notificationBannerTitle = this.notificationBanner.locator('.govuk-notification-banner__title');
        this.notificationBannerHeading = this.notificationBanner.locator('#just-saved-access-changes-banner-text');
        this.contributorAddedSuccessBannerText = this.notificationBanner.locator('#just-added-contributor-banner-text');
        
        this.addPersonButton = this.page.locator('#add-person');
        this.contributorsTableRegion = this.page.locator('table.govuk-table');

        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');

        this.table = new ViewContributorTable(page);
    }
    
    // ===== Actions =====

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backLink.click(),
        ]);
    }

    async clickAddPerson(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.addPersonButton.click(),
        ]);
    }

    // ===== Assertions =====

    async assertPageElements(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        
        await expect(this.backLink).toBeVisible();

        await expect(this.heading).toBeVisible();
        await expect(this.heading).toContainText('Manage access');

        await expect(this.caption).toBeVisible();
        const captionText = (await this.caption.textContent())?.trim() ?? '';
        expect(captionText.length).toBeGreaterThan(0);

        await expect(this.hint).toBeVisible();
        const hintText = await this.hint.innerText();
        expect(hintText).toMatch(/at least 2 people have access/i);
        expect(hintText).toMatch(/The following people have access/i);

        await expect(this.addPersonButton).toBeVisible();
        await expect(this.addPersonButton).toHaveText(/Add person/i);

        await expect(this.contributorsTableRegion).toBeVisible();
    }

    async expectNotificationBannerVisible(): Promise<void> {
        await expect(this.notificationBanner, '❌ Notification banner missing').toBeVisible();
        await expect(this.notificationBanner).toHaveAttribute('role', 'alert');

        await expect(this.notificationBannerTitle).toHaveText('Success');
        await expect(this.contributorAddedSuccessBannerText).toContainText(
            'has been given access'
        );
    }

    async expectErrorSummaryVisible(): Promise<void> {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary).toHaveAttribute('role', 'alert');
        await expect(this.errorSummary).toHaveAttribute('tabindex', '-1');
    }

    async expectMinimumContributorsConstraint(): Promise<void> {
        // If 2 or fewer contributors, expect at least one disabled warning button.
        // If more than 2, expect at least one enabled delete button.
        const rows = this.contributorsTableRegion.locator('tbody.govuk-table__body tr.govuk-table__row');
        const rowCount = await rows.count();

        if (rowCount <= 2) {
            const disabledButtons = this.contributorsTableRegion.locator(
                'button.govuk-button.govuk-button--warning[disabled]'
            );
            await expect(disabledButtons.first(), '❌ Expected disabled "Remove" button when <= 2 contributors')
                .toBeVisible();
        } else {
            const enabledDeleteButtons = this.contributorsTableRegion.locator(
                'button.govuk-button.govuk-button--warning[type="submit"]:not([disabled])'
            );
            await expect(enabledDeleteButtons.first(), '❌ Expected enabled "Delete contributor" button').toBeVisible();
        }
    }

    async expectContributorListed(name: string, email: string): Promise<void> {
        const row = this.contributorsTableRegion.locator(
            `tbody.govuk-table__body tr.govuk-table__row:has-text("${name}"):has-text("${email}")`
        );
        await expect(row, '❌ Contributor row not found').toBeVisible();
    }
}