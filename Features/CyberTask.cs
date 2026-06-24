using System;

namespace CybersecurityChatbot.Features
{
    /// <summary>
    /// Represents a single cybersecurity-related task the user wants to track,
    /// with an optional title, description, and reminder date/time.
    /// Backed by the SQLite Tasks table via DatabaseService.
    /// </summary>
    public class CyberTask
    {
        /// <summary>The database row ID. 0 until the task has been saved.</summary>
        public int Id { get; set; }

        /// <summary>Short title for the task, e.g. "Enable two-factor authentication".</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Optional extra detail about the task.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Whether the task has been marked complete.</summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>When the task was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Optional date/time the user wants to be reminded about this task.</summary>
        public DateTime? ReminderAt { get; set; }

        /// <summary>True if a reminder is set, the task is still open, and the reminder time has passed.</summary>
        public bool IsReminderDue => ReminderAt.HasValue && !IsCompleted && ReminderAt.Value <= DateTime.Now;

        /// <summary>Friendly one-line summary of the reminder, used in the GUI.</summary>
        public string FormatReminder()
        {
            if (!ReminderAt.HasValue) return "No reminder set";
            return $"Reminder: {ReminderAt.Value:ddd dd MMM yyyy, HH:mm}";
        }

        public override string ToString()
        {
            string status = IsCompleted ? "✅" : "⬜";
            string reminder = ReminderAt.HasValue ? $"  ⏰ {ReminderAt.Value:dd MMM HH:mm}" : string.Empty;
            return $"{status} {Title}{reminder}";
        }
    }
}
