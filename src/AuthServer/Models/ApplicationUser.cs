using System.ComponentModel.DataAnnotations;

public class ApplicationUser
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Primary login identifier (normalized phone/email)
    public string NormalizedIdentifier { get; set; } = null!;

    // Raw display name (not PHI heavy)
    public string? DisplayName { get; set; }

    // JSON: auth methods, consents etc. Expand later
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }
}
