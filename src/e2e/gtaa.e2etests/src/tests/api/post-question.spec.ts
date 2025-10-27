import {test} from "@playwright/test";
import {createQuestion} from "../../test-data-seeder/question-data";
import {
    expectHttp, expectQuestionTypes, expectQuestionContent, expectQuestionIO,
    expectQuestionSchema
} from "../../helpers/api-assertions-helper";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('POST Create question api request', () => {
    test('Validate POST create new question', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;
        
        const {
            questionPostResponse,
            question,
            payload
        } = await createQuestion(request, questionnaireId);

         // --- HTTP-level checks ---
         expectHttp(questionPostResponse, 201);
        
        // --- Schema-level checks ---
         expectQuestionSchema(question);
        
         // --- Type sanity checks ---
        expectQuestionTypes(question);
        
         // --- Basic content sanity ---
         expectQuestionContent(question);
        
         // --- I/O checks ---
         expectQuestionIO(question, payload, GUID_REGEX);
    });
});
