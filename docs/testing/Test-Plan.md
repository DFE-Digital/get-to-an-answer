# Test Plan for GTAA

## Revision/Document History

| Version | Date       | Author       | Reviewer(s) | Approver | Description of Changes           |
|---------|------------|--------------|-------------|----------|----------------------------------|
| 1.0     | 26/01/2026 | Imran Khalid |             |          | Initial draft of the test plan   |

## Introduction

### Overview

Care leavers face unique challenges and often lack access to clear support and guidance. As a result, there was a need to design a new service that helps users understand the support options and entitlements available to them based on care leavers' status.

Many care leavers are not aware of their status, and the language used to describe statuses is technical and unfamiliar to young people. Determining a care leavers status isn't simple. It's based on a combination of factors including age, duration in care and the type of care provision they received.

A status checker helps simplify this process for young people. Due to time and resource constraints this need was initially met with a 3rd party solution. The service is now being replaced with a new in-house 'get-to-an-answer' solution.

## Testing Objectives

- Validate that all functionalities meet the business requirements.
- Ensure seamless integration between the web application and external systems.
- Verify the responsiveness and performance of the website.
- Identify and resolve defects before release.
- Focus on user-centric testing, prioritizing accessibility and usability.
- Validate mobile-first design responsiveness.

## Testing Scope

### In-Scope

#### Functional Testing
- Validating admin features
- Front-end content retrieval
- API-driven functionality

#### UI/UX Testing
- Ensuring user-friendly navigation
- Intuitive content display
- Adherence to design guidelines

#### Additional In-Scope Items
- Mobile Responsiveness
- Integration Testing (iFrame embedding)
- Translation Services (Google & Azure Translate API)
- API Testing
- API Failover handling
- Performance & Security Testing

#### Cross-Browser Support
Supported Browsers:
- Chrome (latest 2 versions)
- Edge (latest 2 versions)
- Firefox (latest version)
- Safari-webkit
- iOS (Safari, latest version)
- Android (Chrome, latest version)

### Out-of-Scope

- Contentful Infrastructure Testing
- Third-Party Translation API Behaviour Changes

## Test Approach

### Testing Methodologies

#### Manual Testing
- Exploratory tests
- Smoke and sanity tests

#### Automated Testing
- UI/UX testing
- End to End Testing
- Regression testing

#### Tools
- Playwright with TypeScript
- Zap security scan
- Pa11y

### Testing Phases

#### Unit Testing

**Developer Responsibilities:**
- Cover assigned tickets with unit tests
- Mark tickets as "Dev done" only after completing unit tests

**Guidelines:**
- Focus on individual functions or classes
- Follow clear naming convention (e.g., Method_Condition_ExpectedOutcome)
- Test only public methods and interfaces
- Ensure tests are self-contained and repeatable

**Continuous Integration (CI):**
- Automatic tests with each code check-in
- Halt CI pipeline on test failure

#### Component Integration Testing

**Developer Responsibilities:**
- Verify interactions between components, services, and external dependencies

**Guidelines:**
- Focus on module interactions
- Use clear naming conventions
- Mock dependencies appropriately
- Verify real service interactions when necessary

**Continuous Integration (CI):**
- Automatic integration tests in CI pipeline
- Halt pipeline on test failure

#### End-to-End Testing

**QA Responsibilities:**
- Design and execute E2E tests for critical user journeys
- Use Playwright with TypeScript
- Automate tests across browsers and devices
- Validate UI interactions and content rendering

**Continuous Integration (CI):**
- Run E2E tests in CI/CD pipeline
- Fail pipeline on critical user journey test failures

#### Adhoc Testing

**QA Responsibilities:**
- Conduct manual exploratory testing
- Perform context-driven testing
- Identify usability, accessibility, and performance issues

**Best Practices:**
- Use session-based testing
- Take detailed notes and screenshots
- Quick communication with developers

### Non-Functional Testing

#### Accessibility Testing
- WCAG 2.1 AA standards compliance
- Consider diverse user scenarios

#### Performance Testing
- Evaluate scalability
- Simulate high-traffic scenarios

#### Security Testing
- Penetration testing
- Vulnerability identification
- Use OWASP ZAP for automation

## Definition of Done (DoD)

For a ticket/feature to be complete:
- Code Peer Review completed
- Feature deployed to Test Environment
- Acceptance Criteria met
- No Critical Defects

## Test Environments

| Environment | Purpose |
|-------------|---------|
| Development | Initial testing |
| Testing | Shared QA environment |
| Staging | Pre-production validation |
| Production | Post-deployment testing |

## Release Process

A structured release management process ensures stable deployments:
- CI/CD workflow via GitHub Actions
- Validation in development environment
- Automated E2E tests in test environment
- Production-readiness criteria include:
    - Passed automated tests
    - No critical defects
    - WCAG 2.1 AA compliance
    - Satisfied Definition of Done
    - Contentful migration check
- Release communications via Slack
- Post go-live smoke test

## Risks and Dependencies

- Risks: Delays in third-party API responses, cache inconsistencies, security vulnerabilities.
- Dependencies: None

## Best Practices

- Collaborative approach
- Automation of repetitive tasks
- Detailed documentation

## Deliverables

- Testing Metrics and Reports
- Test Scripts (Automation)
- Test Execution Logs

## Reference Materials

### Acronyms

- CMS: Content Management System
- CI/CD: Continuous Integration/Continuous Deployment
- API: Application Programming Interface
- WCAG: Web Content Accessibility Guidelines