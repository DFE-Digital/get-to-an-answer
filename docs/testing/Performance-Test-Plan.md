# Performance Test Plan GTAA Shared Service

## Objective

To validate that GTAA service can handle expected and peak user loads without performance degradation, while ensuring the underlying Azure infrastructure operates efficiently and remains cost-effective.

## Scope

The scope includes validation of:
- API endpoints
- Front-end user journeys
- Admin interface

Performance testing will ensure acceptable performance under expected and peak load conditions.

## Assumptions

- No historical traffic or performance data is currently available
- Load profiles and volumes derived from:
    - RIDDLE existing data
    - Comparable data from teams like:
        - Care Leavers
        - Childcare
        - Teach in FE (TiFE)
        - Early years qualification checker
- Peak usage occurs during UK business hours

> **Note:** Detailed calculations and supporting rationale are documented in the 'GTAA Volumetrics' document.

## Throughput Calculation Methodology

### Steps to Calculate Throughput for Concurrent Users

1. **Average Journey Time**
    - Get average journey time for each thread group in seconds
    - Sum Gaussian time delays or calculate time difference between user start and end journey in view results tree

2. **Concurrent User Allocation**
    - Distribute users across thread groups
    - Example: TG1(5) + TG1(5) + TG1(5) + TG1(5) + TG1(5) = 25

3. **Journey Executions per Second across thread groups**
    - `Exec/s = Users / Average Journey Time (Seconds)`

4. **Requests per Journey**
    - Count HTTP Requests per journey for each thread group

5. **Requests per Second per Thread Group**
    - `RPS per TG = (Users / Avg Journey Time) * Requests per Journey`

6. **Total Estimated Throughput**
    - Sum of all thread group RPS values

## Projected Usage

### APIs (7 Thread Groups)
- **Normal**: 6 concurrent users (~0.5 request/sec)
- **Spike**: 15 concurrent users (~1 request/sec)
- **Stress**: 25 concurrent users (~2 requests/sec)

### Front-end (6 Thread Groups)
- **Normal**: 40 concurrent users (~10 requests/sec)
- **Spike**: 120 concurrent users (~30 requests/sec)
- **Stress**: 160 concurrent users (~40 requests/sec)

### Admin (5 Thread Groups)
- **Normal**: 6 concurrent users (~1 request/sec)
- **Spike**: 15 concurrent users (~3 request/sec)
- **Stress**: 25 concurrent users (~6 requests/sec)

## User Journeys (Thread Groups)

### APIs
- 7 distinct end-to-end administrative workflows
- Simulate real-world admin activity
- Execute sequence of POST, GET, and DELETE requests

### Front-end
- 6 distinct end-to-end user journeys
- Navigate through questions
- Submit various answer selections
- Reach results page

### Admin
- 5 distinct admin UI user journeys
- Cover workflows such as:
    - Creating and managing questionnaires
    - Managing questions
    - Publishing/unpublishing questionnaires
    - Submitting and deleting answers
    - Performing clean-up actions

## Test Scenarios

### API Test Scenarios

#### Scenario 1: Normal Load
- **Users**: 6 users across 7 thread groups
- **Ramp-up**: 300 seconds per thread group
- **Loop count**: Based on 30-minute run duration
- **Goal**: Validate performance and stability under expected load

#### Scenario 2: Spike Load
- **Users**: 15 users across 7 thread groups
- **Ramp-up**: 1 second per thread group
- **Loop count**: Based on 5-minute run duration
- **Goal**: Observe system behavior during traffic surge

#### Scenario 3: Stress Load
- **Users**: 25 users across 7 thread groups
- **Ramp-up**: 300 seconds per thread group
- **Loop count**: Based on 30-minute run duration
- **Goal**: Identify system limits and monitor infrastructure metrics

### Front-end Test Scenarios

#### Scenario 1: Normal Load
- **Total Users**: 40 users
- **Thread Groups**: 6 thread groups
- **Users per Thread Group**: ~7 users
- **Ramp-up Period**: 300 seconds per thread group
- **Test Duration**: 30 minutes
- **Loop Count**: Calculated based on 30-minute duration
- **Goal**: Validate system performance and stability under expected, realistic load conditions

#### Scenario 2: Spike Load
- **Total Users**: 120 users
- **Thread Groups**: 6 thread groups
- **Users per Thread Group**: 20 users
- **Ramp-up Period**: 1 second per thread group
- **Test Duration**: 5 minutes
- **Loop Count**: Calculated based on 5-minute duration
- **Goal**: Assess system resilience during sudden traffic surge

#### Scenario 3: Stress Load
- **Total Users**: 160 users
- **Thread Groups**: 6 thread groups
- **Users per Thread Group**: ~27 users
- **Ramp-up Period**: 300 seconds per thread group
- **Test Duration**: 30 minutes
- **Loop Count**: Calculated based on 30-minute duration
- **Goal**: Determine system limits and infrastructure performance

### Admin Test Scenarios

#### Scenario 1: Normal Load
- **Total Users**: 6 users
- **Thread Groups**: 5 thread groups
- **Users per Thread Group**: 1-2 users
- **Ramp-up Period**: 300 seconds per thread group
- **Test Duration**: 30 minutes
- **Loop Count**: Calculated based on 30-minute duration
- **Goal**: Validate admin interface performance under standard usage

#### Scenario 2: Spike Load
- **Total Users**: 15 users
- **Thread Groups**: 5 thread groups
- **Users per Thread Group**: 3 users
- **Ramp-up Period**: 1 second per thread group
- **Test Duration**: 5 minutes
- **Loop Count**: Calculated based on 5-minute duration
- **Goal**: Assess admin interface resilience during sudden administrative activity

#### Scenario 3: Stress Load
- **Total Users**: 25 users
- **Thread Groups**: 5 thread groups
- **Users per Thread Group**: 5 users
- **Ramp-up Period**: 300 seconds per thread group
- **Test Duration**: 30 minutes
- **Loop Count**: Calculated based on 30-minute duration
- **Goal**: Identify admin interface and backend system limitations

## Configuration Details

- **HTTP Cookie Manager**:
    - Clear cookies each iteration
- **HTTP Cache Manager**:
    - Clear cache each iteration
- **Timers**:
    - Constant or Gaussian timers to simulate user think time
- **Assertions**:
    - HTTP status = 200
- **Listeners**:
    - Aggregate Report
    - Summary Report
    - View Results Tree

## Success Criteria

- Error rate < 1%
- Average response time < 2 seconds
- 95th percentile < 3 seconds
- Consistent throughput without errors
- No major infrastructure bottlenecks:
    - CPU
    - Memory
    - API gateway

## Tools & Monitoring

- JMeter for load generation
- Azure client and server-side metrics

## Risks & Mitigation

### Risk: Lack of Historic Data
**Mitigation**:
- Used conservative load assumptions
- Incorporated historical data from Riddle and other teams

## Next Steps

1. Prepare .jmx file with 7 thread groups
2. Configure:
    - Ramp-up
    - User load
    - Loops per scenario
3. Schedule tests:
    - Normal load
    - Spike load
    - Stress load
4. Review metrics and performance dashboards