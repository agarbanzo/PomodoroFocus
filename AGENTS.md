# AGENTS.md - PomodoroFocus

## Project Overview
- **Stack**: .NET 10.0 + Blazor WebAssembly
- **Architecture**: Clean Architecture (Domain → Application → Infrastructure → Web)
- **Solution**: `PomodoroFocus/PomodoroFocus.slnx`
- **Dev Server Port**: 5294

## Projects
| Project | Purpose |
|---------|---------|
| `PomodoroFocus.Domain` | Entities, Enums, Value Objects |
| `PomodoroFocus.Application` | Services, Interfaces |
| `PomodoroFocus.Infrastructure` | Infrastructure layer (empty) |
| `PomodoroFocus.Web` | Blazor WebAssembly UI |
| `PomodoroFocus.Tests` | Unit tests (NUnit) |

## Key Commands

```bash
# Build entire solution
dotnet build

# Run the Blazor app
dotnet run --project PomodoroFocus/PomodoroFocus.Web

# Run tests
dotnet test
```

## Entry Points
- **Web App**: `PomodoroFocus/PomodoroFocus.Web/Program.cs`
- **Core Service**: `PomodoroFocus/PomodoroFocus.Application/Services/PomodoroService.cs`
- **UI Component**: `PomodoroFocus/PomodoroFocus.Web/Pages/Pomodoro.razor`

## Testing
- Framework: NUnit 4.3.2
- Test location: `PomodoroFocus/PomodoroFocus.Tests/`

## App Behavior
- Timer uses `System.Timers.Timer` with 1-second intervals
- Auto-transitions: Pomodoro → Short/Long Break → Pomodoro
- Long break triggers after every 4 Pomodoros
- Timer state configurable only when idle (Ready/Completed)

## Configuration Options
| Setting | Default |
|---------|--------|
| Pomodoro Duration | 25 min |
| Short Break | 5 min |
| Long Break | 15 min |
| Pomodoros before Long Break | 4 |

## Git Configuration
- **Local User**: agarbanzo
- **Email**: andrey.garbanzo@gmail.com
- Configured locally for this project (not global)

### Git Workflow
- **Commits**: Agent can create commits locally when requested
- **Push**: User performs push manually (not done by agent)

## Subagent Orchestration

**ALWAYS** route tasks to the appropriate subagent based on what needs to be done:

- architect: Analyzes codebase and produces design plans (read-only)
- backend-developer: Handles domain, application, and infrastructure layers
- frontend-developer: Handles Blazor components, pages, and UI/UX
- qa-engineer: Writes and runs unit/integration tests, reports coverage and failures

| Task Type | Use Subagent |
|-----------|--------------|
| Architecture analysis, design planning | **Architect** |
| Entities, Enums, ValueObjects | **Backend Developer** |
| Services, Interfaces, Business Logic | **Backend Developer** |
| Unit Tests (Domain/Application) | **Backend Developer** |
| Blazor Components, Pages, Layouts | **Frontend Developer** |
| UI/UX, Modals, CSS | **Frontend Developer** |
| QA Testing, Test Coverage, Test Failures | **QA Engineer** |
| Mixed (Backend + Frontend) | Architect first → then invoke both subagents |

**Architect must be invoked first for any non-trivial change.** It researches the codebase and produces a task plan. Once the plan is approved, route tasks to backend-developer and/or frontend-developer as specified.

### Architect
- File: `.opencode/agents/architect.md`
- Handles: Codebase research, impact analysis, execution planning
- Constraints: **Read-only** — never creates or edits files

### Backend Developer Skill
- File: `.opencode/agents/backend-developer.md`
- Handles: Domain layer, Application layer, Infrastructure
- Commands: `dotnet build`, `dotnet test`

### Frontend Developer Skill
- File: `.opencode/agents/frontend-developer.md`
- Handles: Web layer (Blazor components, pages, wwwroot)
- Commands: `dotnet build`, `dotnet run --project PomodoroFocus.Web`

### QA Engineer Skill
- File: `.opencode/agents/qa-engineer.md`
- Handles: Unit tests, integration tests, test coverage, failure analysis
- Skills: NUnit 4.x, Moq, WebApplicationFactory, Playwright (suggest only)
- Commands: `dotnet test`
- Constraint: **Read-only on production code** — only tests and test config files

## Task Routing Examples
- "Add new timer mode" → Backend (modify PomodoroSession, Service)
- "Improve the timer display" → Frontend (modify TimerDisplay.razor)
- "Add sound notification" → Frontend (modify TimerDisplay.razor, audio file)
- "Add persistence" → Backend (Infrastructure) + Frontend (UI integration)
- "Add new setting" → Backend (TimeConfiguration) + Frontend (SettingsModal)