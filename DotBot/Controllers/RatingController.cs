using System.Collections;
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
using DotBot.DAL;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using Object = System.Object;
using User = DotBot.Models.User;

namespace DotBot.Controllers
{
    public class RatingController : Controller
    {

        private DbRepository db = new DbRepository();       
        public RatingController(IConfiguration configuration)
        {
           
        }

        public IActionResult Index()
        {
         
            return View();
        }
        [HttpGet]
        [EnableCors]
        public Object data()
        {
            List<Rating>  data = new List<Rating>();
            List<GameStat> dataSet = db.GameStatRepository.Get(null,null,"User,Weapon,Armor").OrderByDescending(x => x.Exp).Take(25).ToList();
            int count = dataSet.Count;
            foreach (var item in dataSet)
            {
                Rating rating = new Rating();
                bool isMarry = item.User.Marry > 0;
                rating.Id = item.Id;
                rating.IsMarriage = isMarry;
                rating.Lvl = item.Level;
                rating.Name = item.User.Name;
                rating.Strength = item.Power = item.Defence;
                rating.itemStrength = item.Weapon.damage + item.Armor.protect;
                data.Add(rating);
            }
            return JsonConvert.SerializeObject(data);
        }

    }

    public class Rating
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Lvl { get; set; }
        public bool IsMarriage { get; set; } = false;
        public int Strength { get; set; }
        public int itemStrength { get; set; }
    }

    
}
