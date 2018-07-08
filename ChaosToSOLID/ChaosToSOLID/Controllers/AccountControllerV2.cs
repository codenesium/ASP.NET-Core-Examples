using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    /// <summary>
    /// We've changed our account controller to use an injected database context.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV2 : ControllerBase
    {
        private ApplicationDbContext context;
        public AccountControllerV2(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpPost]
        public IActionResult Create(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Name))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            if (context.Accounts.Any(a => a.Name == account.Name))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            if (string.IsNullOrWhiteSpace(account.SSN))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            account.DateCreated = DateTime.Now;
            context.Accounts.Add(account);
            context.SaveChanges();
            return Ok(account);
        }
    }
}
