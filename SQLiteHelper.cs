using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace TaskManagementApp
{
    public static class SQLiteHelper
    {
        private static string dbFile = "TaskManagement.db";

        public static SQLiteConnection GetConnection()
        {
            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
            }

            return new SQLiteConnection($"Data Source={dbFile};Version=3;");
        }

        // ------------------ TASKS ------------------

        public static void CreateTasksTable()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
        CREATE TABLE IF NOT EXISTS Tasks (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TaskName TEXT NOT NULL,
            TaskDescription TEXT,
            TaskDate TEXT,
            Priority TEXT,
            Project TEXT,
            CreatedBy TEXT NOT NULL
        );";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void SaveTask(string taskName, string taskDescription, string taskDate, string priority, string project, string createdBy)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string insertQuery = @"
        INSERT INTO Tasks (TaskName, TaskDescription, TaskDate, Priority, Project, CreatedBy)
        VALUES (@TaskName, @TaskDescription, @TaskDate, @Priority, @Project, @CreatedBy);";

                using (var cmd = new SQLiteCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TaskName", taskName);
                    cmd.Parameters.AddWithValue("@TaskDescription", taskDescription);
                    cmd.Parameters.AddWithValue("@TaskDate", taskDate);
                    cmd.Parameters.AddWithValue("@Priority", priority);
                    cmd.Parameters.AddWithValue("@Project", project);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static List<Task> GetAllTasks(string username)
        {
            var tasks = new List<Task>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Tasks WHERE CreatedBy = @Username;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new Task(
                                Convert.ToInt32(reader["Id"]),
                                reader["TaskName"].ToString(),
                                reader["TaskDescription"].ToString(),
                                reader["TaskDate"].ToString(),
                                reader["Priority"].ToString(),
                                reader["Project"].ToString()
                            ));
                        }
                    }
                }
            }

            return tasks;
        }


        public static void UpdateTask(int id, string taskName, string taskDescription, string taskDate, string priority, string project)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                UPDATE Tasks
                SET TaskName = @TaskName, TaskDescription = @TaskDescription, TaskDate = @TaskDate, Priority = @Priority, Project = @Project
                WHERE Id = @Id;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TaskName", taskName);
                    cmd.Parameters.AddWithValue("@TaskDescription", taskDescription);
                    cmd.Parameters.AddWithValue("@TaskDate", taskDate);
                    cmd.Parameters.AddWithValue("@Priority", priority);
                    cmd.Parameters.AddWithValue("@Project", project);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTask(int taskId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Tasks WHERE Id = @TaskId;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TaskId", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------ USERS ------------------

        public static void CreateUsersTable()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    Email TEXT UNIQUE,
                    ProfileImagePath TEXT
                );";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool RegisterUser(string username, string password, string email, string profileImagePath)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Username", username);
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                        return false;
                }

                string insertQuery = @"
                INSERT INTO Users (Username, Password, Email, ProfileImagePath)
                VALUES (@Username, @Password, @Email, @ProfileImagePath);";

                using (var cmd = new SQLiteCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Email", email ?? "");
                    cmd.Parameters.AddWithValue("@ProfileImagePath", profileImagePath ?? "");
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public static bool ValidateLogin(string username, string password)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password;";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool UserExists(string username, string email)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);

                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static (string Email, string ProfileImagePath) GetUserInfo(string username)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT Email, ProfileImagePath FROM Users WHERE Username = @Username;";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string email = reader["Email"]?.ToString();
                            string profileImage = reader["ProfileImagePath"]?.ToString();
                            return (email, profileImage);
                        }
                    }
                }
            }

            return (null, null);
        }
    }
}
