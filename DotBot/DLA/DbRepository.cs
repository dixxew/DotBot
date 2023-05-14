using DotBot.Data;
using DotBot.DLA;

using DotBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DotBot.DLA
{
    public class DbRepository: IDisposable
    {
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        DbContextOptionsBuilder<vkContext> optionsBuilder = new DbContextOptionsBuilder<vkContext>();
        private GenericRepository<User> userRepository;
        private GenericRepository<GameStat> gsRepository;
        private GenericRepository<Weapon> weaponRepository;
        private GenericRepository<Armor> armorRepository;

        private vkContext context;
        

        public DbRepository()
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            context = new vkContext(optionsBuilder.Options);
        }

        public GenericRepository<User> UserRepository
        {
            get
            {

                if (this.userRepository == null)
                {
                    this.userRepository = new GenericRepository<User>(context);
                }
                return userRepository;
            }
        }

        public GenericRepository<GameStat> GameStatRepository
        {
            get
            {

                if (this.gsRepository == null)
                {
                    this.gsRepository = new GenericRepository<GameStat>(context);
                }
                return gsRepository;
            }
        }

        public GenericRepository<Armor> ArmorRepository
        {
            get
            {

                if (this.armorRepository == null)
                {
                    this.armorRepository = new GenericRepository<Armor>(context);
                }
                return armorRepository;
            }
        }

        public GenericRepository<Weapon> WeaponRepository
        {
            get
            {

                if (this.weaponRepository == null)
                {
                    this.weaponRepository = new GenericRepository<Weapon>(context);
                }
                return weaponRepository;
            }
        }
        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
