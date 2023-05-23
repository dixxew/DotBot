using DotBot.Data;
using DotBot.Models;
using DotBot.Services.Vk;
using Microsoft.AspNetCore.Mvc;


namespace DotBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly vkContext db;

        //initialize VkServices
        // static Assembly a = Assembly.Load("VkServices");
        // static Type VSType = a.GetType("ValidationService");
        // private static System.Object VSObject = a.CreateInstance("ValidationService");
        // private MethodInfo VkServMain = VSType.GetMethod("Main");

        public CallbackController(IConfiguration configuration, vkContext context)
        {
            _configuration = configuration;
            db = context;
        }


        //сюда обращается Vk Callback
        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {

            // Проверяем, что находится в поле "type" 
            switch (updates.Type)
            {
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);
                default:
                    Task.Run(async () =>
                    {
                        ValidationService validationService = new ValidationService();
                        validationService.Main(updates);
                    });                    
                    break;
            }

            // Возвращаем "ok" серверу Callback API        
            return Ok("ok");
            
        }
    }
}