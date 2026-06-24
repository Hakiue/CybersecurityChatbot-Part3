using System.Collections.Generic;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Stores information the user shares during the conversation
    /// and provides personalised recall responses.
    /// Satisfies the Part 2 Memory and Recall requirement.
    /// </summary>
    public class ConversationMemory
    {
        // ── Automatic properties ───────────────────────────────────────────────
        /// <summary>The user's name collected at startup.</summary>
        public string UserName { get; set; } = "User";

        /// <summary>The last cybersecurity topic the user asked about.</summary>
        public string? LastTopic { get; set; }

        /// <summary>The user's self-declared favourite/interest topic.</summary>
        public string? FavouriteTopic { get; set; }

        /// <summary>Whether the user has expressed interest in a topic this session.</summary>
        public bool HasFavouriteTopic => !string.IsNullOrWhiteSpace(FavouriteTopic);

        /// <summary>Total number of messages sent this session.</summary>
        public int MessageCount { get; set; } = 0;

        // ── Topic keyword → friendly label map ────────────────────────────────
        private static readonly Dictionary<string, string> TopicLabels =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                ["cia triad"]         = "the CIA Triad",
                ["confidentiality"]   = "the CIA Triad",
                ["integrity"]         = "the CIA Triad",
                ["availability"]      = "the CIA Triad",
                ["cia"]               = "the CIA Triad",
                ["two-factor"]        = "two-factor authentication",
                ["two factor"]        = "two-factor authentication",
                ["2fa"]               = "two-factor authentication",
                ["mfa"]               = "two-factor authentication",
                ["authentication"]    = "two-factor authentication",
                ["password"]          = "password safety",
                ["phishing"]          = "phishing",
                ["smishing"]          = "smishing and vishing",
                ["vishing"]           = "smishing and vishing",
                ["safe browsing"]     = "safe browsing",
                ["browsing"]          = "safe browsing",
                ["https"]             = "safe browsing",
                ["privacy"]           = "privacy",
                ["popia"]             = "POPIA",
                ["ransomware"]        = "ransomware",
                ["malware"]           = "malware",
                ["virus"]             = "malware",
                ["trojan"]            = "malware",
                ["spyware"]           = "malware",
                ["keylogger"]         = "malware",
                ["vpn"]               = "VPNs",
                ["wifi"]              = "Wi-Fi security",
                ["wi-fi"]             = "Wi-Fi security",
                ["firewall"]          = "firewalls",
                ["backup"]            = "data backups",
                ["scam"]              = "online scams",
                ["fraud"]             = "online scams",
                ["social engineering"] = "social engineering",
                ["pretexting"]        = "social engineering",
                ["identity theft"]    = "identity theft",
                ["data breach"]       = "data breaches",
                ["breach"]            = "data breaches",
                ["sim swap"]          = "SIM swap fraud",
                ["sql injection"]     = "SQL injection",
                ["zero day"]          = "zero-day vulnerabilities",
                ["zero-day"]          = "zero-day vulnerabilities",
                ["patch"]             = "patches and updates",
                ["update"]            = "patches and updates",
                ["antivirus"]         = "antivirus protection",
                ["report"]            = "reporting cybercrime",
                ["cyberbullying"]     = "cyberbullying",
            };

        // ── Public methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Updates the last topic based on the user's input.
        /// </summary>
        public void UpdateTopic(string input)
        {
            string lower = input.ToLower();

            // Keep combined topics together so follow-ups like "tell me more",
            // "give me examples" and "another tip" stay on the full topic
            // instead of only remembering the first keyword, such as SQL injection.
            if (MentionsSqlZeroDayAndPatching(lower))
            {
                LastTopic = "SQL injection, zero-day attacks, and patching";
                return;
            }

            foreach (var pair in TopicLabels)
            {
                if (lower.Contains(pair.Key))
                {
                    LastTopic = pair.Value;
                    return;
                }
            }
        }

        private static bool MentionsSqlZeroDayAndPatching(string lower)
        {
            bool sql = lower.Contains("sql") || lower.Contains("sqli") || lower.Contains("database attack");
            bool zero = lower.Contains("zero day") || lower.Contains("zero-day") || lower.Contains("unpatched vulnerability");
            bool patch = lower.Contains("patch") || lower.Contains("patching") || lower.Contains("updates") || lower.Contains("software update");
            return sql && zero && patch;
        }

        /// <summary>
        /// Checks if the user is declaring an interest (e.g. "I'm interested in privacy").
        /// If so, stores it as their favourite topic and returns a confirmation message.
        /// </summary>
        public string? TryStoreFavouriteTopic(string input)
        {
            string lower = input.ToLower();

            // Detect interest declarations
            bool declaresInterest =
                lower.Contains("interested in") ||
                lower.Contains("i like") ||
                lower.Contains("i love") ||
                lower.Contains("my favourite") ||
                lower.Contains("i care about") ||
                lower.Contains("focus on");

            if (!declaresInterest) return null;

            // Find which topic they mentioned
            foreach (var pair in TopicLabels)
            {
                if (lower.Contains(pair.Key))
                {
                    FavouriteTopic = pair.Value;
                    return $"Great! I'll remember that you're interested in {pair.Value}. " +
                           $"It's a crucial part of staying safe online. " +
                           $"I'll keep that in mind as we chat, {UserName}!";
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a personalised recall message if the user has a stored favourite topic.
        /// Used to proactively reference their interest during relevant responses.
        /// </summary>
        public string? GetRecallMessage()
        {
            if (!HasFavouriteTopic) return null;

            return $"As someone interested in {FavouriteTopic}, " +
                   $"this is especially relevant for you, {UserName}.";
        }

        /// <summary>
        /// Returns a follow-up prompt based on the last topic discussed.
        /// </summary>
        public string? GetFollowUpPrompt()
        {
            if (LastTopic == null) return null;
            return $"Would you like to know more about {LastTopic}? " +
                   $"Just type 'tell me more' or ask a follow-up question.";
        }
    }
}
