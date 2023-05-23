using DotBot.DAL;
using DotBot.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using Object = System.Object;

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
