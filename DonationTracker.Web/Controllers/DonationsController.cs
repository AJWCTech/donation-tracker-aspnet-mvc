using DonationTracker.Web.Models;
using DonationTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DonationTracker.Web.Controllers;

public class DonationsController : Controller
{
    private readonly IDonationService _service;

    public DonationsController(IDonationService service)
    {
        _service = service;
    }

    // GET: /Donations
    public async Task<IActionResult> Index()
    {
        DonationListViewModel model = new DonationListViewModel
        {
            Donations = await _service.GetDonationsAsync(),
            CampaignTotal = await _service.GetCampaignTotalAsync()
        };

        return View(model);
    }

    // GET: /Donations/Create
    public async Task<IActionResult> Create()
    {
        await PopulateSupportersAsync();
        return View(new Donation { DonatedOn = DateTime.Today });
    }

    // POST: /Donations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Donation donation)
    {
        await _service.CreateDonationAsync(donation);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Donations/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        Donation? donation = await _service.GetDonationAsync(id);
        if (donation == null)
        {
            return NotFound();
        }

        await PopulateSupportersAsync();
        return View(donation);
    }

    // POST: /Donations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Donation donation)
    {
        if (id != donation.Id)
        {
            return NotFound();
        }

        bool updated = await _service.UpdateDonationAsync(donation);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // Fills the supporter dropdown used by the Create and Edit views.
    private async Task PopulateSupportersAsync()
    {
        List<Supporter> supporters = await _service.GetSupportersAsync();
        ViewBag.Supporters = new SelectList(supporters, "Id", "FullName");
    }
}
