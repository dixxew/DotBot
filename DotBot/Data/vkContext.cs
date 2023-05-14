using DotBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DotBot.Data
{
    public class vkContext : DbContext
    {
        public vkContext()
        {
        }

        public vkContext(DbContextOptions<vkContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GameStat> GameStats { get; set; }
        public DbSet<Armor> Armors { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
    }
}

