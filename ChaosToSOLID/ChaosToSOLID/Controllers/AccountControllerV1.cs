using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV1 : ControllerBase
    {
        /// <summary>
        /// This system runs as a service in a bank. The website backend that our customers use enter
        /// their account information and then that service calls our service to actually create the accounts for the customers.
        /// Create a method that takes an account name and customer id and creates an account for them.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(Account account)
        {
            if(string.IsNullOrWhiteSpace(account.Name))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            if (string.IsNullOrWhiteSpace(account.Name))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            if (string.IsNullOrWhiteSpace(account.SSN))
            {
                return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
            }

            using (var context = new ApplicationDbContext())
            {
                if (context.Accounts.Any(a => a.Name == account.Name))
                {
                    return new StatusCodeResult((int)System.Net.HttpStatusCode.UnprocessableEntity);
                }
                account.DateCreated = DateTime.Now;
                context.Accounts.Add(account);
                context.SaveChanges();
                return Ok(account);
            }
        }














        /// <summary>
        /// Create method that creates an account record. The method should validate that the account name isn't null and doesn't
        /// already exist.
        /// 
        /// What's wrong with this method?
        /// The Entity Framework context isn't injected so there's not really a way to unit test it.
        /// We're using Entity Framework entities as the model so the client is deeply coupled to our database schema.
        /// If we decided we wanted to use dapper or something else we can't without a lot of refactor work. 
        /// Our validation is mixed in with our data access logic.
        ///
        /// 
        /// What happens when...
        /// We want them to enter their account name twice to confirm they names it correctly?
        /// We need to call another web service to so some sort of validation?
        /// We decide Entity Framework is slow in this particular case? How can we use ADO.NET?
        /// What happens when this account record has 20 fields on it that all need to be validated?
        /// 
        /// This will eventually lead to chaos. 
        /// A 500 line procedural method with validation and service calls and data access code all mixed together.
        /// This method will strike terror into developers who know if they change anything in this method they
        /// risk breaking it. 
        /// 
        /// </summary>
        /// <returns></returns>
    }
}
