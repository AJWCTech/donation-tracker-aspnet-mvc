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

    // GET: /api/donations
    [HttpGet]
    public async Task<ActionResult<List<DonationDto>>> GetDonations()
    {
        List<DonationDto> donations = await _service.GetRecentDonationDtosAsync();
        return Ok(donations);
    }
}
