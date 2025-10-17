import {AnswerBuilder} from '../builders/AnswerBuilder';
import {AnswerDestinationType} from '../constants/test-data-constants'

export async function createMultipleAnswers(
    request: any,
    questionId: string,
    numberOfAnswers: number,
    useDifferentDestinations?: boolean
) {
    const createdAnswers = [];
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

        const res = await request.post('/answers', {data: payload});

        if (!res.ok()) {
            throw new Error(`❌ Failed to create answer ${i}: ${res.status()}`);
        }

        const body = await res.json();
        createdAnswers.push(body);
    }

    console.log(
        `✅ Created ${numberOfAnswers} ${useDifferentDestinations ? 'different' : 'same'} outcomes for question ${questionId}`
    );
    return createdAnswers;
}

export async function createSingleAnswer(
    request: any,
    questionId: string,
    content?: string,
    description?: string,
    answerPrefix?: string,
    weight?: number,
    destinationType?: AnswerDestinationType,
    destination?: string
) {
    const payload = new AnswerBuilder(questionId)
        .withContent(content)
        .withContentPrefix(answerPrefix)
        .withDescription(description)
        .withDestination(destination)
        .withDestinationType(destinationType)
        .withWeight(weight)
        .build();

    const res = await request.post('/answers', {data: payload});

    if (!res.ok()) {
        throw new Error(`❌ Failed to create answer: ${res.status()}`);
    }

    const body = await res.json();
    console.log(
        `✅ Created 1 answer → destination "${destination}" for question ${questionId}`
    );
    return body;
}