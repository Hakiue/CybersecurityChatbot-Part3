using System.Collections.Generic;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Detects the user's emotional sentiment from their input
    /// and returns an empathetic response prefix plus a follow-up tip.
    /// Satisfies the Part 2 Sentiment Detection requirement.
    /// </summary>
    public class SentimentDetector
    {
        /// <summary>Represents a detected sentiment with its response.</summary>
        public class SentimentResult
        {
            public bool SentimentDetected { get; set; }
            public string EmpathyMessage { get; set; } = string.Empty;
            public string FollowUpTip { get; set; } = string.Empty;
        }

        // ── Sentiment keyword pools ────────────────────────────────────────────
        private static readonly List<string> WorriedWords = new List<string>
        {
            "worried", "scared", "afraid", "nervous", "anxious", "fear",
            "terrified", "concerned", "stress", "stressed", "panic", "unsafe"
        };

        private static readonly List<string> FrustratedWords = new List<string>
        {
            "frustrated", "annoyed", "angry", "upset", "fed up", "hate",
            "useless", "confused", "lost", "overwhelmed", "hopeless", "difficult"
        };

        private static readonly List<string> CuriousWords = new List<string>
        {
            "curious", "interested", "want to know", "wondering",
            "i want to learn", "i want to understand", "fascinated"
        };

        private static readonly List<string> HappyWords = new List<string>
        {
            "great", "awesome", "happy", "excited", "love", "fantastic",
            "amazing", "good to know", "helpful", "thanks", "thank you", "cool"
        };

        // ── Empathy responses ──────────────────────────────────────────────────
        private static readonly Dictionary<string, string> EmpathyMessages =
            new Dictionary<string, string>
            {
                ["worried"] =
                    "It's completely understandable to feel worried — cyber threats are real and can be overwhelming. " +
                    "The good news is that awareness is your best defence. Here's something that can help:",

                ["frustrated"] =
                    "I hear you — cybersecurity can feel overwhelming at first, but you're already doing the right thing by learning about it. " +
                    "Let me simplify this for you:",

                ["curious"] =
                    "That curiosity is exactly what keeps you safe online! The more you know, the better protected you are. " +
                    "Here's what you should know:",

                ["happy"] =
                    "Great to hear you're feeling positive! Keeping that mindset while staying vigilant is the perfect combination. " +
                    "Here's a tip to keep you on top of your game:",
            };

        // ── Follow-up tips per sentiment ───────────────────────────────────────
        private static readonly Dictionary<string, string> FollowUpTips =
            new Dictionary<string, string>
            {
                ["worried"] =
                    "🛡 Start with the basics: enable two-factor authentication on your email and banking accounts today. " +
                    "This single step blocks over 99% of automated attacks.",

                ["frustrated"] =
                    "💡 Break it down into one step at a time. Start by changing your most important password to something strong and unique — " +
                    "that alone makes a huge difference.",

                ["curious"] =
                    "🔍 A great place to start exploring is the CIA Triad — Confidentiality, Integrity, and Availability. " +
                    "Type 'CIA triad' to learn about the three pillars that underpin all of cybersecurity.",

                ["happy"] =
                    "⭐ Since you're in a great headspace, why not take it further? " +
                    "Type 'what can I ask you about' to explore all the cybersecurity topics I can help you with.",
            };

        // ── Public method ──────────────────────────────────────────────────────

        /// <summary>
        /// Analyses the input for emotional sentiment.
        /// Returns a SentimentResult with an empathy message and follow-up tip if detected.
        /// </summary>
        public SentimentResult Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new SentimentResult { SentimentDetected = false };

            string lower = input.ToLower();

            string? sentiment = null;

            // Check each sentiment group in priority order
            if (ContainsAny(lower, WorriedWords))     sentiment = "worried";
            else if (ContainsAny(lower, FrustratedWords)) sentiment = "frustrated";
            else if (ContainsAny(lower, CuriousWords))    sentiment = "curious";
            else if (ContainsAny(lower, HappyWords))      sentiment = "happy";

            if (sentiment == null)
                return new SentimentResult { SentimentDetected = false };

            return new SentimentResult
            {
                SentimentDetected = true,
                EmpathyMessage    = EmpathyMessages[sentiment],
                FollowUpTip       = FollowUpTips[sentiment],
            };
        }

        /// <summary>Returns true if the input contains any word from the list.</summary>
        private static bool ContainsAny(string input, List<string> words)
        {
            foreach (string word in words)
                if (input.Contains(word))
                    return true;
            return false;
        }
    }
}
