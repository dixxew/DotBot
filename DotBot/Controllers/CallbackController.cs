using DotBot.Data;
using DotBot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using VkNet;
using VkNet.Model;
using VkServices;


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
        public async Task<IActionResult> Callback([FromBody] Updates updates)
        {
           ValidationService validationService = new ValidationService();

            // Проверяем, что находится в поле "type" 
            switch (updates.Type)
            {
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);
                default:
                     await validationService.Main(updates);                    
                    break;
            }

            // Возвращаем "ok" серверу Callback API        
            return Ok("ok");
            
        }
    }
}