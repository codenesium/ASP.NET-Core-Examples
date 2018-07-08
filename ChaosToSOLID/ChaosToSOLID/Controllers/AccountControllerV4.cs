using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    /// <summary>
    /// Change this controller to use(inject) an external service to validate this customer with the FBI coutnerterrorism database so we can test it. 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV4 : ControllerBase
    {
        private ApplicationDbContext context;
        private IFBIService fbiService;
        public AccountControllerV4(ApplicationDbContext context, IFBIService fbiService)
        {
            this.context = context;
            this.fbiService = fbiService;
        }
        [HttpPost]
        public async Task<IActionResult> Create(Account account)
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

            if (!(await this.fbiService.VerifyWithFBI(account.SSN)))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.BadRequest);
            }

            context.Accounts.Add(account);
            context.SaveChanges();
            return Ok(account);
        }
    }

    public interface IFBIService
    {
        Task<bool> VerifyWithFBI(string ssn);
    }

    public class FBIService : IFBIService
    {
        public async Task<bool> VerifyWithFBI(string ssn)
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");
            if (!string.IsNullOrWhiteSpace(response))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
