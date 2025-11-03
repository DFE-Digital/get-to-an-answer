# Azure Front Door

Azure Front Door Premium with Web Application Firewall (WAF) provides edge security, TLS termination, routing, and request filtering for this service.

## Geoblocking

Access is intended for UK users. Enforce via WAF custom rules:
- Default policy: allow GB traffic; challenge or block non‑GB traffic.
- Minimal exceptions: allow specific workplace preview/embeds where needed (see below).

### Workplace tools (previews/embeds)

Permit selected user agents that perform link previews or embeds for internal collaboration:
```
js
["slack", "embedly", "figma", "skype"]
```
## Threat protection

### Managed rules
- Enable OWASP managed rules (latest available).
- Configure anomaly scoring/blocking as appropriate for environment.

### Rate limiting and high‑cost route protection
For routes that can trigger heavier processing (e.g., file generation, translations, or similar):
- Apply WAF rules to limit/bypass only for trusted user agents and rate‑limit or block others.
- Prefer not linking such routes publicly to reduce incidental discovery.

### Bot and crawler posture
- Default: do not explicitly allow generic crawlers.
- Maintain a small, explicit allowlist only where there is a clear operational need.

## Operations and IaC

- Front Door, WAF policies, routes, origins, and custom rules are defined in Terraform under the infrastructure codebase and applied via GitHub Actions.
- Use environment‑specific parameters per workspace/environment.
- Monitor:
  - Requests by action (allow/block/challenge)
  - Country distribution and geoblock effectiveness
  - Rule matches and false positives
  - Latency at edge and origin health

## Change management

- Make WAF and Front Door changes in code; avoid portal drift.
- Test rule updates in non‑production, validate with sample traffic, then promote.
- Review exceptions periodically; keep the allowlist minimal.
