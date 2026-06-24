using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using CybersecurityChatbot.ChatbotCore;
using CybersecurityChatbot.Features;
using static CybersecurityChatbot.ChatbotCore.ChatDelegates;

namespace CybersecurityChatbot.GUI
{
    /// <summary>
    /// Main WinForms window for the Cybersecurity Awareness Chatbot — Part 3.
    /// Combines every requirement from all three parts in one cohesive app:
    ///   • Chat tab    — Part 1/2 keyword recognition, memory, sentiment, delegates
    ///   • Tasks tab   — Part 3 task assistant with MySQL-backed reminders
    ///   • Quiz tab    — Part 3 12-question cybersecurity mini-game
    ///   • Log tab     — Part 3 activity log of everything the assistant has done
    /// A lightweight NLP layer lets task/quiz/reminder/log actions also be
    /// triggered straight from the Chat tab in natural language.
    /// </summary>
    public class MainForm : Form
    {
        // ── Core chatbot components (Parts 1 & 2) ─────────────────────────────
        private readonly ResponseEngine      _responseEngine;
        private readonly SentimentDetector   _sentimentDetector;
        private readonly ConversationMemory  _memory;

        // ── Core feature components (Part 3) ──────────────────────────────────
        private readonly DatabaseService _databaseService;
        private readonly ActivityLog     _activityLog;
        private readonly QuizEngine      _quizEngine;
        private readonly NlpParser       _nlpParser;

        // ── Delegates ──────────────────────────────────────────────────────────
        private MessageProcessor  _messageProcessor;
        private BotResponseHandler _responseHandler;
        private ActivityLogger    _activityLogger;

        // ── State ──────────────────────────────────────────────────────────────
        private bool   _nameCollected       = false;
        private string _userName            = "User";
        private bool   _awaitingLogShowMore = false;

        // ── Colours ────────────────────────────────────────────────────────────
        private static readonly Color DarkBg      = Color.FromArgb(13,  17,  23);
        private static readonly Color PanelBg     = Color.FromArgb(22,  27,  34);
        private static readonly Color InputBg     = Color.FromArgb(33,  38,  45);
        private static readonly Color AccentBlue  = Color.FromArgb(88, 166, 255);
        private static readonly Color AccentGreen = Color.FromArgb(63, 185, 80);
        private static readonly Color AccentRed   = Color.FromArgb(255,  85,  85);
        private static readonly Color AccentYellow= Color.FromArgb(255, 200,  87);
        private static readonly Color AccentPurple= Color.FromArgb(188, 140, 255);
        private static readonly Color TextPrimary = Color.FromArgb(230, 237, 243);
        private static readonly Color TextMuted   = Color.FromArgb(125, 133, 144);
        private static readonly Color BorderColor = Color.FromArgb(48,  54,  61);

        // ── Shared / Chat tab controls ────────────────────────────────────────
        private TabControl  _tabControl    = null!;
        private TabPage      _chatTab       = null!;
        private TabPage      _tasksTab      = null!;
        private TabPage      _quizTab       = null!;
        private TabPage      _logTab        = null!;

        private RichTextBox _chatDisplay   = null!;
        private TextBox     _inputBox      = null!;
        private Button      _sendButton    = null!;
        private Label       _headerLabel   = null!;
        private Label       _asciiLabel    = null!;
        private Label       _statusLabel   = null!;
        private Panel       _topPanel      = null!;
        private Panel       _chatPanel     = null!;
        private Panel       _inputPanel    = null!;

        // ── Tasks tab controls ────────────────────────────────────────────────
        private TextBox        _taskTitleInput       = null!;
        private TextBox        _taskDescriptionInput = null!;
        private CheckBox        _taskReminderEnabled  = null!;
        private DateTimePicker  _taskReminderPicker   = null!;
        private Button          _addTaskButton        = null!;
        private ListView        _taskListView         = null!;
        private Button          _completeTaskButton   = null!;
        private Button          _deleteTaskButton     = null!;

        // ── Quiz tab controls ─────────────────────────────────────────────────
        private Label            _quizProgressLabel = null!;
        private RichTextBox      _quizQuestionBox   = null!;
        private FlowLayoutPanel  _quizOptionsPanel  = null!;
        private Label            _quizFeedbackLabel = null!;
        private Button           _quizStartButton   = null!;
        private Button           _quizNextButton    = null!;

        // ── Activity Log tab controls ─────────────────────────────────────────
        private RichTextBox _logDisplay = null!;

        // ── Extra UX controls ─────────────────────────────────────────────────
        private Label        _taskCountLabel  = null!;   // "2 of 5 complete" in Tasks header
        private Label        _charCountLabel  = null!;   // character counter on input box
        private ProgressBar  _quizScoreBar    = null!;   // score progress bar in Quiz tab

        // ─────────────────────────────────────────────────────────────────────
        public MainForm()
        {
            _responseEngine    = new ResponseEngine();
            _sentimentDetector = new SentimentDetector();
            _memory            = new ConversationMemory();

            _databaseService = CreateDatabaseServiceWithFallback();
            _activityLog     = new ActivityLog();
            _quizEngine       = new QuizEngine();
            _nlpParser        = new NlpParser();

            // Wire up delegates
            _messageProcessor = FollowUpProcessor(_responseEngine);

            _responseHandler = (sender, message, type) =>
            {
                // This delegate handles all bot output — routes to the display
                AppendMessage(sender, message, type);
            };

            _activityLogger = (activity) =>
            {
                // Auto-detect category from description keywords for colour-coding.
                LogCategory cat = LogCategory.General;
                string lower = activity.ToLower();
                if (lower.Contains("task"))          cat = LogCategory.Task;
                else if (lower.Contains("reminder")) cat = LogCategory.Reminder;
                else if (lower.Contains("quiz"))     cat = LogCategory.Quiz;
                else if (lower.Contains("error") || lower.Contains("database")) cat = LogCategory.Error;

                _activityLog.Add(activity, cat);
                if (_statusLabel != null)
                    _statusLabel.Text = $"  {activity}";
                RefreshActivityLogDisplay();
            };

            InitialiseForm();
            InitialiseComponents();
            LoadTasksIntoListView();
            NotifyDueReminders();
            PlayVoiceGreeting();
            ShowWelcomeSequence();
        }

