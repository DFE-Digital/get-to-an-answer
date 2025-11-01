# Comparison: “Get to an answer” service vs Riddle, for Care Leavers

## Summary
- Recommendation: Migrate to “Get to an answer” for improved accessibility, integration, control, and cost-efficiency, with a simpler authoring model aligned to GOV.UK and DfE standards.

1) Purpose and fit
- Riddle: General-purpose quiz/survey/assessment builder with hosted deployment and embed options. Optimized for marketing and lead capture; adaptable for decision trees but not purpose-built.
- Get to an answer: Purpose-built decision/triage flow tool aligned to GOV.UK patterns, designed to guide users to actionable outcomes within DfE services and sites.

2) Accessibility
- Riddle: Claims WCAG support but varies by theme and embed; customization can introduce accessibility debt; limited control over underlying markup.
- Get to an answer: Uses GDS Design System components/patterns, supports AAA-aligned implementations, standard keyboard navigation, ARIA semantics, error summaries, and consistent heading structures.

3) Integration and rendering
- Riddle: Primarily iframe/embed or hosted links; limited server-side rendering control; translation typically handled outside with duplicated flows or per-variant content.
- Get to an answer: API-first; render natively in product/site (SSR-friendly). Enables:
    - Central content model with on-site rendering
    - Integrated translation/localisation pipelines (potentially)
    - Shared analytics, consent, and cookies policies
    - Consistent design system and routing

4) Auth and management
- Riddle: Separate account system; SSO varies by plan; admin management outside DfE tenant.
- Get to an answer: Integrates with DfE O365 login (Azure AD), enabling:
    - Centralised account lifecycle
    - Role-based access with DfE groups
    - Reduced onboarding/offboarding friction
    - Audit and compliance within DfE

5) Costs and licensing
- Riddle: Ongoing per-seat or tiered plan costs; additional charges for advanced features/SSO; vendor lock-in.
- Get to an answer: Reduced ongoing licensing; internal hosting and open standards; costs primarily in development and platform ops, amortised across services.

6) Flow design and authoring
- Riddle: Visual builder for quizzes/surveys; branching supported but can be complex; decision logic embedded in tool-specific UI; portability limited.
- Get to an answer: Simplified flow model aligned to GOV.UK decision tree conventions:
    - Clear node/edge content model
    - Reusable components, conditional logic, and outcomes
    - Versioning and change control suited to service design
    - Easier maintenance by service teams

7) Content governance and versioning
- Riddle: Version history varies; publishing workflows basic; content export limited to vendor formats.
- Get to an answer: Designed to work with DfE workflows:
    - Git-backed or API-based versioning
    - Environments (dev/test/prod)
    - Review/approval gates and audit trails

8) Data, privacy, and compliance
- Riddle: Data processed within vendor infrastructure; DPIA and DPA required; cookie and analytics vary by embed mode.
- Get to an answer: Data remains within DfE boundaries; simpler DPIA; conforms to existing cookies/analytics frameworks; easier to meet GOV.UK and DfE data policies.

9) Performance and reliability
- Riddle: Dependent on third-party uptime and CDN; performance of embeds can vary.
- Get to an answer: First-party hosting and caching; tighter control of performance budgets, error handling, and observability.

10) Analytics and insights
- Riddle: Built-in analytics biased toward marketing metrics; custom events require workarounds.
- Get to an answer: Native integration with DfE/GOV.UK analytics; granular event tracking aligned to service KPIs; easier funnel analysis across journeys.

11) Theming and UX consistency
- Riddle: Theming via templates; limited fidelity to GOV.UK standards; risk of “non-GOV” look/feel.
- Get to an answer: Native GOV.UK styles and components; consistent typography, spacing, and error states.

12) Internationalisation and translation
- Riddle: Usually duplicate flows per language; limited translation workflow integration.
- Get to an answer: API-driven rendering enables language switching on-site; integrate with translation memory and content pipelines; single source with locale overlays.

13) Vendor lock-in and portability
- Riddle: Logic and content locked to vendor; exports partial.
- Get to an answer: Content and logic represented in open, internal formats; portable across DfE services.

14) Risks and mitigations
- Ongoing maintenance: This will sit under DesignOps so technical documentation needs to be robust.
- Initial migration effort: Mitigate with staged migration, parity checks, and dual-run testing.
- Authoring training: Provide concise guidance and templates aligned to GOV.UK patterns.

## Decision drivers (from Care Leavers)
- AAA accessibility via GDS components
- API rendering for on-site integration and translation
- DfE O365 SSO for simple management
- Reduced licensing costs
- Simplified, maintainable flow system

## Recommendation
- Proceed with migration to “Get to an answer.”
- Pilot: Migrate one representative Care Leavers flow end-to-end, validate accessibility, analytics, and translation.
- Rollout: Migrate remaining flows with a standardised template, shared components, and a deprecation plan for Riddle embeds.
- Governance: Establish content versioning, release process, and ownership within the Care Leavers team.

## Glossary
- SSR: Server-side rendering
- API: Application Programming Interface
- SSO: Single Sign-On
- AAA: Accessibility, Usability, and Accessibility
- GDS: Government Digital Service
- DfE: Department for Education
- DPA: Data Protection Act
- DPIA: Data Protection Impact Assessment
- DPA: Data Protection Act
- DPIA: Data Protection Impact Assessment