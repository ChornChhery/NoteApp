using Dapper;
using MySqlConnector;
using backend.Models;

namespace backend.Repositories;

public class UserRepository
{
    private readonly string _cs;

    public UserRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("DefaultConnection")!;
    }

    private MySqlConnection Conn()
    {
        return new MySqlConnection(_cs);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var c = Conn();

        return await c.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email=@Email",
            new { Email = email }
        );
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var c = Conn();

        var count = await c.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE Email=@Email",
            new { Email = email }
        );

        return count > 0;
    }

    public async Task<User> CreateAsync(RegisterDto dto)
    {
        using var c = Conn();

        var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var sql = @"
            INSERT INTO Users (Username, Email, Password)
            VALUES (@Username, @Email, @Password);
            SELECT LAST_INSERT_ID();";

        var id = await c.ExecuteScalarAsync<int>(
            sql,
            new
            {
                dto.Username,
                dto.Email,
                Password = hashed
            }
        );

        return new User
        {
            Id = id,
            Username = dto.Username,
            Email = dto.Email
        };
    }
}