using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Models;
using backend.Repositories;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly NoteRepository _repo;

    public NotesController(NoteRepository repo)
    {
        _repo = repo;
    }

    private int Uid()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string sort = "newest")
    {
        var notes = await _repo.GetAllAsync(Uid(), search, sort);
        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var note = await _repo.GetByIdAsync(id, Uid());
        return note == null ? NotFound() : Ok(note);
    }

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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNoteDto dto)
    {
        var success = await _repo.UpdateAsync(id, Uid(), dto);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id, Uid());
        return success ? Ok() : NotFound();
    }
}