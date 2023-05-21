using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DotBot.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Marry { get; set; }
        public int MarryageRequest { get; set; }

    }
    public class GameStat 
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int ExpToUp { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Power { get; set; }
        public int Defence { get; set; }
        public int Kills { get; set; }
        public int DamageSum { get; set; }
        public int LevelPoints { get; set; }
        public bool IsHealing { get; set; }
        public int Money { get; set; }
        public Armor Armor { get; set; }
        public Weapon Weapon { get; set; }

    }

    public class Weapon
    {
        public int id { get; set; }
        public string name { get; set; }
        public int damage { get; set; }
        public int cost { get; set; }
    }

    public class Armor
    {
        public int id { get; set; }
        public string name { get; set; }
        public int protect { get; set; }
        public int cost { get; set; }
    }


}

