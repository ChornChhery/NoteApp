using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Models;
using backend.Services;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // all endpoints in this controller require authentication
public class NotesController : ControllerBase
{
    private readonly NoteService _repo;

    public NotesController(NoteService repo)
    {
        _repo = repo;
    }

    // reads the logged-in user's id out of the JWT token
    private int Uid()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // GET: api/notes?search=keyword&sort=oldest
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string sort = "newest")
    {
        var notes = await _repo.GetAllAsync(Uid(), search, sort);
        return Ok(notes);
    }

    // GET: api/notes/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var note = await _repo.GetByIdAsync(id, Uid());
        return note is null ? NotFound() : Ok(note);
    }

    // POST: api/notes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto dto)
    {
        var note = await _repo.CreateAsync(Uid(), dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = note.Id },
            note
        );
    }

    // PUT: api/notes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNoteDto dto)
    {
        var success = await _repo.UpdateAsync(id, Uid(), dto);
        return success ? Ok() : NotFound();
    }

    // DELETE: api/notes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id, Uid());
        return success ? Ok() : NotFound();
    }
}