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
            userGameStats.Exp++;
            UserLevelUp(id);
            db.GameStatRepository.Update(userGameStats);
            db.Save();
        }

        private void UserLevelUp(int id)
        {
            var _gs = db.GameStatRepository.GetByID(id);
            if (_gs.Exp >= _gs.ExpToUp)
            {
                _gs.Level++;
                _gs.LevelPoints++;
                _gs.MaxHp = 8 * _gs.Level;
                _gs.Hp = _gs.MaxHp;
                _gs.ExpToUp = _gs.ExpToUp + 10 * _gs.Level;
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
            User user = new User()
            {
                Name = $"{firtLastNames.FirstName} {firtLastNames.LastName}",
                Nickname = $"{firtLastNames.FirstName} {firtLastNames.LastName}",
                Marry = 0,
                MarryageRequest = 0
            };
            db.UserRepository.Insert(user);
            db.Save();
            GameStat gs = new GameStat()
            {
                Id = id,
                Level = 1,
                Exp = 0,
                ExpToUp = 10,
                Hp = 10,
                MaxHp = 10,
                Power = 0,
                Defence = 0,
                LevelPoints = 0,
                IsHealing = false,
                Kills = 0,
                Weapon = db.WeaponRepository.GetByID(1),
                Armor = db.ArmorRepository.GetByID(1),
                User = user
            };
            
            db.GameStatRepository.Insert(gs);
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
            var user = db.GameStatRepository.Get(x => x.Id==message.from_id,null,"User").First().User;
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
                var user1 = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First().User;
                var user2 = db.GameStatRepository.Get(x => x.Id == user1.MarryageRequest, null, "User").First().User;
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
                var requester = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First().User;
                var requesterMarryId = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First().User.Marry;
                var target = db.GameStatRepository.Get(x => x.Id == message.reply_message.from_id, null, "User").First().User;
                var targetMarryId = db.GameStatRepository.Get(x => x.Id == message.reply_message.from_id, null, "User").First().User.Marry;
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

            var user1 = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First().User;
            var user2 = db.GameStatRepository.Get(x => x.Id == user1.Marry, null, "User").First().User;
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
            GameStat gs = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User,Weapon,Armor").First();

            var result = $@"[id{message.from_id}|{gs.User.Nickname}]
            ---
            GOLD: {gs.Money}
            Kills: {gs.Kills}
            Уровень: {gs.Level}
            Хп: {gs.Hp}
            Сила: {gs.Power}(+{gs.Weapon.damage})
            Защита: {gs.Defence}(+{gs.Armor.protect})
            Очки: {gs.LevelPoints}
            ---
            Оружие - {gs.Weapon.name}
            Броня - {gs.Armor.name}
            ---
            До следующего уровня {gs.ExpToUp - gs.Exp} опыта";
            return result;
        }

        public string kick(Message message)
        {
            StringBuilder result = new StringBuilder("");
            GameStat _gs1 = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User,Armor,Weapon").First();
            GameStat _gs2 = db.GameStatRepository.Get(x => x.Id == message.reply_message.from_id, null, "User,Armor,Weapon").First();
            
            int power1 = _gs1.Power + _gs1.Weapon.damage;
            int power2 = _gs2.Power + _gs2.Weapon.damage;
            int defence1 = _gs1.Defence + _gs1.Armor.protect;
            int defence2 = _gs2.Defence + _gs2.Armor.protect;


            if (_gs1.Id == _gs2.Id)
            {
                return "Не бей себя, глупышка";
            }

            //пользователь жив?
            if (_gs2.Hp > 0)
            {
                //хватает ли сил ударить?
                if (defence2 < power1)
                {
                    _gs2.Hp -= power1 - defence2;
                    _gs1.DamageSum += power1 - defence2;
                    result.Append(@$"[id{_gs1.Id}|{_gs1.User.Nickname}] ударил(а) [id{_gs2.Id}|{_gs2.User.Nickname}]
                    - {power1 - defence2} HP");

                    //убил?
                    if (_gs2.Hp <= 0)
                    {
                        _gs2.Hp = 0;
                        result.Append($"\n[id{_gs2.Id}|{_gs2.User.Nickname}] мертв(а)\n");
                        double exp1Temp = _gs1.ExpToUp;
                        double exp2Temp = _gs2.ExpToUp;
                        //начисление опыта взависимости от разницы уровней
                        int loot;
                        switch (_gs2.Level - _gs1.Level)
                        {
                            case < 0:
                                break;
                            case < 5:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 3);
                                _gs1.Exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 20:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 4);
                                _gs1.Exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 50:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 6);
                                _gs1.Exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                            case < 100:
                                loot = Convert.ToInt32((exp2Temp - exp1Temp) / 10);
                                _gs1.Exp += loot;
                                result.Append($"+{loot} EXP &#10055; | ");
                                break;
                        }

                        //начисление золота
                        result.Append($"&#128176; +{giveMoney(_gs1, _gs2)}G");
                        _gs1.Kills++;
                        UserLevelUp(message.from_id);

                    }
                    else
                    {
                        result.Append($"\nHP:{_gs2.Hp}\n");
                    }
                }
                else
                {
                    result.Append($"[id{_gs1.Id} | {_gs1.User.Nickname}] ты слаб(а)");
                }
            }
            else
            {
                _gs2.Hp = 0;
                result.Append($"[id{_gs2.Id} | {_gs2.User.Nickname}] уничтожен(а)");

            }

            if (!_gs2.IsHealing)
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
            switch (_gs2.Level)
            {
                case < 10:
                    soooo = rnd.Next(1, 5);
                    _gs.Money += soooo;
                    return soooo.ToString();
                    break;
                case < 20:
                    soooo = rnd.Next(50, 100);
                    _gs.Money += soooo;
                    return soooo.ToString();
                    break;
                case < 40:
                    soooo = rnd.Next(1000, 5000);
                    _gs.Money += soooo;
                    return soooo.ToString();
                    break;
                case < 60:
                    soooo = rnd.Next(20000, 100000);
                    _gs.Money += soooo;
                    return soooo.ToString();
                    break;
                case < 80:
                    soooo = rnd.Next(500000, 2000000);
                    _gs.Money += soooo;
                    return soooo.ToString();
                    break;
                default:
                    soooo = rnd.Next(10000000, 50000000);
                    _gs.Money += soooo;
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
                var _gs = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First();
                string result = "";

                switch (Convert.ToChar(parametrs[1].ToLower()))
                {
                    case 'с':
                        if (_gs.LevelPoints > 0)
                        {
                            if (upperParam == 0)
                            {
                                _gs.Power++;
                                _gs.LevelPoints--;
                                result = $"[id{message.from_id}|{_gs.User.Nickname}] Сила увеличена на 1";
                            }
                            else
                            {
                                if (upperParam <= _gs.LevelPoints)
                                {
                                    _gs.Power += upperParam;
                                    _gs.LevelPoints -= upperParam;
                                    result = $"[id{message.from_id}|{_gs.User.Nickname}] Сила увеличена на {upperParam}";
                                }
                                else
                                {
                                    result = $"[id{message.from_id}|{_gs.User.Nickname}] нехватает очков";
                                }
                            }
                        }
                        else
                        {
                            result = $"[id{message.from_id}|{_gs.User.Nickname}] нехватает очков";
                        }

                        break;
                    case 'з':
                        if (_gs.LevelPoints > 0)
                        {
                            if (upperParam == 0)
                            {
                                _gs.Defence++;
                                _gs.LevelPoints--;
                                result = $"[id{message.from_id}|{_gs.User.Nickname}] Защита увеличена на 1";
                            }
                            else
                            {
                                if (upperParam <= _gs.LevelPoints)
                                {
                                    _gs.Defence += upperParam;
                                    _gs.LevelPoints -= upperParam;
                                    result = $"[id{message.from_id}|{_gs.User.Nickname}] Защита увеличена на {upperParam}";
                                }
                                else
                                {
                                    result = $"[id{message.from_id}|{_gs.User.Nickname}] нехватает очков";
                                }
                            }
                        }
                        else
                        {
                            result = $"[id{message.from_id}|{_gs.User.Nickname}] нехватает очков";
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

            GameStat _gs = db.GameStatRepository.Get(x => x.Id == Vk_id, null, "User").First();
            _gs.IsHealing = true;
            db.GameStatRepository.Update(_gs);
            db.Save();
            await Task.Delay(60000);

            _gs.Hp = _gs.MaxHp;
            _gs.IsHealing = false;
            db.GameStatRepository.Update(_gs);
            db.Save();

        }

        public string showShop(Message message)
        {
            int goldCount = db.GameStatRepository.Get(x => x.Id == message.from_id).First().Money;
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
            GameStat _gs = db.GameStatRepository.Get(x => x.Id == message.from_id, null, "User").First();
            List<Armor> armors = db.ArmorRepository.Get().ToList();
            List<Weapon> weapons = db.WeaponRepository.Get().ToList();

            string[] Params;

            /// Парсинг на:
            /// 0 - [Купить]
            /// 1 - [о/б] Оружие или Броня
            /// 2 - [number] номер в списке
            ///
            /// Любое несоответствие - catch
            try
            {
                Params = message.text.Split(' ', 3);
            }
            catch
            {
                return "Купить б Число или Купить о Число";
            }

            // номер айтема
            int num = Convert.ToInt16(Params[2]); 
            string result = "";
            switch (Params[1])
            {
                case "о":
                    if (_gs.Money >= weapons[num].cost)
                    {

                        _gs.Weapon = weapons[num];
                        _gs.Money -= weapons[num].cost;
                        result = $"[id{message.from_id}|{_gs.User.Nickname}]  купил(a) {weapons[num].name} за {weapons[num].cost}G";
                    }
                    else
                    {
                        result = "Уйди нищий!";
                    }

                    break;
                case "б":
                    if (_gs.Money >= armors[num].cost)
                    {

                        _gs.Armor = armors[num];
                        _gs.Money -= armors[num].cost;
                        result = $"[id{message.from_id}|{_gs.User.Nickname}]  купил(a) {armors[num].name} за {armors[num].cost}G";
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

        //weather.api
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
