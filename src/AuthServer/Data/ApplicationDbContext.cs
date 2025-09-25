using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Application users (minimal)
    public DbSet<ApplicationUser> Users { get; set; } = null!;

    // OpenIddict entities
    public DbSet<OpenIddictEntityFrameworkCoreApplication> OpenIddictApplications { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> OpenIddictAuthorizations { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreScope> OpenIddictScopes { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreToken> OpenIddictTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure application user
        builder.Entity<ApplicationUser>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.NormalizedIdentifier).IsUnique();
            b.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // OpenIddict default schema mappings
        builder.UseOpenIddict();
    }
}
