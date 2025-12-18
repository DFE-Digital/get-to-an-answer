import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {goToAddQuestionnairePage, signIn} from '../../helpers/admin-test-helper';
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {ErrorMessages} from "../../constants/test-data-constants";
import {CloneQuestionnairePage} from "../../pages/admin/CloneQuestionnairePage";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";


test.describe.skip('Get to an answer Add bulk answers options to question', () => {
    let token: string;
    let questionnaireTitle: string;
    let cloneQuestionnairePage: CloneQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireTitle = questionnaire.title;
        
        const {question} = await createQuestion(request, questionnaire.id, token);
        expect(question.id).toBeDefined();

        viewQuestionnairePage = await signIn(page, token);
    });

    
    test('Add bulk answers options to question successfully and lands on Edit Questionnaire Page', async ({ page }) => {
    })
})