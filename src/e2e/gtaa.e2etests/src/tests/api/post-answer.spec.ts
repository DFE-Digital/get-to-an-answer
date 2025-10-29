import {test} from "@playwright/test";
import {createQuestionnaire, deleteQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    expectAnswerContent, expectAnswerIO,
    expectAnswerSchema,
    expectAnswerTypes,
    expectHttpStatusCode
} from "../../helpers/api-assertions-helper";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {GUID_REGEX} from "../../constants/test-data-constants";
test.describe('POST answers', () => {
    test('Post an answer for a question in a questionnaire validations', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
            payload: questionnairePayload
        } = await createQuestionnaire(request);

        expectHttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
            payload: questionPayload
        } = await createQuestion(request, questionnaire.id);

        expectHttpStatusCode(questionPostResponse, 201);
        
        const {
            res: answerPostResponse,
            responseBody: answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            },
        );

        // --- HTTP-level checks ---
        expectHttpStatusCode(answerPostResponse, 200);

        // --- Schema-level checks ---
        expectAnswerSchema(answer);

        // --- Type sanity checks ---
        expectAnswerTypes(answer);

        // --- Basic content sanity ---
        expectAnswerContent(answer);

        // --- I/O checks ---
        expectAnswerIO(answer, payload, GUID_REGEX);
    });
});