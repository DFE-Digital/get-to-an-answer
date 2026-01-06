# Development Process

These are the development processes for developers and QAs. These processes are subject to continuous changes so it's advice to frequently review it.

## Developer Development Process

1. Pick up ticket (3 amigos; optional, but necessary for changes to user experience, content, etc.)
2. Define branch.
   ```
   Format:
   
   e.g.
   
   feat/CARE-XXX/focus
   bug/CARE-XXX/focus
   ```
3. Run tests to make sure you know everything is working before you start.
4. Implement the necessary changes
    1. C# .NET changes
    2. Html changes via .cshtml
    3. Css class attachment to content types via .cshtml or ContentRenderer classes
    4. Terraform changes
    5. Etc. 
5. Implement xUnit tests and/or fix broken tests
6. Implement playwright tests and/or fix broken tests
6. Commit changes
    1. Message format: 
      ```
      CARE-XXX {message}
      
      {details}
      ```
7. Create pull request, white and black box tests should be passing.
8. Another developer reviews the pull request, and adds comments if necessary.
9. Another developer approves the pull request.
10. Then the pull request is merged.
11. Move the ticket to the Jira `Test` column and inform QA.
12. Deploy to Development/Test environments.
13. QA manually/automation tests to see if the acceptance criteria were met, then signs it off (if no bugs occur).
14. Deploy to Production environment.
    1. Use `Deploy - Apps` to deploy the code changes to the `production` azure instance.

## QA Test Suite Development Process

1. Define or Pick up ticket in Jira Test column
    1. Subtask to fix related tests (Ideally the responsibility of the Devs).
    2. Define general test fix, load testing, etc. ticket(s).
2. Move Jira ticket the 'In Progress'.
3. Define branch. Format:
   1. task/CARE-XXX/{focus} (for test fixes, modifications, etc.)
   2. test/CARE-XXX/{focus} (for running complex tests, e.g. performance, load, security, etc. testing)
4. Implement the necessary changes
   1. Add test modifications
   2. Potentially, update Contentful development, e2e and/or test environments
5. Commit changes
   1. Message format: "CARE-XXX {message}"
6. Create pull request
7. Move Jira ticket the 'Ready for Review (Tech)'.
8. Pull request approved and merged, by at least a Dev, QA or Technical Architecture
9. Mark ticket as 'Done'.