namespace backend.Models;

public class Note
{
    public int Id { get; set;}
    public int UserId { get; set;}
    public string Title { get; set; } = "";
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTOs (Data Transfer Objects) what API will receive and send
public class CreateNoteDto
{
    public string Title { get; set; } = "";
    public string? Content { get; set; }
}

public class UpdateNoteDto
{
    public string? Title { get; set; } = "";
    public string? Content { get; set; }
}