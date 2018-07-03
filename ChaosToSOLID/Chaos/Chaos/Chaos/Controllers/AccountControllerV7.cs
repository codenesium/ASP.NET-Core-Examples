using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chaos.Controllers
{
    /// <summary>
    /// A new requirement just came in and we need to be able to retrieve accounts but when
    /// the accoutn is created we need a special token to come over but it can never be exposed again.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV7 : ControllerBase
    {
        AccountServiceV7 service;

        public AccountControllerV7(AccountServiceV7 service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountRequestModelV7 model)
        {
            ActionResultV7 result = await this.service.Create(model);
            if(result.Success)
            {
                return this.Ok(result.Object);
            }
            else
            {
                return this.StatusCode(400);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get (int id)
        {
            ActionResultV7 result = await this.service.Get(id);
            if (result.Success)
            {
                return this.Ok(result.Object);
            }
            else
            {
                return this.StatusCode(404);
            }
        }
    }


    #region models
    public class AccountRequestModelV7
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created
        public string Token { get; set; } // secret token. We never want to expose this.
    }

    public class AccountResponseModelV7
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created

    }

    #endregion

    #region service


    public class ActionResultV7
    {
        public bool Success { get; private set; }

        public object Object { get; private set; }

        public ActionResultV7(bool success, object @object)
        {
            this.Success = success;
            this.Object = @object;
        }

        public ActionResultV7(ValidationResultV7 result)
        {
            this.Success = result.Success;
            this.Object = result.Message;
        }
    }

    public class AccountServiceV7
    {
        private IAccountRepositoryV7 repository;
        private IFBIServiceV7 fbiService;
        private IAccountModelValiatorV7 modelValidator;
        public AccountServiceV7(IAccountRepositoryV7 repository, IAccountModelValiatorV7 modelValidator, IFBIServiceV7 fbiService)
        {
            this.repository = repository;
            this.fbiService = fbiService;
            this.modelValidator = modelValidator;
        }

        public async Task<ActionResultV7> Create(AccountRequestModelV7 model)
        {
            ValidationResultV7 result = this.modelValidator.Validate(model);

            if (result.Success)
            {
                if (!await this.fbiService.VerifyWithFBI(model.GlobalCustomerId))
                {
                    return new ActionResultV7(false, new ValidationResultV7(false, "Unable to validate with FBI"));
                }

                var account = new Account2(0, model.Name, model.GlobalCustomerId, DateTime.Now, model.Token);

                this.repository.Create(account);

                return new ActionResultV7(true, account);
            }
            else
            {
                return new ActionResultV7(result);
            }
        }

        public async Task<ActionResultV7> Get(int id)
        {
            Account2 record = await this.repository.Find(id);
            if(record == null)
            {
                return new ActionResultV7(false, new ValidationResultV7(false, "Record not found"));
            }
            else
            {
                var response = new AccountResponseModelV7()
                {
                    DateCreated = record.DateCreated,
                    GlobalCustomerId = record.GlobalCustomerId,
                    Id = record.Id,
                    Name = record.Name
                };
                return new ActionResultV7(true, response);
            }
        }


    }

    #endregion

    #region validation
    public class ValidationResultV7
    {
        public bool Success { get; private set; } = true;
        public string Message { get; private set; } = "";

        public ValidationResultV7(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public ValidationResultV7()
        {
        }
    }


    public interface IAccountModelValiatorV7
    {
        ValidationResultV7 Validate(AccountRequestModelV7 account);
    }

    public class AccountModelValiatorV7
    {
        private IAccountRepositoryV7 repository;
        public AccountModelValiatorV7(IAccountRepositoryV7 repository)
        {
            this.repository = repository;
        }

        public ValidationResultV7 Validate(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Name))
            {
                return new ValidationResultV7(false, "Account name cannot be empty");
            }

            if (this.repository.Exists(account.Name))
            {
                return new ValidationResultV7(false, "Account name already exists");
            }

            if (account.GlobalCustomerId == Guid.Empty)
            {
                return new ValidationResultV7(false, "Customer id cannot be empty");
            }

            return new ValidationResultV7();
        }
    }
    #endregion

    #region repository
    public interface IAccountRepositoryV7
    {
        bool Exists(string name);

        void Create(Account2 account);

        Task<Account2> Find(int id);
    }

    public class AccountRepositoryV7 : IAccountRepositoryV7
    {
        private ApplicationDbContext context;

        public AccountRepositoryV7(ApplicationDbContext context)
        {
            this.context = context;
        }

        public bool Exists(string name)
        {
            return this.context.Accounts.Any(x => x.Name == name);
        }
        public void Create(Account2 account)
        {
            this.context.Accounts2.Add(account);
            context.SaveChanges();
        }

        public async Task<Account2> Find(int id)
        {
           return await this.context.Accounts2.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
    #endregion

    #region fbiService
    public interface IFBIServiceV7
    {
        Task<bool> VerifyWithFBI(Guid customerId);
    }

    public class FBIServiceV7 : IFBIServiceV7
    {
        HttpClient client;
        public FBIServiceV7(HttpClient client)
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
 * Now our code is looking SOLID.
 * Our code is mostly single responsibility. We're not using public setters anywhere. We're using dependency inversion.
 * 
 * Our controller does very little. It calls the service and returns a response based on the service result.
 * The meat of our app has been moved to the service layer. This means if we wanted to get rid of ASP.NET we should 
 * be able to reference the service dll and it should work like a charm. Ideally you would split this code into projects like
 * Repositories
 * Service
 * Models
 * Controllers.
 * 
 * What are some issues we still see with this code?
 * Is the object mapping in the service the best is could be? What potential bugs has this created?
 * 
 * It's still pretty smelly how we're doing validation. I'd really like to test each rule independently without
 * having to fill out the entire account class which is a maintenance nightmare.
 * 
 * We still don't have a good FBI test.
 * 
 * 
 * 
 */ 





