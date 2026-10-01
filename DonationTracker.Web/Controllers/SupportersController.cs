using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DonationTracker.Web.Controllers;

public class SupportersController : Controller
{
    private readonly IDonationService _service;

    public SupportersController(IDonationService service)
    {
        _service = service;
    }

    // GET: /Supporters
    public async Task<IActionResult> Index()
    {
        List<Supporter> supporters = await _service.GetSupportersAsync();
        return View(supporters);
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

        bool updated = await _service.UpdateSupporterAsync(supporter);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
