using MySql.Data.MySqlClient; // For interacting with MySQL DB
using TodoApi.Models; // Importing model classes like TaskItem, User

namespace TodoApi.Services;

// Service class to handle all DB operations related to tasks and users
public class TaskDbService
{
    private readonly string _connectionString;

    // Constructor: fetches the DB connection string from app settings
    public TaskDbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DB connection string.");
    }

    // Returns all tasks from the database
    public List<TaskItem> GetAll()
    {
        var tasks = new List<TaskItem>();
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM tasks", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new TaskItem
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                IsCompleted = reader.GetBoolean("is_completed")
            });
        }
        return tasks;
    }

    // Returns a specific task by its ID
    public TaskItem? GetById(int id)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new TaskItem
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                IsCompleted = reader.GetBoolean("is_completed")
            };
        }
        return null;
    }

    // Creates a new task in the database
    public TaskItem Create(TaskItem task)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        var cmd = new MySqlCommand(
            "INSERT INTO tasks (title, is_completed) VALUES (@title, @is_completed); SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@is_completed", task.IsCompleted);

        task.Id = Convert.ToInt32(cmd.ExecuteScalar()); // Assign that ID to the task object in memory.
        return task;
    }

    // Updates an existing task (title and completion status) by ID
    public bool Update(int id, TaskItem task)  // what to update and what to update it to.
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        var cmd = new MySqlCommand(
            "UPDATE tasks SET title = @title, is_completed = @is_completed WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@is_completed", task.IsCompleted);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0; // returns true if at least one row was updated
    }

    // Deletes a task from the database by ID
    public bool Delete(int id)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        var cmd = new MySqlCommand("DELETE FROM tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Gets a user by email from the database (used for login and checking duplicates)
    public User? GetUserByEmail(string email)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        var cmd = new MySqlCommand("SELECT * FROM users WHERE email = @Email", conn);
        cmd.Parameters.AddWithValue("@Email", email);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Email = reader.GetString("email"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password")
            };
        }
        return null;
    }

    // Creates a new user in the database
    public void CreateUser(User user)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        var cmd = new MySqlCommand(
            "INSERT INTO users (email, username, password) VALUES (@Email, @Username, @Password)", conn);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@Password", user.Password);

        cmd.ExecuteNonQuery();
    }
}
