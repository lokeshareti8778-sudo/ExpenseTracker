using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Amount { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
