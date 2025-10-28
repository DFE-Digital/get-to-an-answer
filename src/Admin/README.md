# Admin — Questionnaire Management Console

The Admin project is a secure back-office web application for planning, editing, previewing, and publishing questionnaires used by the service. It enables authorised users to model question flows, manage content, collaborate with contributors, and control versioning and publication lifecycle.

## Key capabilities
- Create and organise questionnaires, questions, and answers
- Manage content blocks, start pages, and slugs
- Configure support contact details and privacy information
- Invite/manage contributors and track changes over time
- Preview questionnaire journeys before publishing
- Publish, unpublish, clone, and delete questionnaires with version history

## User flow (high level)

```mermaid
flowchart TD
  A[Sign in / Access Admin] --> B[Manage Questionnaires]
  B -->|Create| C[Create Questionnaire]
  B -->|Edit| D[Edit Questionnaire Details]
  D --> E[Manage Questions]
  E --> F[Add Answers & Destinations]
  D --> G[Manage Content Blocks]
  D --> H[Set Start Page & Slug]
  D --> I[Support & Privacy Details]
  D --> J[Manage Contributors]
  D --> K[Preview Journey]
  K --> L{Ready to publish?}
  L -- Yes --> M[Publish Questionnaire]
  L -- No --> E
  M --> N[Track Versions & Changes]
  M --> O[Unpublish or Clone if needed]
```


## Pages and purpose

```mermaid
graph LR
  MQ[Manage Questionnaires] --- CQ[Create Questionnaire]
  MQ --- EQ[Edit Questionnaire]
  EQ --- SQ[Edit Start Page / Slug]
  EQ --- MC[Manage Content]
  EQ --- MQs[Manage Questions]
  MQs --- AQ[Add Question]
  AQ --- AA[Add Answers]
  MQs --- EQn[Edit Question]
  MC --- CC[Create Content]
  MC --- EC[Edit Content]
  EQ --- SP[Support & Privacy]
  EQ --- INV[Manage Contributors]
  EQ --- PRV[Preview]
  EQ --- PUB[Publish / Unpublish]
  EQ --- CLN[Clone]
  EQ --- DEL[Delete]
  N1[Version Diff]:::muted -.-> EQ

  classDef muted fill:#f5f5f5,stroke:#bbb,color:#666;
```


- Manage Questionnaires: list and access drafts/published items.
- Create Questionnaire: start a new questionnaire draft.
- Edit Questionnaire: change titles, descriptions, and settings.
- Edit Start Page / Slug: configure entry content and public URL path.
- Manage Questions: view ordering and structure of questions.
- Add Question / Edit Question: define prompts, types, and help text.
- Add Answers: add options, routing destinations, scores.
- Manage Content: list reusable content blocks for the journey.
- Create Content / Edit Content: author and update content blocks.
- Support & Privacy: configure support contact details and privacy info.
- Manage Contributors: invite/remove collaborators.
- Preview: simulate the end-user journey before release.
- Publish / Unpublish: control visibility and create version snapshots.
- Clone: duplicate an existing questionnaire for iteration.
- Delete: remove a questionnaire (subject to rules).
- Version Diff: compare versions to see what changed.