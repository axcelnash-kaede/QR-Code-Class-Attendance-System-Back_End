using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QRAttendance.API.DTOs;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardRepository _repo;

    public DashboardController(DashboardRepository repo)
    {
        _repo = repo;
    }

    // ================= STUDENT DASHBOARD =================
    [Authorize(Roles = "Student")]
    [HttpGet("student")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard()
    {
        var studentId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var result = await _repo.GetStudentDashboard(studentId);

        return Ok(result);
    }

    // ================= TEACHER DASHBOARD =================
    [Authorize(Roles = "Teacher")]
    [HttpGet("teacher")]
    public async Task<ActionResult<TeacherDashboardDto>> GetTeacherDashboard()
    {
        var result = await _repo.GetTeacherDashboard();
        return Ok(result);
    }
}