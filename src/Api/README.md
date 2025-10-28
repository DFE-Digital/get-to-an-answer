# API — Questionnaire Service Backend

The API project provides the backend HTTP endpoints for managing and serving questionnaire data. It supports secure operations for authoring and lifecycle management, and exposes read endpoints for the public experience to consume published content.

## Key capabilities
- Create, read, update, publish/unpublish, delete questionnaires
- Manage questions, answers, content, and contributors
- Clone questionnaires and track versioned snapshots
- Expose read endpoints consumed by Frontend

## High-level request flow

```mermaid
sequenceDiagram
  autonumber
  participant Admin as Admin UI
  participant Frontend as Public Frontend
  participant API as API
  participant DB as Persistence

  Admin->>API: Authenticated write requests (CRUD, publish, contributors)
  API->>DB: Validate access, persist changes
  API-->>Admin: Results (IDs, status, errors)

  Frontend->>API: Read published questionnaire
  API->>DB: Load latest published version
  API-->>Frontend: Questionnaire JSON (structure, content)
```


## Domain interactions

```mermaid
flowchart LR
  A[Questionnaire] --> B[Questions]
  B --> C[Answers]
  A --> D[Content Blocks]
  A --> E[Contributors]
  A --> F[Versions]
  F --> G[Published Snapshots]

  style A fill:#e8f4ff,stroke:#5aa0ff,color:#003e74
  style F fill:#f5f5f5,stroke:#bbb,color:#333
```


## Endpoint purposes (conceptual)
- Questionnaires: create, get one/list, update metadata, publish/unpublish/delete.
- Questions: add/edit/remove questions within a questionnaire.
- Answers: add/edit/remove answers, destinations, and scoring.
- Content: manage reusable content blocks.
- Contributors: list/add/remove collaborators.
- Versions: store and retrieve published snapshots for diffing and rollback scenarios.