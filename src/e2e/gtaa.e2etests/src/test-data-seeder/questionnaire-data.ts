import {QuestionnaireBuilder} from '../builders/QuestionnaireBuilder';

export async function createQuestionnaire(
    request: any,
    questionnairePrefix?: string,
    title?: string,
    description?: string,
    slug?: string
) {
    const payload = new QuestionnaireBuilder()
        .withTitle(title)
        .withTitlePrefix(questionnairePrefix)
        .withDescription(description)
        .withSlug(slug)
        .build();

    const response = await request.post('/questionnaires', {data: payload});

    if (!response.ok()) {
        throw new Error(`Failed to create questionnaire: ${response.status()}`);
    }
    return await response.json();
}

export async function getQuestionnaire(
    request: any,
    questionnaireId: number
) {
    const response = await request.get(`/questionnaires/${questionnaireId}`);

    if (!response.ok()) {
        throw new Error(`Failed to fetch required questionnaire: ${response.status()}`);
    }
    return await response.json();
}