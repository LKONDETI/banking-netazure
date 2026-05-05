---
name: new-endpoint
description: Full pipeline skill for building a new API endpoint. Walks through all stages in order: plan → architect → implement → code review → security review → tests → run tests. Invoke manually when starting a new endpoint or feature.
disable-model-invocation: true
---

# New Endpoint Pipeline

You are orchestrating the full development pipeline for a new API endpoint.

Ask the user: "What endpoint or feature do you want to build?" if not already specified.

Then walk through each stage IN ORDER, waiting for the user to approve before moving to the next:

---

## Stage 1: Task Planning
Use the `task-planner` agent to break down the feature into ordered tasks.
Present the task list to the user and ask: "Does this task breakdown look right? Shall we move to architecture?"

## Stage 2: Architecture Design
Use the `architect` agent to design the folder structure, interfaces, and patterns.
Present the design to the user and ask: "Does this architecture look right? Shall we start implementing?"

## Stage 3: Implementation
Use the `coding-expert` agent to implement the code following the architecture.
After implementation, confirm: "Code written and builds successfully. Ready for review?"

## Stage 4: Code Quality Review
Use the `code-reviewer` agent to review the implementation for quality issues.
Present findings. If Critical or High issues found, loop back to Stage 3 to fix them first.

## Stage 5: Security Review
Use the `security-reviewer` agent to check for security vulnerabilities.
Present findings. If Critical issues found, loop back to Stage 3 to fix them first.

## Stage 6: Write Tests
Use the `testing-expert` agent to write unit and integration tests.
After writing tests, run them and confirm all pass.

## Stage 7: Final Verification
Run `dotnet build` and `dotnet test` to confirm everything is green.
Report: total tests, passing, failing.

## Stage 8: Task Summary
Use the `task-summarizer` agent to produce a concise end-of-session summary.
Include: what was built, architecture decisions, task recap table, test coverage, known gaps, and quick stats.

---

At the end, present the full summary from the `task-summarizer` agent.
