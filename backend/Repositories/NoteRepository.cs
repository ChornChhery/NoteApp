using Dapper;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Repositories;

public class NoteRepository
{
    private readonly string _connectionString;

    public NoteRepository(IConfiguration config)
    {
        // Read connection string from appsettings.json
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // Helper: creates a new database connection each time
    private SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<IEnumerable<Note>> GetAllAsync(int userId)
    {
        using var conn = GetConnection();

        var sql = @"
            SELECT *
            FROM Notes
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";

        return await conn.QueryAsync<Note>(sql, new { UserId = userId });
    }

    public async Task<Note?> GetByIdAsync(int id, int userId)
    {
        using var conn = GetConnection();

        var sql = @"
            SELECT *
            FROM Notes
            WHERE Id = @Id AND UserId = @UserId";

        return await conn.QueryFirstOrDefaultAsync<Note>(
            sql,
            new { Id = id, UserId = userId }
        );
    }

    public async Task<Note> CreateAsync(int userId, CreateNoteDto dto)
    {
        using var conn = GetConnection();

        var sql = @"
            INSERT INTO Notes (UserId, Title, Content)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Title, @Content)";

        var id = await conn.ExecuteScalarAsync<int>(
            sql,
            new
            {
                UserId = userId,
                dto.Title,
                dto.Content
            }
        );

        return (await GetByIdAsync(id, userId))!;
    }

    public async Task<bool> UpdateAsync(int id, int userId, UpdateNoteDto dto)
    {
        using var conn = GetConnection();

        var sql = @"
            UPDATE Notes
            SET Title = @Title,
                Content = @Content,
                UpdatedAt = GETDATE()
            WHERE Id = @Id AND UserId = @UserId";

        var rows = await conn.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                UserId = userId,
                dto.Title,
                dto.Content
            }
        );

        return rows > 0; // true if any row was updated
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        using var conn = GetConnection();

        var sql = @"
            DELETE FROM Notes
            WHERE Id = @Id AND UserId = @UserId";

        var rows = await conn.ExecuteAsync(
            sql,
            new { Id = id, UserId = userId }
        );

        return rows > 0;
    }
}