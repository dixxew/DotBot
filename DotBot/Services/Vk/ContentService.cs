using DotBot.DAL;
using DotBot.Models;
using System.Text;
using VkNet;
using VkNet.Model;
using WeatherAPI.NET;
using WeatherAPI.NET.Entities;
using Message = DotBot.Models.Message;
using User = DotBot.Models.User;

namespace DotBot.Services.Vk
{
    public class ContentService
    {
        VkApi api = new VkApi();
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private DbRepository db = new DbRepository();


        public ContentService()
        {
        }



        #region service Handlers

        public void MessageHandler(int id)
        {
            var userGameStats = db.GameStatRepository.GetByID(id);
            userGameStats.exp++;
            UserLevelUp(id);
            db.GameStatRepository.Update(userGameStats);
            db.Save();
        }

        private void UserLevelUp(int id)
        {
            var _gs = db.GameStatRepository.GetByID(id);
            if (_gs.exp >= _gs.expToUp)
            {
                _gs.lvl++;
                _gs.lvlPoints++;
                _gs.maxHp = 8 * _gs.lvl;
                _gs.hp = _gs.maxHp;
                _gs.expToUp = _gs.expToUp + 10 * _gs.lvl;
            }
            db.GameStatRepository.Update(_gs);
            db.Save();
        }
        public void CreateUser(int id)
        {
            if (!api.IsAuthorized)
            {
                api.Authorize(new ApiAuthParams
                {
                    AccessToken = configuration["Config:AccessToken"]
                });
            }
            var firtLastNames = api.Users.Get(new long[] { id }).FirstOrDefault();
            GameStat gs = new GameStat()
            {
                Id = id,
                lvl = 1,
                exp = 0,
                expToUp = 10,
                hp = 10,
                maxHp = 10,
                power = 0,
                defence = 0,
                lvlPoints = 0,
                isHealing = false,
                kills = 0,
                weaponId = 1,
                armorId = 1,
            };

            User user = new User()
            {
                Id = id,
                Name = $"{firtLastNames.FirstName} {firtLastNames.LastName}",
                Nickname = $"{firtLastNames.FirstName} {firtLastNames.LastName}",
                IsAdmin = false,
            };
            db.GameStatRepository.Insert(gs);
            db.Save();
            db.UserRepository.Insert(user);
            db.Save();

        }


        #endregion

        #region response Handlers

        public string webLink(Message message)
        {
            string url = configuration["Config:WebUrl"];
            return url;
        }

        public string changeNickaname(Message message)
        {
            var user = db.UserRepository.GetByID(message.from_id);
            string nickname = message.text.Split(' ', 2)[1];
            user.Nickname = nickname;
            db.UserRepository.Update(user);
            db.Save();
            string result = $"Ник изменен на {nickname}";
            return result;
        }

        public string showHelp(Message message)
        {
            string result = @"
            Статы - Характеристики вашего персонажа
            ---
            Хп - Очки здоровья 
            ---
            Ударить - Перешлите сообщение с этой командой чтобы ударить пользователя
            --- \n
            Повысить - параметры с - Сила или з - Защита (обязательные), число (необязательный)
            Например: Повысить з 5
            ";
            return result;
        }



        public string marriage(Message message)
        {
            try
            {
                string[] requString = message.text.Split(' ', 2);
                var user1 = db.UserRepository.GetByID(message.from_id);
                var user2 = db.UserRepository.GetByID(user1.MarryageRequest);
                string result = "If you somehow stumbled upon this text, know that you are cool and found my Easter egg";
                switch (requString[1].ToLower())
                {
                    case "да":
                        user1.Marry = user2.Id;
                        user2.Marry = user1.Id;
                        result = $"[id{user1.Id.ToString()}|{user1.Nickname}] и [id{user2.Id.ToString()}|{user2.Nickname}] теперь состоят в браке";
                        user1.MarryageRequest = 0;
                        break;
                    case "нет":
                        result = $"[id{user1.Id.ToString()}|{user1.Nickname}] не хочет брак(((((";
                        user1.MarryageRequest = 0;
                        break;

                }

                db.UserRepository.Update(user1);
                db.UserRepository.Update(user2);
                db.Save();
                return result;
            }
            catch
            {
                var requester = db.UserRepository.GetByID(message.from_id);
                var requesterMarryId = db.UserRepository.GetByID(message.from_id).Marry;
                var target = db.UserRepository.GetByID(message.reply_message.from_id);
                var targetMarryId = db.UserRepository.GetByID(message.reply_message.from_id).Marry;
                string result;
                if (requesterMarryId == 0)
                {
                    if (targetMarryId == 0)
                    {
                        result = @$"[id{requester.Id.ToString()}|{requester.Nickname}] предложил(а) [id{target.Id.ToString()}|{target.Nickname}] брак
                             Для ответа: Брак да / нет";
                        target.MarryageRequest = requester.Id;
                    }
                    else
                    {
                        User targetMarry = db.UserRepository.GetByID(targetMarryId);
                        result = @$"[id{target.Id.ToString()}|{target.Nickname}] состоит в браке c [id{targetMarry.Id.ToString()}|{targetMarry.Nickname}]";

                    }
                }
                else
                {
                    User requesterMarry = db.UserRepository.GetByID(requesterMarryId);
                    result = $"Как вам не стыдно.[id{requesterMarry.Id}|{requesterMarry.Nickname}] посмотрите на это";
                }
                db.Save();
                return result;
            }

        }

