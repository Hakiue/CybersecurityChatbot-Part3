using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CybersecurityChatbot.Features
{
    /// <summary>Category used to colour-code log entries in the Activity Log tab.</summary>
    public enum LogCategory { General, Task, Reminder, Quiz, Error }

    /// <summary>
    /// A single timestamped activity record.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public LogCategory Category { get; set; } = LogCategory.General;

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Description}";
    }

    /// <summary>
    /// Records and retrieves a timestamped history of every significant action
    /// performed across the chat, task manager, and quiz features — tasks
    /// added/completed/deleted, reminders set, quiz started/completed, and
    /// NLP-detected actions. Satisfies the Part 3 Activity Log requirement.
    /// </summary>
    public class ActivityLog
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();
        private const int DefaultRecentCount = 8;

        /// <summary>True if there are more entries than the default recent window shows.</summary>
        public bool HasMore => _entries.Count > DefaultRecentCount;

        /// <summary>Total number of actions logged this session.</summary>
        public int TotalCount => _entries.Count;

        /// <summary>Records a new timestamped activity with an optional category for colour-coding.</summary>
        public void Add(string description, LogCategory category = LogCategory.General)
        {
            _entries.Add(new LogEntry { Description = description, Timestamp = DateTime.Now, Category = category });
        }

        /// <summary>Returns the most recent entries, newest first (default: last 8).</summary>
        public List<LogEntry> GetRecent(int count = DefaultRecentCount)
        {
            return _entries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>Returns every logged entry, newest first.</summary>
        public List<LogEntry> GetAll()
        {
            return _entries
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>Formats the most recent entries for display in the chat window.</summary>
        public string FormatForChat(int count = DefaultRecentCount)
        {
            if (_entries.Count == 0)
                return "No activity has been logged yet this session. Try adding a task or starting the quiz!";

            var recent = GetRecent(count);
            var sb = new StringBuilder();
            sb.Append($"📜 Here are your last {recent.Count} action(s):\n");

            foreach (var entry in recent)
                sb.Append($"\n   • {entry}");

            if (HasMore)
                sb.Append($"\n\n...and {TotalCount - recent.Count} more. Type 'show full log' to see everything.");

            return sb.ToString();
        }

        /// <summary>Formats the entire history for display in the chat window or Activity Log tab.</summary>
        public string FormatFullForChat()
        {
            if (_entries.Count == 0)
                return "No activity has been logged yet this session. Try adding a task or starting the quiz!";

            var all = GetAll();
            var sb = new StringBuilder();
            sb.Append($"📜 Full activity history ({all.Count} action(s)):\n");

            foreach (var entry in all)
                sb.Append($"\n   • {entry}");

            return sb.ToString();
        }
    }
}
