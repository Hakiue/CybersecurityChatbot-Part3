using System;
using System.IO;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Handles playing the WAV voice greeting on application startup.
    /// Uses System.Media.SoundPlayer for cross-compatible playback.
    /// </summary>
    public static class VoiceGreeting
    {
        /// <summary>
        /// Attempts to play the WAV greeting file.
        /// Falls back gracefully with a console notice if the file is missing or playback fails.
        /// </summary>
        public static void Play()
        {
            // Resolve path relative to the executable so the app works from any directory
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string wavPath = Path.Combine(basePath, "Assets", "greeting.wav");

            if (!File.Exists(wavPath))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("  [Audio] Voice greeting file not found — skipping audio.");
                Console.WriteLine($"  [Audio] Expected: {wavPath}");
                Console.ResetColor();
                return;
            }

            try
            {
                // SoundPlayer is Windows-only (System.Media), appropriate for this C# console app
                using var player = new System.Media.SoundPlayer(wavPath);
                player.Load();
                player.PlaySync(); // PlaySync blocks until the audio finishes
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"  [Audio] Could not play greeting: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
