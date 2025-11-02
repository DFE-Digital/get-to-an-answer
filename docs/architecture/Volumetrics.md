# Volumetrics

## Introduction
This document estimates expected load for the Get-To-An-Answer service to inform capacity planning, performance testing, and cost control in Azure.

Assumptions are based on similar government services. We will refine these figures after go‑live by observing Azure metrics and application telemetry.

## Projected Volumetrics

- Monthly active users: ~1,000
- Daily average users: ~33
- Usage pattern: UK business hours, with higher activity during work hours
- Peak concurrency driver: short sessions and intermediary usage

Assumptions:
- Average session length: ~5 minutes
- Peak active users per hour: ~10
- Concurrency estimate: 10 users × 5 minutes ≈ ~30 concurrent requests at peak
- Spike factor: up to 3× during comms/announcements

| Test Type | Concurrent Users | Requests per second |
|-----------|------------------|---------------------|
| Normal    | 30               | ~3 req/s            |
| Spike     | 90               | ~9 req/s            |
| Stress    | 150              | ~15 req/s           |

Notes:
- RPS estimates assume ~1 request every 10 seconds per active user averaged across the cohort (mix of page views and API calls).
- Back‑end work is predominantly read‑heavy for public endpoints and mixed read/write for admin authoring.

## Performance Test Targets
- P95 API latency (public read endpoints): < 200 ms
- P95 API latency (admin authoring): < 400 ms
- Error rate: < 0.5% at Normal, < 1% at Spike
- Throughput sustain: ≥ 15 req/s for 10 minutes without degradation (Stress)

## Capacity and Scaling Considerations
- Ensure autoscaling rules accommodate the Spike profile (3× headroom).
- Use CDN/caching for static assets and content where applicable.
- Optimize database read paths and ensure indexes for primary queries.
- Monitor:
    - CPU, memory, and thread pool saturation
    - DB DTUs/vCores, query latency, and connection pool usage
    - 429/5xx rates and dependency call failures

## Cost Awareness
- Right‑size compute for Normal load; rely on autoscale for peaks.
- Use staging for load tests to avoid impacting production users.
- Review Azure cost reports monthly; adjust scale thresholds based on observed patterns.

## Validation Plan
- Run baseline load test at Normal profile before major releases.
- Run Spike tests quarterly or ahead of comms campaigns.
- Execute Stress tests during non‑business hours in a non‑prod environment to validate headroom and recovery.