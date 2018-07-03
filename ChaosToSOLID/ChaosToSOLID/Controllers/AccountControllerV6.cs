using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    /// <summary>
    /// A new requirement just came in and we need to be able to retrieve accounts but when
    /// the account is created we need a special token to come over from the web service but it can never be exposed again.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV6 : ControllerBase
    {
        private IAccountRepositoryV6 repository;
        private IFBIServiceV6 fbiService;
        private IAccountModelValiatorV6 modelValidator;

        public AccountControllerV6(IAccountRepositoryV6 repository, IAccountModelValiatorV6 modelValidator, IFBIServiceV6 fbiService)
        {
            this.repository = repository;
            this.fbiService = fbiService;
            this.modelValidator = modelValidator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Account account)
        {
            ValidationResultV6 result = this.modelValidator.Validate(account);

            if (result.Success)
            {
                if (!await this.fbiService.VerifyWithFBI(account.GlobalCustomerId))
                {
                    return new StatusCodeResult((int)System.Net.HttpStatusCode.BadRequest);
                }

                this.repository.Create(account);

                return this.Ok(account);
            }
            else
            {
                return this.StatusCode(422, result);
            }
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            Account record = this.repository.Find(id);

            if (record == null)
            {
                return this.StatusCode(404);
            }
            else
            {
                return this.Ok(record);
            }
        }
    }

    #region validation
    public class ValidationResultV6
    {
        public bool Success { get; private set; } = true;
        public string Message { get; private set; } = "";

        public ValidationResultV6(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public ValidationResultV6()
        {
        }
    }


    public interface IAccountModelValiatorV6
    {
        ValidationResultV6 Validate(Account account);
    }

    public class AccountModelValiatorV6
    {
        private IAccountRepositoryV6 repository;
        public AccountModelValiatorV6(IAccountRepositoryV6 repository)
        {
            this.repository = repository;
        }

        public ValidationResultV6 Validate(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Name))
            {
                return new ValidationResultV6(false, "Account name cannot be empty");
            }

            if (this.repository.Exists(account.Name))
            {
                return new ValidationResultV6(false, "Account name already exists");
            }

            if (account.GlobalCustomerId == Guid.Empty)
            {
                return new ValidationResultV6(false, "Customer id cannot be empty");
            }

            return new ValidationResultV6();
        }
    }
    #endregion

    #region  repository
    public interface IAccountRepositoryV6
    {
        bool Exists(string name);

        void Create(Account account);

        Account Find(int id);
    }

    public class AccountRepositoryV6 : IAccountRepositoryV6
    {
        private ApplicationDbContext context;

        public AccountRepositoryV6(ApplicationDbContext context)
        {
            this.context = context;
        }

        public bool Exists(string name)
        {
            return this.context.Accounts.Any(x => x.Name == name);
        }
        public void Create(Account account)
        {
            this.context.Accounts.Add(account);
            context.SaveChanges();
        }

        public Account Find(int id)
        {
           return this.context.Accounts.FirstOrDefault(x => x.Id == id);
        }
    }
    #endregion

    #region fbiService
    public interface IFBIServiceV6
    {
        Task<bool> VerifyWithFBI(Guid customerId);
    }

    public class FBIServiceV6 : IFBIServiceV6
    {
        HttpClient client;
        public FBIServiceV6(HttpClient client)
        {
            this.client = client;
        }

        public async Task<bool> VerifyWithFBI(Guid customerId)
        {
            string response = await this.client.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");
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
    #endregion
}













/*
* There isn't a test project for this iteration because I wanted to demonstrate the changing requirments.
* V7 will fix the issues and test them.
* We've added our method to allow retrieving the account but we're
* exposing the secret token we're not supposed to. Our response and request model
* has diverged! What do we do?
* You may have realized by now that using your database entity as your request model is not a 
* good idea. We've discovered that when the model diverges you're in a pickle. You can hack it to make it
* work which means having extra fields or you split your model into a request and a response. 
* 
* Another thing to think about now is how would we dump ASP.NET and move our system to a service bus?
* The validation and the FBI service is still in the controller. Would the service bus have to call the controller in code?
* Our system is still too coupled. You should be able to replace the controller layer with a winform app or a service bus and there
* should be no issues there. The root problem is there is still too much happening in our controller.
*
*/