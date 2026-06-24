using System;
using System.Threading;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Orchestrates the chatbot session: startup sequence, main conversation loop,
    /// and shutdown. Delegates UI, responses, and validation to dedicated classes.
    /// </summary>
    public class ChatbotEngine
    {
        private readonly ResponseEngine _responseEngine;
        private UserSession _session;

        // Commands that end the session
        private static readonly string[] ExitCommands = { "exit", "quit", "bye", "goodbye", "end", "close", "q" };

        public ChatbotEngine()
        {
            _responseEngine = new ResponseEngine();
            _session = new UserSession();
        }

        // ─── Public entry point ───────────────────────────────────────────────────

        /// <summary>
        /// Starts the chatbot: plays greeting, shows logo, collects name, then loops.
        /// </summary>
        public void Start()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "🛡 Cybersecurity Awareness Bot";

            // 1. Play voice greeting before showing any text
            ConsoleUI.PrintDoubleDivider();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  🔊 Playing voice greeting...");
            Console.ResetColor();
            VoiceGreeting.Play();

            // 2. Display ASCII logo
            ConsoleUI.DisplayLogo();

            // 3. Text welcome banner
            PrintWelcomeBanner();

            // 4. Ask for the user's name
            CollectUserName();

            // 5. Personalised welcome message after name is known
            Thread.Sleep(300);
            ConsoleUI.PrintBotMessage(
                $"Great to meet you, {_session.Name}! 🙂 I'm your Cybersecurity Awareness Assistant. " +
                "I'll help you understand online threats and how to stay protected. " +
                "Type 'what can I ask you about' to see all available topics, or just dive in with a question!");

            ConsoleUI.PrintBotMessage(
                "Type 'exit' or 'quit' at any time to end the session.");

            // 6. Main conversation loop
            RunConversationLoop();

            // 7. Goodbye
            PrintGoodbye();
        }

        // ─── Startup helpers ──────────────────────────────────────────────────────

        private void PrintWelcomeBanner()
        {
            ConsoleUI.PrintDoubleDivider();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  🇿🇦  Department of Cybersecurity — Awareness Campaign 2026");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  Helping South African citizens identify and avoid cyber threats.");
            ConsoleUI.PrintDoubleDivider();
            Console.ResetColor();
        }

        private void CollectUserName()
        {
            ConsoleUI.PrintSectionHeader("User Registration");
            ConsoleUI.TypeWrite("  Hello! Before we begin, I'd love to know your name.", ConsoleColor.Cyan, 20);

            while (true)
            {
                string nameInput = ConsoleUI.PromptUser("Name");
                var validation = InputValidator.ValidateName(nameInput);

                if (validation.IsValid)
                {
                    // Capitalise first letter for a polished feel
                    _session.Name = char.ToUpper(nameInput[0]) + nameInput.Substring(1).ToLower();
                    break;
                }

                ConsoleUI.PrintError(validation.ErrorMessage);
                ConsoleUI.TypeWrite("  Please try again:", ConsoleColor.Yellow, 20);
            }
        }

        // ─── Conversation loop ────────────────────────────────────────────────────

        private void RunConversationLoop()
        {
            ConsoleUI.PrintDoubleDivider();
            ConsoleUI.PrintSectionHeader($"Chat Session — {_session.Name}");

            while (true)
            {
                string userInput = ConsoleUI.PromptUser(_session.Name);

                // Check for exit commands
                if (IsExitCommand(userInput))
                {
                    // Provide a farewell response via the response engine
                    string? farewell = _responseEngine.GetResponse(userInput);
                    if (farewell != null)
                        ConsoleUI.PrintBotMessage(farewell);
                    break;
                }

                // Validate input
                var validation = InputValidator.Validate(userInput);
                if (!validation.IsValid)
                {
                    ConsoleUI.PrintError(validation.ErrorMessage);
                    continue;
                }

                _session.MessageCount++;

                // Show thinking animation for a realistic feel
                ConsoleUI.ShowThinking();

                // Get response
                string response = BuildResponse(userInput);
                ConsoleUI.PrintBotMessage(response);

                // Periodically remind the user what topics are available
                if (_session.ShouldShowTopicReminder())
                {
                    Thread.Sleep(400);
                    ConsoleUI.PrintTip("Don't forget: type 'what can I ask you about' to see all available topics.");
                }
            }
        }

        /// <summary>
        /// Gets a response from the engine, falling back to a helpful default.
        /// </summary>
        private string BuildResponse(string input)
        {
            string? response = _responseEngine.GetResponse(input);

            if (response != null)
                return response;

            // Default fallback — friendly and instructive
            return "I didn't quite understand that. Could you rephrase? 🤔\n" +
                   "   Try typing a keyword like 'password', 'phishing', 'scam', or 'privacy'.\n" +
                   "   Or type 'what can I ask you about' for a full topic list.";
        }

        private static bool IsExitCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string lower = input.Trim().ToLower();
            foreach (string cmd in ExitCommands)
                if (lower == cmd || lower.Contains(cmd)) return true;
            return false;
        }

        // ─── Shutdown ─────────────────────────────────────────────────────────────

        private void PrintGoodbye()
        {
            ConsoleUI.PrintDoubleDivider();
            Console.ForegroundColor = ConsoleColor.Cyan;
            ConsoleUI.TypeWrite($"\n  👋 Thanks for chatting, {_session.Name}!", ConsoleColor.Cyan, 25);
            ConsoleUI.TypeWrite(
                "  Remember: Think before you click. Update your passwords. Enable 2FA.",
                ConsoleColor.Green, 20);
            ConsoleUI.TypeWrite(
                "  Stay cyber-safe! 🛡\n",
                ConsoleColor.Magenta, 20);
            ConsoleUI.PrintDoubleDivider();
            Console.ResetColor();

            Console.WriteLine("\n  Press any key to close...");
            Console.ReadKey(true);
        }
    }
}
