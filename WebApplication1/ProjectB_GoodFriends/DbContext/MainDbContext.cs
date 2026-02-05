using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Configuration;
using Models.DTO;
using DbModels;
using Microsoft.Extensions.Hosting.Internal;
using DbContext.Extensions;

namespace DbContext;

public class MainDbContext : Microsoft.EntityFrameworkCore.DbContext
{
#if DEBUG
    public string dbConnection => System.Text.RegularExpressions.Regex.Replace(
        this.Database.GetConnectionString() ?? "", @"(pwd|password)=[^;]*;?",
        "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
#endif

    #region C# model of database tables
    public DbSet<FriendDbM> Friends { get; set; } = null!;
    public DbSet<AddressDbM> Addresses { get; set; } = null!;
    public DbSet<PetDbM> Pets { get; set; } = null!;
    public DbSet<QuoteDbM> Quotes { get; set; } = null!;
    public DbSet<UserDbM> Users { get; set; } = null!;
    #endregion

    #region constructors
    public MainDbContext() { }
    public MainDbContext(DbContextOptions options) : base(options) { }
    #endregion

    #region model the Views
    public DbSet<GstUsrInfoDbDto> InfoDbView { get; set; } = null!;
    public DbSet<GstUsrInfoFriendsDto> InfoFriendsView { get; set; } = null!;
    public DbSet<GstUsrInfoPetsDto> InfoPetsView { get; set; } = null!;
    public DbSet<GstUsrInfoQuotesDto> InfoQuotesView { get; set; } = null!;
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region model the Views
        modelBuilder.Entity<GstUsrInfoDbDto>().ToView("vwInfoDb", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoFriendsDto>().ToView("vwInfoFriends", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoPetsDto>().ToView("vwInfoPets", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoQuotesDto>().ToView("vwInfoQuotes", "gstusr").HasNoKey();
        #endregion

        #region prevent EF from mapping interface-based properties from base models
        // FriendDbM inherits csFriend which exposes interface-based props.
        // EF must only map DbModel navigations: AddressDbM, PetsDbM, QuotesDbM
        modelBuilder.Entity<FriendDbM>().Ignore(x => x.Pets);
        modelBuilder.Entity<FriendDbM>().Ignore(x => x.Quotes);
        modelBuilder.Entity<FriendDbM>().Ignore(x => x.Address);
        #endregion

        #region relationships

        // FriendDbM -> AddressDbM (many friends can share one address)
        modelBuilder.Entity<FriendDbM>(b =>
        {
            b.HasOne(f => f.AddressDbM)
             .WithMany(a => a.FriendsDbM)
             .HasForeignKey(f => f.AddressId)
             .OnDelete(DeleteBehavior.SetNull);

            b.Property(f => f.AddressId).IsRequired(false);
        });

        // FriendDbM -> PetDbM (one friend, many pets) - cascade delete
        modelBuilder.Entity<PetDbM>(b =>
        {
            b.HasOne(p => p.FriendDbM)
             .WithMany(f => f.PetsDbM)
             .HasForeignKey(p => p.FriendId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(p => p.FriendId).IsRequired();
        });

        modelBuilder.Entity<FriendDbM>()
            .HasMany(f => f.QuotesDbM)
            .WithMany(q => q.FriendsDbM)
            .UsingEntity<Dictionary<string, object>>(
                "FriendDbMQuoteDbM",
                j => j.HasOne<QuoteDbM>()
                    .WithMany()
                    .HasForeignKey("QuotesDbMQuoteId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<FriendDbM>()
                    .WithMany()
                    .HasForeignKey("FriendsDbMFriendId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("FriendsDbMFriendId", "QuotesDbMQuoteId");
                    j.ToTable("FriendDbMQuoteDbM", "supusr");
                }
            );




        #endregion

        #region Users table mapping
        modelBuilder.Entity<UserDbM>(b =>
        {
            b.ToTable("Users", "supusr");
            b.HasKey(x => x.UserId);

            b.Property(x => x.UserName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).IsRequired().HasMaxLength(200);
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Role).IsRequired().HasMaxLength(50);

            b.HasIndex(x => x.UserName).IsUnique();
            b.HasIndex(x => x.Email).IsUnique();
        });
        #endregion

        base.OnModelCreating(modelBuilder);
    }

    #region DbContext for some popular databases
    public class SqlServerDbContext : MainDbContext
    {
        public SqlServerDbContext() { }
        public SqlServerDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) =>
                        options.UseSqlServer(connectionString, o => o.EnableRetryOnFailure()));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HaveColumnType("money");
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class MySqlDbContext : MainDbContext
    {
        public MySqlDbContext() { }
        public MySqlDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                            b => b.SchemaBehavior(
                                Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Translate,
                                (schema, table) => $"{schema}_{table}")));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");
            base.ConfigureConventions(configurationBuilder);
        }
    }

    public class PostgresDbContext : MainDbContext
    {
        public PostgresDbContext() { }
        public PostgresDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) => options.UseNpgsql(connectionString));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");
            base.ConfigureConventions(configurationBuilder);
        }
    }
    #endregion
}
