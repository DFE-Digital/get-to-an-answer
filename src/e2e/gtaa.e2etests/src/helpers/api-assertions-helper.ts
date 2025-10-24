import {expect, APIResponse} from '@playwright/test';

const isEmpty = (v: any) =>
    v == null || (typeof v === 'string' && v.trim() === '');

export function expectHttp(response: APIResponse, expectedStatus: number = 201) {
    expect(response.ok(), 'Response should be OK').toBeTruthy();
    expect(response.status(), `Expected status ${expectedStatus}`).toBe(expectedStatus);
}

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

export function expectQuestionnaireTypes(q: any) {
    expect(typeof q.id).toBe('string');
    expect(typeof q.title).toBe('string');
    expect(typeof q.description).toBe('string');
    expect(typeof q.slug).toBe('string');
    expect(typeof q.createdAt).toBe('string');
    expect(typeof q.updatedAt).toBe('string');
    expect(typeof q.status).toBe('number');
    expect(typeof q.version).toBe('number');
    expect(Array.isArray(q.questions)).toBe(true);
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