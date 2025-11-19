using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repository.Entity;
using Repository.Interface;

namespace Repository.Data;

public class ApplicationDbContext :IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly ICurrentUserService currentUserService;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) :base(options)
    {
        this.currentUserService = currentUserService;
    }
    public DbSet<UserNumericIdentity> AspNetUserNumericIdentity { get; set; }
    public DbSet<EntityType> EntityType { get; set; }
    public DbSet<EntityRoleMapping> EntittyRoleMapping { get; set; }
    public DbSet<MenuMaster> MenuMaster { get; set; }
    public DbSet<MenuMapping> MenuMapping { get; set; }
    public DbSet<EventLogger> EventLogger { get; set; }
    public DbSet<ErrorLogger> ErrorLogger { get; set; }

 

    public DbSet<logindetails> logindetails { get; set; }

    public DbSet<departments> departments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ensure unique constraint on RoleName and DepartmentId
        builder.Entity<ApplicationRole>()
            .HasIndex(r => new { r.Name, r.DepartmentId })
            .IsUnique();
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (EntityEntry<BaseAuditableEntity> entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserService.UserId;
                    entry.Entity.Created = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = currentUserService.UserId;
                    entry.Entity.LastModified = DateTime.UtcNow;
                    break;
            }
        }
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
