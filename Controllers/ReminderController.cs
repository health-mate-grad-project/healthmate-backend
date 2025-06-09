using healthmate_backend.Models;

namespace healthmate_backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using healthmate_backend.Services;
using System.Globalization;

[ApiController]
[Route("api/patient/{patientId:int}/reminders")]
[Authorize(Roles = "patient,Doctor")]
public class ReminderController : ControllerBase
{
    private readonly ReminderService _svc;
    public ReminderController(ReminderService svc) => _svc = svc;

    // GET …/reminders/today
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(int patientId)
    {
        var items = await _svc.GetTodayAsync(patientId, DateTime.UtcNow);
        return Ok(items);
    }

    // POST …/reminders/{reminderId}/dose/check?scheduled=2025-06-10T08:00:00Z
    [HttpPost("{reminderId:int}/dose/check")]
    public async Task<IActionResult> CheckDose(
        int patientId,
        int reminderId,
        [FromQuery(Name = "scheduledUtc")] string scheduledUtc     // ← match our Flutter
    )
    {
        if (!DateTime.TryParse(
                scheduledUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var schedUtc))
            return BadRequest("Invalid scheduledUtc param");

        var ok = await _svc.MarkDoseTakenAsync(reminderId, schedUtc, patientId);
        return ok ? Ok() : NotFound();
    }
  

}