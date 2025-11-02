# Release Process

This document describes how we promote changes to Test and Production using GitHub Actions and Terraform.

---

## Automated deployments to Test

On merge to `main`:
1. Version tagging: the workflow locates the latest semver tag (e.g., `v1.2.3`), increments it (e.g., `v1.2.4`), and tags the merge commit.
2. Application deploys: API, Frontend, and Admin deploy to the Test environment via their respective GitHub workflows.
3. Validation: automated tests (unit, integration, accessibility, e2e, security scan) run via dedicated workflows.

Outcome: Test reflects the latest `main` plus a new tag.

---

## Infrastructure

Infrastructure changes are managed in `src/Infrastructure/terraform` and applied by the GitHub workflow dedicated to Azure infrastructure.  
Guidelines:
- Keep Terraform changes small and isolated; open a PR with a clear plan summary.
- Merges to `main` will plan/apply to the Test subscription/environment as configured by the workflow.
- Promote the same Terraform state/config to Production via the Production run of the infrastructure workflow (see below).

---

## Deployments to environments

Production deployments are manual after successful validation in Test.

### 1) GitHub Release (primary)

1. Choose a tag to release (e.g., `v1.2.4`).
2. In GitHub, go to Releases → Draft a new release.
3. Select the tag, add release notes, and Publish.
4. Publishing triggers Production deployment workflows for API, Frontend, and Admin.

### 2) Manual workflow dispatch (alternative)

Use for hotfixes or exceptional cases.

1. Open GitHub → Actions.
2. Select the relevant deploy workflow (API, Frontend, Admin, or Infrastructure).
3. Click “Run workflow”.
4. Choose a branch or specific tag, select the target environment (Test or Production), and any optional flags (e.g., clear cache where available).
5. Run workflow to start deployment.

---

## Order of operations for Production

1. Infrastructure (if required): run the Infrastructure workflow for Production to apply Terraform.
2. Applications: run or allow the release-triggered workflows to deploy API, Frontend, and Admin.
3. Post‑deploy checks: confirm health, logs, and monitors; verify smoke tests.

---

## Rollback

- Applications: re-run the deploy workflow with a previous known-good tag to the affected environment.
- Infrastructure: revert the Terraform change (git revert or follow-up PR) and re-apply via the Infrastructure workflow.

---

## Notes

- Kept environment configuration in code; avoided manual portal changes.
- We have alerts plus we manually monitor deployments via GitHub Actions logs and environment protection rules.