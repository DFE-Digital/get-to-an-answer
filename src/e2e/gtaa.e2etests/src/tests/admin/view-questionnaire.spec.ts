import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {signIn, goToAddQuestionnairePage} from "../../helpers/admin-test-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createQuestionnaire, getQuestionnaire, listQuestionnaires} from "../../test-data-seeder/questionnaire-data";
import {JwtHelper} from "../../helpers/JwtHelper";
import {EntityStatus} from "../../constants/test-data-constants";

test.describe('Get to an answer views questionnaire', () => {
    let token: string;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async () => {
        token = JwtHelper.NoRecordsToken();
    });

    test('Validate presence of elements on view questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        await viewQuestionnairePage.assertPageElements();
    });

    test("Header section - H1 and paragraph presence", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);

        await viewQuestionnairePage.verifyHelpUserHeadingVisible();
        await viewQuestionnairePage.verifyHelpUserDescriptionVisible();
    });

    test("Create questionnaire CTA navigates to Add page", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);

        await viewQuestionnairePage.clickCreateNewQuestionnaire();
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.verifyLabelAndHintPresent();
    });

    test("Questionnaires table - columns, title link navigates", async ({page, request}) => {
        await createQuestionnaire(request, token);

        viewQuestionnairePage = await signIn(page, token);

        await viewQuestionnairePage.table.verifyHeaders();
        await viewQuestionnairePage.table.verifyFirstTitleIsLink();

        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeadingAndStatus();
    });

    test("Questionnaires table - validate data", async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);

        const lisQuestionnaireResponse = await listQuestionnaires(request, token);
        const list: any[] = lisQuestionnaireResponse.questionnaireGetBody
        expect(list.length).toBeGreaterThan(0);
        const firstQuestionnaire = list[0];

        const questionnaireResponse = await getQuestionnaire(
            request,
            questionnaire.id,
            token
        );

        const statusValue = questionnaireResponse.questionnaireGetBody.status;
        const statusName = EntityStatus[statusValue];

        const expectedRows = [
            {
                title: questionnaireResponse.questionnaireGetBody.title,
                createdBy: firstQuestionnaire.createdBy,
                updatedAt: firstQuestionnaire.updatedAt,
                status: statusName
            }
        ];

        viewQuestionnairePage = await signIn(page, token);
        await viewQuestionnairePage.table.verifyTableData(expectedRows);
    });

    test('Table and list region have proper accessibility attributes', async ({page, request}) => {
        await createQuestionnaire(request, token);

        viewQuestionnairePage = await signIn(page, token);

        await viewQuestionnairePage.validateTableAccessibility();
        await viewQuestionnairePage.validateListRegionAccessibility();
    });
});
