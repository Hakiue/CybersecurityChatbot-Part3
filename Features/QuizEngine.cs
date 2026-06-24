using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot.Features
{
    /// <summary>Whether a quiz question is multiple-choice or true/false.</summary>
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
    }

    /// <summary>A single quiz question with its options, correct answer, and explanation.</summary>
    public class QuizQuestion
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public string[] Options { get; set; } = Array.Empty<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    /// <summary>Result of submitting an answer to the current question.</summary>
    public class AnswerResult
    {
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string CorrectAnswerText { get; set; } = string.Empty;
        public bool QuizComplete { get; set; }
    }

    /// <summary>
    /// Runs the Cybersecurity Mini-Game Quiz: 12 questions mixing multiple-choice
    /// and true/false, presented one at a time with immediate feedback and a
    /// final score summary. Satisfies the Part 3 Quiz requirement.
    /// </summary>
    public class QuizEngine
    {
        private readonly List<QuizQuestion> _questions;
        private int _currentIndex;
        private int _score;
        private bool _isActive;

        public QuizEngine()
        {
            _questions = BuildQuestionBank();
        }

        public bool IsActive => _isActive;
        public int CurrentQuestionNumber => _currentIndex + 1;
        public int TotalQuestions => _questions.Count;
        public int Score => _score;

        /// <summary>Starts (or restarts) the quiz with a freshly shuffled question order.</summary>
        public void Start()
        {
            _currentIndex = 0;
            _score = 0;
            _isActive = true;
            ShuffleQuestions();
        }

        private void ShuffleQuestions()
        {
            var random = new Random();
            for (int i = _questions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_questions[i], _questions[j]) = (_questions[j], _questions[i]);
            }
        }

        /// <summary>Returns the current question, or null if the quiz hasn't started / has finished.</summary>
        public QuizQuestion? GetCurrentQuestion()
        {
            if (!_isActive || _currentIndex >= _questions.Count) return null;
            return _questions[_currentIndex];
        }

        /// <summary>Formats the current question (with options) as plain text for a chat-style display.</summary>
        public string FormatCurrentQuestion()
        {
            var q = GetCurrentQuestion();
            if (q == null) return "No active question. Start the quiz first!";

            var sb = new StringBuilder();
            sb.Append($"Question {CurrentQuestionNumber} of {TotalQuestions}\n\n");
            sb.Append(q.Text);

            if (q.Type == QuestionType.MultipleChoice)
            {
                for (int i = 0; i < q.Options.Length; i++)
                    sb.Append($"\n   {(char)('A' + i)}. {q.Options[i]}");
            }
            else
            {
                sb.Append("\n   True or False?");
            }

            return sb.ToString();
        }

        /// <summary>Submits an answer for the current question and advances the quiz.</summary>
        public AnswerResult SubmitAnswer(int selectedIndex)
        {
            var q = GetCurrentQuestion();
            if (q == null)
            {
                return new AnswerResult { IsCorrect = false, Explanation = "No active question.", QuizComplete = true };
            }

            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _score++;

            _currentIndex++;
            bool complete = _currentIndex >= _questions.Count;
            if (complete) _isActive = false;

            return new AnswerResult
            {
                IsCorrect = correct,
                Explanation = q.Explanation,
                CorrectAnswerText = q.Options[q.CorrectIndex],
                QuizComplete = complete,
            };
        }

        /// <summary>Builds a friendly final score message based on percentage correct.</summary>
        public string GetScoreSummary()
        {
            double percentage = TotalQuestions == 0 ? 0 : (_score * 100.0 / TotalQuestions);
            string message;

            if (percentage >= 100) message = "🏆 Perfect score! You're a cybersecurity expert!";
            else if (percentage >= 80) message = "🌟 Excellent! You really know your stuff.";
            else if (percentage >= 60) message = "👍 Good effort! A little more practice and you'll be an expert.";
            else if (percentage >= 40) message = "📘 Not bad, but it's worth revisiting a few topics.";
            else message = "💡 Keep learning! Try exploring topics in the Chat tab, then retake the quiz.";

            return $"Quiz complete! You scored {_score} out of {TotalQuestions} ({percentage:0}%).\n{message}";
        }

        private static List<QuizQuestion> BuildQuestionBank()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Text = "What does the 'C' in the CIA Triad stand for?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "Confidentiality", "Connectivity", "Cryptography", "Compliance" },
                    CorrectIndex = 0,
                    Explanation = "Confidentiality means ensuring information is only accessible to authorised people.",
                },
                new QuizQuestion
                {
                    Text = "Two-factor authentication only ever requires a password.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "2FA requires a password PLUS a second factor, such as an OTP or fingerprint, making accounts much harder to compromise.",
                },
                new QuizQuestion
                {
                    Text = "Which of these is the strongest password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "password123", "Tbl3-Sunset!92Kite", "qwerty", "MyName2024" },
                    CorrectIndex = 1,
                    Explanation = "Strong passwords are long and random, combining letters, numbers, and symbols — avoiding real words or predictable patterns.",
                },
                new QuizQuestion
                {
                    Text = "Phishing emails often create a sense of urgency to pressure you into acting quickly.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Urgency (\"your account will be suspended!\") is a classic phishing tactic designed to stop you thinking carefully before clicking.",
                },
                new QuizQuestion
                {
                    Text = "What does HTTPS provide that plain HTTP does not?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "Faster page loading", "Encrypted communication", "Free domain names", "Automatic backups" },
                    CorrectIndex = 1,
                    Explanation = "The 'S' in HTTPS stands for Secure — it encrypts data sent between your browser and the website.",
                },
                new QuizQuestion
                {
                    Text = "Ransomware encrypts a victim's files and demands payment for the decryption key.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Ransomware locks your files with encryption, and attackers demand a ransom — usually in cryptocurrency — to restore access.",
                },
                new QuizQuestion
                {
                    Text = "Which South African law governs the protection of personal information?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "POPIA", "FICA", "RICA", "ECTA" },
                    CorrectIndex = 0,
                    Explanation = "POPIA (Protection of Personal Information Act) regulates how personal information must be collected, used, and protected in South Africa.",
                },
                new QuizQuestion
                {
                    Text = "Using public Wi-Fi without a VPN is always completely safe for online banking.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "Public Wi-Fi traffic can be intercepted by attackers on the same network — a VPN encrypts your traffic and helps protect sensitive activity like banking.",
                },
                new QuizQuestion
                {
                    Text = "What is 'social engineering' in cybersecurity?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "A type of firewall", "Manipulating people into revealing information", "A coding language", "An antivirus feature" },
                    CorrectIndex = 1,
                    Explanation = "Social engineering tricks people — rather than systems — into giving up confidential information or access.",
                },
                new QuizQuestion
                {
                    Text = "A zero-day vulnerability is a weakness that has already been patched by the vendor.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "A zero-day is the opposite — it's unknown to the vendor, so no patch exists yet, making it especially dangerous.",
                },
                new QuizQuestion
                {
                    Text = "What should you do if you suspect your identity has been stolen?",
                    Type = QuestionType.MultipleChoice,
                    Options = new[] { "Ignore it and hope it resolves itself", "Report it, monitor accounts, and change passwords", "Only tell close friends", "Wait a year before acting" },
                    CorrectIndex = 1,
                    Explanation = "Acting quickly — reporting to your bank/authorities, freezing accounts, and updating passwords — limits the damage from identity theft.",
                },
                new QuizQuestion
                {
                    Text = "Regularly backing up your data protects you against data loss from ransomware.",
                    Type = QuestionType.TrueFalse,
                    Options = new[] { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Reliable, regular backups mean that even if ransomware encrypts your files, you can restore them without paying the ransom.",
                },
            };
        }
    }
}
