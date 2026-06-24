using System;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Provides static input validation utilities for user-entered text.
    /// </summary>
    public static class InputValidator
    {
        private const int MaxInputLength = 500;

        /// <summary>
        /// Validates a user's chat message.
        /// </summary>
        public static ValidationResult Validate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ValidationResult.Fail("Input was empty. Please type a message.");

            if (input.Length > MaxInputLength)
                return ValidationResult.Fail(
                    $"Your message is too long (max {MaxInputLength} characters).");

            if (IsNonAlphaContent(input))
                return ValidationResult.Fail(
                    "That doesn't look like a question. Try typing a cybersecurity topic!");

            return ValidationResult.Ok();
        }

        /// <summary>
        /// Validates a name entry — must contain at least one letter.
        /// </summary>
        public static ValidationResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Fail("Name cannot be empty.");

            string trimmed = name.Trim();

            if (trimmed.Length > 30)
                return ValidationResult.Fail("Please enter only your name, not a full question or topic.");

            // A name should not contain punctuation that normally appears in a topic/question.
            // This prevents inputs like "SQL injection, zero-day attacks, and patching"
            // from being accepted as the user's name.
            foreach (char c in trimmed)
            {
                if (!(char.IsLetter(c) || c == ' ' || c == '-' || c == '\''))
                    return ValidationResult.Fail("Please enter a real name using letters only, for example: Jake.");
            }

            string[] words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 3)
                return ValidationResult.Fail("Please enter just your name first. After that, you can ask cybersecurity questions.");

            foreach (char c in trimmed)
                if (char.IsLetter(c)) return ValidationResult.Ok();

            return ValidationResult.Fail("Please enter a name with at least one letter.");
        }

        private static bool IsNonAlphaContent(string input)
        {
            foreach (char c in input)
                if (char.IsLetter(c)) return false;
            return true;
        }
    }

    /// <summary>
    /// Represents the outcome of an input validation check.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        private ValidationResult() { }

        public static ValidationResult Ok()
            => new ValidationResult { IsValid = true };

        public static ValidationResult Fail(string message)
            => new ValidationResult { IsValid = false, ErrorMessage = message };
    }
}
