import {expect, APIResponse} from '@playwright/test';

const isEmpty = (v: any) =>
    v == null || (typeof v === 'string' && v.trim() === '');

//Expect 200 status codes
export function expect200HttpStatusCode(response: APIResponse, expectedStatus: number = 201) {
    expect(response.ok(), 'Response should be OK').toBeTruthy();
    expect(response.status(), `Expected status ${expectedStatus}`).toBe(expectedStatus);
}

// --- Questionnaire related validation functions ---
export function expectQuestionnaireSchema(questionnaire: any) {
    // Add detailed logging
    //console.log('=== Questionnaire Schema Validation ===');
    //console.log('Questionnaire object:', JSON.stringify(questionnaire, null, 2));
    //console.log('Questionnaire is null?', questionnaire === null);
    //console.log('Questionnaire is undefined?', questionnaire === undefined);
    //console.log('Questionnaire type:', typeof questionnaire);

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
        const hasProperty = questionnaire && questionnaire.hasOwnProperty(prop);
        //console.log(`Property '${prop}': ${hasProperty ? '✅ EXISTS' : '❌ MISSING'}`);
        if (hasProperty) {
            //console.log(`  Value: ${JSON.stringify(questionnaire[prop])}`);
            //console.log(`  Type: ${typeof questionnaire[prop]}`);
        }
        expect(questionnaire, `Missing property: ${prop}`).toHaveProperty(prop);
    }
    //console.log('=== Schema Validation Complete ===');
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
    // Add detailed logging
    //console.log('=== Question Schema Validation ===');
    //console.log('Question object:', JSON.stringify(question, null, 2));
    //console.log('Question is null?', question === null);
    //console.log('Question is undefined?', question === undefined);
    //console.log('Question type:', typeof question);

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
        const hasProperty = question && question.hasOwnProperty(prop);
        //console.log(`Property '${prop}': ${hasProperty ? '✅ EXISTS' : '❌ MISSING'}`);
        if (hasProperty) {
            //console.log(`  Value: ${JSON.stringify(question[prop])}`);
            //console.log(`  Type: ${typeof question[prop]}`);
        }
        expect(question, `Missing property: ${prop}`).toHaveProperty(prop);
    }
    //console.log('=== Schema Validation Complete ===');
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
    // Add detailed logging
    //console.log('=== Answer Schema Validation ===');
    //console.log('Answer object:', JSON.stringify(answer, null, 2));
    //console.log('Answer is null?', answer === null);
    //console.log('Answer is undefined?', answer === undefined);
    //console.log('Answer type:', typeof answer);

    const expectedProps = [
        'id',
        'questionId',
        'questionnaireId',
        'content',
        'description',
        'destinationUrl',
        'destinationType',
        'destinationQuestionId',
        'priority',
        'createdAt',
        'updatedAt'
    ];

    for (const prop of expectedProps) {
        const hasProperty = answer && answer.hasOwnProperty(prop);
        //console.log(`Property '${prop}': ${hasProperty ? '✅ EXISTS' : '❌ MISSING'}`);
        if (hasProperty) {
            //console.log(`  Value: ${JSON.stringify(answer[prop])}`);
            //console.log(`  Type: ${typeof answer[prop]}`);
        }
        expect(answer, `Missing property: ${prop}`).toHaveProperty(prop);
    }
    //console.log('=== Schema Validation Complete ===');
}

export function expectAnswerTypes(answer: any) {
    expect(typeof answer.id).toBe('string');
    expect(typeof answer.questionId).toBe('string');
    expect(typeof answer.questionnaireId).toBe('string');
    expect(typeof answer.content).toBe('string');
    expect(typeof answer.description).toBe('string');
    if (answer.destinationUrl) {
        expect(typeof answer.destinationUrl).toBe('string');
    }
    expect(typeof answer.priority).toBe('number');
    expect(typeof answer.createdAt).toBe('string');
    expect(typeof answer.updatedAt).toBe('string');
    if (answer.destinationQuestionId !== null) {
        expect(typeof answer.destinationType).toBe('number');
        expect(typeof answer.destinationQuestionId).toBe('string');
    }
}

export function expectAnswerContent(a: any) {
    expect(a.content?.trim().length).toBeGreaterThan(0);

    if (isEmpty(a.description)) {
        expect(isEmpty(a.description)).toBeTruthy();
    } else {
        expect(a.description.trim().length).toBeGreaterThan(0);
    }

    if (isEmpty(a.destinationUrl)) {
        expect(isEmpty(a.destinationUrl)).toBeTruthy();
    } else {
        expect(a.destinationUrl.trim().length).toBeGreaterThan(0);
    }

    if (a.destinationQuestionId) {
        expect([1, 2]).toContain(a.destinationType);
        expect(typeof a.priority).toBe('number');
        
        expect(a.destinationQuestionId.trim().length).toBeGreaterThan(0);
    }

    //timestamp-sanity
    const c = new Date(a.createdAt).getTime();
    const u = new Date(a.updatedAt).getTime();
    expect(!isNaN(c)).toBeTruthy();
    expect(!isNaN(u)).toBeTruthy();
    expect(u).toBeGreaterThanOrEqual(c);
}

export function expectAnswerIO(a: any, payload: any, guidRegex: RegExp) {
    expect(a.id).toMatch(guidRegex);
    expect(a.questionId).toBe(payload.questionId);
    expect(a.questionnaireId).toBe(payload.questionnaireId);
    expect(a.content).toBe(payload.content);
    expect(a.priority).toBe(payload.priority);

    if (isEmpty(payload.description)) {
        expect(isEmpty(a.description)).toBeTruthy();
    } else {
        expect(a.description).toBe(payload.description);
    }

    if (isEmpty(payload.destinationUrl)) {
        expect(isEmpty(a.destinationUrl)).toBeTruthy();
    } else if (payload.destinationType) {
        expect(a.destinationUrl).toBe(payload.destinationUrl);
    }

    if (payload.destinationQuestionId) {
        expect(a.destinationType).toBe(payload.destinationType);
    }
    
    if (!payload.destinationQuestionId || isEmpty(payload.destinationQuestionId)) {
        expect(a.destinationQuestionId === null || a.destinationQuestionId === undefined).toBeTruthy();
    } else {
        expect(a.destinationQuestionId).toBe(payload.destinationQuestionId);
        expect(a.destinationQuestionId).toMatch(guidRegex);
    }
}