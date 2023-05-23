using DotBot.DAL;
using DotBot.Models;
using DotBot.Services.Vk;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;
using Object = System.Object;
using User = DotBot.Models.User;

namespace DotBot.Controllers
{
    public class CharController : Controller
    {
        private DbRepository db = new DbRepository();
        private VkApi api = new VkApi();
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public CharController()
        {
            
        }

        // GET: CharController

        [HttpGet]
        [EnableCors]
        //[Authorize]
        public Object data([FromQuery] int id)
        {
            if (!api.IsAuthorized)
            {
                api.Authorize(new ApiAuthParams
                {
                    AccessToken = configuration["Config:AccessToken"]
                });
            }

            GameStat gs;
            try
            {
                gs = db.GameStatRepository.Get(x => x.Id == id, null, "User,Armor,Weapon").First();

            }
            catch
            {
                ValidationService vs = new ValidationService();
                vs.CreateUser(id);
                gs = db.GameStatRepository.Get(x => x.Id == id, null, "User,Armor,Weapon").First();
            }

            Char data = new Char();

            data.Name = gs.User.Nickname;
            data.AvatarUrl = api.Users.Get(new long[] { id }, VkNet.Enums.Filters.ProfileFields.PhotoMax).FirstOrDefault().PhotoMax;

            data.Level = gs.Level.ToString();
            data.LevelPoints = gs.LevelPoints.ToString();

            data.Exp = gs.Exp.ToString();
            data.MaxExp = gs.ExpToUp.ToString();

            data.Kills = gs.Kills.ToString();
            data.Money = gs.Money.ToString();

            data.Defence = gs.Defence.ToString();
            data.Power = gs.Power.ToString();

            data.ArmorDefence = gs.Armor.protect.ToString();
            data.WeaponPower = gs.Weapon.damage.ToString();

            return JsonConvert.SerializeObject(data);
        }
    }

    public class Char
    {
        public string Name { get; set; }
        public Uri AvatarUrl { get; set; }
        public string Level { get; set; }
        public string Kills { get; set; }
        public string Exp { get; set; }
        public string MaxExp { get; set; }
        public string LevelPoints { get; set; }
        public string Power { get; set; }
        public string WeaponPower { get; set; }
        public string Defence { get; set; }
        public string ArmorDefence { get; set;}
        public string Money { get; set; }

    }
}


