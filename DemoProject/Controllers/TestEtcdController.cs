using DemoProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DemoProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestEtcdController : ControllerBase
    {
        private readonly AppSettingsFromEtcd _AppSettingsFromEtcd;
        private readonly Dev2 _Dev2;

        public TestEtcdController(IOptionsMonitor<AppSettingsFromEtcd> optionsMonitor, IOptionsMonitor<Dev2> optionsMonitor2)
        {
            _AppSettingsFromEtcd = optionsMonitor.CurrentValue;
            _Dev2 = optionsMonitor2.CurrentValue;
        }

        [HttpGet(Name = "get/all")]
        public IActionResult Get()
        {
            return Ok(new { AppSettingsFromEtcd = _AppSettingsFromEtcd, Dev2 = _Dev2 });
        }
    }
}