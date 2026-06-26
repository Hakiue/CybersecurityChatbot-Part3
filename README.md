# 🛡️ Cybersecurity Awareness Chatbot — Part 3 (Combined POE)

**Student:** Zaakir Shaibu
**Student Number:** ST10443440
**Module:** Programming 2A (PROG6221)
**Assessment:** Portfolio of Evidence — Part 3
**Institution:** The Independent Institute of Education (IIE) — Rosebank College

---

## 📋 Overview

Part 3 brings the chatbot's journey full circle. The Part 1 console foundation and the Part 2 WinForms GUI are both preserved **completely unchanged**, and three brand-new feature areas are layered on top in a single, cohesive application:

| Tab | What it does |
|---|---|
| 💬 **Chat** | Everything from Parts 1 & 2 — 30+ cybersecurity topics, random tips, follow-up conversation flow, memory & recall, and sentiment detection — plus a new lightweight NLP layer that lets you manage tasks, reminders, the quiz, and the activity log straight from natural language. |
| 📋 **Tasks** | A MySQL-backed task assistant. Add a task with an optional title, description, and reminder date/time; view every task in a sortable list; mark complete or delete — full CRUD, persisted between runs. |
| 🎮 **Quiz** | A 12-question cybersecurity mini-game mixing multiple-choice and true/false questions, one at a time, with immediate feedback, an explanation for every answer, and a final score summary. |
| 📜 **Activity Log** | A timestamped history of everything the assistant has done — tasks added/completed/deleted, reminders set, quiz started/completed, and every NLP action — with a "last 8" view in chat and a full history on demand. |

---

## 🧠 The NLP Simulation

Rather than only reacting to button clicks, the Chat tab understands varied phrasings of the same request using `string.Contains()`, basic string manipulation, and a handful of regular expressions (see `Features/NlpParser.cs`). A few things you can type:

| You type | What happens |
|---|---|
| `Add a task to enable 2FA` | Creates a task titled "Enable 2FA" |
| `Remind me to update my password tomorrow` | Creates a task with a reminder set for tomorrow at 09:00 |
| `Show my tasks` / `what tasks do I have` | Lists every open and completed task |
| `Complete task 2` / `mark task 1 as done` | Marks that task complete |
| `Delete task 3` | Removes that task |
| `Start the quiz` / `test my knowledge` | Switches to the Quiz tab and starts a new round |
| `Show activity log` / `what have you done for me?` | Shows your last 8 actions, with a "show full log" option |

Cybersecurity-topic keywords such as `password` and `phishing` continue to be handled by the existing Part 1/2 `ResponseEngine`, which already recognises 30+ topics — the new `NlpParser` focuses purely on the Part 3 task/quiz/log actions, so nothing from Parts 1–2 had to be duplicated or rewritten.

---

## 🗂️ Project Structure

```
CybersecurityChatbot/
├── Program.cs                          # Entry point — launches WinForms
├── CybersecurityChatbot.csproj         # net9.0-windows, WinForms, + MySqlConnector
├── README.md
├── Assets/
│   └── greeting.wav                    # Voice greeting WAV (from Part 1, unchanged)
├── ChatbotCore/                        # ── Parts 1 & 2 — UNCHANGED ──
│   ├── ChatbotEngine.cs                # Part 1 console engine
│   ├── ConsoleUI.cs                    # Part 1 console UI
│   ├── ResponseEngine.cs               # 30+ keyword responses + random tips
│   ├── UserSession.cs                  # Session state for the console app
│   ├── VoiceGreeting.cs                # WAV playback via System.Media
│   ├── InputValidator.cs               # Input validation
│   ├── SentimentDetector.cs            # Detects worried/frustrated/curious/happy
│   ├── ConversationMemory.cs           # Stores name, last topic, favourite topic
│   └── ChatDelegates.cs                # MessageProcessor, BotResponseHandler, ActivityLogger
├── Features/                            # ── Part 3 — NEW ──
│   ├── CyberTask.cs                    # Task model (Id, Title, Description, ReminderAt, ...)
│   ├── DatabaseService.cs              # MySQL CRUD — AddTask, GetAllTasks, CompleteTask, DeleteTask, SetReminder
│   ├── ActivityLog.cs                  # LogEntry + Add/GetRecent/GetAll/FormatForChat
│   ├── QuizEngine.cs                   # 12-question quiz engine with shuffle, scoring, explanations
│   └── NlpParser.cs                    # Intent detection for tasks/quiz/reminders/log
├── GUI/
│   └── MainForm.cs                     # 4-tab WinForms window: Chat, Tasks, Quiz, Activity Log
└── .github/
    └── workflows/
        └── dotnet-ci.yml               # GitHub Actions CI
```

---

## 🗄️ Database — MySQL

