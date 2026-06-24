# 🛡️ Cybersecurity Awareness Chatbot — Part 3 (POE)

**Module:** Programming 2A (PROG6221)
**Assessment:** Portfolio of Evidence — Part 3

---

## 📋 Overview

Part 3 completes the Cybersecurity Awareness Chatbot. The Part 1 console foundation and Part 2 WinForms GUI are both preserved **completely unchanged**, with three new feature areas layered on top in a single, cohesive application:

| Tab | What it does |
|---|---|
| 💬 **Chat** | Everything from Parts 1 & 2 — 30+ cybersecurity topics, random tips, follow-up conversation flow, memory & recall, and sentiment detection — plus a new NLP layer that lets you manage tasks, reminders, the quiz, and the activity log straight from natural language. |
| 📋 **Tasks** | A MySQL-backed task assistant. Add a task with a title, description, and optional reminder date/time; view every task in a list; mark complete or delete — full CRUD, persisted between runs. |
| 🎮 **Quiz** | A 12-question cybersecurity mini-game mixing multiple-choice and true/false questions, one at a time, with immediate feedback and a final score summary. |
| 📜 **Activity Log** | A timestamped history of every significant action — tasks added/completed/deleted, reminders set, quiz started/completed, and every NLP action — with a "last 8" quick view and full history on demand. |

---

## 🧠 NLP Simulation

The Chat tab understands varied phrasings of the same request using `string.Contains()`, basic string manipulation, and regular expressions (see `Features/NlpParser.cs`):

| You type | What happens |
|---|---|
| `Add a task to enable 2FA` | Creates a task titled "Enable 2FA" |
| `Remind me to update my password tomorrow` | Creates a task with a reminder set for tomorrow at 09:00 |
| `Show my tasks` / `what tasks do I have` | Lists every task |
| `Complete task 2` / `mark task 1 as done` | Marks that task complete |
| `Delete task 3` | Removes that task |
| `Start the quiz` / `test my knowledge` | Switches to the Quiz tab and starts a new round |
| `Show activity log` / `what have you done for me?` | Shows your last 8 actions |

---

## 🗂️ Project Structure

```
CybersecurityChatbot/
├── Program.cs
├── CybersecurityChatbot.csproj         # net9.0-windows, WinForms, MySqlConnector
├── README.md
├── Assets/
│   └── greeting.wav                    # Voice greeting WAV (Part 1, unchanged)
├── ChatbotCore/                        # ── Parts 1 & 2 — UNCHANGED ──
│   ├── ChatbotEngine.cs
│   ├── ConsoleUI.cs
│   ├── ResponseEngine.cs               # 30+ keyword responses + random tips
│   ├── UserSession.cs
│   ├── VoiceGreeting.cs
│   ├── InputValidator.cs
│   ├── SentimentDetector.cs
│   ├── ConversationMemory.cs
│   └── ChatDelegates.cs
├── Features/                           # ── Part 3 — NEW ──
│   ├── CyberTask.cs
│   ├── DatabaseService.cs              # MySQL CRUD with error handling
│   ├── ActivityLog.cs
│   ├── QuizEngine.cs                   # 12-question quiz with shuffle + scoring
│   └── NlpParser.cs
├── GUI/
│   └── MainForm.cs                     # 4-tab WinForms window
└── .github/
    └── workflows/
        └── dotnet-ci.yml               # GitHub Actions CI
```

---

## 🗄️ Database — MySQL

Connects to a real **MySQL** server via [`MySqlConnector`](https://mysqlconnector.net/). On first run, `DatabaseService` automatically creates the `cybersecurity_chatbot` database and `Tasks` table — no manual schema setup needed.

### Setup (pick one)

- **MySQL Community Server** — [download here](https://dev.mysql.com/downloads/mysql/)
- **XAMPP / WAMP** — bundles MySQL with a start/stop control panel; default root has no password
- **Docker:**
  ```powershell
  docker run --name mysql-cybertasks -e MYSQL_ROOT_PASSWORD=yourpassword -p 3306:3306 -d mysql:8
  ```

### Connection config

Defaults: `localhost:3306`, user `root`, no password, database `cybersecurity_chatbot`. Override via environment variables:

| Variable | Default |
|---|---|
| `CYBERTASKS_DB_SERVER` | `localhost` |
| `CYBERTASKS_DB_PORT` | `3306` |
| `CYBERTASKS_DB_USER` | `root` |
| `CYBERTASKS_DB_PASSWORD` | *(empty)* |
| `CYBERTASKS_DB_NAME` | `cybersecurity_chatbot` |

---

## 🚀 Getting Started

**Prerequisites:** Windows 10+, [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9), a running MySQL server

```powershell
# If using Docker:
docker start mysql-cybertasks
$env:CYBERTASKS_DB_PASSWORD = "yourpassword"

dotnet restore
dotnet run
```

The database and table are created automatically on first run.

---

## ✅ CI — GitHub Actions

`.github/workflows/dotnet-ci.yml` restores and builds on a `windows-latest` runner on every push.

> 📸 *[Add screenshot of green CI run here]*

---

## 🏷️ Release Tags

| Tag | Description |
|---|---|
| `v1.0` | Part 1 — Console chatbot |
| `v2.0` | Part 2 — WinForms GUI |
| `v3.0` | Part 3 — Tasks, Quiz, NLP, Activity Log, MySQL |

---

## 🔗 Links

- **Part 1 repo:** https://github.com/Hakiue/CybersecurityChatbot
- **Part 1 video:** https://youtu.be/kDrrleHz-GU
- **Part 2 repo:** https://github.com/Hakiue/CybersecurityChatbot-Part2
- **Part 2 video:** https://youtu.be/6C6pwtOeXOc
- **Part 3 repo:** https://github.com/Hakiue/CybersecurityChatbot-Part3
- **Part 3 video:** *[Add after recording]*

---

## 🎥 Video Presentation

> 🎬 *[Add YouTube unlisted link after recording]*

8–10 minutes, own voice, covering:
1. Parts 1 & 2 still working inside the Chat tab
2. Tasks tab — add, complete, delete, set reminder
3. Quiz tab — answer questions, reach final score
4. Activity Log tab
5. NLP — typing commands directly into Chat
6. Code walkthrough: `NlpParser.cs`, `DatabaseService.cs`, `MainForm.cs`

---

## ⚖️ Design Notes

- **Single-turn reminders** — the brief's example shows a two-turn reminder flow; this project handles it in one turn (title + reminder together), with a dedicated ⏰ Set Reminder button for modifying an existing task afterwards.
- **NLP is intentionally simple** — uses `string.Contains()` and `Regex` as instructed, not a real language model.
- **MySqlConnector** — MIT-licensed ADO.NET driver, functionally interchangeable with `MySql.Data` for the CRUD operations used here.

---

## 📚 References

Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review. *The African Journal of Information and Communication*, 28(28). doi: https://doi.org/10.23962/10539/32213.
