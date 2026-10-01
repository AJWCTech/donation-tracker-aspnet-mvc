using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DonationTracker.Web.Controllers;

[ApiController]
[Route("api/donations")]
public class DonationsApiController : ControllerBase
{
    private readonly IDonationService _service;

    public DonationsApiController(IDonationService service)
    {
        _service = service;
    }

    // GET: /api/donations?page=1&pageSize=25
    [HttpGet]
    public async Task<ActionResult<DonationPageDto>> GetDonations(int page = 1, int pageSize = 25)
    {
        DonationPageDto donations = await _service.GetDonationPageDtoAsync(page, pageSize);
        return Ok(donations);
    }
}
