using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Configuration;
using Models.DTO;
using DbModels;
using Microsoft.Extensions.Hosting.Internal;
using DbContext.Extensions;

namespace DbContext;

//DbContext namespace is a fundamental EFC layer of the database context and is
//used for all Database connection as well as for EFC CodeFirst migration and database updates 
public class MainDbContext : Microsoft.EntityFrameworkCore.DbContext
{
#if DEBUG
    // remove password from connection string in debug mode
    // this is useful for debugging and logging purposes, but should not be used in production code
    public string dbConnection => System.Text.RegularExpressions.Regex.Replace(
        this.Database.GetConnectionString() ?? "", @"(pwd|password)=[^;]*;?", "",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
#endif

    #region C# model of database tables
    public DbSet<FriendDbM> Friends { get; set; }
    public DbSet<AddressDbM> Addresses { get; set; }
    public DbSet<PetDbM> Pets { get; set; }
    public DbSet<QuoteDbM> Quotes { get; set; }    
    public DbSet<UserDbM> Users { get; set; }
    #endregion

    #region constructors
    public MainDbContext() { }
    public MainDbContext(DbContextOptions options) : base(options)
    { }
    #endregion

    #region model the Views
    public DbSet<GstUsrInfoDbDto> InfoDbView { get; set; }
    public DbSet<GstUsrInfoFriendsDto> InfoFriendsView { get; set; }
    public DbSet<GstUsrInfoPetsDto> InfoPetsView { get; set; }
    public DbSet<GstUsrInfoQuotesDto> InfoQuotesView { get; set; }
    #endregion

    //Here we can modify the migration building
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        #region model the Views
        modelBuilder.Entity<GstUsrInfoDbDto>().ToView("vwInfoDb", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoFriendsDto>().ToView("vwInfoFriends", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoPetsDto>().ToView("vwInfoPets", "gstusr").HasNoKey();
        modelBuilder.Entity<GstUsrInfoQuotesDto>().ToView("vwInfoQuotes", "gstusr").HasNoKey();        
        #endregion

        #region override modelbuilder
        modelBuilder.Entity("DbModels.FriendDbM", b =>
        {
            b.HasOne("DbModels.AddressDbM", "AddressDbM")
                .WithMany("FriendsDbM")
                .HasForeignKey("AddressId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("AddressDbM");
        });
        #endregion

        // Users table mapping
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

        
        base.OnModelCreating(modelBuilder);
    }

    #region DbContext for some popular databases
    public class SqlServerDbContext : MainDbContext
    {
        public SqlServerDbContext() { }
        public SqlServerDbContext(DbContextOptions options) 
            : base(options) { }


        //Used only for CodeFirst Database Migration and database update commands
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) => options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure()));
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
            //Add your own modelling based on done migrations
            base.OnModelCreating(modelBuilder);
        }
    }

    public class MySqlDbContext : MainDbContext
    {
        public MySqlDbContext() { }
        public MySqlDbContext(DbContextOptions options) : base(options) { }


        //Used only for CodeFirst Database Migration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                            b => b.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}")));
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
        public PostgresDbContext(DbContextOptions options) : base(options){ }


        //Used only for CodeFirst Database Migration
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
