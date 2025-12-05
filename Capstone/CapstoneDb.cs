using System.Data.SQLite;

namespace Capstone;

public class CapstoneDb
{
    public string ConnectionString { get; set; }

    public CapstoneDb(string connectionString)
    {
        ConnectionString = connectionString;        
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var conn = new SQLiteConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT Id, UserName, Email, Password, Role FROM Users WHERE UserName = @username", conn);
        cmd.Parameters.AddWithValue("@username", username);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var user = new User();
            user.Id = reader.GetGuid(0);
            user.UserName = reader.GetString(1);
            user.Email = reader.GetString(2);
            user.Password = reader.GetString(3);
            user.Role = reader.GetString(4);
            return user;
        }
        return null;
    }

    public async Task<User> RegisterUserAsync(User user)
    {
        user.Id = Guid.CreateVersion7();
        using var conn = new SQLiteConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("INSERT INTO Users (Id, UserName, Email, Password, Role) VALUES (@id, @username, @email, @password, @role)", conn);
        cmd.Parameters.AddWithValue("@id", Guid.CreateVersion7());
        cmd.Parameters.AddWithValue("@username", user.UserName);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@password", user.Password);
        cmd.Parameters.AddWithValue("@role", user.Role);
        await cmd.ExecuteNonQueryAsync();
        return user;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        using var conn = new SQLiteConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT Id, UserName, Email, Password, Role FROM Users", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        var users = new List<User>();
        while (reader.Read())
        {
            var user = new User();
            user.Id = reader.GetGuid(0);
            user.UserName = reader.GetString(1);
            user.Email = reader.GetString(2);
            user.Password = reader.GetString(3);
            user.Role = reader.GetString(4);
            users.Add(user);
        }
        return users;
    }

    public async Task AssignRoleAsync(Guid userId, string role)
    {
        using var conn = new SQLiteConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("UPDATE Users SET Role = @role WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}