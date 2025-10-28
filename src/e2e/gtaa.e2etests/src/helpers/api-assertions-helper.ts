import {expect, APIResponse} from '@playwright/test';

const isEmpty = (v: any) =>
    v == null || (typeof v === 'string' && v.trim() === '');

export function expectHttpStatusCode(response: APIResponse, expectedStatus: number = 201) {
    expect(response.ok(), 'Response should be OK').toBeTruthy();
    expect(response.status(), `Expected status ${expectedStatus}`).toBe(expectedStatus);
}

// --- Questionnaire related validation functions ---
export function expectQuestionnaireSchema(questionnaire: any) {
    const expectedProps = [
        'id',
        'title',
        'description',
        'slug',
        'createdAt',
        'updatedAt',
        'status',
        'version',
        'questions'
    ];

    for (const prop of expectedProps) {
        expect(questionnaire, `Missing property:${prop}`).toHaveProperty((prop))
    }
}

export function expectQuestionnaireInitStateTypes(questionnaire: any) {
    expect(typeof questionnaire.id).toBe('string');
    expect(typeof questionnaire.title).toBe('string');
    expect(typeof questionnaire.createdAt).toBe('string');
    expect(typeof questionnaire.updatedAt).toBe('string');
    expect(typeof questionnaire.status).toBe('number');
    expect(typeof questionnaire.version).toBe('number');
    expect(Array.isArray(questionnaire.questions)).toBe(true);
}

export function expectQuestionnaireTypes(questionnaire: any) {
    expect(typeof questionnaire.id).toBe('string');
    expect(typeof questionnaire.title).toBe('string');
    expect(typeof questionnaire.description).toBe('string');
    expect(typeof questionnaire.slug).toBe('string');
    expect(typeof questionnaire.createdAt).toBe('string');
    expect(typeof questionnaire.updatedAt).toBe('string');
    expect(typeof questionnaire.status).toBe('number');
    expect(typeof questionnaire.version).toBe('number');
    expect(Array.isArray(questionnaire.questions)).toBe(true);
}

export function expectQuestionnaireInitStateContent(q: any) {

    expect(q.title?.trim().length).toBeGreaterThan(0);

    expect(isEmpty(q.description)).toBeTruthy();
    expect(isEmpty(q.slug)).toBeTruthy();

    expect(q.questions.length).toBeGreaterThanOrEqual(0);
    expect([1, 2, 3, 4]).toContain(q.status);
    expect(q.version).toBeGreaterThanOrEqual(0);

    //timestamp-sanity
    const c = new Date(q.createdAt).getTime();
    const u = new Date(q.updatedAt).getTime();
    expect(!isNaN(c)).toBeTruthy();
    expect(!isNaN(u)).toBeTruthy();
    expect(u).toBeGreaterThanOrEqual(c);
}

export function expectQuestionnaireContent(q: any) {

    expect(q.title?.trim().length).toBeGreaterThan(0);

    if (isEmpty(q.description)) {
        expect(isEmpty(q.description)).toBeTruthy();
    } else {
        expect(q.description.trim().length).toBeGreaterThan(0);
    }

    if (isEmpty(q.slug)) {
        expect(isEmpty(q.slug)).toBeTruthy();
    } else {
        expect(q.slug.trim().length).toBeGreaterThan(0);
    }

    expect(q.questions.length).toBeGreaterThanOrEqual(0);
    expect([1, 2, 3, 4]).toContain(q.status);
    expect(q.version).toBeGreaterThanOrEqual(0);

    //timestamp-sanity
    const c = new Date(q.createdAt).getTime();
    const u = new Date(q.updatedAt).getTime();
    expect(!isNaN(c)).toBeTruthy();
    expect(!isNaN(u)).toBeTruthy();
    expect(u).toBeGreaterThanOrEqual(c);
}

export function expectQuestionnaireInitStateIO(q: any, payload: any, guidRegex: RegExp) {
    expect(q.id).toMatch(guidRegex);
    expect(q.title).toBe(payload.title);
    expect(isEmpty(q.description)).toBeTruthy();
    expect(isEmpty(q.slug)).toBeTruthy();
}

export function expectQuestionnaireIO(q: any, payload: any, guidRegex: RegExp) {
    expect(q.id).toMatch(guidRegex);
    expect(q.title).toBe(payload.title);

    if (isEmpty(payload.description)) {
        expect(isEmpty(q.description)).toBeTruthy();
    } else {
        expect(q.description).toBe(payload.description);
    }

    if (isEmpty(payload.slug)) {
        expect(isEmpty(q.slug)).toBeTruthy();
    } else {
        expect(q.slug).toBe(payload.slug);
    }
}

// --- Question related validation functions ---
export function expectQuestionSchema(question: any) {
    const expectedProps = [
        'id',
        'questionnaireId',
        'order',
        'content',
        'description',
        'type',
        'createdAt',
        'updatedAt',
        'answers'
    ];

    for (const prop of expectedProps) {
        expect(question, `Missing property:${prop}`).toHaveProperty((prop))
    }
}

export function expectQuestionTypes(question: any) {
    expect(typeof question.id).toBe('string');
    expect(typeof question.order).toBe('number');
    expect(typeof question.content).toBe('string');
    expect(typeof question.description).toBe('string');
    expect(typeof question.type).toBe('number');
    expect(typeof question.createdAt).toBe('string');
    expect(typeof question.updatedAt).toBe('string');
    expect(Array.isArray(question.answers)).toBe(true);
}

export function expectQuestionContent(q: any) {

    expect(q.order).toBeGreaterThanOrEqual(0);
    expect(q.content?.trim().length).toBeGreaterThan(0);

    if (isEmpty(q.description)) {
        expect(isEmpty(q.description)).toBeTruthy();
    } else {
        expect(q.description.trim().length).toBeGreaterThan(0);
    }

    expect(q.answers.length).toBeGreaterThanOrEqual(0);
    expect([1, 2, 3]).toContain(q.type);

    //timestamp-sanity
    const c = new Date(q.createdAt).getTime();
    const u = new Date(q.updatedAt).getTime();
    expect(!isNaN(c)).toBeTruthy();
    expect(!isNaN(u)).toBeTruthy();
    expect(u).toBeGreaterThanOrEqual(c);
}

export function expectQuestionIO(q: any, payload: any, guidRegex: RegExp) {
    expect(q.id).toMatch(guidRegex);
    expect(q.content).toBe(payload.content);
    expect(q.type).toBe(payload.type);
    
    if (isEmpty(payload.description)) {
        expect(isEmpty(q.description)).toBeTruthy();
    } else {
        expect(q.description).toBe(payload.description);
    }
}
// --- Answer related validation functions ---
export function expectAnswerSchema(answer: any) {
    const expectedProps = [
        'id',
        'questionId',
        'questionnaireId',
        'content',
        'description',
        'destinationUrl',
        'destinationType',
        'score',
        'createdAt',
        'updatedAt'
    ];
    
    for (const prop of expectedProps) {
        expect(answer, `Missing property:${prop}`).toHaveProperty((prop))
    }
}