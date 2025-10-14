module.exports = function (migration) {
    // Questionnaire
    const questionnaire = migration.createContentType('questionnaire')
        .name('Questionnaire')
        .displayField('title')
        .description('Questionnaire root entity');

    questionnaire.createField('title').name('Title').type('Symbol').required(true);
    questionnaire.createField('slug').name('Slug').type('Symbol').required(false);
    questionnaire.createField('description').name('Description').type('Text');
    questionnaire.createField('status').name('Status').type('Symbol').validations([
        { in: ['Draft', 'Published', 'Archived', 'Deleted'] }
    ]);
    questionnaire.createField('version').name('Version').type('Integer').defaultValue({ 'en-US': 0 });
    questionnaire.createField('contributors').name('Contributors').type('Array').items({ type: 'Symbol' });
    questionnaire.createField('createdAtUtc').name('Created At (UTC)').type('Date');
    questionnaire.createField('updatedAtUtc').name('Updated At (UTC)').type('Date');

    // Question
    const question = migration.createContentType('question')
        .name('Question')
        .displayField('content');

    question.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    question.createField('content').name('Content').type('Text').required(true);
    question.createField('description').name('Description').type('Text');
    question.createField('type').name('Type').type('Symbol').validations([
        { in: ['SingleSelect', 'MultiSelect', 'DropdownSelect']}
    ]);
    question.createField('order').name('Order').type('Integer').required(true);
    question.createField('status').name('Status').type('Symbol').validations([
        { in: ['Draft', 'Published', 'Archived', 'Deleted'] }
    ]);
    question.createField('createdAtUtc').name('Created At (UTC)').type('Date');
    question.createField('updatedAtUtc').name('Updated At (UTC)').type('Date');

    // Answer
    const answer = migration.createContentType('answer')
        .name('Answer')
        .displayField('content');

    answer.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    answer.createField('question').name('Question').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['question'] }]).required(true);
    answer.createField('content').name('Content').type('Text').required(true);
    answer.createField('description').name('Description').type('Text');
    answer.createField('score').name('Score').type('Number');
    answer.createField('destinationType').name('Destination Type').type('Symbol').validations([
        { in: ['Question', 'ExternalLink', 'InternalPage'] }
    ]);
    answer.createField('destination').name('Destination (URL)').type('Symbol');
    answer.createField('destinationQuestion').name('Destination Question').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['question'] }]);
    answer.createField('destinationContent').name('Destination Content').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['content'] }]);
    answer.createField('createdAtUtc').name('Created At (UTC)').type('Date');
    answer.createField('updatedAtUtc').name('Updated At (UTC)').type('Date');

    // Content (internal pages)
    const content = migration.createContentType('content')
        .name('Content')
        .displayField('title');

    content.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    content.createField('title').name('Title').type('Symbol').required(true);
    content.createField('content').name('Content Body').type('RichText'); // or 'Text' if you prefer
    content.createField('createdAtUtc').name('Created At (UTC)').type('Date');
    content.createField('updatedAtUtc').name('Updated At (UTC)').type('Date');

    // Questionnaire Version (snapshot)
    const questionnaireVersion = migration.createContentType('questionnaireVersion')
        .name('Questionnaire Version')
        .displayField('version')
        .description('Snapshot of a questionnaire at publish time');

    questionnaireVersion.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    questionnaireVersion.createField('version').name('Version').type('Integer').required(true);
    questionnaireVersion.createField('questionnaireJson').name('Questionnaire JSON').type('Text').required(true);
    questionnaireVersion.createField('createdAtUtc').name('Created At (UTC)').type('Date');

    // Optional: indexes/uniqueness hints (apply manually if needed)
    // - questionnaire.slug unique per space
    // - question order unique within questionnaire (enforce in app)
    // - answers per question (enforce in app)
};