using System.Reflection;
using DotBot.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AspNet.Security.OAuth.Vkontakte;
using DotBot.Controllers;
using DotBot.DLA;
using DotBot.Models;
using Microsoft.CodeAnalysis;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using User = DotBot.Models.User;

namespace DotBot.Controllers


{
    public class CharController : Controller
    {
        private readonly IConfiguration _configuration;
        private DbRepository db = new DbRepository();
        private VkApi api = new VkApi();
        public CharController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		// GET: CharController

		//[Authorize]
		public IActionResult Index()
        {
	        if (!api.IsAuthorized)
	        {
		        api.Authorize(new ApiAuthParams
		        {
			        AccessToken = configuration["Config:AccessToken"]
		        });
	        }
			charModel cm = new charModel();
            GameStat gs;
            User user;
            Armor armor;
            Weapon weapon;
            int id = 346320821;
            //int id = Convert.ToInt32(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            try
            {
                gs = db.GameStatRepository.GetByID(id);
                user = db.UserRepository.GetByID(id);
                armor = db.ArmorRepository.GetByID(gs.armorId);
                weapon = db.WeaponRepository.GetByID(gs.weaponId);
            }
            catch
            {
                cm.isEmpty = true;
                return View(cm);
            }
            var vkInfo = api.Users.Get(new long[] { id }, VkNet.Enums.Filters.ProfileFields.PhotoMax).FirstOrDefault();

            cm.isEmpty = false;
            cm.name = user.Nickname;
            cm.avatar = vkInfo.PhotoMax;

            cm.lvl = gs.lvl;
            cm.exp = gs.exp;
            cm.expToUp = gs.expToUp;
            cm.hp = gs.hp;
            cm.maxHp = gs.maxHp;
            cm.power = gs.power;
            cm.weaponPower = weapon.damage;
            cm.defence = gs.defence;
            cm.armorPower = armor.protect;
            cm.lvlPoints = gs.lvlPoints;
            cm.money = gs.money;


            return View(cm);
        }

        


    }
    public class charModel
    {
        public bool isEmpty { get; set; }
        public string name { get; set; }
        public Uri avatar { get; set; }
        public int lvl { get; set; }
        public int exp { get; set; }
        public int expToUp { get; set; }
        public int hp { get; set; }
        public int maxHp { get; set; }
        public int power { get; set; }
        public int weaponPower { get; set; }
        public int defence { get; set; }
        public int armorPower { get; set; }
        public int lvlPoints { get; set; }
        public int money { get; set;}
        public int kills { get;}

    }

}
