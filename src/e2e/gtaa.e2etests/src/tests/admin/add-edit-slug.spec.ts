import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, getQuestionnaire, updateQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {goToDesignQuestionnairePageByUrl, signIn} from "../../helpers/admin-test-helper";
import {PageHeadings} from "../../constants/test-data-constants";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";

test.describe('Get to an answer add or edit questionnaire slug', () => {
    let token: string;
    let questionnaireId: string;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;

    const invalidSlugScenarios = [
        {input: 'UPPERCASE-SLUG', description: 'Uppercase letters'},
        {input: 'slug with spaces', description: 'Spaces'},
        {input: 'slug!with@symbols#', description: 'Invalid symbols'},
        {input: 'slug_with_underscore', description: 'Underscores'}
    ];


    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
    });

    test('Back link takes to design questionnaire from Edit Slug page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();

        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING)

        await updateQuestionnaireSlugPage.clickBackLink();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();
    });

    test('Error summary appears on invalid slug submission', async ({page, browserName}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        await updateQuestionnaireSlugPage.submit();
        await updateQuestionnaireSlugPage.validateEmptySlugMessageSummary(browserName);
    });

    test('Inline error appears on invalid slug submission', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        await updateQuestionnaireSlugPage.submit();
        await updateQuestionnaireSlugPage.validateEmptySlugInlineSlugError();
    });

    for (const scenario of invalidSlugScenarios) {
        test(`Validate error for slug with ${scenario.description}`, async ({page, browserName}) => {
            await signIn(page, token);

            designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
            await designQuestionnairePage.createQuestionnaireId();
            updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
            await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

            await updateQuestionnaireSlugPage.enterSlug(scenario.input);
            await updateQuestionnaireSlugPage.submit();

            await updateQuestionnaireSlugPage.validateSlugFormatMessageSummary(browserName);
            await updateQuestionnaireSlugPage.validateSlugFormatInlineSlugError();
        });
    }

    test('Validate slug field retains input after unsuccessful submission', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        const slugInput = `TEST-SLUG-${Date.now()}`;
        await updateQuestionnaireSlugPage.enterSlug(slugInput);

        await updateQuestionnaireSlugPage.submit();

        const currentSlugValue = await updateQuestionnaireSlugPage.slugInput.inputValue();
        expect(currentSlugValue).toBe(slugInput);
    });

    test('Successfully submit valid slug:', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        const slugInput = `test-slug-${Date.now()}`;
        await updateQuestionnaireSlugPage.enterSlug(slugInput);
        await updateQuestionnaireSlugPage.submit();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();
        await designQuestionnairePage.expectSuccessBannerVisible();
        await designQuestionnairePage.assertQuestionnaireUpdatedSuccessBanner();
    });

    test('Successfully update questionnaire slug', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        // First slug submission
        const firstSlug = `first-slug-${Date.now()}`;
        await updateQuestionnaireSlugPage.enterSlug(firstSlug);
        await updateQuestionnaireSlugPage.submit();

        // Verify first submission
        await designQuestionnairePage.validateHeadingAndStatus();
        await designQuestionnairePage.expectSuccessBannerVisible();

        // Navigate back to slug page and verify existing slug
        await designQuestionnairePage.createQuestionnaireId();
        const currentSlugValue = await updateQuestionnaireSlugPage.slugInput.inputValue();
        expect(currentSlugValue).toBe(firstSlug);

        // Submit a new valid slug
        const secondSlug = `second-slug-${Date.now()}`;
        await updateQuestionnaireSlugPage.enterSlug(secondSlug);
        await updateQuestionnaireSlugPage.submit();

        // Verify the second submission
        await designQuestionnairePage.validateHeadingAndStatus();
        await designQuestionnairePage.expectSuccessBannerVisible();

        // Navigate back to slug page and verify new slug
        await designQuestionnairePage.createQuestionnaireId();
        const newSlugValue = await updateQuestionnaireSlugPage.slugInput.inputValue();
        expect(newSlugValue).toBe(secondSlug);
    });

    test('Duplicate slug from another questionnaire is not allowed ', async ({request, page}) => {
        const initialSlug = `questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;
        const {questionnaireGetBody} = await getQuestionnaire(
            request,
            questionnaireId,
            token
        );
        
        const {
            updatedQuestionnairePostResponse
        } = await updateQuestionnaire(
            request,
            questionnaireGetBody.id,
            {
                slug: initialSlug
            }, token
        );

        expect200HttpStatusCode(updatedQuestionnairePostResponse, 204);

        const {questionnaire} = await createQuestionnaire(request, token); // second questionnaire

        viewQuestionnairePage = await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);
        await designQuestionnairePage.createQuestionnaireId();

        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);

        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING)
        await updateQuestionnaireSlugPage.enterSlug(initialSlug);

        await updateQuestionnaireSlugPage.submit()

        await updateQuestionnaireSlugPage.validateDuplicateSlugMessageSummary('webkit')
        await updateQuestionnaireSlugPage.validateDuplicateSlugInlineSlugError();
    });
});