using IdentityService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Dapper ile kaydedeceğimiz bir AuditLog tablosu için DbSet (isteğe bağlı, EF Core tarafından da yönetilebilir)
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fluent API ile Identity tabloları için tablo adlarını belirleyelim (PostgreSQL uyumluluğu için)
            builder.HasDefaultSchema("public"); // PostgreSQL'de varsayılan şema 'public'tir

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users"); // Varsayılan AspNetUsers yerine "Users"
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles"); // Varsayılan AspNetRoles yerine "Roles"
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            // AuditLog tablosu için konfigürasyon (EF Core ile de kullanılabilir)
            builder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs"); // Tablo adını belirle
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Username).HasMaxLength(256).IsRequired(false); // Nullable olabilir
                entity.Property(e => e.Details).IsRequired(false); // Nullable olabilir
            });
        }
    }
}