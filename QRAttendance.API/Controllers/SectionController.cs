using Microsoft.AspNetCore.Mvc;
using QRAttendance.API.Repositories;

namespace QRAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionsController : ControllerBase
    {
        private readonly UserRepository _repo;

        public SectionsController(UserRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _repo.GetSectionsAsync();
            return Ok(sections);
        }
    }
}