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
    public class AccountControllerV8 : ControllerBase
    {
        AccountServiceV8 service;

        public AccountControllerV8(AccountServiceV8 service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountRequestModelV8 model)
        {
            ActionResultV8 result = await this.service.Create(model);
            if (result.Success)
            {
                return this.Ok(result.Object);
            }
            else
            {
                return this.StatusCode(400);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            ActionResultV8 result = await this.service.Get(id);
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
    public class AccountRequestModelV8
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public string SSN { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created
        public string Token { get; set; } // secret token. We never want to expose this.

        public AccountRequestModelV8()
        {

        }

        public AccountRequestModelV8(int id, string name, string ssn, DateTime dateCreated, string token)
        {
            this.Id = id;
            this.Name = name;
            this.SSN = ssn;
            this.DateCreated = dateCreated;
            this.Token = token;
        }
    }

    public class AccountResponseModelV8
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public string SSN { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created

        public AccountResponseModelV8(int id, string name, string ssn, DateTime dateCreated)
        {
            this.Id = id;
            this.Name = name;
            this.SSN = ssn;
            this.DateCreated = dateCreated;
        }
    }

    #endregion

    #region service

    public class ActionResultV8
    {
        public bool Success { get; private set; }

        public object Object { get; private set; }

        public ActionResultV8(bool success, object @object)
        {
            this.Success = success;
            this.Object = @object;
        }

        public ActionResultV8(ValidationResultV8 result)
        {
            this.Success = result.Success;
            this.Object = result.Message;
        }

        public ActionResultV8(ValidationResult result)
        {
            this.Success = result.IsValid;
            this.Object = result.Errors.FirstOrDefault()?.ErrorMessage;
        }
    }

    public class AccountServiceV8
    {
        private IAccountRepositoryV8 repository;
        private IFBIServiceV8 fbiService;
        private AccountRequestModelValidatorV8 modelValidator;
        private IDateServiceV8 dateService;
        private IAccountMapperV8 mapper;
        public AccountServiceV8(IAccountRepositoryV8 repository, 
            AccountRequestModelValidatorV8 modelValidator, 
            IFBIServiceV8 fbiService, 
            IDateServiceV8 dateService,
            IAccountMapperV8 mapper)
        {
            this.repository = repository;
            this.fbiService = fbiService;
            this.modelValidator = modelValidator;
            this.dateService = dateService;
            this.mapper = mapper;
        }


        public async Task<ActionResultV8> Create(AccountRequestModelV8 model)
        {
            ValidationResult result = this.modelValidator.Validate(model);

            if (result.IsValid)
            {
                if (!await this.fbiService.VerifyWithFBI(model.SSN))
                {
                    return new ActionResultV8(false, new ValidationResultV8(false, "Unable to validate with FBI"));
                }

                Account account = this.mapper.MapRequestToEntity(model);

                this.repository.Create(account);

                return new ActionResultV8(true, account);
            }
            else
            {
                return new ActionResultV8(result);
            }
        }

        public async Task<ActionResultV8> Get(int id)
        {
            Account record = await this.repository.Find(id);
            if (record == null)
            {
                return new ActionResultV8(false, new ValidationResultV8(false, "Record not found"));
            }
            else
            {
                AccountResponseModelV8 response = this.mapper.MapEntityToResponse(record);

                return new ActionResultV8(true, response);
            }
        }


    }

    #endregion

    #region mapper

    public interface IAccountMapperV8
    {
        Account MapRequestToEntity(AccountRequestModelV8 model);
        AccountResponseModelV8 MapEntityToResponse(Account entity);
    }

    /// <summary>
    /// You could also use AutoMapper here. I prefer just using constructors or methods to set the properties.
    /// 
    /// </summary>
    public class AccountMapperV8 : IAccountMapperV8
    {
        public Account MapRequestToEntity(AccountRequestModelV8 model)
        {
            return new Account(model.Id, model.Name, model.SSN, model.DateCreated, model.Token);
        }

        public AccountResponseModelV8 MapEntityToResponse(Account entity)
        {
            return new AccountResponseModelV8(entity.Id,entity.Name, entity.SSN, entity.DateCreated);
        }
    }

    #endregion

    #region validation

    public class AccountRequestModelValidatorV8 : AbstractValidator<AccountRequestModelV8>
    {
        IAccountRepositoryV8 repository;
        public AccountRequestModelValidatorV8(IAccountRepositoryV8 repository)
        {
            this.repository = repository;
            this.RuleFor(x => x.Name).NotEmpty();
            this.RuleFor(x => x.SSN).NotEmpty();
            this.RuleFor(x => x.Token).NotEmpty();
            this.RuleFor(x => x.Name).Must(this.AccountValidated).WithMessage("Account already exists");
        }

        private bool AccountValidated(string name)
        {
            return !this.repository.Exists(name);
        }

    }
    public class ValidationResultV8
    {
        public bool Success { get; private set; } = true;
        public string Message { get; private set; } = "";

        public ValidationResultV8(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public ValidationResultV8()
        {
        }
    }
    #endregion

    #region repository
    public interface IAccountRepositoryV8
    {
        bool Exists(string name);

        void Create(Account account);

        Task<Account> Find(int id);
    }

    public class AccountRepositoryV8 : IAccountRepositoryV8
    {
        private ApplicationDbContext context;

        public AccountRepositoryV8(ApplicationDbContext context)
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

        public async Task<Account> Find(int id)
        {
            return await this.context.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
    #endregion

    #region fbiService
    public interface IFBIServiceV8
    {
        Task<bool> VerifyWithFBI(string ssn);
    }

    public class FBIServiceV8 : IFBIServiceV8
    {
        HttpClient client;
        public FBIServiceV8(HttpClient client)
        {
            this.client = client;
        }

        public async Task<bool> VerifyWithFBI(string ssn)
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
    public interface IDateServiceV8
    {
        DateTime Now();
    }

    public class DateServiceV8 : IDateServiceV8
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
 * Our code is single responsibility now with clear separation of concern.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */


