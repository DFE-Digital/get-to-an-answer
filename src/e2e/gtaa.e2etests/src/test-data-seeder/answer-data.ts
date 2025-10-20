import {AnswerBuilder} from '../builders/AnswerBuilder';
import {AnswerDestinationType} from '../constants/test-data-constants'
import { JwtHelper } from "../helpers/JwtHelper";

export async function createMultipleAnswers(
    request: any,
    questionId: string,
    numberOfAnswers: number,
    useDifferentDestinations?: boolean,
    bearerToken?: string,
) {
    const createdAnswers = [];
    const createdPayloads = [];
    
    const destinationType = AnswerDestinationType.PAGE;
    const destination = '/default-destination'
    const weight = 0.0;

    for (let i = 1; i <= numberOfAnswers; i++) {
        const content = `Auto-generated answer content - Choice ${i}`;
        const description = `Auto-generated description - option ${i}`;

        // Either same destination or /default-destination-1, /default-destination-2, etc.
        const finalDestination = useDifferentDestinations ? `${destination}-${i}` : destination;

        const payload = new AnswerBuilder(questionId)
            .withContent(content)
            .withDescription(description)
            .withDestination(finalDestination)
            .withDestinationType(destinationType)
            .withWeight(weight)
            .build();

        const response = await request.post('/api/answers', {
            data: payload,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
            }
        });

        if (!response.ok()) {
            throw new Error(`❌ Failed to create answer ${i}: ${response.status()}`);
        }

        const body = await response.json();
        createdAnswers.push(body);
        createdPayloads.push(payload);
    }

    console.log(
        `✅ Created ${numberOfAnswers} ${useDifferentDestinations ? 'different' : 'same'} outcomes for question ${questionId}`
    );
    return {createdAnswers, createdPayloads};
}

interface CreateAnswerRequest {
    questionId: string;
    content?: string;
    description?: string;
    answerPrefix?: string;
    weight?: number;
    destinationType?: AnswerDestinationType;
    destination?: string;
}

export async function createSingleAnswer(
    request: any,
    answerRequest: CreateAnswerRequest,
    bearerToken?: string,
) {
    const payload = new AnswerBuilder(answerRequest.questionId)
        .withContent(answerRequest.content)
        .withContentPrefix(answerRequest.answerPrefix)
        .withDescription(answerRequest.description)
        .withDestination(answerRequest.destination)
        .withDestinationType(answerRequest.destinationType)
        .withWeight(answerRequest.weight)
        .build();

    const response = await request.post('/api/answers', {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to create answer: ${response.status()}`);
    }

    const answerPostResponse = await response.json();
    console.log(
        `✅ Created 1 answer → destination "${answerRequest.destination}" for question ${answerRequest.questionId}`
    );
    return {answerPostResponse, payload};
}

export async function getAnswer(
    request: any,
    answerId: number,
    bearerToken?: string,
) {
    const response = await request.get(`/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to get answer: ${response.status()}`);
    }
    
    return await response.json();
}

export async function listAnswers(
    request: any,
    questionId: string,
    bearerToken?: string,
) {
    const response = await request.get(`/api/questions/${questionId}/answers`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to get answers: ${response.status()}`);
    }

    return await response.json();
}

export async function updateAnswer(
    request: any,
    answerId: number,
    data: any,
    bearerToken?: string,
) {
    const response = await request.put(`/api/answers/${answerId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to update answer: ${response.status()}`);
    }

    return await response.json();
}

export async function deleteAnswer(
    request: any,
    answerId: number,
    bearerToken?: string,
) {
    const response = await request.delete(`/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to delete answer: ${response.status()}`);
    }

    return await response.json();
}