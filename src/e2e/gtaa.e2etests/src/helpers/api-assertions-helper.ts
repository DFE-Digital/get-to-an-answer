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
        'slug'
    ];

    for (const prop of expectedProps) {
        expect(questionnaire, `Missing property:${prop}`).toHaveProperty((prop))
    }
}

export function expectQuestionnaireTypes(questionnaire: any) {
    expect(typeof questionnaire.id).toBe('string');
    expect(typeof questionnaire.title).toBe('string');
    expect(typeof questionnaire.description).toBe('string');
    expect(typeof questionnaire.slug).toBe('string');
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