        public string divorce(Message message)
        {

            var user1 = db.UserRepository.GetByID(message.from_id);
            var user2 = db.UserRepository.GetByID(user1.Marry);
            string result = "";
            if (user1.Marry != 0)
            {
                user1.Marry = 0;
                user2.Marry = 0;
                result = $"[id{user1.Id.ToString()}|{user1.Nickname}] и [id{user2.Id.ToString()}|{user2.Nickname}] больше не в браке. Поздравляю!!!";
            }
            else
            {
                result = "Для этого неплохо было бы иметь парнера";
            }

            db.UserRepository.Update(user1);
            db.UserRepository.Update(user2);
            db.Save();
            return result;
        }



        public string showStats(Message message)
        {
            GameStat gs = db.GameStatRepository.GetByID(message.from_id);
            User user = db.UserRepository.GetByID(message.from_id);
            Weapon weap = db.WeaponRepository.GetByID(gs.weaponId);
            Armor armor = db.ArmorRepository.GetByID(gs.armorId);

            var result = $@"[id{message.from_id}|{user.Nickname}]
            ---
            GOLD: {gs.money}
            Kills: {gs.kills}
            Уровень: {gs.lvl}
            Хп: {gs.hp}
            Сила: {gs.power}(+{weap.damage})
            Защита: {gs.defence}(+{armor.protect})
            Очки: {gs.lvlPoints}
            ---
            Оружие - {weap.name}
            Броня - {armor.name}
            ---
            До следующего уровня {gs.expToUp - gs.exp} опыта";
            return result;
        }

