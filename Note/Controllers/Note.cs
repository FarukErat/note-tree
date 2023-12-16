using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Workout.Controllers;
using Workout.Notes.Interfaces;
using Workout.Notes.Models;
using WorkoutProject.Helpers;

namespace Workout.Notes.Controllers;

[SessionAuth]
public class NoteController : ApiController
{
    private readonly INoteService _noteService;
    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpPost("save-notes")]
    public async Task<IActionResult> SaveNotesAsync([FromBody] List<Note> notes)
    {
        ErrorOr<Success> saveNotesResult = await _noteService.SaveNotesAsync(notes, HttpContext);

        return saveNotesResult.Match(
            result => Ok(new { message = "Notes saved!" }),
            errors => Problem(errors)
        );
    }

    [HttpGet("get-notes")]
    public async Task<IActionResult> GetNotesAsync()
    {
        ErrorOr<List<Note>> getNotesResult = await _noteService.GetNotesAsync(HttpContext);

        return getNotesResult.Match(
            result => Ok(result),
            errors => Problem(errors)
        );
    }
}
