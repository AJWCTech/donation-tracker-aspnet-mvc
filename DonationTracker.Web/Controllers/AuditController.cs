using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationTracker.Web.Controllers;

// The audit trail is for staff only.
[Authorize]
public class AuditController : Controller
{
    private readonly IDonationService _service;

    public AuditController(IDonationService service)
    {
        _service = service;
    }

    // GET: /Audit
    public async Task<IActionResult> Index()
    {
        List<AuditEntry> entries = await _service.GetRecentAuditEntriesAsync();
        return View(entries);
    }
}
