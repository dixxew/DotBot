using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotBot.Models;
using DotBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model;
using Microsoft.Data.SqlClient;
using System.Data;
using User = DotBot.Models.User;

namespace DotBot.Controllers
{
    public class RatingController : Controller
    {

        private readonly IConfiguration _configuration;
        VkApi api = new VkApi();
        private readonly vkContext db;
       
        public RatingController(IConfiguration configuration, vkContext context)
        {
            _configuration = configuration;
            db = context;
            if (!api.IsAuthorized)
            {
                api.Authorize(new ApiAuthParams
                {
                    AccessToken = _configuration["Config:AccessToken"]
                });
            }
        }

        public IActionResult Index()
        {
         List<GameStat> fromGsData;
        List<Rating> usersModel = new List<Rating>();
        Rating item = new Rating();
         List<Weapon> weapDat;
         List<Armor> armDat;
         List<User> fromUsersData;
        weapDat = db.Weapons.ToList();
            armDat = db.Armors.ToList();
            fromGsData = db.GameStats.OrderByDescending(x => x.exp).Take(50).ToList();
            int[] ids = fromGsData.Select(x => x.Id).ToArray();
            fromUsersData = db.Users.Where(z => ids.Contains(z.Id)).ToList();
            foreach (int id in ids)
            {
                var gsItem = fromGsData.FirstOrDefault(x => x.Id == id);
                item = new Rating();
                item.itemStrength = armDat.FirstOrDefault(x => x.id == gsItem.armorId).protect;
                item.itemStrength += weapDat.FirstOrDefault(x => x.id == gsItem.weaponId).damage;
                if (fromUsersData.Find(x => x.Id == gsItem.Id).Marry > 0)
                {
                    item.IsMarriage = true;
                }
                item.Name = fromUsersData.FirstOrDefault(x => x.Id == gsItem.Id).Nickname;
                item.photoUrl = api.Users.Get(new long[] { gsItem.Id }, VkNet.Enums.Filters.ProfileFields.Photo50).FirstOrDefault().Photo50;
                item.Lvl = gsItem.lvl;
                item.Id = gsItem.Id;
                item.Strength = gsItem.defence + gsItem.power;
                item.profileUrl = "https://vk.com/id" + gsItem.Id;
                usersModel.Add(item);
            }
            return View(usersModel);
        }

        

    }

    public class Rating
    {
        public int Id { get; set; }
        public string profileUrl { get; set; }
        public Uri photoUrl { get; set; }
        public string Name { get; set; }
        public int Lvl { get; set; }
        public bool IsMarriage { get; set; } = false;
        public int Strength { get; set; }
        public int itemStrength { get; set; }
    }

    
}
