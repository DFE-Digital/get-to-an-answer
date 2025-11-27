import {test, expect} from '@playwright/test';
import {
    createQuestionnaire,
    updateQuestionnaire,
    publishQuestionnaire, 
    listQuestionnaireVersions
} from "../../test-data-seeder/questionnaire-data";
import {AnswerDestinationType, GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestion, updateQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer, deleteAnswer, updateAnswer} from "../../test-data-seeder/answer-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test('should return versions for existing questionnaire', async ({request}) => {
    // Create questionnaire
    const { questionnaire } = await createQuestionnaire(request);
    const id = questionnaire.id;

    // Update questionnaire to create a new version
    await updateQuestionnaire(request, id, {title: 'Updated Title'});
    
    const { question } = await createQuestion(request, id, undefined, 'Q1?')
    
    await createSingleAnswer(request, {
        questionnaireId: id, questionId: question.id, content: 'A1', 
        destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
    })
    
    await publishQuestionnaire(request, id)

    // Get versions
    const { versions } = await listQuestionnaireVersions(request, id);

    expect(versions).toBeInstanceOf(Array);
    expect(versions.length).toBe(2);
    expect(versions[0].id).toMatch(GUID_REGEX);
    expect(versions[0].version).toBe(2);
    expect(versions[1].id).toMatch(GUID_REGEX);
    expect(versions[1].version).toBe(1);
});

// Scenario: Successfully retrieve all versions of a questionnaire
test('retrieve all versions of a questionnaire includes required fields', async ({ request }) => {
    // Given a questionnaire exists with 5 published versions
    const { questionnaire } = await createQuestionnaire(request, undefined, 
        'Base Q', "Content");

    const { question } = await createQuestion(request, questionnaire.id, undefined, 'Q1?')
    const { answer } = await createSingleAnswer(request, {
        questionnaireId: questionnaire.id,
        questionId: question.id,
        content: 'A1'
    } as any)
    
    const id = questionnaire.id;
    
    let tempAnswer = null;

    // Create 5 published versions
    for (let i = 1; i <= 5; i++) {
        await updateQuestionnaire(request, id, {
            title: `Title v${i}`,
            content: `Content v${i}`,
        });

        await updateQuestion(request, questionnaire.id, undefined, `Q1 v${i}?`)
        await updateAnswer(request, answer.id, {
            content: `A1 v${i}`,
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl: `https://example.com/v${i}`,
        })
        
        if (i === 2) {
            const { answer: answer2 } = await createSingleAnswer(request, {
                questionnaireId: questionnaire.id,
                questionId: question.id,
                content: 'A2',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: `https://example.com/v${i}`,
            } as any)

            tempAnswer = answer2;
        } else if (i === 4) {
            await deleteAnswer(request, tempAnswer.id);
        }
        
        const { response } = await publishQuestionnaire(request, id);
        
        expect(response.status()).toBe(204);
    }

    // Current draft version
    await updateQuestion(request, questionnaire.id, undefined, `Q1 draft?`)
    await updateAnswer(request, answer.id, {
        content: `A1 draft`,
        destinationType: AnswerDestinationType.ExternalLink,
        destinationUrl: `https://example.com/draft`,
    })

    // When I request all versions
    const { versions } = await listQuestionnaireVersions(request, id);

    // Then the response should contain 5 questionnaire versions
    expect(Array.isArray(versions)).toBe(true);
    expect(versions.length).toBe(6); // 5 published + 1 current draft version

    // And each version should include required fields
    for (const v of versions) {
        expect(v.id).toMatch(GUID_REGEX);
        expect(v.questionnaireId).toBe(id);
        expect(typeof v.version).toBe('number');
        expect(v.createdAt ?? v.created_at).toBeTruthy();

        // questionnaireJson existence and shape
        expect(v.changeLog).toBeTruthy();
        expect(Array.isArray(v.changeLog)).toBe(true);
    }

    assertChanges(versions[0].changeLog, {
        path: "$.status",
        kind: 3,
        thisValue: "Published",
        thatValue: "Draft"
    }, {
        path: "$.questions[0].answers[0].content",
        kind: 3,
        thisValue: "A1 v5",
        thatValue: "A1 draft"
    })

    for (let i = 1; i < versions.length-1; i++) {
        const v = 5 - i
        
        if (versions[i].version === 2) {
            expect(versions[i].changeLog.length).toBe(8);
        } else if (versions[i].version === 4) {
            expect(versions[i].changeLog.length).toBe(8);
        } else {
            expect(versions[i].changeLog.length).toBe(3);
        }
        assertChanges(versions[i].changeLog, {
            path: "$.title",
            kind: 3,
            thisValue: `Title v${v}`,
            thatValue: `Title v${v+1}`
        }, {
            path: "$.questions[0].answers[0].content",
            kind: 3,
            thisValue: `A1 v${v}`,
            thatValue: `A1 v${v+1}`
        },
        ...(versions[i].version === 2 ? [{
            path: "$.questions[0].answers[1].content",
            kind: 1,
            thisValue: null,
            thatValue: 'A2'
        }, {
            path: "$.questions[0].answers[1].destinationType",
            kind: 1,
            thisValue: null,
            thatValue: 'ExternalLink'
        }, {
            path: "$.questions[0].answers[1].destinationUrl",
            kind: 1,
            thisValue: null,
            thatValue: 'https://example.com/v2'
        }] : []),
        ...(versions[i].version === 4 ? [{
            path: "$.questions[0].answers[1].content",
            kind: 2,
            thisValue: 'A2',
            thatValue: null
        }, {
            path: "$.questions[0].answers[1].destinationType",
            kind: 2,
            thisValue: 'ExternalLink',
            thatValue: null
        }, {
            path: "$.questions[0].answers[1].destinationUrl",
            kind: 2,
            thisValue: 'https://example.com/v2',
            thatValue: null
        }] : []))
    }

    expect(versions[versions.length - 1].changeLog.length).toBe(0)
});

