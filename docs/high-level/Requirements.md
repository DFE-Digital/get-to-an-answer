# GTAA Requirements Document

## Objectives
- Provide a public-facing questionnaire experience that guides users from a start page through sequential questions to custom information pages or external destinations.
- Provide an admin console to plan, create, edit, preview, version, and publish questionnaires, including content, support details, and privacy information.
- Align with to the DfE Standards and GOV.UK Design System patterns for usability and accessibility.

## Stakeholders
- Public Users: complete the questionnaire, receive guidance and support.
- Content Designers/Admins: configure questionnaires, content, routing, and publication lifecycle.
- Service Owners: ensure compliance, accessibility, and reliability.
- Technical Team: build, operate, support the platform and integration into their own systems (via the api).

## User Personas and Needs
- Public User
    - Understand purpose and how data is used.
    - Answer one question per page with clear guidance and validation.
    - Be routed to appropriate next steps or external services.
    - Access support information at any time.
- Admin/Designer
    - Create and manage questionnaires, questions, answers, and routing.
    - Manage start page content, slugs/URLs, reusable content blocks.
    - Configure support and privacy information.
    - Preview journeys and publish with version control and change history.
    - Manage contributors and permissions.
- Developers
    - Integrate questionnaire experience into their own systems.
    - Integrate the platform with their contentful and CMS systems.

## Scope
- In-Scope
    - Public site: start page, question pages (one question per page), validation, routing to destinations, support and privacy pages.
    - Admin: CRUD for questionnaires, questions, answers, content; routing configuration; preview; publish/unpublish/clone/delete; version history; contributors.
    - Accessibility, analytics hooks, basic auditing.
    - Contentful integration to load questionnaire iframes into their own pages.
    - Open API service for self service integration.
    - Questionnaire iframe for self service integration.
- Nice to have:
    - Custom content pages (reusable content blocks).
    - Support and privacy pages with contact details and privacy policy.
- Out-of-Scope
    - Payments, identity verification, complex reporting dashboards, multilingual content (unless specified), advanced conditional logic beyond defined routing rules.

## Functional Requirements

### Frontend - Public Questionnaire
- Questionnaire IFrame
    - Embed questionnaire iframe into a page manually or via a CMS integration.
- Start Page
    - Display title, description, what to expect, and start button.
    - Link to support and privacy information.
- Question Pages
    - One question per page, using appropriate component types (e.g., radios, checkboxes, inputs) per GOV.UK Design System.
    - Show hint/help text and optional guidance content.
    - Validate inputs (required, type/format, option constraints) with error summaries.
    - Persist answers per session and allow back navigation.
- Routing and Destinations
    - Configure next question or destination per answer selection.
    - Support branch/skip logic and terminal outcomes (custom page or external link).
    - Display result page content or redirect externally with parameters where needed.
- Support and Privacy
    - Dedicated pages with contact details and privacy policy.
    - Accessible from start and throughout the flow.
- Session Management
    - Anonymous session for answers; timeout handling and restart option.
- Accessibility
    - WCAG 2.2 AA alignment; semantic markup; keyboard operability; focus management; error messaging; ARIA where needed.

### Admin Console
- Authentication/Authorisation
    - Secure access; invitation based permissions (Admin <=> Editor).
- Questionnaire Management
    - List, filter, and search questionnaires by status (draft/published).
    - Create, edit metadata (titles, descriptions), clone, delete (soft delete).
- Start Page and Slug
    - Configure entry content, guidance, and public slug/URL.
- Questions and Answers
    - Add/edit/delete questions; order and group them.
    - Configure answer options, destinations (routing; question, external link and custom page).
    - Support common types (single-select, multi-select and dropdown options).
- Custom Content Pages (nice to have)
    - Create/edit reusable content (help/guidance/partials) for insertion in pages.
- Support & Privacy Configuration
    - Define support contact details (email, phone, hours, links).
    - Maintain privacy statements and versioning notes.
- Preview
    - Simulate user journey with current draft configuration, including routing.
- Publication Lifecycle
    - Publish/unpublish with a version snapshot and changelog.
    - View version history and diffs between versions.
- Contributors
    - Invite/manage contributors; show audit trail of changes.
