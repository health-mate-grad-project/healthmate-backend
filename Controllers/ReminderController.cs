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
    public async Task<IActionResult> GetToday(
        int patientId,
        [FromQuery(Name = "date")] string date)
    {
        // parse YYYY-MM-DD or default to today
        DateTime dayUtc;
        if (!string.IsNullOrEmpty(date)
            && DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            dayUtc = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }
        else
        {
            dayUtc = DateTime.UtcNow.Date;
        }

        var items = await _svc.GetTodayAsync(patientId, dayUtc);
        return Ok(items);
    }

    // POST …/reminders/{reminderId}/dose/check?scheduled=2025-06-10T08:00:00Z
    [HttpPost("{reminderId:int}/dose/check")]
    public async Task<IActionResult> CheckDose(
        int patientId,
        int reminderId,
        [FromQuery(Name = "doseId")] int doseId  // ✅ use doseId instead of scheduledUtc
    )
    {
        // ❗ Although reminderId is in the route, we use doseId for lookup
        var ok = await _svc.MarkDoseTakenAsync(doseId, patientId);  // ✅ pass only doseId and patientId
        return ok ? Ok() : NotFound();
    }

  
    
    // Returns every scheduled dose for today + tomorrow by default.
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(
        int patientId,
        [FromQuery(Name = "date")] string date,
        [FromQuery(Name = "days")] int days = 2)
    {
        // parse the start date or default to UTC today
        DateTime startUtc;
        if (!string.IsNullOrEmpty(date) &&
            DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            startUtc = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }
        else
        {
            startUtc = DateTime.UtcNow.Date;
        }

        var list = await _svc.GetUpcomingAsync(patientId, startUtc, days);
        return Ok(list);
    }

}