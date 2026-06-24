using System;
using System.Collections.Generic;
using MySqlConnector;

namespace CybersecurityChatbot.Features
{
    /// <summary>
    /// Thrown when a database operation fails. Wraps the underlying MySQL
    /// exception with a friendly, user-facing message so the GUI can show a
    /// helpful error instead of crashing.
    /// </summary>
    public class DatabaseOperationException : Exception
    {
        public DatabaseOperationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Handles all MySQL database access for cybersecurity tasks, as required by the
    /// Part 3 brief ("you must integrate a simple database (MySQL)").
    ///
    /// Connects to a local MySQL server (default localhost:3306) and automatically
    /// creates both the database and the Tasks table on first run, so the only manual
    /// setup required is having a MySQL server installed and running — see the README
    /// for setup options (MySQL Community Server, XAMPP/WAMP, or Docker).
    ///
    /// Connection details can be overridden via constructor parameters or the
    /// CYBERTASKS_DB_* environment variables, without changing any code.
    ///
    /// Provides full CRUD: Add (Create), GetAllTasks (Read), CompleteTask /
    /// SetReminder (Update), and DeleteTask (Delete). Every operation is wrapped in
    /// error handling so a closed server, bad credentials, or network issue surfaces
    /// as a friendly message rather than a crash.
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _serverConnectionString;

        /// <summary>
        /// Creates the service and ensures the database and Tasks table exist.
        /// Any parameter left null falls back to a CYBERTASKS_DB_* environment
        /// variable, then to a sensible local-development default.
        /// </summary>
        public DatabaseService(string? server = null, string? database = null, string? userId = null,
            string? password = null, uint? port = null)
        {
            string resolvedServer = server
                ?? Environment.GetEnvironmentVariable("CYBERTASKS_DB_SERVER") ?? "localhost";
            string resolvedDatabase = database
                ?? Environment.GetEnvironmentVariable("CYBERTASKS_DB_NAME") ?? "cybersecurity_chatbot";
            string resolvedUser = userId
                ?? Environment.GetEnvironmentVariable("CYBERTASKS_DB_USER") ?? "root";
            string resolvedPassword = password
                ?? Environment.GetEnvironmentVariable("CYBERTASKS_DB_PASSWORD") ?? "";
            uint resolvedPort = port
                ?? (uint.TryParse(Environment.GetEnvironmentVariable("CYBERTASKS_DB_PORT"), out uint envPort) ? envPort : 3306u);

            _databaseName = resolvedDatabase;

            // Connect without a Database= first, so CREATE DATABASE IF NOT EXISTS can run
            // even the very first time, before the schema exists.
            _serverConnectionString =
                $"Server={resolvedServer};Port={resolvedPort};User={resolvedUser};Password={resolvedPassword};";
            _connectionString =
                $"Server={resolvedServer};Port={resolvedPort};Database={resolvedDatabase};User={resolvedUser};Password={resolvedPassword};";

            try
            {
                InitialiseDatabase();
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException(
                    $"Could not connect to MySQL at {resolvedServer}:{resolvedPort}. " +
                    "Make sure a MySQL server is running and the credentials in DatabaseService.cs " +
                    "(or the CYBERTASKS_DB_* environment variables) are correct.", ex);
            }
        }

        private void InitialiseDatabase()
        {
            using (var connection = new MySqlConnection(_serverConnectionString))
            {
                connection.Open();
                var createDb = connection.CreateCommand();
                createDb.CommandText = $"CREATE DATABASE IF NOT EXISTS `{_databaseName}`;";
                createDb.ExecuteNonQuery();
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();
                createTable.CommandText =
                    @"CREATE TABLE IF NOT EXISTS Tasks (
                        Id INT PRIMARY KEY AUTO_INCREMENT,
                        Title VARCHAR(255) NOT NULL,
                        Description TEXT NOT NULL,
                        IsCompleted TINYINT(1) NOT NULL DEFAULT 0,
                        CreatedAt DATETIME NOT NULL,
                        ReminderAt DATETIME NULL
                    );";
                createTable.ExecuteNonQuery();
            }
        }

        /// <summary>Inserts a new task and returns it with its generated Id populated.</summary>
        public CyberTask AddTask(string title, string description, DateTime? reminderAt)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                DateTime createdAt = DateTime.Now;

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                    @"INSERT INTO Tasks (Title, Description, IsCompleted, CreatedAt, ReminderAt)
                      VALUES (@title, @description, 0, @createdAt, @reminderAt);";
                insertCommand.Parameters.AddWithValue("@title", title);
                insertCommand.Parameters.AddWithValue("@description", description ?? string.Empty);
                insertCommand.Parameters.AddWithValue("@createdAt", createdAt);
                insertCommand.Parameters.AddWithValue("@reminderAt",
                    reminderAt.HasValue ? (object)reminderAt.Value : DBNull.Value);
                insertCommand.ExecuteNonQuery();

                var idCommand = connection.CreateCommand();
                idCommand.CommandText = "SELECT LAST_INSERT_ID();";
                long newId = Convert.ToInt64(idCommand.ExecuteScalar());

                return new CyberTask
                {
                    Id = (int)newId,
                    Title = title,
                    Description = description ?? string.Empty,
                    IsCompleted = false,
                    CreatedAt = createdAt,
                    ReminderAt = reminderAt,
                };
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException(
                    $"Could not save the task \"{title}\" to the database.", ex);
            }
        }

        /// <summary>Returns every task, open tasks first, newest first within each group.</summary>
        public List<CyberTask> GetAllTasks()
        {
            try
            {
                var tasks = new List<CyberTask>();

                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT Id, Title, Description, IsCompleted, CreatedAt, ReminderAt " +
                    "FROM Tasks ORDER BY IsCompleted ASC, CreatedAt DESC;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(new CyberTask
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        IsCompleted = reader.GetInt32(3) == 1,
                        CreatedAt = reader.GetDateTime(4),
                        ReminderAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    });
                }

                return tasks;
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException("Could not load your tasks from the database.", ex);
            }
        }

        /// <summary>Fetches a single task by its Id, or null if it doesn't exist.</summary>
        public CyberTask? GetTaskById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT Id, Title, Description, IsCompleted, CreatedAt, ReminderAt FROM Tasks WHERE Id = @id;";
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                if (!reader.Read()) return null;

                return new CyberTask
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    IsCompleted = reader.GetInt32(3) == 1,
                    CreatedAt = reader.GetDateTime(4),
                    ReminderAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                };
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException($"Could not load task #{id} from the database.", ex);
            }
        }

        /// <summary>Marks a task complete. Returns true if a row was updated.</summary>
        public bool CompleteTask(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id;";
                command.Parameters.AddWithValue("@id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException($"Could not mark task #{id} as complete.", ex);
            }
        }

        /// <summary>Sets or updates the reminder date/time for an existing task.</summary>
        public bool SetReminder(int id, DateTime reminderAt)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Tasks SET ReminderAt = @reminderAt WHERE Id = @id;";
                command.Parameters.AddWithValue("@reminderAt", reminderAt);
                command.Parameters.AddWithValue("@id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException($"Could not set a reminder for task #{id}.", ex);
            }
        }

        /// <summary>Permanently deletes a task. Returns true if a row was removed.</summary>
        public bool DeleteTask(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Tasks WHERE Id = @id;";
                command.Parameters.AddWithValue("@id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) when (ex is not DatabaseOperationException)
            {
                throw new DatabaseOperationException($"Could not delete task #{id}.", ex);
            }
        }
    }
}
