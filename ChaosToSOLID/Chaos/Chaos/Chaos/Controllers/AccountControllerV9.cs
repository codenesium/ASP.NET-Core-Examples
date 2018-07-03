using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chaos.Controllers
{
    #region controller
    /// <summary>
    /// A new requirement just came in and we need to be able to retrieve accounts but when
    /// the accoutn is created we need a special token to come over but it can never be exposed again.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountControllerV9 : ControllerBase
    {
        IAccountServiceV9 service;

        public AccountControllerV9(IAccountServiceV9 service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountRequestModelV9 model)
        {
            ActionResultV9 result = await this.service.Create(model);
            if (result.Success)
            {
                return this.Ok(result.Object);
            }
            else
            {
                return this.StatusCode(422);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            ActionResultV9 result = await this.service.Get(id);
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

    #endregion

    #region models
    public class AccountRequestModelV9
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created
        public string Token { get; set; } // secret token. We never want to expose this.

        public AccountRequestModelV9()
        {

        }

        public AccountRequestModelV9(int id, string name, Guid globalCustomerId, DateTime dateCreated, string token)
        {
            this.Id = id;
            this.Name = name;
            this.GlobalCustomerId = globalCustomerId;
            this.DateCreated = dateCreated;
            this.Token = token;
        }
    }

    public class AccountResponseModelV9
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created

        public AccountResponseModelV9(int id, string name, Guid globalCustomerId, DateTime dateCreated)
        {
            this.Id = id;
            this.Name = name;
            this.GlobalCustomerId = globalCustomerId;
            this.DateCreated = dateCreated;
        }
    }

    #endregion

    #region service

    public interface IAccountServiceV9
    {
        Task<ActionResultV9> Create(AccountRequestModelV9 model);
        Task<ActionResultV9> Get(int id);
    }

    public class AccountServiceV9 : IAccountServiceV9
    {
        private IAccountRepositoryV9 repository;
        private IFBIServiceV9 fbiService;
        private AccountRequestModelValidatorV9 modelValidator;
        private IDateServicev8 dateService;
        private IAccountMapperV9 mapper;
        public AccountServiceV9(IAccountRepositoryV9 repository, 
            AccountRequestModelValidatorV9 modelValidator, 
            IFBIServiceV9 fbiService, 
            IDateServicev8 dateService,
            IAccountMapperV9 mapper)
        {
            this.repository = repository;
            this.fbiService = fbiService;
            this.modelValidator = modelValidator;
            this.dateService = dateService;
            this.mapper = mapper;
        }


        public async Task<ActionResultV9> Create(AccountRequestModelV9 model)
        {
            ValidationResult result = this.modelValidator.Validate(model);

            if (result.IsValid)
            {
                if (!await this.fbiService.VerifyWithFBI(model.GlobalCustomerId))
                {
                    return new ActionResultV9(false, new ValidationResultV9(false, "Unable to validate with FBI"));
                }

                Account2 account = this.mapper.MapRequestToEntity(model);

                this.repository.Create(account);

                return new ActionResultV9(true, account);
            }
            else
            {
                return new ActionResultV9(result);
            }
        }

        public async Task<ActionResultV9> Get(int id)
        {
            Account2 record = await this.repository.Find(id);
            if (record == null)
            {
                return new ActionResultV9(false, new ValidationResultV9(false, "Record not found"));
            }
            else
            {
                AccountResponseModelV9 response = this.mapper.MapEntityToResponse(record);

                return new ActionResultV9(true, response);
            }
        }
    }

    public class ActionResultV9
    {
        public bool Success { get; private set; }

        public object Object { get; private set; }

        public ActionResultV9(bool success, object @object)
        {
            this.Success = success;
            this.Object = @object;
        }

        public ActionResultV9(ValidationResultV9 result)
        {
            this.Success = result.Success;
            this.Object = result.Message;
        }

        public ActionResultV9(ValidationResult result)
        {
            this.Success = result.IsValid;
            this.Object = result.Errors.FirstOrDefault()?.ErrorMessage;
        }
    }



    #endregion

    #region mapper

    public interface IAccountMapperV9
    {
        Account2 MapRequestToEntity(AccountRequestModelV9 model);
        AccountResponseModelV9 MapEntityToResponse(Account2 entity);
    }

    /// <summary>
    /// You could also use AutoMapper here. I prefer just using constructors or methods to set the properties.
    /// 
    /// </summary>
    public class AccountMapperV9 : IAccountMapperV9
    {
        public Account2 MapRequestToEntity(AccountRequestModelV9 model)
        {
            return new Account2(model.Id, model.Name, model.GlobalCustomerId, model.DateCreated, model.Token);
        }

        public AccountResponseModelV9 MapEntityToResponse(Account2 entity)
        {
            return new AccountResponseModelV9(entity.Id,entity.Name, entity.GlobalCustomerId, entity.DateCreated);
        }
    }

    #endregion

    #region validation

    public class AccountRequestModelValidatorV9 : AbstractValidator<AccountRequestModelV9>
    {
        IAccountRepositoryV9 repository;
        public AccountRequestModelValidatorV9(IAccountRepositoryV9 repository)
        {
            this.repository = repository;
            this.RuleFor(x => x.Name).NotEmpty();
            this.RuleFor(x => x.GlobalCustomerId).NotEmpty();
            this.RuleFor(x => x.Token).NotEmpty();
            this.RuleFor(x => x.Name).Must(this.AccountValidated).WithMessage("Account already exists");
        }

        private bool AccountValidated(string name)
        {
            return !this.repository.Exists(name);
        }

    }
    public class ValidationResultV9
    {
        public bool Success { get; private set; } = true;
        public string Message { get; private set; } = "";

        public ValidationResultV9(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public ValidationResultV9()
        {
        }
    }
    #endregion

    #region repository
    public interface IAccountRepositoryV9
    {
        bool Exists(string name);

        void Create(Account2 account);

        Task<Account2> Find(int id);
    }

    public class AccountRepositoryV9 : IAccountRepositoryV9
    {
        private ApplicationDbContext context;

        public AccountRepositoryV9(ApplicationDbContext context)
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
    public interface IFBIServiceV9
    {
        Task<bool> VerifyWithFBI(Guid customerId);
    }

    public class FBIServiceV9 : IFBIServiceV9
    {
        HttpClient client;
        public FBIServiceV9(HttpClient client)
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

    #region dateService
    public interface IDateServicev9
    {
        DateTime Now();
    }

    public class DateServiceV9 : IDateServicev8
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }

    #endregion
}





/*
 * We've created object mappers and injected those into our service.
 * We introduced fluent validation which makes managing our validators much easier
 * We created a date service so we can test date time things without a depenency on DateTime.Now
 * Our code is single responsibility now with clear separation of convert.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */


