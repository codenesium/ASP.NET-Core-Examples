using Chaos.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using Xunit;
using Moq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation.Results;
using System.Collections.Generic;
using FluentValidation.TestHelper;

namespace Chaos.Tests
{
    public class UnitTestAccountV9Controller
    {
        [Fact]
        public async void Test_Account_Create_200()
        {
            //setup
            Mock<IAccountServiceV9> serviceMock = new Mock<IAccountServiceV9>();
            serviceMock.Setup(x => x.Create(It.IsAny<AccountRequestModelV9>())).Returns(Task.FromResult(new ActionResultV9(true, default(object))));
            AccountV9Controller controller = new AccountV9Controller(serviceMock.Object);

            //act
            IActionResult result = await controller.Create(new AccountRequestModelV9());

            //verify
            result.Should().BeOfType<OkObjectResult>();
            (result as ObjectResult).StatusCode.Should().Be(200);
            serviceMock.Verify(x => x.Create(It.IsAny<AccountRequestModelV9>()));
        }

        [Fact]
        public async void Test_Account_Create_422()
        {
            //setup
            Mock<IAccountServiceV9> serviceMock = new Mock<IAccountServiceV9>();
            serviceMock.Setup(x => x.Create(It.IsAny<AccountRequestModelV9>())).Returns(Task.FromResult(new ActionResultV9(false, default(object))));
            AccountV9Controller controller = new AccountV9Controller(serviceMock.Object);

            //act
            IActionResult result = await controller.Create(new AccountRequestModelV9());

            //verify
            result.Should().BeOfType<StatusCodeResult>();
            (result as StatusCodeResult).StatusCode.Should().Be(422);
            serviceMock.Verify(x => x.Create(It.IsAny<AccountRequestModelV9>()));
        }


        [Fact]
        public async void Test_Account_Get_200()
        {
            //setup
            Mock<IAccountServiceV9> serviceMock = new Mock<IAccountServiceV9>();
            AccountV9Controller controller = new AccountV9Controller(serviceMock.Object);
            serviceMock.Setup(x => x.Get(It.IsAny<int>())).Returns(Task.FromResult(new ActionResultV9(true, new AccountResponseModelV9())));

            //act
            IActionResult result = await controller.Get(default(int));

            //verify
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult).StatusCode.Should().Be(200);
            (result as OkObjectResult).Value.Should().NotBeNull();
            serviceMock.Verify(x => x.Get(It.IsAny<int>()));
        }

        [Fact]
        public async void Test_Account_Get_404()
        {
            //setup
            Mock<IAccountServiceV9> serviceMock = new Mock<IAccountServiceV9>();
            AccountV9Controller controller = new AccountV9Controller(serviceMock.Object);
            serviceMock.Setup(x => x.Get(It.IsAny<int>())).Returns(Task.FromResult(new ActionResultV9(false, default(object))));

            //act
            IActionResult result = await controller.Get(default(int));

            //verify
            result.Should().BeOfType<StatusCodeResult>();
            (result as StatusCodeResult).StatusCode.Should().Be(404);
            serviceMock.Verify(x => x.Get(It.IsAny<int>()));
        }

        [Fact]
        public async void Test_AccountService_Create_Success()
        {
            //setup
            Mock<IAccountRepositoryV9> repository = new Mock<IAccountRepositoryV9>();
            Mock<IAccountMapperV9> mapperMock = new Mock<IAccountMapperV9>();
            Mock<IAccountModelValidator> modelValidatorMock = new Mock<IAccountModelValidator>();
            Mock<IDateServiceV9> dateServiceMock = new Mock<IDateServiceV9>();
            Mock<IFBIServiceV9> fbiServiceMock = new Mock<IFBIServiceV9>();

            modelValidatorMock.Setup(x => x.Validate(It.IsAny<AccountRequestModelV9>())).Returns(new FluentValidation.Results.ValidationResult());
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult(true));
            mapperMock.Setup(x => x.MapEntityToResponse(It.IsAny<Account>())).Returns(new AccountResponseModelV9());
            repository.Setup(x => x.Create(It.IsAny<Account>()));

            IAccountServiceV9 service = new AccountServiceV9(repository.Object, modelValidatorMock.Object, fbiServiceMock.Object, dateServiceMock.Object, mapperMock.Object);

            //act
            ActionResultV9 result = await service.Create(new AccountRequestModelV9());

            //verify
            result.Success.Should().BeTrue();
            result.Object.Should().BeOfType<AccountResponseModelV9>();
            modelValidatorMock.Verify(x => x.Validate(It.IsAny<AccountRequestModelV9>()));
            mapperMock.Verify(x => x.MapEntityToResponse(It.IsAny<Account>()));
            repository.Verify(x => x.Create(It.IsAny<Account>()));
            fbiServiceMock.Verify(x => x.VerifyWithFBI(It.IsAny<string>()));
            dateServiceMock.Verify(x => x.Now());
        }

        [Fact]
        public async void Test_AccountService_Create_ValidationFailure()
        {
            //setup
            Mock<IAccountRepositoryV9> repository = new Mock<IAccountRepositoryV9>();
            Mock<IAccountMapperV9> mapperMock = new Mock<IAccountMapperV9>();
            Mock<IAccountModelValidator> modelValidatorMock = new Mock<IAccountModelValidator>();
            Mock<IDateServiceV9> dateServiceMock = new Mock<IDateServiceV9>();
            Mock<IFBIServiceV9> fbiServiceMock = new Mock<IFBIServiceV9>();

            modelValidatorMock.Setup(x => x.Validate(It.IsAny<AccountRequestModelV9>())).Returns(new ValidationResult(new List<ValidationFailure>() { new ValidationFailure(default(string), default(string)) }));
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult(true));
            mapperMock.Setup(x => x.MapEntityToResponse(It.IsAny<Account>())).Returns(new AccountResponseModelV9());
            repository.Setup(x => x.Create(It.IsAny<Account>()));

            IAccountServiceV9 service = new AccountServiceV9(repository.Object, modelValidatorMock.Object, fbiServiceMock.Object, dateServiceMock.Object, mapperMock.Object);

            //act
            ActionResultV9 result = await service.Create(new AccountRequestModelV9());

            //verify
            result.Success.Should().BeFalse();
            result.Object.Should().BeOfType<List<ValidationFailure>>();
            modelValidatorMock.Verify(x => x.Validate(It.IsAny<AccountRequestModelV9>()));
        }

        [Fact]
        public async void Test_AccountService_Create_FBIService_Failure()
        {
            //setup
            Mock<IAccountRepositoryV9> repository = new Mock<IAccountRepositoryV9>();
            Mock<IAccountMapperV9> mapperMock = new Mock<IAccountMapperV9>();
            Mock<IAccountModelValidator> modelValidatorMock = new Mock<IAccountModelValidator>();
            Mock<IDateServiceV9> dateServiceMock = new Mock<IDateServiceV9>();
            Mock<IFBIServiceV9> fbiServiceMock = new Mock<IFBIServiceV9>();

            modelValidatorMock.Setup(x => x.Validate(It.IsAny<AccountRequestModelV9>())).Returns(new FluentValidation.Results.ValidationResult());
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult(false));
            mapperMock.Setup(x => x.MapEntityToResponse(It.IsAny<Account>())).Returns(new AccountResponseModelV9());
            repository.Setup(x => x.Create(It.IsAny<Account>()));

            IAccountServiceV9 service = new AccountServiceV9(repository.Object, modelValidatorMock.Object, fbiServiceMock.Object, dateServiceMock.Object, mapperMock.Object);

            //act
            ActionResultV9 result = await service.Create(new AccountRequestModelV9());

            //verify
            result.Success.Should().BeFalse();
            result.Object.Should().BeOfType<ValidationResultV9>();
            fbiServiceMock.Verify(x => x.VerifyWithFBI(It.IsAny<string>()));
        }

        [Fact]
        public async void Test_AccountService_Get_Found()
        {
            //setup
            Mock<IAccountRepositoryV9> repository = new Mock<IAccountRepositoryV9>();
            Mock<IAccountMapperV9> mapperMock = new Mock<IAccountMapperV9>();
            Mock<IAccountModelValidator> modelValidatorMock = new Mock<IAccountModelValidator>();
            Mock<IDateServiceV9> dateServiceMock = new Mock<IDateServiceV9>();
            Mock<IFBIServiceV9> fbiServiceMock = new Mock<IFBIServiceV9>();

            modelValidatorMock.Setup(x => x.Validate(It.IsAny<AccountRequestModelV9>())).Returns(new ValidationResult());
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult(true));
            mapperMock.Setup(x => x.MapEntityToResponse(It.IsAny<Account>())).Returns(new AccountResponseModelV9());
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns(Task.FromResult<Account>(new Account()));

            IAccountServiceV9 service = new AccountServiceV9(repository.Object, modelValidatorMock.Object, fbiServiceMock.Object, dateServiceMock.Object, mapperMock.Object);

            //act
            ActionResultV9 result = await service.Get(default(int));

            //verify
            result.Success.Should().BeTrue();
            result.Object.Should().BeOfType<AccountResponseModelV9>();
            mapperMock.Verify(x => x.MapEntityToResponse(It.IsAny<Account>()));
            repository.Verify(x => x.Find(It.IsAny<int>()));
        }

        [Fact]
        public async void Test_AccountService_Get_Not_Found()
        {
            //setup
            Mock<IAccountRepositoryV9> repository = new Mock<IAccountRepositoryV9>();
            Mock<IAccountMapperV9> mapperMock = new Mock<IAccountMapperV9>();
            Mock<IAccountModelValidator> modelValidatorMock = new Mock<IAccountModelValidator>();
            Mock<IDateServiceV9> dateServiceMock = new Mock<IDateServiceV9>();
            Mock<IFBIServiceV9> fbiServiceMock = new Mock<IFBIServiceV9>();

            modelValidatorMock.Setup(x => x.Validate(It.IsAny<AccountRequestModelV9>())).Returns(new ValidationResult());
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult(true));
            mapperMock.Setup(x => x.MapEntityToResponse(It.IsAny<Account>())).Returns(new AccountResponseModelV9());
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns(Task.FromResult<Account>(null));

            IAccountServiceV9 service = new AccountServiceV9(repository.Object, modelValidatorMock.Object, fbiServiceMock.Object, dateServiceMock.Object, mapperMock.Object);

            //act
            ActionResultV9 result = await service.Get(default(int));

            //verify
            result.Success.Should().BeFalse();
            repository.Verify(x => x.Find(It.IsAny<int>()));
        }

        [Fact]
        public void Test_Mapper_MapRequestToEntity()
        {
            //setup
            IAccountMapperV9 mapper = new AccountMapperV9();

            var request = new AccountRequestModelV9(1, "A", "000-05-1120", DateTime.Parse("7/3/2018 4:41:42 PM"), "A");

            //act
            Account entity = mapper.MapRequestToEntity(request);

            //verify
            entity.DateCreated.Should().Be(DateTime.Parse("7/3/2018 4:41:42 PM"));
            entity.Id.Should().Be(1);
            entity.Name.Should().Be("A");
            entity.SSN.Should().Be("000-05-1120");
            entity.Token.Should().Be("A");
        }

        [Fact]
        public void Test_Mapper_MapEntityToResponse()
        {
            //setup
            IAccountMapperV9 mapper = new AccountMapperV9();

            var entity = new Account(1, "A", "000-05-1120", DateTime.Parse("7 /3/2018 4:41:42 PM"), "A");

            //act
            AccountResponseModelV9 response = mapper.MapEntityToResponse(entity);

            //verify
            response.DateCreated.Should().Be(DateTime.Parse("7/3/2018 4:41:42 PM"));
            response.Id.Should().Be(1);
            response.Name.Should().Be("A");
            response.SSN.Should().Be("000-05-1120");
        }

        [Fact]
        public void Test_ModelValidator_Name_NotEmpty_Valid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldNotHaveValidationErrorFor(x => x.Name, "A");
        }

        [Fact]
        public void Test_ModelValidator_Name_NotEmpty_Invalid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldHaveValidationErrorFor(x => x.Name, "");
            validator.ShouldHaveValidationErrorFor(x => x.Name, null as string);
        }

        [Fact]
        public void Test_ModelValidator_NameUnique_Valid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldNotHaveValidationErrorFor(x => x.Name, "A");
        }

        [Fact]
        public void Test_ModelValidator_NameUnique_Invalid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldHaveValidationErrorFor(x => x.Name, "A");
        }


        [Fact]
        public void Test_ModelValidator_GlobalCustomerId_NotEmpty_Valid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldNotHaveValidationErrorFor(x => x.SSN, "000-05-1120");
        }

        [Fact]
        public void Test_ModelValidator_GlobalCustomerId_NotEmpty_Invalid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldHaveValidationErrorFor(x => x.SSN, string.Empty);
        }

        [Fact]
        public void Test_ModelValidator_Token_NotEmpty_Valid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldNotHaveValidationErrorFor(x => x.Token, "A");
        }

        [Fact]
        public void Test_ModelValidator_Token_NotEmpty_Invalid()
        {
            //setup
            Mock<IAccountRepositoryV9> repositoryMock = new Mock<IAccountRepositoryV9>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountRequestModelValidatorV9 validator = new AccountRequestModelValidatorV9(repositoryMock.Object);

            //act and verify
            validator.ShouldHaveValidationErrorFor(x => x.Token, "");
            validator.ShouldHaveValidationErrorFor(x => x.Token, null as string);
        }


        [Fact]
        public void Test_Account_Repository_Create()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV9 repository = new AccountRepositoryV9(context);

            //act
            repository.Create(new Account());

            //verify
            context.Accounts.Count().Should().Be(1);
        }


        [Fact]
        public void Test_Account_Repository_Exists_Record_Exists()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV9 repository = new AccountRepositoryV9(context);
            context.Accounts.Add(new Account(default(int), "test", default(string), default(DateTime), default(string)));
            context.SaveChanges();

            //act
            bool exists = repository.Exists("test");

            //verify
            exists.Should().BeTrue();
        }

        [Fact]
        public void Test_Account_Repository_Exists_Record_Not_Exists()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV9 repository = new AccountRepositoryV9(context);

            //act
            bool exists = repository.Exists("testing");
            exists.Should().BeFalse();

        }

        [Fact]
        public async void Test_Account_Repository_Find_Record_Exists()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV9 repository = new AccountRepositoryV9(context);
            context.Accounts.Add(new Account(1, default(string), default(string), default(DateTime), default(string)));
            context.SaveChanges();

            //act
            Account record = await repository.Find(1);

            //verify
            record.Should().NotBeNull();
        }

        [Fact]
        public async void Test_Account_Repository_Find_Record_Not_Exists()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV9 repository = new AccountRepositoryV9(context);

            //act
            Account record = await repository.Find(1);

            //verify
            record.Should().BeNull();

        }

        [Fact]
        public void Test_DatetimeService()
        {
            //setup
            IDateServiceV9 service = new DateServiceV9();
            DateTime now = DateTime.Now;

            //act
            DateTime result = service.Now();

            //verify
            result.Should().BeCloseTo(now, 1000);
        }


        [Fact]
        public async void IntegrationTest_FBIService_Success()
        {
            //setup
            IFBIServiceV9 service = new FBIServiceV9("https://jsonplaceholder.typicode.com/posts/1");

            //act
            bool result = await service.VerifyWithFBI(default(string));

            //verify
            result.Should().BeTrue();
        }

        [Fact]
        public async void IntegrationTest_FBIService_Failure()
        {
            //setup
            IFBIServiceV9 service = new FBIServiceV9("https://jsonplaceholder.typicode.com/posts/fail");

            //act
            bool result = await service.VerifyWithFBI(default(string));

            //verify
            result.Should().BeFalse();
        }

    }
}





/*
 * We made lots of changes to really make our code testable. 
 * We introduced Fluent Validation to make testing easier. 
 * We wrote all of our tests including an integration test for the FBI service
 * 
 * There is at least one big issue I still see with these tests. There is a lot of 
 * repetitive setup code. I would take the time to build factories to build your default mocks and
 * make the code as DRY as possible. 
 * 
 * 
 */