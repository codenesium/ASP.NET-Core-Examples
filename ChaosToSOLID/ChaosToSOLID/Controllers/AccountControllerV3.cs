using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    /// <summary>
    /// A new requirment came in and we have to change this controller to use an external service to validate this customer with the FBI coutnerterrorism database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV3 : ControllerBase
    {
        private ApplicationDbContext context;
        public AccountControllerV3(ApplicationDbContext context)
        {
            this.context = context;
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

            if(!(await this.VerifyWithFBI(account.SSN)))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.BadRequest);
            }

            context.Accounts.Add(account);
            context.SaveChanges();
            return Ok(account);
        }

        private async Task<bool> VerifyWithFBI(string ssn)
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");
            if(!string.IsNullOrWhiteSpace(response))
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







/*
 * Note that we're missing coverage if the FBI service fails. We don't really have a way to test that.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */