using EntityFrameworkExample.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkExample
{
    [Route("api/values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        [Route("Devices")]
        public async virtual Task<IActionResult> All()
        {
            using (var context = new IOTContext())
            {
                var devices = context.Device.ToList();
                return this.Ok(devices);
            }
          
        }
    }
}