        /// <summary>
        /// Creates the DatabaseService, surfacing a clear message instead of letting a
        /// failed MySQL connection crash the app silently. See README for MySQL setup options.
        /// </summary>
        private static DatabaseService CreateDatabaseServiceWithFallback()
        {
            try
            {
                return new DatabaseService();
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(
                    "Could not connect to the MySQL database, so the Tasks feature will be unavailable " +
                    $"this session.\n\n{ex.Message}\n\n" +
                    "See the README for MySQL setup options (local install, XAMPP/WAMP, or Docker).",
                    "Task database unavailable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>Logs a quick activity-log notice on startup if any saved tasks have an overdue reminder.</summary>
        private void NotifyDueReminders()
        {
            try
            {
                int dueCount = _databaseService.GetAllTasks().Count(t => t.IsReminderDue);
                if (dueCount > 0)
                    _activityLogger($"⏰ {dueCount} task(s) have an overdue reminder — check the Tasks tab");
            }
            catch (DatabaseOperationException)
            {
                // Already surfaced to the user via LoadTasksIntoListView's own error handling.
            }
        }

        // ── Form initialisation ───────────────────────────────────────────────

        private void InitialiseForm()
        {
            Text            = "🛡 Cybersecurity Awareness Bot — Part 3 (Combined POE)";
            Size            = new Size(980, 780);
            MinimumSize     = new Size(860, 640);
            BackColor       = DarkBg;
            ForeColor       = TextPrimary;
            Font            = new Font("Segoe UI", 10f);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
        }

        private void InitialiseComponents()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
            };

            _chatTab  = new TabPage("💬 Chat")         { BackColor = DarkBg };
            _tasksTab = new TabPage("📋 Tasks")        { BackColor = DarkBg };
            _quizTab  = new TabPage("🎮 Quiz")         { BackColor = DarkBg };
            _logTab   = new TabPage("📜 Activity Log") { BackColor = DarkBg };

            BuildChatTab(_chatTab);
            BuildTasksTab(_tasksTab);
            BuildQuizTab(_quizTab);
            BuildLogTab(_logTab);

            _tabControl.TabPages.Add(_chatTab);
            _tabControl.TabPages.Add(_tasksTab);
            _tabControl.TabPages.Add(_quizTab);
            _tabControl.TabPages.Add(_logTab);

            // ── Status bar (shared across all tabs) ───────────────────────────
            _statusLabel = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                BackColor = Color.FromArgb(8, 12, 16),
                ForeColor = TextMuted,
                Font      = new Font("Segoe UI", 8.5f),
                Text      = "  Ready",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            Controls.Add(_tabControl);
            Controls.Add(_statusLabel);
        }

        // ── Tab 1: Chat (Parts 1 & 2, unchanged behaviour) ────────────────────

        private void BuildChatTab(TabPage tab)
        {
            _topPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 160,
                BackColor = PanelBg,
                Padding   = new Padding(16, 8, 16, 8),
            };

            _asciiLabel = new Label
            {
                Text      = GetAsciiArt(),
                Font      = new Font("Courier New", 6.5f, FontStyle.Bold),
                ForeColor = AccentBlue,
                BackColor = PanelBg,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            _headerLabel = new Label
            {
                Text      = "🇿🇦  Department of Cybersecurity  •  Awareness Campaign 2026  •  Protecting South African Citizens",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TextMuted,
                BackColor = PanelBg,
                Dock      = DockStyle.Bottom,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            _topPanel.Controls.Add(_asciiLabel);
            _topPanel.Controls.Add(_headerLabel);

            _chatPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = DarkBg,
                Padding   = new Padding(12, 8, 12, 8),
            };

            _chatDisplay = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = DarkBg,
                ForeColor   = TextPrimary,
                Font        = new Font("Segoe UI", 10.5f),
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                WordWrap    = true,
            };

            _chatPanel.Controls.Add(_chatDisplay);

            _inputPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 60,
                BackColor = PanelBg,
                Padding   = new Padding(12, 10, 12, 10),
            };

            _sendButton = new Button
            {
                Text      = "Send  ➤",
                Dock      = DockStyle.Right,
                Width     = 110,
                BackColor = AccentBlue,
                ForeColor = DarkBg,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            _sendButton.FlatAppearance.BorderSize = 0;
            _sendButton.Click += SendButton_Click;

            _inputBox = new TextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = InputBg,
                ForeColor   = TextPrimary,
                Font        = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Type a message, cybersecurity topic, or try 'add a task to enable 2FA'...",
                MaxLength   = 500,
            };
            _inputBox.KeyDown += InputBox_KeyDown;

            _charCountLabel = new Label
            {
                Text = "0 / 500", Dock = DockStyle.Right, Width = 60,
                BackColor = InputBg, ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8f), TextAlign = ContentAlignment.MiddleRight,
            };
            _inputBox.TextChanged += (s, e) =>
            {
                int len = _inputBox.Text.Length;
                _charCountLabel.Text = $"{len} / 500";
                _charCountLabel.ForeColor = len > 450 ? AccentYellow : TextMuted;
            };

            _inputPanel.Controls.Add(_inputBox);
            _inputPanel.Controls.Add(_charCountLabel);
            _inputPanel.Controls.Add(_sendButton);