- Version Control
    - Track changes to questionnaire configuration.
    - View diff between versions.
    - Revert to previous version.
    - Unpublish to revert to previous state.
    - Clone to create a new draft.
    - Delete (soft delete) to remove from the platform.
- Customise Branding and Theme
  - Questionnaire button text, page font, colour palettes, logo, etc.

### API Service
- Authentication/Authorisation
    - Secure access.
- Open API (open to DfE)
    - Expose questionnaire configuration via an open API service.

## Non-Functional Requirements
- Accessibility: PA11y
- Availability: 99.9% for public experience; scheduled maintenance windows communicated.
- Security: OWASP ASVS L2; protect against XSS, CSRF, SSRF, injection; secure headers; secrets management.
- Privacy: Data minimisation; clear privacy notice; retention policy; DPIA where required.
- Observability: Structured logs with correlation IDs; basic metrics (traffic, completion rate, errors); alerting on error rates and latency.
- Scalability: Handle expected concurrency spikes; horizontal scaling.
- Compliance: Adhere to GOV.UK Design System and service standards.

## Content and Design Requirements
- Use GOV.UK Design System components for:
    - Start page pattern, radios, checkboxes, inputs, error summary, back link, buttons.
- One question per page pattern; clear page titles; progressive disclosure where appropriate.
- Plain language; helper text for complex questions; consistent tone.

## Data Model (conceptual)
- Questionnaire: id, slug, title, description, status, versions.
- QuestionnaireVersion: version number, created by, created at, changelog, published flag.
- Question: id, questionnaireId, type, prompt, hint, order, validation rules.
- Answer: id, questionId, label, value, nextDestination (questionId or external).
- Content: id, key, body, format.
- Audit: entityId, entityType, action, actor, timestamp, diff.

## Routing Logic Requirements
- Each answer option may define:
    - Each answer is score for multi-selection, so the selected answer with the highest score to dictates the next route.
    - Next question ID OR custom content page ID OR external URL.
- Terminal outcomes:
    - Custom page content OR redirect to external URL, both with relevant guidance and support.
- Validation ensures no orphan questions or dead ends or cycles.

## Error Handling
- Public
    - Input validation errors with inline messages and error summary.
    - Friendly error page for 404/500 with support contact.
- Admin
    - Form-level validation; conflict handling on concurrent edits; descriptive error messages.

## Analytics and Telemetry
- Page views and drop-off points.
- Answered vs skipped (where applicable).
- Outcome distribution (without PII).
- Admin actions audit trail.

## Security and Privacy
- DfE SSO; strong auth; session timeout; CSRF protection.
- Public: no data collection; consent where tracking is used; cookie banner if applicable.
- Secrets via environment configuration; rotation policy.

## Release and Versioning
- Draft → Preview → Publish (immutable snapshot).
- Unpublish reverts public endpoint to previous version or offline state.
- Version diff to highlight changes (content, routing, validation).

## Operational Requirements
- Environments: Dev, Test, Staging, Production.
- Configuration via environment variables.
- Automated tests: unit, integration, e2e for critical paths (start → outcome).
- Backups for configuration datastore; recovery procedures documented.

## Acceptance Criteria (high level)
- Public journey
    - Given a published questionnaire, when a user starts, they can complete a path to a valid destination with correct routing.
    - Validation errors are shown per design system; accessibility checks pass key flows.
- Admin
    - Can create a questionnaire, add questions and routing, preview, and publish.
    - Version history records changes; diff view available; unpublish works.
- Compliance
    - Pages meet GOV.UK Design System patterns; axe checks show no critical violations.

## Risks and Mitigations
- Misconfigured routing causing dead ends → Pre-publish validation and graph checks.
- Accessibility regressions → CI accessibility tests and design review.
- Content sprawl/duplicates → Reusable content blocks and governance.
- PII leakage → Data minimisation, privacy reviews, and log scrubbing.

## Future Enhancements
- Advanced logic (multi-criteria branching).
- Multilingual content.
- Exportable analytics reports.
- Integration with identity or CRM systems.
- A/B testing on content variants.

## Glossary
- Questionnaire: A collection of questions and routes leading to outcomes.
- Outcome/Destination: Final step for a branch, either a custom page or external link.
- Custom Content Pages: Custom pages hosted by this service to help provide additional relevant content to service users.
- Version: Immutable snapshot of a questionnaire’s configuration at publication.