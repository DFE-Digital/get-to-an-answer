# GTAA Volumetrics

## Introduction

It is important to understand the volumetrics of GTAA to ensure that it can handle the expected traffic without degradation in performance, without incurring unnecessary costs.

This is a new service so there is not historical data. However, as GTAA will replace several existing solutions which do have usage data that can be used to estimate usage, and predictions of potential future new users.

## Existing Usage & Volume Analysis

Most of the existing users (of Riddle or other similar solutions) only have small numbers of users (between '00s and low '000's per month).

The largest existing use of a questionnaire/checker component is on the campaign site for childcare choices – Best start in life. For context childcare content is also DfE's most visited GOV.UK content (by a significant amount). They have 120k people start their checker each month (108k completions).

We have potentially identified a few other teams who may be able to make use of GTAA in the future. Most of these services also have relatively low volumes. However, one possible use case is within 'working together to safeguard children' which is another of the most highly visited content DfE owns. The likelihood of this team adopting GTAA in the short-term is low, but if we were to plan for this level of usage, we would have captured the two most plausible and highest volume use cases in DfE.

### Usage by Site

| Site                              | Users per month                              | Peak Usage                                                       | Interaction Time  |
|-----------------------------------|----------------------------------------------|------------------------------------------------------------------|-------------------|
| Care Leavers                      | 75                                           | Two peaks since May, Max users 90 in 1 week                      | 20 secs           |
| Childcare (Best start in life)    | 120,000                                      | Not specified                                                    | Not specified     |
| Teach in FE (TiFE)                | 60,000 (views of first page)<br>9,250 starts | Two peaks since March. Max views 37k in a week with 4,500 starts | 50 secs           |
| Early Years Qualification Checker | 550                                          | Most users on Monday, lower use on weekends                      | Estimated 50 secs |
| Working to Safeguard Children     | 20,000                                       | Most use M-F 8am-7pm                                             | Estimated 3 mins  |

## Projected Volumetrics – Front-end

The assumptions will be based on 150,000 users per month.

We can make a reasonable assumption that the demand is not evenly spread over the month, with higher demand during working hours. We have used trend data across DfE web content to get the user splits by hours.

- Highest demand hour(s) slot in a weekday = c1.6% of all visits.
- Max users per hour estimation: 150,000 * 1.6% = 2400
- Assuming an average user session lasts 60 secs.
- Concurrent users: 2400 / 60 = 40

During spike times, we expect around x3 this amount (such as when comms have been sent out).

### Front-end Test Scenarios

| Test Type  | Concurrent Users | Requests per Second |
|------------|------------------|---------------------|
| Normal     | 40               | ~10 requests/sec    |
| Spike      | 120              | ~30 requests/sec    |
| Stress     | 160              | ~40 requests/sec    |

## Projected Volumetrics – Admin

This service will mostly be used by the content designers (CD) in digital teams. This gives an upper limit of live users of c 25. Plus, some product, service and interaction designers will choose to have an account so they can view their team's GTAA.

In reality, there are only ever likely to be a couple of teams who would be using GTAA concurrently, with many questionnaires once live only subject to minor periodical updates.

When creating a new GTAA, a CD will be using the admin interface multiple times over a few weeks. There is a lot of iteration in the development phase to align different requirements (UCD, policy asks, devs build etc.). The longest task is likely to be working out and building up the logic.

### Internal User Characteristics
- Content designers will likely work in GTAA for hours at a time
- Multiple sessions over a couple of weeks when setting up a new questionnaire
- Likely scenario: 2-3 users viewing the same questionnaire simultaneously
- Expected concurrent teams: 2-3 teams

### Admin Test Scenarios

| Test Type | Concurrent Users  | Requests per Second |
|-----------|-------------------|---------------------|
| Normal    | 6                 | ~1 request/sec      |
| Spike     | 15                | ~3 requests/sec     |
| Stress    | 25                | ~6 requests/sec     |