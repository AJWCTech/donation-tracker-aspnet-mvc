using System.ComponentModel.DataAnnotations;

namespace DonationTracker.Web.Models;

// One row for every add or edit: who did it, when, and what changed.
// Rows are only ever inserted; the app has no way to edit or delete them.
public class AuditEntry
{
    public int Id { get; set; }

    // Stored in UTC.
    public DateTime ChangedOn { get; set; }

    [Required]
    [StringLength(256)]
    public string ChangedBy { get; set; } = string.Empty;

    // "Created" or "Updated".
    [Required]
    [StringLength(20)]
    public string Action { get; set; } = string.Empty;

    // "Supporter" or "Donation".
    [Required]
    [StringLength(50)]
    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Summary { get; set; } = string.Empty;
}
