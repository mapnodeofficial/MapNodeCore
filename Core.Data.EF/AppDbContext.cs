
using BeCoreApp.Data.Entities;
using Core.Data.EF.Configurations;
using Core.Data.EF.Extensions;
using Core.Data.Entities;
using Core.Data.Interfaces;
using Core.Data.SpEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Core.Data.EF
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Language> Languages { set; get; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<MenuGroup> MenuGroups { set; get; }
        public DbSet<MenuItem> MenuItems { set; get; }
        public DbSet<BlogCategory> BlogCategories { set; get; }
        public DbSet<Blog> Blogs { set; get; }
        public DbSet<BlogTag> BlogTags { set; get; }
        public DbSet<Feedback> Feedbacks { set; get; }
        public DbSet<Tag> Tags { set; get; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<TicketTransaction> TicketTransactions { get; set; }
        public DbSet<SavingDefi> SavingDefis { get; set; }

        public DbSet<SaleDefi> SaleDefis { get; set; }
        public DbSet<Support> Supports { get; set; }
        public DbSet<Notify> Notifies { get; set; }
        public DbSet<WalletTransfer> WalletTransfers { get; set; }

        public DbSet<QueueTask> QueueTasks { get; set; }

        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public DbSet<Saving> Savings { get; set; }
        public DbSet<SavingReward> SavingRewards { get; set; }

        public DbSet<TokenConfig> TokenConfigs { get; set; }

        public DbSet<GoogleMapCategories> GoogleMapCategories { get; set; }

        public DbSet<GoogleMapGIS> GoogleMapGIS { get; set; }

        public DbSet<GoogleMapGISCategoryMappings> GoogleMapGISCategoryMappings { get; set; }

        public DbSet<GoogleApiLogs> GoogleApiLogs { get; set; }

        public DbSet<GoogleMapGISNearby> GoogleMapGISNearby { get; set; }

        public DbSet<AppUsersCupItemHistories> AppUsersCupItemHistories { get; set; }

        public DbSet<CupItems> CupItems { get; set; }

        public DbSet<MachineItems> MachineItems { get; set; }

        public DbSet<DrinkToEarnHistories> DrinkToEarnHistories { get; set; }

        public DbSet<DrinkAccessCode> DrinkAccessCodes { get; set; }

        public DbSet<SaleBlock> SaleBlocks { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<ShopItem> ShopItems { get; set; }

        public DbSet<Config> Configs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region Identity Config

            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims")
                .HasKey(x => x.Id);

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims")
                .HasKey(x => x.Id);

            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins")
                .HasKey(x => x.UserId);

            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles")
                .HasKey(x => new { x.UserId, x.RoleId,  });



            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens")
                .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            #endregion Identity Config

            builder.AddConfiguration(new TagConfiguration());
            builder.AddConfiguration(new BlogTagConfiguration());
            builder.AddConfiguration(new FunctionConfiguration());
            builder.AddConfiguration(new BlogConfiguration());
            builder.AddConfiguration(new BlogCategoryConfiguration());
            builder.AddConfiguration(new QueueTaskConfiguration());

            builder.Entity<SavingDefi>()
                .Property(p => p.USDAmount)
                .HasColumnType("decimal(18,4)");

            builder.Entity<SavingReward>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,4)");

            builder.Entity<Wallet>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,4)");

            builder.Entity<WalletTransaction>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");
            builder.Entity<WalletTransaction>()
                .Property(p => p.AmountReceive).HasColumnType("decimal(18,4)");

            builder.Entity<SaleBlock>()
                .Property(p => p.Amount).HasColumnType("decimal(18,4)");

            builder.Entity<SaleDefi>()
                .Property(p => p.USDAmount).HasColumnType("decimal(18,4)");
            builder.Entity<SaleDefi>()
                .Property(p => p.BNBAmount).HasColumnType("decimal(18,6)");

            //base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

            foreach (EntityEntry item in modified)
            {
                var changedOrAddedItem = item.Entity as IDateTracking;
                if (changedOrAddedItem != null)
                {
                    if (item.State == EntityState.Added)
                        changedOrAddedItem.DateCreated = DateTime.Now;

                    changedOrAddedItem.DateModified = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}
