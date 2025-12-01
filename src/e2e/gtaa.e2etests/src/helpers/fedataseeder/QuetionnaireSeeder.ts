import path from "path";
import fs from "fs/promises";
import {
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {createContent} from "../../test-data-seeder/content-data";
import {APIRequest, APIRequestContext} from "@playwright/test";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType} from "../../constants/test-data-constants";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

export class QuestionnaireSeeder {
    public static async seed(request: APIRequestContext, filename: string, token: string) {
        const jsonFilePath = path.join(__dirname, `../../helpers/fedataseeder/${filename}`);
        const jsonText = await fs.readFile(jsonFilePath, 'utf8');
        const data = JSON.parse(jsonText);

        // Create questionnaire
        const apiQuestionnaireResponse = await createQuestionnaire(request, token, data.title, data.description);
        const questionnaireId = apiQuestionnaireResponse.questionnaire.id;

        await updateQuestionnaire(
            request,
            questionnaireId,
            {
                displayTitle: data.displayTitle,
                description: data.description,
                slug: data.slug
            }, 
            token
        );
        
        const contentIdMap: Record<string, string> = {};

        for (const c of data.contents ?? []) {
            const apiContentResponse = await createContent(request, {
                questionnaireId: questionnaireId,
                title: c.title,
                content: c.content,
                referenceName: c.referenceName
            }, token)

            contentIdMap[c.key] = apiContentResponse.content.id;
        }

        const questionIdMap: Record<string, string> = {};

        for (const q of data.questions) {
            const {question} = await createQuestion(request, questionnaireId,token, q.text, q.type, q.hint ?? null);
            questionIdMap[q.key] = question.id;
        }

        // Create answers for every question
        for (const q of data.questions) {
            const questionId = questionIdMap[q.key];

            for (const answer of q.answers) {

                let destinationType: AnswerDestinationType | undefined;
                let destinationQuestionId: string | undefined;
                let destinationContentId: string | undefined;
                let destinationUrl: string | undefined;

                if (answer.navigation) {
                    const nav = answer.navigation;
                    
                    // Determine navigation type and set appropriate values
                    if (nav.type === 'next-question') {
                        destinationType = AnswerDestinationType.Question;
                        destinationQuestionId = questionIdMap[nav.targetQuestionKey];
                    } else if (nav.type === 'external-link') {
                        destinationType = AnswerDestinationType.ExternalLink;
                        destinationUrl = nav.url;
                    } else if (nav.type === 'internal-link') {
                        destinationType = AnswerDestinationType.CustomContent;
                        destinationContentId = contentIdMap[nav.targetContentKey];
                    }
                }

                await createSingleAnswer(request, {
                    questionId,
                    questionnaireId,
                    content: answer.text,
                    description: answer.hint ?? null,
                    priority: answer.priority ?? 0,
                    destinationType,
                    destinationQuestionId,
                    destinationContentId,
                    destinationUrl
                }, token);
            }
        }
        console.log('Seeding completed for questionnaire:', questionnaireId);
        await publishQuestionnaire(request, questionnaireId, token);
    }
}