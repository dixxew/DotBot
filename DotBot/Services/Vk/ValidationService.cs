using DotBot.DAL;
using DotBot.Models;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using Message = DotBot.Models.Message;
using User = DotBot.Models.User;

namespace DotBot.Services.Vk
{
    public class ValidationService
    {
        private ContentService content;
        VkApi api = new VkApi();
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private DbRepository db = new DbRepository();


        public void Main(Updates updates)
        {
            if (updates.Object.message.from_id < 0) { return; }

            Message message = updates.Object.message;
            CheckUser(message.from_id);
            if (message.reply_message != null)
            {

                if (updates.Object.message.from_id! < 0)
                {
                    CheckUser(message.reply_message.from_id);
                }
            }
            CheckMessage(message);
        }

        void CheckUser(int fromId)
        {

            /// проверка на сообщество
            /// проверка на наличие юзера в бд
            /// наличие reply message
            /// таблица для конфы

            content = new ContentService();
            User? user = db.UserRepository.GetByID(fromId);
            if (user == null)
            {
                content.CreateUser(fromId);
            }

        }


        void CheckMessage(Message message)
        {
            content = new ContentService();
            content.MessageHandler(message.from_id);

            string result = "";
            try
            {
                string[] NameParam = message.text.Split(' ', 2);
                result = VkMethodsDict.funcDict[NameParam[0].ToLower()].Invoke(message);

            }
            catch
            {
                if (VkMethodsDict.funcDict.ContainsKey(message.text))
                {
                    result = VkMethodsDict.funcDict[message.text.ToLower()].Invoke(message);
                }
            }
            db.Dispose();
            if (result != "")
            {
                if (!api.IsAuthorized)
                {
                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = configuration["Config:AccessToken"]
                    });
                }
                MessagesSendParams msp = new MessagesSendParams()
                {
                    RandomId = new Random().Next(0, int.MaxValue),
                    PeerId = message.peer_id,
                    Message = result
                };
                api.Messages.Send(msp);
            }

        }
    }
}