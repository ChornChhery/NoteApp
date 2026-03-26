using Dapper;
using MySqlConnector;
using backend.Models;

namespace backend.Repositories;

public class NoteRepository
{
    private readonly string _cs;

    public NoteRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("DefaultConnection")!;
    }

    private MySqlConnection Conn()
    {
        return new MySqlConnection(_cs);
    }

    public async Task<IEnumerable<Note>> GetAllAsync(int userId, string? search, string sort)
    {
        using var c = Conn();

        var sql = "SELECT * FROM Notes WHERE UserId=@UserId";

        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (Title LIKE @S OR Content LIKE @S)";
        }

        sql += sort switch
        {
            "oldest" => " ORDER BY CreatedAt ASC",
            "title"  => " ORDER BY Title ASC",
            _        => " ORDER BY CreatedAt DESC"
        };

        return await c.QueryAsync<Note>(
            sql,
            new
            {
                UserId = userId,
                S = $"%{search}%"
            }
        );
    }

    public async Task<Note?> GetByIdAsync(int id, int userId)
    {
        using var c = Conn();

        return await c.QueryFirstOrDefaultAsync<Note>(
            "SELECT * FROM Notes WHERE Id=@Id AND UserId=@UserId",
            new { Id = id, UserId = userId }
        );
    }

    public async Task<Note> CreateAsync(int userId, CreateNoteDto dto)
    {
        using var c = Conn();

        var sql = @"
            INSERT INTO Notes (UserId, Title, Content)
            VALUES (@UserId, @Title, @Content);
            SELECT LAST_INSERT_ID();";

        var id = await c.ExecuteScalarAsync<int>(
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
        using var c = Conn();

        var sql = @"
            UPDATE Notes
            SET Title=@Title,
                Content=@Content
            WHERE Id=@Id AND UserId=@UserId";

        var rows = await c.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                UserId = userId,
                dto.Title,
                dto.Content
            }
        );

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        using var c = Conn();

        var rows = await c.ExecuteAsync(
            "DELETE FROM Notes WHERE Id=@Id AND UserId=@UserId",
            new { Id = id, UserId = userId }
        );

        return rows > 0;
    }
}