This project connects to a real **MySQL** server via the [`MySqlConnector`](https://mysqlconnector.net/) NuGet package, as required by the brief. On first run, `DatabaseService` automatically:

1. Connects to the MySQL server
2. Runs `CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot`
3. Runs `CREATE TABLE IF NOT EXISTS Tasks (...)` inside that database

So once a MySQL server is reachable, there's no manual schema setup — just install/start MySQL and run the app.

### Setting up a local MySQL server (pick one)

- **MySQL Community Server** — [download here](https://dev.mysql.com/downloads/mysql/), install with default settings, remember the root password you set.
- **XAMPP / WAMP** — both bundle MySQL (MariaDB-compatible) and a control panel to start/stop it; the default root user has no password.
- **Docker** (fastest if you already have Docker Desktop):
  ```bash
  docker run --name mysql-cybertasks -e MYSQL_ROOT_PASSWORD=yourpassword -p 3306:3306 -d mysql:8
  ```

### Configuring the connection

Defaults (in `DatabaseService.cs`) are `Server=localhost`, `Port=3306`, `User=root`, `Password=""`, `Database=cybersecurity_chatbot`. Override any of them **without touching code** via environment variables:

| Variable | Default |
|---|---|
| `CYBERTASKS_DB_SERVER` | `localhost` |
| `CYBERTASKS_DB_PORT` | `3306` |
| `CYBERTASKS_DB_USER` | `root` |
| `CYBERTASKS_DB_PASSWORD` | *(empty)* |
| `CYBERTASKS_DB_NAME` | `cybersecurity_chatbot` |

For example, on Windows PowerShell, before running the app:
```powershell
$env:CYBERTASKS_DB_PASSWORD = "yourpassword"
dotnet run
```

If the app can't connect, it shows a clear error message explaining what to check, instead of crashing — see [Design Decisions](#-design-decisions--known-limitations).

---

## 🚀 Getting Started

### Prerequisites
- Windows 10 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9)
- A running MySQL server (see above) reachable at `localhost:3306` — or set the `CYBERTASKS_DB_*` environment variables to point elsewhere

### Run

```bash
dotnet restore
dotnet run
```

The database and table are created automatically on first run. Tasks persist between runs.

---

## ✨ Features Demonstrated

### Carried over from Parts 1 & 2 (unchanged)
- Keyword recognition across 30+ cybersecurity topics
- Random tips for phishing, passwords, scams, and safe browsing
- Conversation flow — `tell me more`, `give me examples`, `another tip`
- Memory and recall of the user's name and favourite topic
- Sentiment detection — worried, frustrated, curious, happy
- Delegates — `MessageProcessor`, `BotResponseHandler`, `ActivityLogger`

### New in Part 3
- **Task Assistant** — add, view, complete, and delete tasks with optional reminders, backed by MySQL; reminders can also be added or changed on an existing task via the **⏰ Set Reminder** button
- **Quiz** — 12 shuffled multiple-choice/true-false questions, instant feedback, final score message
- **NLP Simulation** — natural-language task/quiz/reminder/log commands from the Chat tab
- **Activity Log** — full audit trail of every significant action, with a "last 8" and "show full log" view
- **Error handling** — every database operation is wrapped in try/catch and surfaces a friendly message instead of crashing; a failed connection on startup shows a clear explanation rather than an unhandled exception
- **Overdue reminders** — tasks with a reminder in the past are highlighted in red in the Tasks tab, and a one-time notice is logged on startup if any exist

---

## ⚖️ Design Decisions & Known Limitations

In the interest of transparency for marking:

- **Single-turn reminders, not two-turn.** The brief's example shows the bot asking "Would you like a reminder?" as a separate follow-up turn. This project sets the reminder in the *same* turn instead — e.g. "remind me to update my password tomorrow" creates the task and the reminder together — which needs fewer round-trips for the same outcome. The two-turn flow isn't implemented; the **⏰ Set Reminder** button covers adding/changing a reminder afterwards instead.
- **NLP is intentionally simple.** As instructed, intent detection uses `string.Contains()` and `Regex` rather than a real language model, so very unusual phrasings (or typos) may fall through to "Unknown" and continue to the normal chat pipeline rather than being misread as a task/quiz/log command.
- **MySqlConnector, not MySql.Data.** Both are real ADO.NET drivers that connect to an actual MySQL server. `MySqlConnector` was chosen for this project as it's MIT-licensed, actively maintained, and has a smaller dependency footprint — functionally interchangeable with Oracle's official `MySql.Data` connector for the CRUD operations used here.
- **A MySQL server must be running.** Unlike an embedded file database, this requires MySQL to be installed and started before running the app (see the Database section above for setup options). This is the trade-off of matching the brief's named technology exactly.

---

## ✅ CI/CD — GitHub Actions

This project includes `.github/workflows/dotnet-ci.yml`, which restores and builds the project on a `windows-latest` runner — including downloading the `MySqlConnector` package — on every push. After pushing the project, add a screenshot of the green CI run here.

> 📸 *Add your green CI screenshot here*

## 🏷️ Releases and Tags

Part 3 requires a minimum of three release tags. Suggested tags for this repository:

- `v1.0` — Part 1 console foundation
- `v2.0` — Part 2 WinForms GUI conversion
- `v3.0` — Part 3: Tasks, Quiz, NLP, and Activity Log

---

## 🔗 Links

- **Part 1 (console):** https://github.com/Hakiue/CybersecurityChatbot
- **Part 1 video:** https://youtu.be/kDrrleHz-GU
- **Part 2 (WinForms GUI):** https://github.com/Hakiue/CybersecurityChatbot-Part2
- **Part 2 video:** https://youtu.be/6C6pwtOeXOc
- **Part 3 (this repo):** https://github.com/Hakiue/CybersecurityChatbot-Part3
- **Part 3 video:** https://youtu.be/Nm7pjBscl2M

---

## 🎥 Video Presentation

> 🎬 [YouTube video](https://youtu.be/Nm7pjBscl2M)

The video covers:
1. A quick recap of Parts 1 & 2 still working inside the Chat tab
2. The Tasks tab — adding a task with a reminder, completing it, deleting it
3. The Quiz tab — answering a few questions and reaching the final score
4. The Activity Log tab
5. The NLP layer — typing `add a task to enable 2FA` and `start the quiz` directly into the Chat tab
6. A look at the code: `NlpParser.cs`, `DatabaseService.cs`, and how `MainForm.cs` ties all three parts together

---

## 📚 References

Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review. *The African Journal of Information and Communication*, 28(28). doi: https://doi.org/10.23962/10539/32213.
