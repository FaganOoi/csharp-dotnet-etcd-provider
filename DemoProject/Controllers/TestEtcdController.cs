using DemoProject.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DemoProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestEtcdController : ControllerBase
    {
        private readonly AppSettingsFromEtcd _AppSettingsFromEtcd;

        public TestEtcdController(IOptionsMonitor<AppSettingsFromEtcd> optionsMonitor)
        {
            _AppSettingsFromEtcd = optionsMonitor.CurrentValue;
        }

        [HttpGet(Name = "get/all")]
        public AppSettingsFromEtcd Get()
        {
            return _AppSettingsFromEtcd;
        }
    }
}