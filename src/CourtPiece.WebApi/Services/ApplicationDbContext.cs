using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
    {

    }

    //public DbSet<PlayerState> PlayerStates { get; set; }


    //public DbSet<RoomState> Rooms { get; set; }
    public DbSet<GrainState> GrainStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<PlayerState>().Property(i => i.Cards).HasConversion(i => JsonConvert.SerializeObject(i), i => JsonConvert.DeserializeObject<int[]>(i));
        //modelBuilder.Entity<PlayerState>().OwnsOne(post => post.Cards, builder => { builder.ToJson(); });
        //modelBuilder.Entity<RoomState>().OwnsOne(post => post.PlayerIds, builder => { builder.ToJson(); });
        //modelBuilder.Entity<RoomState>().Property(i => i.PlayerIds).HasConversion(i => JsonConvert.SerializeObject(i), i => JsonConvert.DeserializeObject<List<Guid>>(i));

        base.OnModelCreating(modelBuilder);
    }  

}
