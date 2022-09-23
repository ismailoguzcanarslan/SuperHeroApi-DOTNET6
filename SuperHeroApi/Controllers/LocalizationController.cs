using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SuperHeroApi.Resources;
using SuperHeroApi.Resources.Localization;

namespace SuperHeroApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private readonly IStringLocalizer<Localization> _stringLocalizer;

        public LocalizationController(IStringLocalizer<Localization> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet]
        [Route("getmessage")] 
        public IActionResult LocalizationEndPoint()
        {
            var mes = _stringLocalizer.GetString("message");

            return Ok(mes.Value);
        }
    }
}
