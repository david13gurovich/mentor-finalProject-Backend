using Domain;
using Domain.Priv;
using Microsoft.EntityFrameworkCore;


namespace Repository
{
    public class MentorDataContext : DbContext
    {
        public MentorDataContext(DbContextOptions<MentorDataContext> options)
            : base(options)
        {
        }

        public DbSet<User>? User { get; set; }

        public DbSet<Mission>? Mission { get; set; }

        public DbSet<ScheduleSetting>? ScheduleSetting { get; set; }

        public DbSet<MissionRank>? MissionRank { get; set; }

    }
}
