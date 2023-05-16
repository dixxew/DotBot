using DotBot.Models;

namespace DotBot.Services.Vk
{
    public static class VkMethodsDict
    {
        private static KeyValuePair<string, Func<Message, string>> weblink = new KeyValuePair<string, Func<Message, string>>("ссылка", new ContentService().webLink);
        private static KeyValuePair<string, Func<Message, string>> changeNickaname = new KeyValuePair<string, Func<Message, string>>("ник", new ContentService().changeNickaname);
        private static KeyValuePair<string, Func<Message, string>> showHelp = new KeyValuePair<string, Func<Message, string>>("помощь", new ContentService().showHelp);
        private static KeyValuePair<string, Func<Message, string>> marriageRequest = new KeyValuePair<string, Func<Message, string>>("брак", new ContentService().marriage);
        private static KeyValuePair<string, Func<Message, string>> divorce = new KeyValuePair<string, Func<Message, string>>("развод", new ContentService().divorce);
        private static KeyValuePair<string, Func<Message, string>> showStats = new KeyValuePair<string, Func<Message, string>>("статы", new ContentService().showStats);
        private static KeyValuePair<string, Func<Message, string>> kick = new KeyValuePair<string, Func<Message, string>>("ударить", new ContentService().kick);
        private static KeyValuePair<string, Func<Message, string>> useLvlPoints = new KeyValuePair<string, Func<Message, string>>("повысить", new ContentService().useLvlPoints);
        private static KeyValuePair<string, Func<Message, string>> showShop = new KeyValuePair<string, Func<Message, string>>("магазин", new ContentService().showShop);
        private static KeyValuePair<string, Func<Message, string>> buyEquip = new KeyValuePair<string, Func<Message, string>>("купить", new ContentService().buyEquip);
        private static KeyValuePair<string, Func<Message, string>> gpt = new KeyValuePair<string, Func<Message, string>>("gpt", new ContentService().gptCaller);
        private static KeyValuePair<string, Func<Message, string>> weather = new KeyValuePair<string, Func<Message, string>>("погода", new ContentService().weatherCaller);

        private readonly static List<KeyValuePair<string, Func<Message, string>>> funcList = new List<KeyValuePair<string, Func<Message, string>>>()
        {
            weblink,
            changeNickaname,
            showHelp,
            marriageRequest,
            divorce,
            showStats,
            kick,
            useLvlPoints,
            showShop,
            buyEquip,
            gpt,
            weather
        };

        public static readonly Dictionary<string, Func<Message, string>> funcDict = new Dictionary<string, Func<Message, string>>(funcList);
    }
}
