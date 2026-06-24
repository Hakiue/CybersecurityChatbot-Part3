using System;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot.Features
{
    /// <summary>
    /// The kind of action the user is asking the assistant to perform,
    /// as detected by <see cref="NlpParser"/>.
    /// </summary>
    public enum Intent
    {
        AddTask,
        ViewTasks,
        CompleteTask,
        DeleteTask,
        SetReminder,
        StartQuiz,
        ShowLog,
        ShowFullLog,
        ShowHelp,
        Unknown,
    }

    /// <summary>The outcome of parsing a chat message for task/quiz/log intent.</summary>
    public class ParseResult
    {
        public Intent Intent { get; set; } = Intent.Unknown;
        public string? TaskTitle { get; set; }
        public int? DaysFromNow { get; set; }
        public int? TaskId { get; set; }
    }

    /// <summary>
    /// A lightweight NLP simulation that recognises varied phrasings of task,
    /// quiz, reminder, and activity-log requests using keyword matching and
    /// basic string manipulation (string.Contains, Regex) rather than a true
    /// language model. Satisfies the Part 3 NLP Simulation requirement.
    ///
    /// Cybersecurity-topic keywords such as "password" and "phishing" are not
    /// duplicated here — they continue to be handled by the existing
    /// ResponseEngine from Parts 1/2, which already recognises 30+ topics.
    /// This class focuses purely on the new Part 3 task/quiz/log actions.
    /// </summary>
    public class NlpParser
    {
        private static readonly string[] DayNames =
            { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };

        /// <summary>Parses a chat message and returns the detected intent and any extracted parameters.</summary>
        public ParseResult Parse(string input)
        {
            var result = new ParseResult();
            if (string.IsNullOrWhiteSpace(input)) return result;

            string lower = input.ToLower().Trim();

            // ── Reminders — "remind me to...", "set a reminder..." ──────────────
            if (lower.Contains("remind me") || lower.Contains("set a reminder") ||
                lower.Contains("set reminder") || lower.Contains("reminder to") || lower.Contains("reminder for"))
            {
                result.Intent = Intent.SetReminder;
                result.TaskTitle = ExtractTaskTitle(input);
                result.DaysFromNow = ExtractDays(lower);
                return result;
            }

            // ── Add task — "add a task to...", "create a task...", "new task..." ─
            if ((lower.Contains("add") && lower.Contains("task")) ||
                (lower.Contains("create") && lower.Contains("task")) ||
                lower.Contains("new task") ||
                lower.StartsWith("task:") || lower.StartsWith("todo:") ||
                (lower.Contains("add") && lower.Contains("to my list")))
            {
                result.Intent = Intent.AddTask;
                result.TaskTitle = ExtractTaskTitle(input);
                result.DaysFromNow = ExtractDays(lower);
                return result;
            }

            // ── View tasks — "show my tasks", "what tasks do I have" ─────────────
            if ((lower.Contains("show") || lower.Contains("view") || lower.Contains("list") ||
                 lower.Contains("what") || lower.Contains("see")) && lower.Contains("task"))
            {
                result.Intent = Intent.ViewTasks;
                return result;
            }

            // ── Complete task — "complete task 2", "mark task as done" ───────────
            if (lower.Contains("task") &&
                (lower.Contains("complete") || lower.Contains("done") || lower.Contains("finish") || lower.Contains("mark")))
            {
                result.Intent = Intent.CompleteTask;
                result.TaskId = ExtractNumber(lower);
                result.TaskTitle = result.TaskId == null ? ExtractTaskTitle(input) : null;
                return result;
            }

            // ── Delete task — "delete task 1", "remove the 2fa task" ─────────────
            if (lower.Contains("task") &&
                (lower.Contains("delete") || lower.Contains("remove") || lower.Contains("cancel")))
            {
                result.Intent = Intent.DeleteTask;
                result.TaskId = ExtractNumber(lower);
                result.TaskTitle = result.TaskId == null ? ExtractTaskTitle(input) : null;
                return result;
            }

            // ── Quiz — "start the quiz", "test my knowledge" ─────────────────────
            if (lower.Contains("quiz") || lower.Contains("test my knowledge") || lower.Contains("test me"))
            {
                result.Intent = Intent.StartQuiz;
                return result;
            }

            // ── Activity log — "show activity log", "what have you done for me?" ─
            bool mentionsLog =
                lower.Contains("activity log") ||
                lower.Contains("activity") ||
                lower.Contains("what have you done") ||
                lower.Contains("what you've done") ||
                lower.Contains("show history") ||
                (lower.Contains("show") && lower.Contains("log")) ||
                (lower.Contains("log") && lower.Contains("history"));

            if (mentionsLog)
            {
                bool wantsMore = lower.Contains("more") || lower.Contains("full") ||
                                 lower.Contains("everything") || lower.Contains("all");
                result.Intent = wantsMore ? Intent.ShowFullLog : Intent.ShowLog;
                return result;
            }

            // ── Help with the assistant's task/quiz/log features ─────────────────
            if (lower.Contains("what can you do") || lower.Contains("what can you help") ||
                (lower.Contains("help") && (lower.Contains("task") || lower.Contains("quiz") || lower.Contains("reminder"))))
            {
                result.Intent = Intent.ShowHelp;
                return result;
            }

            return result; // Intent.Unknown — falls through to existing chat handling
        }

        // ── Extraction helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Tries several common phrasings to pull a task title out of the
        /// original (case-preserved) input, then strips trailing date/status words.
        /// </summary>
        public string? ExtractTaskTitle(string input)
        {
            string[] patterns =
            {
                @"remind me to (.+)",
                @"remind me about (.+)",
                @"remind me that i need to (.+)",
                @"set a reminder to (.+)",
                @"set a reminder for (.+)",
                @"set reminder to (.+)",
                @"reminder to (.+)",
                @"add a task to (.+)",
                @"add task to (.+)",
                @"add a task for (.+)",
                @"create a task to (.+)",
                @"create a task for (.+)",
                @"new task to (.+)",
                @"new task[:\s]+(.+)",
                @"task[:\s]+(.+)",
                @"todo[:\s]+(.+)",
                @"add (.+) to my (?:task list|to-?do list|tasks)",
            };

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    string captured = match.Groups[1].Value.Trim();
                    string cleaned = CleanTitle(captured);
                    if (!string.IsNullOrWhiteSpace(cleaned))
                        return CapitaliseFirst(cleaned);
                }
            }

            return null;
        }

        private static string CleanTitle(string text)
        {
            string[] dateTriggers =
            {
                "tomorrow", "today", "tonight", "next week", "this weekend",
                "on monday", "on tuesday", "on wednesday", "on thursday", "on friday", "on saturday", "on sunday",
                "by monday", "by tuesday", "by wednesday", "by thursday", "by friday", "by saturday", "by sunday",
            };

            string result = text;
            foreach (string trigger in dateTriggers)
                result = Regex.Replace(result, $@"\b{Regex.Escape(trigger)}\b", "", RegexOptions.IgnoreCase);

            result = Regex.Replace(result, @"\bin\s+\d+\s+days?\b", "", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\s+as\s+(done|complete|completed|finished)\s*$", "", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"[\.,!]+$", "");
            return result.Trim();
        }

        private static string CapitaliseFirst(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>Detects relative-day phrases ("tomorrow", "in 3 days", "on Friday") and returns an offset.</summary>
        public int? ExtractDays(string lower)
        {
            if (lower.Contains("tomorrow")) return 1;
            if (lower.Contains("today") || lower.Contains("tonight")) return 0;
            if (lower.Contains("next week")) return 7;
            if (lower.Contains("this weekend")) return DaysUntilWeekend();

            Match numberMatch = Regex.Match(lower, @"in\s+(\d+)\s+days?");
            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out int n))
                return n;

            for (int i = 0; i < DayNames.Length; i++)
            {
                if (lower.Contains("on " + DayNames[i]) || lower.Contains("by " + DayNames[i]) || lower.Contains(DayNames[i]))
                    return DaysUntil((DayOfWeek)i);
            }

            return null;
        }

        private static int DaysUntilWeekend()
        {
            int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)DateTime.Now.DayOfWeek + 7) % 7;
            return daysUntilSaturday == 0 ? 7 : daysUntilSaturday;
        }

        private static int DaysUntil(DayOfWeek target)
        {
            int diff = ((int)target - (int)DateTime.Now.DayOfWeek + 7) % 7;
            return diff == 0 ? 7 : diff;
        }

        /// <summary>
        /// Extracts a task ID, but only when a number immediately follows the word
        /// "task" (optionally with "number" or "#"), so phrases like "enable 2FA" don't
        /// get mistaken for "task 2".
        /// </summary>
        public int? ExtractNumber(string lower)
        {
            Match match = Regex.Match(lower, @"task\s*(?:number|#)?\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int n))
                return n;
            return null;
        }
    }
}