        public string kick(Message message)
        {
            StringBuilder result = new StringBuilder("");
            GameStat _gs1 = db.GameStatRepository.GetByID(message.from_id);
            GameStat _gs2 = db.GameStatRepository.GetByID(message.reply_message.from_id);
            User user1 = db.UserRepository.GetByID(message.from_id);
            User user2 = db.UserRepository.GetByID(message.reply_message.from_id);
            Weapon weapon1 = db.WeaponRepository.GetByID(_gs1.weaponId);
            Weapon weapon2 = db.WeaponRepository.GetByID(_gs2.weaponId);
            Armor armor1 = db.ArmorRepository.GetByID(_gs1.armorId);
            Armor armor2 = db.ArmorRepository.GetByID(_gs2.armorId);
            int power1 = _gs1.power + weapon1.damage;
            int power2 = _gs2.power + weapon2.damage;
            int defence1 = _gs1.defence + armor1.protect;
            int defence2 = _gs2.defence + armor2.protect;


            if (_gs1.Id == _gs2.Id)
            {
                return "Не бей себя, глупышка";
            }

            //пользователь жив?
            if (_gs2.hp > 0)
            {
                //хватает ли сил ударить?
                if (defence2 < power1)
                {
                    _gs2.hp -= power1 - defence2;
                    _gs1.damageSum += power1 - defence2;
                    result.Append(@$"[id{user1.Id}|{user1.Nickname}] ударил(а) [id{user2.Id}|{user2.Nickname}]
                    - {power1 - defence2} HP");

                    //убил?
                    if (_gs2.hp <= 0)
                    {
                        _gs2.hp = 0;
                        result.Append($"\n[id{user2.Id}|{user2.Nickname}] мертв(а)\n");
                        double exp1Temp = _gs1.expToUp;
                        double exp2Temp = _gs2.expToUp;
                        //начисление опыта взависимости от разницы уровней
                        int loot;
                        switch (_gs2.lvl - _gs1.lvl)
                        {
                            case < 0:
                                break;
                            case < 5:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 3);
                                _gs1.exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 20:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 4);
                                _gs1.exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 50:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 6);
                                _gs1.exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 100:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 10);
                                _gs1.exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                        }

                        //начисление золота
                        result.Append($"&#128176; +{giveMoney(_gs1, _gs2)}G");
                        _gs1.kills++;
                        UserLevelUp(message.from_id);

                    }
                    else
                    {
                        result.Append($"\nHP:{_gs2.hp}\n");
                    }
                }
                else
                {
                    result.Append($"[id{user1.Id}|{user1.Nickname}] ты слаб(а)");
                }
            }
            else
            {
                _gs2.hp = 0;
                result.Append($"[id{user2.Id}|{user2.Nickname}] уничтожен(а)");

            }

            if (!_gs2.isHealing)
            {
                healing(message.reply_message.from_id);
            }

            db.GameStatRepository.Update(_gs1);
            db.GameStatRepository.Update(_gs2);
            db.Save();
            return result.ToString();

        }


        public string giveMoney(GameStat _gs, GameStat _gs2)
        {
            Random rnd = new Random();
            int soooo;
            //начисление денег за убийство в зависимости от уровня пользователя
            switch (_gs2.lvl)
            {
                case < 10:
                    soooo = rnd.Next(1, 5);
                    _gs.money += soooo;
                    return soooo.ToString();
                    break;
                case < 20:
                    soooo = rnd.Next(50, 100);
                    _gs.money += soooo;
                    return soooo.ToString();
                    break;
                case < 40:
                    soooo = rnd.Next(1000, 5000);
                    _gs.money += soooo;
                    return soooo.ToString();
                    break;
                case < 60:
                    soooo = rnd.Next(20000, 100000);
                    _gs.money += soooo;
                    return soooo.ToString();
                    break;
                case < 80:
                    soooo = rnd.Next(500000, 2000000);
                    _gs.money += soooo;
                    return soooo.ToString();
                    break;
                default:
                    soooo = rnd.Next(10000000, 50000000);
                    _gs.money += soooo;
                    return soooo.ToString();
            }

        }

        public string useLvlPoints(Message message)
        {
            try
            {
                int upperParam = 0;
                string[]? parametrs;
                try
                {
                    parametrs = message.text.Split(' ', 3);
                    upperParam = Convert.ToInt32(parametrs[2]);
                }
                catch
                {
                    parametrs = message.text.Split(' ', 2);
                }
                //[Повысить] [c] [7]
                var _gs = db.GameStatRepository.GetByID(message.from_id);
                string result = "";

                switch (Convert.ToChar(parametrs[1].ToLower()))
                {
                    case 'с':
                        if (_gs.lvlPoints > 0)
                        {
                            if (upperParam == 0)
                            {
                                _gs.power++;
                                _gs.lvlPoints--;
                                result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] Сила увеличена на 1";
                            }
                            else
                            {
                                if (upperParam <= _gs.lvlPoints)
                                {
                                    _gs.power += upperParam;
                                    _gs.lvlPoints -= upperParam;
                                    result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] Сила увеличена на {upperParam}";
                                }
                                else
                                {
                                    result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] нехватает очков";
                                }
                            }
                        }
                        else
                        {
                            result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] нехватает очков";
                        }

                        break;
                    case 'з':
                        if (_gs.lvlPoints > 0)
                        {
                            if (upperParam == 0)
                            {
                                _gs.defence++;
                                _gs.lvlPoints--;
                                result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] Защита увеличена на 1";
                            }
                            else
                            {
                                if (upperParam <= _gs.lvlPoints)
                                {
                                    _gs.defence += upperParam;
                                    _gs.lvlPoints -= upperParam;
                                    result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] Защита увеличена на {upperParam}";
                                }
                                else
                                {
                                    result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] нехватает очков";
                                }
                            }
                        }
                        else
                        {
                            result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id).Nickname}] нехватает очков";
                        }

                        break;
                }
                db.GameStatRepository.Update(_gs);
                db.Save();
                return result;
            }
            catch
            {
                return "Пищи как сказали, а не то по бащке!";
            }
        }

        public async void healing(int Vk_id)
        {
            //отхил запускается после первого удара
            //через минуту здоровье полностью восстановиться

            GameStat _gs = db.GameStatRepository.GetByID(Vk_id);
            _gs.isHealing = true;
            db.GameStatRepository.Update(_gs);
            db.Save();
            await Task.Delay(60000);

            _gs.hp = _gs.maxHp;
            _gs.isHealing = false;
            db.GameStatRepository.Update(_gs);
            db.Save();

        }

        public string showShop(Message message)
        {
            int goldCount = db.GameStatRepository.GetByID(message.from_id).money;
            IEnumerable<Weapon> weapons = db.WeaponRepository.Get();
            IEnumerable<Armor> armors = db.ArmorRepository.Get();
            StringBuilder result = new StringBuilder($"GOLD: {goldCount.ToString()}\n----\n");
            result.Append("Оружие\n");
            int i = 1;
            foreach (var item in weapons)
            {
                result.Append($"{i} {item.name}: +{item.damage} - {item.cost}G \n");
                i++;
            }

            result.Append("----\n");
            result.Append("Броня\n");
            i = 1;
            foreach (var item in armors)
            {
                result.Append($"{i} {item.name}: +{item.protect} - {item.cost}G \n");
                i++;
            }

            result.Append($"----\nДля покупки: Купить б 2");
            return result.ToString();
        }

        public string buyEquip(Message message)
        {
            GameStat _gs = db.GameStatRepository.GetByID(message.from_id);
            Weapon weapons;
            Armor armors;
            string[] Params;
            try
            {
                Params = message.text.Split(' ', 3);
            }
            catch
            {
                return "Купить б Число или Купить о Число";
            }

            int num = Convert.ToInt16(Params[2]);
            string result = "";
            switch (char.ToLower(message.text[8]))
            {
                case 'о':
                    weapons = db.WeaponRepository.GetByID(Params[3]);
                    if (_gs.money >= weapons.cost)
                    {

                        _gs.weaponId = num;
                        _gs.money -= weapons.cost;
                        result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id)}]  купил(a) {weapons.name} за {weapons.cost}G";
                    }
                    else
                    {
                        result = "Уйди нищий!";
                    }

                    break;


                case 'б':
                    armors = db.ArmorRepository.GetByID(num);
                    if (_gs.money >= armors.cost)
                    {

                        _gs.armorId = num;
                        _gs.money -= armors.cost;
                        result = $"[id{message.from_id}|{db.UserRepository.GetByID(message.from_id)}]  купил(a) {armors.name} за {armors.cost}G";
                    }
                    else
                    {
                        result = "Уйди нищий!";
                    }

                    break;
            }

            db.GameStatRepository.Update(_gs);
            db.Save();
            return result;
        }

        #endregion

        #region AIregion

        public string gptCaller(Message message)
        {
            return gptText(message).Result;
        }

        public async Task<string> gptText(Message vkMessage)
        {
            string apiKey = configuration["Config:GptKey"];
            string endpoint = "https://api.openai.com/v1/chat/completions";
            List<GptMessage> messages = new List<GptMessage>();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            string result;
            try
            {
                string[] NameParam = vkMessage.text.Split(' ', 2);
                result = NameParam[1];

            }
            catch
            {
                return "Error";
            }
            var content = result;

            var message = new GptMessage() { Role = "user", Content = content };
            messages.Add(message);

            var requestData = new GptRequest()
            {
                ModelId = "gpt-3.5-turbo",
                Messages = messages
            };
            using var response = await httpClient.PostAsJsonAsync(endpoint, requestData);


            GptResponseData? responseData = await response.Content.ReadFromJsonAsync<GptResponseData>();

            var choices = responseData?.Choices ?? new List<GptChoice>();
            if (choices.Count == 0)
            {
                return "No choices were returned by the API";

            }
            var choice = choices[0];
            var responseMessage = choice.Message;
            messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();
            return responseText;

        }

        public string weatherCaller(Message message)
        {
            string result = weather(message).Result;
            return result;
        }


        #endregion

        #region ApiServices



        public async Task<string> weather(Message message)
        {
            string[] NameParam;
            try
            {
                 NameParam = message.text.Split(' ', 2);

            }
            catch
            {
                return "Error";
            }

            string apiKey = configuration["Config:WeatherKey"];

            var weatherApiClient = new WeatherAPIClient(apiKey);


            var stringFormat = "{0}, {1}С,{2} - по ощущениям {3}!";


            var request = new RealtimeRequestEntity()
                .WithAirQualityData(true)
                .WithCityName(NameParam[1])
                .WithLanguage("ru");

            var response = await weatherApiClient.Realtime.GetCurrentAsync(request).ConfigureAwait(false);

            string result = string.Format(stringFormat, response.Location.Name, response.Current.TemperatureC, response.Current.Condition.Description, response.Current.FeelsLikeC);
            
            // Keeps the console window open at the end of the program.
            return result;
        }

        #endregion
    }
}
