using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Repositories;
using backend.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")] // api/notes
[Authorize] // Endpoints require authentication login
public class NotesController : ControllerBase
{
    private readonly NoteRepository _repository;

    public NotesController(NoteRepository repo)
    {
        _repository = repo; // Get current logged-in user's ID from JWT token claims
    }

}