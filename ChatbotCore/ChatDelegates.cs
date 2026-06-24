using System;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Defines and manages delegates used throughout the chatbot.
    /// Delegates satisfy the Part 2 rubric requirement for delegate usage.
    /// A delegate is a type that holds a reference to a method,
    /// allowing methods to be passed as parameters and invoked dynamically.
    /// </summary>
    public static class ChatDelegates
    {
        // ── Delegate type declarations ─────────────────────────────────────────

        /// <summary>
        /// Delegate for processing a user message and returning a bot response.
        /// Used to swap response strategies at runtime.
        /// </summary>
        public delegate string MessageProcessor(string userInput, ConversationMemory memory);

        /// <summary>
        /// Delegate for handling bot output — allows the GUI to hook into responses.
        /// </summary>
        public delegate void BotResponseHandler(string sender, string message, MessageType type);

        /// <summary>
        /// Delegate for logging activity events.
        /// </summary>
        public delegate void ActivityLogger(string activity);

        // ── Message type enum ──────────────────────────────────────────────────
        public enum MessageType
        {
            UserMessage,
            BotResponse,
            BotTip,
            BotError,
            BotSentiment,
            BotMemory,
            System
        }

        // ── Built-in processors ────────────────────────────────────────────────

        /// <summary>
        /// Standard processor: runs input through the ResponseEngine.
        /// </summary>
        public static MessageProcessor StandardProcessor(ResponseEngine engine)
        {
            return (input, memory) =>
            {
                memory.UpdateTopic(input);
                string? response = engine.GetResponse(input);
                return response ?? "I didn't quite understand that. Could you try rephrasing? " +
                                   "Type 'what can I ask you about' to see all available topics.";
            };
        }

        /// <summary>
        /// Follow-up processor: handles "tell me more", "explain more", "another tip" etc.
        /// </summary>
        public static MessageProcessor FollowUpProcessor(ResponseEngine engine)
        {
            return (input, memory) =>
            {
                string lower = input.ToLower();
                bool isFollowUp =
                    lower.Contains("tell me more") ||
                    lower.Contains("explain more") ||
                    lower.Contains("more details") ||
                    lower.Contains("another tip") ||
                    lower.Contains("give me more") ||
                    lower.Contains("give me another") ||
                    lower.Contains("elaborate") ||
                    lower.Contains("expand on") ||
                    lower.Contains("continue") ||
                    lower.Contains("examples") ||
                    lower.Contains("example");

                if (isFollowUp && memory.LastTopic != null)
                {
                    // Give context-aware follow-up details instead of losing the topic.
                    string? response;
                    if (lower.Contains("another tip") || lower.Contains("give me another"))
                        response = engine.GetRandomTipForTopic(memory.LastTopic);
                    else if (lower.Contains("example") || lower.Contains("examples"))
                        response = engine.GetExamplesForTopic(memory.LastTopic);
                    else
                        response = engine.GetMoreForTopic(memory.LastTopic);

                    if (response != null)
                        return response;
                }

                // Fall through to standard processing
                return StandardProcessor(engine)(input, memory);
            };
        }
    }
}