            tab.Controls.Add(_chatPanel);
            tab.Controls.Add(_inputPanel);
            tab.Controls.Add(_topPanel);
        }

        // ── Tab 2: Tasks (Part 3 Task Assistant) ──────────────────────────────

        private void BuildTasksTab(TabPage tab)
        {
            // ── Add-task panel (top) ───────────────────────────────────────────
            var addPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 172,
                BackColor = PanelBg,
                Padding   = new Padding(16, 12, 16, 12),
            };

            var titleLabel = new Label
            {
                Text = "Task title:", ForeColor = TextMuted, BackColor = PanelBg,
                Location = new Point(16, 8), AutoSize = true,
            };
            _taskTitleInput = new TextBox
            {
                BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(16, 28), Width = 300,
                PlaceholderText = "e.g. Enable two-factor authentication",
            };
            _taskTitleInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AddTaskButton_Click(null, EventArgs.Empty); } };

            var descLabel = new Label
            {
                Text = "Description (optional):", ForeColor = TextMuted, BackColor = PanelBg,
                Location = new Point(330, 8), AutoSize = true,
            };
            _taskDescriptionInput = new TextBox
            {
                BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(330, 28), Width = 300,
                PlaceholderText = "Add more detail...",
            };

            _taskReminderEnabled = new CheckBox
            {
                Text = "Set reminder for:", ForeColor = TextPrimary, BackColor = PanelBg,
                Location = new Point(16, 64), AutoSize = true,
            };
            _taskReminderEnabled.CheckedChanged += (s, e) => _taskReminderPicker.Enabled = _taskReminderEnabled.Checked;

            _taskReminderPicker = new DateTimePicker
            {
                Location = new Point(150, 61), Width = 240,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "ddd dd MMM yyyy   HH:mm",
                Value = DateTime.Now.AddDays(1),
                Enabled = false,
            };

            _addTaskButton = new Button
            {
                Text = "➕ Add Task", BackColor = AccentBlue, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(16, 102), Width = 160, Height = 32, Cursor = Cursors.Hand,
            };
            _addTaskButton.FlatAppearance.BorderSize = 0;
            _addTaskButton.Click += AddTaskButton_Click;

            var reminderHintLabel = new Label
            {
                Text = "Tip: select a task below and click '⏰ Set Reminder' to apply the date/time above to it too.",
                ForeColor = TextMuted, BackColor = PanelBg, Font = new Font("Segoe UI", 8f),
                Location = new Point(16, 140), AutoSize = true,
            };

            _taskCountLabel = new Label
            {
                Text = "No tasks yet", ForeColor = TextMuted, BackColor = PanelBg,
                Font = new Font("Segoe UI", 8.5f), Location = new Point(190, 107), AutoSize = true,
            };

            addPanel.Controls.Add(titleLabel);
            addPanel.Controls.Add(_taskTitleInput);
            addPanel.Controls.Add(descLabel);
            addPanel.Controls.Add(_taskDescriptionInput);
            addPanel.Controls.Add(_taskReminderEnabled);
            addPanel.Controls.Add(_taskReminderPicker);
            addPanel.Controls.Add(_addTaskButton);
            addPanel.Controls.Add(_taskCountLabel);
            addPanel.Controls.Add(reminderHintLabel);

            // ── Task list (fill) ────────────────────────────────────────────────
            _taskListView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                BackColor     = DarkBg,
                ForeColor     = TextPrimary,
                Font          = new Font("Segoe UI", 9.5f),
                BorderStyle   = BorderStyle.None,
            };
            _taskListView.Columns.Add("✓", 40);
            _taskListView.Columns.Add("Title", 220);
            _taskListView.Columns.Add("Description", 240);
            _taskListView.Columns.Add("Reminder", 180);
            _taskListView.Columns.Add("Created", 120);
            _taskListView.DoubleClick += (s, e) => CompleteTaskButton_Click(null, EventArgs.Empty);

            // ── Action panel (bottom) ───────────────────────────────────────────
            var actionPanel = new Panel
            {
                Dock = DockStyle.Bottom, Height = 56, BackColor = PanelBg, Padding = new Padding(16, 10, 16, 10),
            };

            _completeTaskButton = new Button
            {
                Text = "✅ Mark Complete", BackColor = AccentGreen, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(16, 6), Width = 150, Height = 32, Cursor = Cursors.Hand,
            };
            _completeTaskButton.FlatAppearance.BorderSize = 0;
            _completeTaskButton.Click += CompleteTaskButton_Click;
            var toolTip = new ToolTip { ShowAlways = true };
            toolTip.SetToolTip(_completeTaskButton, "Mark the selected task as complete");

            _deleteTaskButton = new Button
            {
                Text = "🗑 Delete", BackColor = AccentRed, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(176, 6), Width = 110, Height = 32, Cursor = Cursors.Hand,
            };
            _deleteTaskButton.FlatAppearance.BorderSize = 0;
            _deleteTaskButton.Click += DeleteTaskButton_Click;
            toolTip.SetToolTip(_deleteTaskButton, "Permanently delete the selected task");

            var setReminderButton = new Button
            {
                Text = "⏰ Set Reminder", BackColor = AccentPurple, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(296, 6), Width = 150, Height = 32, Cursor = Cursors.Hand,
            };
            setReminderButton.FlatAppearance.BorderSize = 0;
            setReminderButton.Click += SetReminderButton_Click;
            toolTip.SetToolTip(setReminderButton, "Apply the date/time above as a reminder on the selected task");

            var refreshButton = new Button
            {
                Text = "🔄 Refresh", BackColor = InputBg, ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f),
                Location = new Point(456, 6), Width = 110, Height = 32, Cursor = Cursors.Hand,
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => LoadTasksIntoListView();

            actionPanel.Controls.Add(_completeTaskButton);
            actionPanel.Controls.Add(_deleteTaskButton);
            actionPanel.Controls.Add(setReminderButton);
            actionPanel.Controls.Add(refreshButton);

            tab.Controls.Add(_taskListView);
            tab.Controls.Add(actionPanel);
            tab.Controls.Add(addPanel);
        }

        // ── Tab 3: Quiz (Part 3 Mini-Game Quiz) ───────────────────────────────

        private void BuildQuizTab(TabPage tab)
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top, Height = 60, BackColor = PanelBg, Padding = new Padding(16, 12, 16, 12),
            };

            _quizProgressLabel = new Label
            {
                Dock = DockStyle.Fill, ForeColor = TextMuted, BackColor = PanelBg,
                Font = new Font("Segoe UI", 10f), TextAlign = ContentAlignment.MiddleLeft,
                Text = "Press 'Start Quiz' to test your cybersecurity knowledge.",
            };

            _quizScoreBar = new ProgressBar
            {
                Dock = DockStyle.Bottom, Height = 4,
                Minimum = 0, Maximum = 100, Value = 0,
                Style = ProgressBarStyle.Continuous,
                ForeColor = AccentGreen, BackColor = InputBg,
            };

            _quizStartButton = new Button
            {
                Text = "▶ Start Quiz", Dock = DockStyle.Right, Width = 150,
                BackColor = AccentBlue, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            _quizStartButton.FlatAppearance.BorderSize = 0;
            _quizStartButton.Click += QuizStartButton_Click;

            headerPanel.Controls.Add(_quizProgressLabel);
            headerPanel.Controls.Add(_quizStartButton);
            headerPanel.Controls.Add(_quizScoreBar);

            var bodyPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = DarkBg, Padding = new Padding(24, 16, 24, 16),
            };

            _quizQuestionBox = new RichTextBox
            {
                Dock = DockStyle.Top, Height = 150, BackColor = DarkBg, ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 11.5f), ReadOnly = true, BorderStyle = BorderStyle.None,
                Text = "🎮 Welcome to the Cybersecurity Quiz!\n\n" +
                       "Test your knowledge across 12 questions covering passwords, phishing, " +
                       "malware, privacy, and more. Click 'Start Quiz' above to begin — or just " +
                       "type 'start the quiz' in the Chat tab.",
            };

            _quizOptionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 190, BackColor = DarkBg,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true,
            };

            _quizFeedbackLabel = new Label
            {
                Dock = DockStyle.Fill, BackColor = DarkBg, ForeColor = AccentBlue,
                Font = new Font("Segoe UI", 10f), TextAlign = ContentAlignment.TopLeft,
                Text = string.Empty,
            };

            _quizNextButton = new Button
            {
                Text = "Next Question ▶", Dock = DockStyle.Bottom, Height = 42,
                BackColor = AccentBlue, ForeColor = DarkBg,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand, Visible = false,
            };
            _quizNextButton.FlatAppearance.BorderSize = 0;
            _quizNextButton.Click += QuizNextButton_Click;

            // WinForms processes Controls in reverse-add order for docking:
            // Fill must be added first so it occupies remaining space after Top controls.
            // Top controls are added in bottom-to-top visual order (last-added renders topmost).
            bodyPanel.Controls.Add(_quizFeedbackLabel);   // Fill   — added first, sits in middle
            bodyPanel.Controls.Add(_quizNextButton);      // Bottom — sits at the very bottom
            bodyPanel.Controls.Add(_quizOptionsPanel);    // Top    — renders below question
            bodyPanel.Controls.Add(_quizQuestionBox);     // Top    — added last, renders topmost

            tab.Controls.Add(bodyPanel);
            tab.Controls.Add(headerPanel);
        }

        // ── Tab 4: Activity Log (Part 3) ──────────────────────────────────────

        private void BuildLogTab(TabPage tab)
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top, Height = 56, BackColor = PanelBg, Padding = new Padding(16, 10, 16, 10),
            };

            var titleLabel = new Label
            {
                Text      = "📜 A timestamped history of everything this assistant has done for you.",
                Dock      = DockStyle.Fill, ForeColor = TextMuted, BackColor = PanelBg,
                Font      = new Font("Segoe UI", 9.5f), TextAlign = ContentAlignment.MiddleLeft,
            };

            var showFullLogButton = new Button
            {
                Text = "Show Full History", Dock = DockStyle.Right, Width = 160,
                BackColor = InputBg, ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand,
            };
            showFullLogButton.FlatAppearance.BorderSize = 0;
            showFullLogButton.Click += (s, e) => RefreshActivityLogDisplay(showAll: true);

            var refreshLogButton = new Button
            {
                Text = "🔄 Refresh", Dock = DockStyle.Right, Width = 110,
                BackColor = InputBg, ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand,
            };
            refreshLogButton.FlatAppearance.BorderSize = 0;
            refreshLogButton.Click += (s, e) => RefreshActivityLogDisplay(showAll: false);

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(showFullLogButton);
            headerPanel.Controls.Add(refreshLogButton);

            _logDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill, BackColor = DarkBg, ForeColor = TextPrimary,
                Font = new Font("Consolas", 10f), ReadOnly = true, BorderStyle = BorderStyle.None,
                Text = "No activity has been logged yet this session.",
            };

            tab.Controls.Add(_logDisplay);
            tab.Controls.Add(headerPanel);
        }

        // ── Startup sequence ──────────────────────────────────────────────────

        private void PlayVoiceGreeting()
        {
            try { VoiceGreeting.Play(); }
            catch { /* graceful fallback */ }
        }

        private void ShowWelcomeSequence()
        {
            AppendMessage("System", "════════════════════════════════════════════════════════", MessageType.System);
            AppendMessage("System", "🛡  CYBERSECURITY AWARENESS BOT  —  Part 3", MessageType.System);
            AppendMessage("System", "════════════════════════════════════════════════════════", MessageType.System);
            AppendMessage("Bot", "Hello! Welcome to the Cybersecurity Awareness Bot. " +
                "I'm here to help you stay safe online. 🛡", MessageType.BotResponse);
            AppendMessage("Bot", "Before we begin — what's your name?", MessageType.BotResponse);
            _activityLogger("Waiting for user name...");
        }

        // ── Message handling (Chat tab) ───────────────────────────────────────

        private void SendButton_Click(object? sender, EventArgs e) => ProcessInput();

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ProcessInput();
            }
        }

        private async void ProcessInput()
        {
            string input = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            _inputBox.Clear();
            _inputBox.Enabled   = false;
            _sendButton.Enabled = false;

            // Show user message
            AppendMessage(_userName, input, MessageType.UserMessage);

            // Collect name first. If the user accidentally types a cybersecurity topic
            // or a Part 3 command instead of their name, do not store that as their name.
            if (!_nameCollected)
            {
                if (LooksLikeCybersecurityTopicOrCommand(input))
                {
                    _responseHandler("Bot",
                        "That looks like a cybersecurity topic rather than a name. 😊 Please enter your name first, for example: Jake. After that I’ll answer your question.",
                        MessageType.BotError);
                    _inputBox.Enabled   = true;
                    _sendButton.Enabled = true;
                    _inputBox.Focus();
                    return;
                }

                var nameValidation = InputValidator.ValidateName(input);
                if (!nameValidation.IsValid)
                {
                    _responseHandler("Bot", nameValidation.ErrorMessage, MessageType.BotError);
                    _responseHandler("Bot", "Please enter your name to continue.", MessageType.BotResponse);
                }
                else
                {
                    _userName = input.Length <= 4 && input == input.ToUpper()
                        ? input.ToUpper()
                        : char.ToUpper(input[0]) + input.Substring(1).ToLower();

                    _memory.UserName = _userName;
                    _nameCollected = true;
                    Text = $"🛡 Cybersecurity Awareness Bot — Hello, {_userName}!";

                    await Task.Delay(400);
                    _responseHandler("Bot",
                        $"Great to meet you, {_userName}! 😊 I'm your Cybersecurity Awareness Assistant. " +
                        "I'll help you understand online threats and stay protected.",
                        MessageType.BotResponse);

                    await Task.Delay(300);
                    _responseHandler("Bot",
                        "You can ask me about phishing, passwords, the CIA triad, ransomware, VPNs, POPIA, and much more. " +
                        "Type 'what can I ask you about' for a full list.",
                        MessageType.BotTip);

                    await Task.Delay(300);
                    _responseHandler("Bot",
                        "New this version: I can also manage tasks and reminders, run a quick cybersecurity quiz, and keep an " +
                        "activity log. Try saying 'add a task to enable 2FA', 'start the quiz', or 'show activity log' — or just " +
                        "use the tabs above. 🚀",
                        MessageType.BotTip);

                    _activityLogger($"Session started for {_userName}");
                }

                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            // End-session commands should be handled gracefully instead of falling through to the default error.
            if (IsExitCommand(input))
            {
                string goodbye = _responseEngine.GetResponse(input) ??
                    $"Thanks for chatting, {_userName}. Stay cyber-safe! 👋";

                _responseHandler("Bot", goodbye, MessageType.BotResponse);
                _activityLogger("Chat session ended by user");
                _inputBox.Enabled = false;
                _sendButton.Enabled = false;
                return;
            }

            // Validate input
            var validation = InputValidator.Validate(input);
            if (!validation.IsValid)
            {
                _responseHandler("Bot", validation.ErrorMessage, MessageType.BotError);
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            _memory.MessageCount++;
            _activityLogger($"Processing message #{_memory.MessageCount}...");

            // Simulate thinking delay
            await Task.Delay(500);

            string lowerForLog = input.ToLower().Trim();

            // 0a. Contextual "show more" follow-up right after viewing the recent activity log.
            if (_awaitingLogShowMore &&
                (lowerForLog == "show more" || lowerForLog == "more" ||
                 lowerForLog.Contains("show everything") || lowerForLog.Contains("show all")))
            {
                _responseHandler("Bot", _activityLog.FormatFullForChat(), MessageType.BotResponse);
                _activityLogger("Viewed full activity log via chat");
                _awaitingLogShowMore = false;
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            // 0b. Task / quiz / reminder / activity-log intent detection — Part 3 NLP Simulation.
            // Runs before the Part 1/2 chat pipeline so commands like "add a task to enable 2FA"
            // are handled directly, without falling back to a generic chat response.
            ParseResult nlpResult = _nlpParser.Parse(input);
            if (nlpResult.Intent != Intent.Unknown)
            {
                await HandleNlpIntent(nlpResult, input);
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            // 1. Check if user is declaring a favourite topic (memory)
            string? memoryResponse = _memory.TryStoreFavouriteTopic(input);
            if (memoryResponse != null)
            {
                _responseHandler("Bot", memoryResponse, MessageType.BotMemory);
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                _activityLogger($"Stored favourite topic: {_memory.FavouriteTopic}");
                return;
            }

            // 2. Handle follow-up commands BEFORE sentiment detection.
            // This prevents "tell me more" from being treated as general curiosity
            // and keeps the bot on the current topic, as required by Part 2 conversation flow.
            string lowerInput = input.ToLower();
            bool isFollowUpOnly =
                lowerInput.Contains("tell me more") ||
                lowerInput.Contains("explain more") ||
                lowerInput.Contains("more details") ||
                lowerInput.Contains("give me examples") ||
                lowerInput.Contains("examples") ||
                lowerInput.Contains("another tip") ||
                lowerInput.Contains("give me another") ||
                lowerInput.Contains("elaborate") ||
                lowerInput.Contains("expand on") ||
                lowerInput.Contains("continue");

            if (isFollowUpOnly && _memory.LastTopic != null)
            {
                string followUpResponse = _messageProcessor(input, _memory);
                _responseHandler("Bot", followUpResponse, MessageType.BotResponse);
                _activityLogger($"Follow-up answered  •  Topic: {_memory.LastTopic}");
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            // 3. Check for sentiment
            var sentiment = _sentimentDetector.Detect(input);
            if (sentiment.SentimentDetected)
            {
                _responseHandler("Bot", sentiment.EmpathyMessage, MessageType.BotSentiment);
                await Task.Delay(300);
                _responseHandler("Bot", sentiment.FollowUpTip, MessageType.BotTip);

                // Also try to answer the underlying cybersecurity topic in the same turn.
                _memory.UpdateTopic(input);
                string? topicResponse = _responseEngine.GetResponse(input);
                if (topicResponse != null)
                {
                    await Task.Delay(300);
                    _responseHandler("Bot", topicResponse, MessageType.BotResponse);
                }

                _activityLogger("Sentiment detected and response personalised");
                _inputBox.Enabled   = true;
                _sendButton.Enabled = true;
                _inputBox.Focus();
                return;
            }

            // 4. Run through delegate-based message processor
            string botResponse = _messageProcessor(input, _memory);
            _responseHandler("Bot", botResponse, MessageType.BotResponse);

            // 5. Append recall message if relevant
            if (_memory.HasFavouriteTopic && _memory.MessageCount % 4 == 0)
            {
                string? recall = _memory.GetRecallMessage();
                if (recall != null)
                {
                    await Task.Delay(200);
                    _responseHandler("Bot", recall, MessageType.BotMemory);
                }
            }

            _activityLogger($"Response sent  •  Topic: {_memory.LastTopic ?? "general"}");
            _inputBox.Enabled   = true;
            _sendButton.Enabled = true;
            _inputBox.Focus();
        }

        // ── Part 3 NLP intent handling ─────────────────────────────────────────

        private async Task HandleNlpIntent(ParseResult result, string originalInput)
        {
            _awaitingLogShowMore = result.Intent == Intent.ShowLog;

            try
            {
            switch (result.Intent)
            {
                case Intent.AddTask:
                {
                    string title = result.TaskTitle ?? originalInput.Trim();
                    DateTime? reminder = result.DaysFromNow.HasValue
                        ? DateTime.Now.Date.AddDays(result.DaysFromNow.Value).AddHours(9)
                        : (DateTime?)null;

                    CyberTask task = _databaseService.AddTask(title, string.Empty, reminder);
                    LoadTasksIntoListView();

                    string confirmation = reminder.HasValue
                        ? $"✅ Added \"{task.Title}\" to your tasks with a reminder for {reminder.Value:ddd dd MMM, HH:mm}. You can manage it in the Tasks tab."
                        : $"✅ Added \"{task.Title}\" to your tasks. You can view, complete, or delete it in the Tasks tab.";

                    _responseHandler("Bot", confirmation, MessageType.BotResponse);
                    _activityLogger(reminder.HasValue
                        ? $"Task added via chat: \"{task.Title}\"  •  Reminder set"
                        : $"Task added via chat: \"{task.Title}\"");
                    break;
                }

                case Intent.SetReminder:
                {
                    string title = result.TaskTitle ?? "Untitled reminder";
                    int days = result.DaysFromNow ?? 1;
                    DateTime reminder = DateTime.Now.Date.AddDays(days).AddHours(9);

                    CyberTask task = _databaseService.AddTask(title, string.Empty, reminder);
                    LoadTasksIntoListView();

                    _responseHandler("Bot",
                        $"⏰ Got it! I'll remind you to \"{task.Title}\" on {reminder:ddd dd MMM yyyy} at {reminder:HH:mm}. " +
                        "I've added it as a task with a reminder — check the Tasks tab any time.",
                        MessageType.BotResponse);
                    _activityLogger($"Reminder set via chat: \"{task.Title}\"  •  {reminder:dd MMM yyyy HH:mm}");
                    break;
                }

                case Intent.ViewTasks:
                {
                    var tasks = _databaseService.GetAllTasks();
                    if (tasks.Count == 0)
                    {
                        _responseHandler("Bot",
                            "You don't have any tasks yet. Try saying 'add a task to enable 2FA' to create one!",
                            MessageType.BotResponse);
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append($"📋 You have {tasks.Count} task(s):\n");
                        foreach (CyberTask t in tasks)
                            sb.Append($"\n   {t}");
                        _responseHandler("Bot", sb.ToString(), MessageType.BotResponse);
                    }
                    _activityLogger("Viewed task list via chat");
                    break;
                }

                case Intent.CompleteTask:
                {
                    CyberTask? task = FindTask(result);
                    if (task == null)
                    {
                        _responseHandler("Bot",
                            "I couldn't find that task. Try 'show my tasks' to see the list, or check the Tasks tab.",
                            MessageType.BotError);
                    }
                    else
                    {
                        _databaseService.CompleteTask(task.Id);
                        LoadTasksIntoListView();
                        _responseHandler("Bot", $"✅ Marked \"{task.Title}\" as complete. Nice work!", MessageType.BotResponse);
                        _activityLogger($"Task completed via chat: \"{task.Title}\"");
                    }
                    break;
                }

                case Intent.DeleteTask:
                {
                    CyberTask? task = FindTask(result);
                    if (task == null)
                    {
                        _responseHandler("Bot",
                            "I couldn't find that task to delete. Try 'show my tasks' to see the list, or check the Tasks tab.",
                            MessageType.BotError);
                    }
                    else
                    {
                        _databaseService.DeleteTask(task.Id);
                        LoadTasksIntoListView();
                        _responseHandler("Bot", $"🗑 Deleted \"{task.Title}\" from your tasks.", MessageType.BotResponse);
                        _activityLogger($"Task deleted via chat: \"{task.Title}\"");
                    }
                    break;
                }

                case Intent.StartQuiz:
                    StartQuizFromChat();
                    break;

                case Intent.ShowLog:
                    _responseHandler("Bot", _activityLog.FormatForChat(), MessageType.BotResponse);
                    _activityLogger("Viewed recent activity log via chat");
                    break;

                case Intent.ShowFullLog:
                    _responseHandler("Bot", _activityLog.FormatFullForChat(), MessageType.BotResponse);
                    _activityLogger("Viewed full activity log via chat");
                    break;

                case Intent.ShowHelp:
                    _responseHandler("Bot",
                        "Here's what I can help you with:\n" +
                        "   • 📋 Tasks — \"add a task to enable 2FA\", \"show my tasks\", \"complete task 1\", \"delete task 2\"\n" +
                        "   • ⏰ Reminders — \"remind me to update my password tomorrow\"\n" +
                        "   • 🎮 Quiz — \"start the quiz\" or \"test my knowledge\"\n" +
                        "   • 📜 Activity log — \"show activity log\" or \"what have you done for me?\"\n" +
                        "   • 🛡 Cybersecurity topics — phishing, passwords, malware, POPIA, and more!",
                        MessageType.BotResponse);
                    break;
            }
            }
            catch (DatabaseOperationException ex)
            {
                _responseHandler("Bot", $"⚠️ {ex.Message} Please try again in a moment.", MessageType.BotError);
                _activityLogger($"Database error: {ex.Message}");
            }

            await Task.Delay(150);
        }

        private CyberTask? FindTask(ParseResult result)
        {
            var tasks = _databaseService.GetAllTasks();

            if (result.TaskId.HasValue)
                return tasks.FirstOrDefault(t => t.Id == result.TaskId.Value);

            if (!string.IsNullOrWhiteSpace(result.TaskTitle))
            {
                return tasks.FirstOrDefault(t => !t.IsCompleted && t.Title.Contains(result.TaskTitle!, StringComparison.OrdinalIgnoreCase))
                    ?? tasks.FirstOrDefault(t => t.Title.Contains(result.TaskTitle!, StringComparison.OrdinalIgnoreCase));
            }

            return tasks.FirstOrDefault(t => !t.IsCompleted);
        }

        private void StartQuizFromChat()
        {
            _tabControl.SelectedTab = _quizTab;
            QuizStartButton_Click(null, EventArgs.Empty);
            _responseHandler("Bot",
                "🎮 I've started the quiz for you over in the Quiz tab — 12 questions on passwords, phishing, malware, and more. Good luck!",
                MessageType.BotResponse);
        }

        private bool LooksLikeCybersecurityTopicOrCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string lower = input.ToLower();

            // Topic detection from the response engine catches inputs such as
            // "SQL injection", "phishing", "password safety", etc.
            if (_responseEngine.DetectTopic(lower) != null) return true;

            // Part 3 task/quiz/reminder/log commands should not be mistaken for a name either.
            if (_nlpParser.Parse(input).Intent != Intent.Unknown) return true;

            // Common chatbot commands should also not become the user's name.
            return lower.Contains("what can i ask") ||
                   lower.Contains("tell me more") ||
                   lower.Contains("give me examples") ||
                   lower.Contains("another tip") ||
                   lower.Contains("explain") ||
                   lower.Contains("topic") ||
                   lower.Contains("cyber");
        }

        private static bool IsExitCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string lower = input.Trim().ToLower();
            return lower == "exit" || lower == "quit" || lower == "bye" || lower == "goodbye" || lower == "end" || lower == "close";
        }

        // ── Tasks tab event handlers ──────────────────────────────────────────

        private void LoadTasksIntoListView()
        {
            if (_taskListView == null) return;

            try
            {
                _taskListView.Items.Clear();
                var tasks = _databaseService.GetAllTasks();

                foreach (CyberTask task in tasks)
                {
                    var item = new ListViewItem(task.IsCompleted ? "✅" : "⬜");
                    item.SubItems.Add(task.Title);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(task.Description) ? "—" : task.Description);

                    string reminderText;
                    if (!task.ReminderAt.HasValue)
                        reminderText = "—";
                    else if (task.IsReminderDue)
                        reminderText = $"⏰ OVERDUE — {task.ReminderAt.Value:dd MMM yyyy HH:mm}";
                    else
                        reminderText = task.ReminderAt.Value.ToString("dd MMM yyyy HH:mm");
                    item.SubItems.Add(reminderText);

                    item.SubItems.Add(task.CreatedAt.ToString("dd MMM yyyy"));
                    item.Tag = task;

                    if (task.IsCompleted)
                        item.ForeColor = TextMuted;
                    else if (task.IsReminderDue)
                        item.ForeColor = AccentRed;
                    else
                        item.ForeColor = TextPrimary;
                    item.BackColor = DarkBg;

                    _taskListView.Items.Add(item);
                }
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update task counter label
            if (_taskCountLabel != null)
            {
                try
                {
                    var all = _databaseService.GetAllTasks();
                    int total = all.Count;
                    int done  = all.Count(t => t.IsCompleted);
                    _taskCountLabel.Text      = total == 0 ? "No tasks yet" : $"{done} of {total} complete";
                    _taskCountLabel.ForeColor = done == total && total > 0 ? AccentGreen : TextMuted;
                }
                catch { /* silently skip counter update on DB error */ }
            }
        }

        private void AddTaskButton_Click(object? sender, EventArgs e)
        {
            string title = _taskTitleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Task title required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string description = _taskDescriptionInput.Text.Trim();
            DateTime? reminder = _taskReminderEnabled.Checked ? _taskReminderPicker.Value : (DateTime?)null;

            try
            {
                CyberTask task = _databaseService.AddTask(title, description, reminder);
                LoadTasksIntoListView();

                _taskTitleInput.Clear();
                _taskDescriptionInput.Clear();
                _taskReminderEnabled.Checked = false;

                string logMessage = reminder.HasValue
                    ? $"Task added: \"{task.Title}\"  •  Reminder set for {reminder.Value:dd MMM yyyy HH:mm}"
                    : $"Task added: \"{task.Title}\"";
                _activityLogger(logMessage);
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetReminderButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a task first.", "No task selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selected = _taskListView.SelectedItems[0];
            if (selected.Tag is not CyberTask task) return;

            DateTime reminderValue = _taskReminderPicker.Value;

            try
            {
                _databaseService.SetReminder(task.Id, reminderValue);
                LoadTasksIntoListView();
                _activityLogger($"Reminder set: \"{task.Title}\"  •  {reminderValue:dd MMM yyyy HH:mm}");
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CompleteTaskButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a task first.", "No task selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selected = _taskListView.SelectedItems[0];
            if (selected.Tag is not CyberTask task) return;

            if (task.IsCompleted)
            {
                MessageBox.Show("That task is already marked complete.", "Already complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _databaseService.CompleteTask(task.Id);
                LoadTasksIntoListView();
                _activityLogger($"Task completed: \"{task.Title}\"");
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTaskButton_Click(object? sender, EventArgs e)
        {
            if (_taskListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a task first.", "No task selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListViewItem selected = _taskListView.SelectedItems[0];
            if (selected.Tag is not CyberTask task) return;

            DialogResult confirm = MessageBox.Show(
                $"Delete \"{task.Title}\"? This cannot be undone.",
                "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _databaseService.DeleteTask(task.Id);
                LoadTasksIntoListView();
                _activityLogger($"Task deleted: \"{task.Title}\"");
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Quiz tab event handlers ───────────────────────────────────────────

        private void QuizStartButton_Click(object? sender, EventArgs e)
        {
            _quizEngine.Start();
            _quizFeedbackLabel.Text = string.Empty;
            _quizNextButton.Visible = false;
            _quizStartButton.Text = "🔄 Restart Quiz";
            if (_quizScoreBar != null) _quizScoreBar.Value = 0;
            RenderCurrentQuizQuestion();
            _activityLogger("Quiz started");
        }

        private void RenderCurrentQuizQuestion()
        {
            QuizQuestion? question = _quizEngine.GetCurrentQuestion();
            if (question == null) return;

            _quizProgressLabel.Text =
                $"Question {_quizEngine.CurrentQuestionNumber} of {_quizEngine.TotalQuestions}  •  Score: {_quizEngine.Score}";
            _quizQuestionBox.Text = question.Text;

            _quizOptionsPanel.Controls.Clear();
            for (int i = 0; i < question.Options.Length; i++)
            {
                int optionIndex = i; // captured per-button for the click handler
                var optionButton = new Button
                {
                    Text      = question.Type == QuestionType.MultipleChoice
                        ? $"{(char)('A' + i)}.  {question.Options[i]}"
                        : question.Options[i],
                    Width     = 480,
                    Height    = 36,
                    BackColor = InputBg,
                    ForeColor = TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font      = new Font("Segoe UI", 10f),
                    Cursor    = Cursors.Hand,
                    Margin    = new Padding(0, 4, 0, 4),
                };
                optionButton.FlatAppearance.BorderColor = BorderColor;
                optionButton.Click += (s, e) => QuizOptionButton_Click(optionIndex);
                _quizOptionsPanel.Controls.Add(optionButton);
            }

            _quizFeedbackLabel.Text = string.Empty;
        }

        private void QuizOptionButton_Click(int selectedIndex)
        {
            AnswerResult result = _quizEngine.SubmitAnswer(selectedIndex);

            // Update score progress bar
            if (_quizScoreBar != null)
            {
                int pct = _quizEngine.TotalQuestions == 0 ? 0
                    : (int)Math.Round(_quizEngine.Score * 100.0 / _quizEngine.TotalQuestions);
                _quizScoreBar.Value = Math.Min(pct, 100);
            }

            string feedback = result.IsCorrect
                ? $"✅ Correct! {result.Explanation}"
                : $"❌ Not quite. The correct answer was \"{result.CorrectAnswerText}\". {result.Explanation}";

            _quizFeedbackLabel.ForeColor = result.IsCorrect ? AccentGreen : AccentRed;
            _quizFeedbackLabel.Text = feedback;

            _activityLogger(result.IsCorrect ? "Quiz answer correct" : "Quiz answer incorrect");

            // Highlight the clicked button green/red and disable all options
            int btnIdx = 0;
            foreach (Control control in _quizOptionsPanel.Controls)
            {
                control.Enabled = false;
                if (control is Button btn)
                {
                    if (btnIdx == selectedIndex)
                        btn.BackColor = result.IsCorrect ? AccentGreen : AccentRed;
                    btnIdx++;
                }
            }

            if (result.QuizComplete)
            {
                _quizProgressLabel.Text = "Quiz complete!";
                _quizStartButton.Text = "🔄 Try Again";
                _activityLogger($"Quiz completed  •  Score: {_quizEngine.Score}/{_quizEngine.TotalQuestions}");
                _quizFeedbackLabel.Text = feedback + "\n\n" + _quizEngine.GetScoreSummary();
                _quizNextButton.Visible = false;
            }
            else
            {
                _quizNextButton.Text = "Next Question ▶";
                _quizNextButton.Visible = true;
            }
        }

        private void QuizNextButton_Click(object? sender, EventArgs e)
        {
            _quizNextButton.Visible = false;
            RenderCurrentQuizQuestion();
        }


        // ── Activity Log tab helpers ──────────────────────────────────────────

        private void RefreshActivityLogDisplay(bool showAll = false)
        {
            if (_logDisplay == null) return;

            var entries = showAll ? _activityLog.GetAll() : _activityLog.GetRecent();
            _logDisplay.Clear();

            if (entries.Count == 0)
            {
                _logDisplay.Text = "No activity has been logged yet this session. Try adding a task or starting the quiz!";
                return;
            }

            string header = showAll
                ? $"\U0001F4DC Full activity history ({_activityLog.TotalCount} action(s)):\n\n"
                : $"\U0001F4DC Last {entries.Count} action(s):\n\n";

            AppendLogColoured(header, TextMuted);

            foreach (var entry in entries)
            {
                Color entryColor = entry.Category switch
                {
                    LogCategory.Task     => AccentGreen,
                    LogCategory.Reminder => AccentYellow,
                    LogCategory.Quiz     => AccentBlue,
                    LogCategory.Error    => AccentRed,
                    _                    => TextPrimary,
                };
                AppendLogColoured($"  \u2022 {entry}\n", entryColor);
            }

            if (!showAll && _activityLog.HasMore)
                AppendLogColoured($"\n  ...and {_activityLog.TotalCount - entries.Count} more. Click 'Show Full History' to see everything.\n", TextMuted);

            // Auto-scroll to latest entry
            _logDisplay.SelectionStart = _logDisplay.Text.Length;
            _logDisplay.ScrollToCaret();
        }

        private void AppendLogColoured(string text, Color color)
        {
            int start = _logDisplay.TextLength;
            _logDisplay.AppendText(text);
            _logDisplay.Select(start, text.Length);
            _logDisplay.SelectionColor = color;
            _logDisplay.SelectionLength = 0;
        }

        // ── Chat display rendering ────────────────────────────────────────────

        /// <summary>
        /// Appends a coloured, formatted message to the chat display.
        /// Uses the BotResponseHandler delegate for all output routing.
        /// </summary>
        private void AppendMessage(string sender, string message, MessageType type)
        {
            if (_chatDisplay.InvokeRequired)
            {
                _chatDisplay.Invoke(new Action(() => AppendMessage(sender, message, type)));
                return;
            }

            _chatDisplay.SuspendLayout();

            // Determine colours based on message type
            Color senderColor = type switch
            {
                MessageType.UserMessage  => AccentYellow,
                MessageType.BotResponse  => AccentGreen,
                MessageType.BotTip       => AccentBlue,
                MessageType.BotError     => AccentRed,
                MessageType.BotSentiment => AccentPurple,
                MessageType.BotMemory    => Color.FromArgb(255, 160, 80),
                MessageType.System       => TextMuted,
                _                        => TextPrimary,
            };

            string icon = type switch
            {
                MessageType.UserMessage  => "👤",
                MessageType.BotResponse  => "🤖",
                MessageType.BotTip       => "💡",
                MessageType.BotError     => "⚠️",
                MessageType.BotSentiment => "💬",
                MessageType.BotMemory    => "🧠",
                MessageType.System       => "─",
                _                        => "•",
            };

            if (type == MessageType.System)
            {
                AppendColoured($"\n  {message}\n", TextMuted);
            }
            else
            {
                // Sender line
                AppendColoured($"\n  {icon} ", senderColor);
                AppendColoured($"{sender,-8}", senderColor, bold: true);
                AppendColoured(" │  ", BorderColor);

                // Message — handle multi-line
                string[] lines = message.Split('\n');
                AppendColoured(lines[0] + "\n", TextPrimary);
                for (int i = 1; i < lines.Length; i++)
                    AppendColoured($"           │  {lines[i]}\n", TextPrimary);
            }

            _chatDisplay.ResumeLayout();
            _chatDisplay.ScrollToCaret();
            _chatDisplay.SelectionStart = _chatDisplay.Text.Length;
            _chatDisplay.ScrollToCaret();
        }

        private void AppendColoured(string text, Color color, bool bold = false)
        {
            int start = _chatDisplay.TextLength;
            _chatDisplay.AppendText(text);
            _chatDisplay.Select(start, text.Length);
            _chatDisplay.SelectionColor = color;
            _chatDisplay.SelectionFont  = bold
                ? new Font(_chatDisplay.Font, FontStyle.Bold)
                : _chatDisplay.Font;
            _chatDisplay.SelectionLength = 0;
        }

        // ── ASCII art ─────────────────────────────────────────────────────────
        private static string GetAsciiArt()
        {
            return
                "  ██████╗██╗   ██╗██████╗ ███████╗██████╗      █████╗ ██╗    ██╗ █████╗ ██████╗ ███████╗\n" +
                " ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗    ██╔══██╗██║    ██║██╔══██╗██╔══██╗██╔════╝\n" +
                " ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝    ███████║██║ █╗ ██║███████║██████╔╝█████╗  \n" +
                " ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗    ██╔══██║██║███╗██║██╔══██║██╔══██╗██╔══╝  \n" +
                " ╚██████╗   ██║   ██████╔╝███████╗██║  ██║    ██║  ██║╚███╔███╔╝██║  ██║██║  ██║███████╗\n" +
                "  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝    ╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝";
        }
    }
}
