using Microsoft.EntityFrameworkCore;

public interface IUserStore
{
    Task<ApplicationUser?> FindByNormalizedIdentifierAsync(string normalized);
    Task<ApplicationUser?> FindByIdAsync(Guid id);
    Task<ApplicationUser> CreateAsync(string normalizedIdentifier);
}

public class EfUserStore : IUserStore
{
    private readonly ApplicationDbContext _db;
    public EfUserStore(ApplicationDbContext db) => _db = db;

    public async Task<ApplicationUser?> FindByNormalizedIdentifierAsync(string normalized)
        => await _db.Users.FirstOrDefaultAsync(u => u.NormalizedIdentifier == normalized);

    public async Task<ApplicationUser?> FindByIdAsync(Guid id)
        => await _db.Users.FindAsync(id);

    public async Task<ApplicationUser> CreateAsync(string normalizedIdentifier)
    {
        var user = new ApplicationUser
        {
            NormalizedIdentifier = normalizedIdentifier,
            DisplayName = null
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
