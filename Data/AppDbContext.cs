using JobPortalCORE.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobPortalCORE.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<JobProfile> JobProfile { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        // Ye method Entity Framework ka default Save method hai, hum ise override kar rahe hain
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        // 💡 NAYA CODE: Ye method Normal (Sync) save ke liye hai. Ise add kar le!
        public override int SaveChanges()
        {
            // Save hone se pehle check karo kya changes huye hain
            OnBeforeSaveChanges();

            // Ab asli save hone do
            return base.SaveChanges();
        }

        private void OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();

            // 💡 NAYA LOGIC (VIP LIST): Yahan un tables ke exact naam likho jinhe track karna hai.
            // Dhyan rakhna, ye C# ke Model class ka naam hona chahiye (jaise "Employee", "JobPost", "ApplicationUser")
            // "Employees" ko "Employee" aur "JobPosts" ko "JobPost" kar do
            var auditableTables = new List<string> { "Employee", "JobPost", "JobProfile" };

            foreach (var entry in ChangeTracker.Entries())
            {
                // Agar data mein koi change nahi hua, toh ignore karo
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var tableName = entry.Entity.GetType().Name; // Class ka naam nikal rahe hain

                // 💡 NAYA LOGIC: Agar table ka naam VIP list mein NAHI hai, ya ye AuditLog khud hai, toh aage badh jao (skip karo)
                if (!auditableTables.Contains(tableName) || entry.Entity is AuditLog)
                    continue;

                var auditEntry = new AuditLog
                {
                    TableName = tableName,
                    Action = entry.State.ToString(), // Inserted, Modified, ya Deleted
                    Timestamp = DateTime.UtcNow,
                    UserId = "System" // Baad mein ise logged-in user se replace karenge
                };

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    if (entry.State == EntityState.Added)
                    {
                        newValues[propertyName] = property.CurrentValue;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        oldValues[propertyName] = property.OriginalValue;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                    }
                }

                auditEntry.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
                auditEntry.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);

                auditEntries.Add(auditEntry);
            }

            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry);
            }
        }

        // Ye Entity Framework ka ek special method hai database ka design (schema) customize karne ke liye
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tables ke liye base method call karna zaroori hai
            base.OnModelCreating(builder);

            // 💡 NAYA CODE: AuditLog table ke TableName column par Index bana rahe hain
            builder.Entity<AuditLog>()
                .HasIndex(a => a.TableName);

            // Hum Timestamp par bhi index laga rahe hain, kyunki logs date ke hisaab se bohot search hote hain
            builder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);
        }
    }
}