function assertChanges(changeLog: any, ...expectedChanges: any) {
    const foundAll = expectedChanges.every((expectedChange:any) => {
        return changeLog.some((change:any) => {
            return Object.keys(expectedChange).every(key => {
                return change[key] === expectedChange[key];
            });
        });
    })

    expect(foundAll, `Expected: ${JSON.stringify(expectedChanges, null, 2)}\nActual: ${JSON.stringify(changeLog, null, 2)}`).toBeTruthy();
}

// Scenario: Retrieve versions ordered by most recent first
test('versions are ordered by version number descending', async ({ request }) => {
    // Given a questionnaire with specific publish sequence
    const { questionnaire } = await createQuestionnaire(request, undefined, 'Order Test');
    const id = questionnaire.id;

    const { question } = await createQuestion(request, id, undefined, 'Q1?')

    await createSingleAnswer(request, {
        questionnaireId: id, questionId: question.id, content: 'A1',
        destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
    })

    // Publish version 1
    await updateQuestionnaire(request, id, { title: 'v1' });
    await publishQuestionnaire(request, id);

    // Publish version 2
    await updateQuestionnaire(request, id, { title: 'v2' });
    await publishQuestionnaire(request, id);

    // Publish version 3
    await updateQuestionnaire(request, id, { title: 'v3' });
    await publishQuestionnaire(request, id /* date: '2024-03-01' */);

    // When requesting versions
    const { versions } = await listQuestionnaireVersions(request, id);

    // Then ordered by most recent (highest version first as proxy)
    expect(versions.length).toBe(4);
    const versionNumbers = versions.map((v:any) => v.version);
    const sorted = [...versionNumbers].sort((a, b) => b - a);
    expect(versionNumbers).toEqual(sorted);

    // And first version is the most recent (version 3)
    expect(versions[0].version).toBe(4);
});

// Scenario: Retrieve versions for questionnaire with single version
test('single-version questionnaire returns exactly one publish and one current draft', async ({ request }) => {
    const { questionnaire } = await createQuestionnaire(request, undefined, 'Single' );
    const id = questionnaire.id;

    const { question } = await createQuestion(request, id, undefined, 'Q1?')

    await createSingleAnswer(request, {
        questionnaireId: id, questionId: question.id, content: 'A1',
        destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
    })

    await updateQuestionnaire(request, id, { title: 'v1' });
    await publishQuestionnaire(request, id);

    await updateQuestionnaire(request, id, { title: 'vDraft' });

    const { versions } = await listQuestionnaireVersions(request, id);

    expect(Array.isArray(versions)).toBe(true);
    expect(versions.length).toBe(2);
    expect(versions[0].version).toBe(2);
    
    const getStatusChange = (version:any) => versions[version].changeLog.find((c:any) => c.path === '$.status');
    const statusChange = getStatusChange(0);

    expect(statusChange.thisValue).toBe("Published");
    expect(statusChange.thatValue).toBe("Draft");

    expect(getStatusChange(1)).toBeUndefined();
    expect(statusChange.thatValue).toBe("Draft");
});

// Scenario: Attempt to retrieve versions of non-existent questionnaire
test('non-existent questionnaire returns 404', async ({ request }) => {
    const nonExistentId = 'q-999';

    const { response: res } = await listQuestionnaireVersions(request, nonExistentId);

    expect(res.status()).toBe(404);
});

// Scenario: Unauthorized access attempt
test('unauthenticated request returns 403', async ({ request }) => {
    const { questionnaire } = await createQuestionnaire(request, JwtHelper.UnauthorizedToken);
    const id = questionnaire.id;

    const { response: res } = await listQuestionnaireVersions(request, id);
    expect(res.status()).toBe(403);
});

// Utility to safely parse questionnaireJson (string or object)
function asObject<T = any>(maybeJson: unknown): T {
    if (typeof maybeJson === 'string') {
        try {
            return JSON.parse(maybeJson) as T;
        } catch {
            // fall through
        }
    }
    return (maybeJson ?? {}) as T;
}