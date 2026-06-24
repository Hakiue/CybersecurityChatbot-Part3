using System;
using System.Threading;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Handles all console UI rendering: colours, borders, typing effects, ASCII art.
    /// </summary>
    public static class ConsoleUI
    {
        // ─── Colour palette ───────────────────────────────────────────────────────
        public static readonly ConsoleColor AccentColor    = ConsoleColor.Cyan;
        public static readonly ConsoleColor BotColor       = ConsoleColor.Green;
        public static readonly ConsoleColor UserColor      = ConsoleColor.Yellow;
        public static readonly ConsoleColor ErrorColor     = ConsoleColor.Red;
        public static readonly ConsoleColor HeaderColor    = ConsoleColor.Magenta;
        public static readonly ConsoleColor SubtleColor    = ConsoleColor.DarkCyan;
        public static readonly ConsoleColor NeutralColor   = ConsoleColor.White;

        // ─── ASCII Art ────────────────────────────────────────────────────────────
        /// <summary>
        /// Displays the full ASCII art logo for the chatbot.
        /// </summary>
        public static void DisplayLogo()
        {
            Console.ForegroundColor = AccentColor;
            Console.WriteLine(@"
  ██████╗██╗   ██╗██████╗ ███████╗██████╗      █████╗ ██╗    ██╗ █████╗ ██████╗ ███████╗
 ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗    ██╔══██╗██║    ██║██╔══██╗██╔══██╗██╔════╝
 ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝    ███████║██║ █╗ ██║███████║██████╔╝█████╗  
 ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗    ██╔══██║██║███╗██║██╔══██║██╔══██╗██╔══╝  
 ╚██████╗   ██║   ██████╔╝███████╗██║  ██║    ██║  ██║╚███╔███╔╝██║  ██║██║  ██║███████╗
  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝    ╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝");

            Console.ForegroundColor = HeaderColor;
            Console.WriteLine(@"
                    ╔══════════════════════════════════════════╗
                    ║    🛡️  CYBERSECURITY AWARENESS BOT  🛡️    ║
                    ║      Protecting South African Citizens    ║
                    ╚══════════════════════════════════════════╝");

            Console.ForegroundColor = SubtleColor;
            Console.WriteLine(@"
                         /\      Stay Safe Online
                        /  \     ──────────────────
                       / 🔒 \    • Phishing Alerts
                      /______\   • Password Safety
                     |        |  • Safe Browsing
                     |  [===] |  • Scam Detection
                     |________|");

            Console.ResetColor();
            PrintDivider();
        }

        // ─── Divider & spacing helpers ────────────────────────────────────────────
        public static void PrintDivider(char symbol = '─', int width = 70)
        {
            Console.ForegroundColor = SubtleColor;
            Console.WriteLine(new string(symbol, width));
            Console.ResetColor();
        }

        public static void PrintDoubleDivider(int width = 70)
        {
            Console.ForegroundColor = AccentColor;
            Console.WriteLine(new string('═', width));
            Console.ResetColor();
        }

        public static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = HeaderColor;
            Console.WriteLine($"  ◆ {title.ToUpper()} ◆");
            Console.ForegroundColor = SubtleColor;
            Console.WriteLine($"  {new string('─', title.Length + 6)}");
            Console.ResetColor();
        }

        // ─── Typed-output effect ──────────────────────────────────────────────────
        /// <summary>
        /// Prints text one character at a time to simulate a typing effect.
        /// </summary>
        public static void TypeWrite(string text, ConsoleColor color = ConsoleColor.White, int delayMs = 18)
        {
            Console.ForegroundColor = color;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delayMs);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Prints a bot response with the bot prefix and typing animation.
        /// </summary>
        public static void PrintBotMessage(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = BotColor;
            Console.Write("  🤖 Bot  : ");
            Console.ResetColor();
            TypeWrite(message, NeutralColor, 15);
            Console.WriteLine();
        }

        /// <summary>
        /// Prints an error/warning message.
        /// </summary>
        public static void PrintError(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine($"  ⚠  {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Prints an informational tip with a shield icon.
        /// </summary>
        public static void PrintTip(string tip)
        {
            Console.WriteLine();
            Console.ForegroundColor = AccentColor;
            Console.WriteLine($"  🛡  TIP : {tip}");
            Console.ResetColor();
        }

        /// <summary>
        /// Prompts the user for input and returns the trimmed response.
        /// </summary>
        public static string PromptUser(string name = "You")
        {
            Console.ForegroundColor = UserColor;
            Console.Write($"\n  👤 {name,-6}: ");
            Console.ForegroundColor = NeutralColor;
            string input = Console.ReadLine() ?? string.Empty;
            Console.ResetColor();
            return input.Trim();
        }

        /// <summary>
        /// Shows an animated "thinking" indicator.
        /// </summary>
        public static void ShowThinking(int steps = 3)
        {
            Console.ForegroundColor = SubtleColor;
            Console.Write("\n  ⏳ Thinking");
            for (int i = 0; i < steps; i++)
            {
                Thread.Sleep(300);
                Console.Write(".");
            }
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
