using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chaos.Controllers
{
    /// <summary>
    /// Refactor this controller to be SOLID so we can test it. 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV5 : ControllerBase
    {
        private IAccountRepositoryV5 repository;
        private IFBIServiceV5 fbiService;
        private IAccountModelValiatorV5 modelValidator;

        public AccountControllerV5(IAccountRepositoryV5 repository, IAccountModelValiatorV5 modelValidator, IFBIServiceV5 fbiService)
        {
            this.repository = repository;
            this.fbiService = fbiService;
            this.modelValidator = modelValidator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Account account)
        {
            ValidationResultV5 result = this.modelValidator.Validate(account);

            if (result.Success)
            {
                if (!await this.fbiService.VerifyWithFBI(account.SSN))
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
    }

    #region validation
    public class ValidationResultV5
    {
        public bool Success { get; private set; } = true;
        public string Message { get; private set; } = "";

        public ValidationResultV5(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public ValidationResultV5()
        {
        }
    }


    public interface IAccountModelValiatorV5
    {
        ValidationResultV5 Validate(Account account);
    }

    public class AccountModelValiatorV5
    {
        private IAccountRepositoryV5 repository;
        public AccountModelValiatorV5(IAccountRepositoryV5 repository)
        {
            this.repository = repository;
        }

        public ValidationResultV5 Validate(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Name))
            {
                return new ValidationResultV5(false, "Account name cannot be empty");
            }

            if (this.repository.Exists(account.Name))
            {
                return new ValidationResultV5(false, "Account name already exists");
            }

            if (string.IsNullOrWhiteSpace(account.SSN))
            {
                return new ValidationResultV5(false, "Customer id cannot be empty");
            }

            return new ValidationResultV5();
        }
    }
    #endregion

    #region repository
    public interface IAccountRepositoryV5
    {
        bool Exists(string name);

        void Create(Account account);
    }

    public class AccountRepositoryV5 : IAccountRepositoryV5
    {
        private ApplicationDbContext context;

        public AccountRepositoryV5(ApplicationDbContext context)
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
    }
    #endregion

    #region fbiService
    public interface IFBIServiceV5
    {
        Task<bool> VerifyWithFBI(string ssn);
    }

    public class FBIServiceV5 : IFBIServiceV5
    {
        public FBIServiceV5()
        {
        }

        /// <summary>
        /// This can only be tested in an integration test.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<bool> VerifyWithFBI(string ssn)
        {
            var client = new HttpClient();
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
    #endregion
}














/*
 * Our controller still isn't single responsibility.
 * It's calling validation, it's calling the FBI service.
 * 
 * Our controller does have distinct pieces injected which is good. You can work on validation
 * or the repository in isolation and make sure it works without worrying about the rest of the controller method.
 * 
 * 
 */