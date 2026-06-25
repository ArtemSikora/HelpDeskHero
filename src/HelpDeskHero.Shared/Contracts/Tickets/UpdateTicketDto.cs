using System.ComponentModel.DataAnnotations;

namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class UpdateTicketDto
{
    [Required]
    [StringLength(200)]
    public string Title
    {
        get;
        set;
    } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description
    {
        get;
        set;
    } = string.Empty;

    [Required]
    [RegularExpression(
        "New|Closed")]
    public string Status
    {
        get;
        set;
    } = "New";

    [Required]
    [RegularExpression(
        "Low|Medium|High")]
    public string Priority
    {
        get;
        set;
    } = "Medium";

    [Required]
    public string RowVersionBase64 { get; set; } =
        string.Empty;
}
