using System;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Represents the current user's session data.
    /// Uses automatic properties for clean, encapsulated data access.
    /// </summary>
    public class UserSession
    {
        /// <summary>Gets or sets the user's name.</summary>
        public string Name { get; set; } = "User";

        /// <summary>Gets or sets the user's favourite cybersecurity topic.</summary>
        public string? FavouriteTopic { get; set; }

        /// <summary>Gets or sets when the session started.</summary>
        public DateTime SessionStart { get; set; } = DateTime.Now;

        /// <summary>Gets or sets how many messages the user has sent.</summary>
        public int MessageCount { get; set; } = 0;

        /// <summary>Returns a greeting string using the user's name.</summary>
        public string GetPersonalisedGreeting()
            => $"Welcome back, {Name}! How can I help you stay cyber-safe today?";

        /// <summary>Returns true every 5 messages to remind the user of the topic list.</summary>
        public bool ShouldShowTopicReminder()
            => MessageCount > 0 && MessageCount % 5 == 0;
    }
}
