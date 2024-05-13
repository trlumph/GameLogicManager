using MySql.Data.MySqlClient;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> UserExistsAsync(string name)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE name = @name", connection);
        cmd.Parameters.AddWithValue("@name", name);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<bool> AddUserAsync(string name, string passwordHash)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = new MySqlCommand("INSERT INTO users (name, password) VALUES (@name, @password)", connection);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@password", passwordHash);
        var result = await cmd.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<string?> GetUserPasswordAsync(string name)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = new MySqlCommand("SELECT password FROM users WHERE name = @name", connection);
        cmd.Parameters.AddWithValue("@name", name);
        var dbPassword = (string?)await cmd.ExecuteScalarAsync();
        return dbPassword;
    }
}