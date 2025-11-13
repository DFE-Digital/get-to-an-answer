import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {landing, signIn} from "../../helpers/admin-test-helper";
import {SignInPage} from "../../pages/admin/SignInPage";
import {TermsOfUsePage} from "../../pages/admin/TermsOfUsePage";

test.describe('Get to an answer terms of use page', () => {
    let token: string;
    let signInPage: SignInPage;
    let termsOfUsePage: TermsOfUsePage;
    let viewQuestionnairePage: ViewQuestionnairePage;

    test.beforeEach(async ({page}) => {
        token = JwtHelper.NoRecordsToken();
        signInPage = await landing(page, token);
    });

    test('Validate presence of elements on sign in page', async ({page}) => {
        await signInPage.clickSignIn();

        termsOfUsePage = await TermsOfUsePage.create(page);
        await termsOfUsePage.assertPageElements();
    });

    test('Acceptance control present - fieldset and checkbox are visible', async ({page}) => {
        await signInPage.clickSignIn();
        termsOfUsePage = await TermsOfUsePage.create(page);

        await termsOfUsePage.verifyFieldsetVisible();
        await expect(termsOfUsePage.agreeCheckbox).not.toBeChecked();
    });

    test('Validation when not accepted - error summary, inline error appear', async ({page}) => {
        await signInPage.clickSignIn();
        termsOfUsePage = await TermsOfUsePage.create(page);

        await termsOfUsePage.submitWithoutAccepting();

        await termsOfUsePage.validateErrorMessage();
    });

    test('Successful acceptance - checkbox is ticked and user navigates to next step', async ({page}) => {
        await signInPage.clickSignIn();
        termsOfUsePage = await TermsOfUsePage.create(page);

        await termsOfUsePage.acceptTerms();
        await termsOfUsePage.submit();

        viewQuestionnairePage = new ViewQuestionnairePage(page);
        await viewQuestionnairePage.waitForPageLoad();
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();
    });
});