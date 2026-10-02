using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationTracker.Web.Controllers;

// Every action needs a signed-in user unless it is marked [AllowAnonymous].
[Authorize]
public class SupportersController : Controller
{
    private readonly IDonationService _service;

    public SupportersController(IDonationService service)
    {
        _service = service;
    }

    // GET: /Supporters
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        List<Supporter> supporters = await _service.GetSupportersAsync();
        return View(supporters);
    }

    // GET: /Supporters/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        SupporterDetails? details = await _service.GetSupporterDetailsAsync(id);
        if (details == null)
        {
            return NotFound();
        }

        return View(details);
    }

    // GET: /Supporters/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Supporters/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supporter supporter)
    {
        if (!ModelState.IsValid)
        {
            return View(supporter);
        }

        await _service.CreateSupporterAsync(supporter);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Supporters/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        Supporter? supporter = await _service.GetSupporterAsync(id);
        if (supporter == null)
        {
            return NotFound();
        }

        return View(supporter);
    }

    // POST: /Supporters/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supporter supporter)
    {
        if (id != supporter.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(supporter);
        }

        bool updated = await _service.UpdateSupporterAsync(supporter);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
