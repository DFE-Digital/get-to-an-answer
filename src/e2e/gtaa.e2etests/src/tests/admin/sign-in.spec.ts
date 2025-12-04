import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {landing} from "../../helpers/admin-test-helper";
import {SignInPage} from "../../pages/admin/SignInPage";
import {TermsOfUsePage} from "../../pages/admin/TermsOfUsePage";

test.describe('Get to an answer sign in page', () => {
    let token: string;
    let signInPage: SignInPage;
    let termsOfUsePage: TermsOfUsePage;

    test.beforeEach(async ({page}) => {
        token = JwtHelper.NoRecordsToken();
        signInPage = await landing(page, token);
    });

    test('Validate presence of elements on sign in page', async ({page}) => {
        await signInPage.assertPageElements();
    });

    test('Clicking sign in button navigates to terms od service agreement', async ({page}) => {
        await signInPage.clickSignIn();
        
        termsOfUsePage = await TermsOfUsePage.create(page);
        await termsOfUsePage.verifyHeading();
    });

    test('Accessibility - keyboard navigation and heading hierarchy', async ({page}) => {
        await signInPage.verifySignInButtonIsFocused();

        termsOfUsePage = await TermsOfUsePage.create(page);
        await termsOfUsePage.verifyHeading();
        
        // TBC CARE-1571, to be covered later during accessibility testing
        //await signInPage.headingHierarchyIsCorrect();
    });
});