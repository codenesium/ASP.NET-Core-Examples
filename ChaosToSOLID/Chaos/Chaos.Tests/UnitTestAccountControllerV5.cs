using Chaos.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace Chaos.Tests
{
    public class UnitTestAccountControllerV5
    {
        [Fact]
        public async void Test_Account_Create_Happy_Path()
        {
            //setup
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();
            Mock<IAccountModelValiatorV5> modelValidatorMock = new Mock<IAccountModelValiatorV5>();
            Mock<IFBIServiceV5> fbiServiceMock = new Mock<IFBIServiceV5>();

            // all of our mocks are returning what they should for a successful request
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<Guid>())).Returns(Task.FromResult<bool>(true));
            modelValidatorMock.Setup(x => x.Validate(It.IsAny<Account>())).Returns(new ValidationResultV5());

            //act
            AccountControllerV5 controller = new AccountControllerV5(repositoryMock.Object, modelValidatorMock.Object, fbiServiceMock.Object);
            IActionResult result = await controller.Create(new Account());

            //verify
            Assert.True((result as OkObjectResult).StatusCode == 200); // ok result
            modelValidatorMock.Verify(x => x.Validate(It.IsAny<Account>())); // verify the methods on our objects are actually called
            fbiServiceMock.Verify(x => x.VerifyWithFBI(It.IsAny<Guid>()));
        }


        [Fact]
        public async void Test_Account_Create_Validation_Failed()
        {
            //setup
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();
            Mock<IAccountModelValiatorV5> modelValidatorMock = new Mock<IAccountModelValiatorV5>();
            Mock<IFBIServiceV5> fbiServiceMock = new Mock<IFBIServiceV5>();

            // all of our mocks are returning what they should for a successful request
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<Guid>())).Returns(Task.FromResult<bool>(true));
            repositoryMock.Setup(x => x.Create(It.IsAny<Account>()));
            modelValidatorMock.Setup(x => x.Validate(It.IsAny<Account>())).Returns(new ValidationResultV5(false, "error"));

            //act
            AccountControllerV5 controller = new AccountControllerV5(repositoryMock.Object, modelValidatorMock.Object, fbiServiceMock.Object);
            IActionResult result = await controller.Create(new Account());

            //verify
            Assert.True((result as ObjectResult).StatusCode == 422);
        }


        [Fact]
        public async void Test_Account_Create_FBIService_Failed()
        {
            //setup
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();
            Mock<IAccountModelValiatorV5> modelValidatorMock = new Mock<IAccountModelValiatorV5>();
            Mock<IFBIServiceV5> fbiServiceMock = new Mock<IFBIServiceV5>();

            // all of our mocks are returning what they should for a successful request
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<Guid>())).Returns(Task.FromResult<bool>(false));
            repositoryMock.Setup(x => x.Create(It.IsAny<Account>()));
            modelValidatorMock.Setup(x => x.Validate(It.IsAny<Account>())).Returns(new ValidationResultV5());

            //act
            AccountControllerV5 controller = new AccountControllerV5(repositoryMock.Object, modelValidatorMock.Object, fbiServiceMock.Object);
            IActionResult result = await controller.Create(new Account());

            //verify
            Assert.True((result as StatusCodeResult).StatusCode == 400);
        }


        [Fact]
        public void Test_Account_Model_Validator_Empty_Account_Name()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();

            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.NewGuid();
            account.Name = string.Empty;
            ValidationResultV5 result = valiator.Validate(account);

            Assert.False(result.Success);
            Assert.Equal("Account name cannot be empty", result.Message);
        }

        [Fact]
        public void Test_Account_Model_Validator_Valid_Account_Name()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();

            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.NewGuid();
            account.Name = "Test";
            ValidationResultV5 result = valiator.Validate(account);

            Assert.True(result.Success);
            Assert.True(result.Message == string.Empty);
        }

        [Fact]
        public void Test_Account_Model_Validator_Empty_CustomerId()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();

            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.Empty;
            account.Name = "test";
            ValidationResultV5 result = valiator.Validate(account);

            Assert.False(result.Success);
            Assert.Equal("Customer id cannot be empty", result.Message);
        }

        [Fact]
        public void Test_Account_Model_Validator_Valid_CustomerId()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();

            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.NewGuid();
            account.Name = "test";
            ValidationResultV5 result = valiator.Validate(account);

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public async void Test_Account_Model_Validator_Account_Exists()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.NewGuid();
            account.Name = "test";
            ValidationResultV5 result = valiator.Validate(account);

            Assert.False(result.Success);
            Assert.Equal("Account name already exists", result.Message);
        }

        [Fact]
        public async void Test_Account_Model_Validator_Account_Not_Exists()
        {
            Mock<IAccountRepositoryV5> repositoryMock = new Mock<IAccountRepositoryV5>();
            repositoryMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            AccountModelValiatorV5 valiator = new AccountModelValiatorV5(repositoryMock.Object);
            Account account = new Account();
            account.DateCreated = DateTime.Now;
            account.GlobalCustomerId = Guid.NewGuid();
            account.Name = "test";
            ValidationResultV5 result = valiator.Validate(account);

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Message);
        }


        [Fact]
        public async void Test_Account_Repository_Create()
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV5 repository = new AccountRepositoryV5(context);
            repository.Create(new Account());
            Assert.True(context.Accounts.Count() == 1);
        }


        [Fact]
        public async void Test_Account_Repository_Exists_Record_Exists()
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV5 repository = new AccountRepositoryV5(context);

            var account = new Account();
            account.Name = "testing";
            context.Accounts.Add(account);
            context.SaveChanges();

            bool exists = repository.Exists("testing");
            Assert.True(exists);
        }

        [Fact]
        public async void Test_Account_Repository_Exists_Record_Not_Exists()
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            IAccountRepositoryV5 repository = new AccountRepositoryV5(context);

            bool exists = repository.Exists("testing");
            Assert.False(exists);
        }
    }
}



















/*
 * We've closer to having separation of concern in our controller.
 * The validation, repository and FBI service are in their own classes. You should be able to work on them
 * in isolation with their tests without worrying about the rest of the controller.
 * We're testing each validation method in isolation for a positive and negative result. There are some issues still because we're
 * having to fill out an object each time which means if we ever change the properties or add a property to our
 * object we will have to update all of the tests.
 * Our controller has tests for a positive or negative result from the validator or the FBI service. 
 * The FBI service can be mocked but it can't really be unit tested. HttpClient is difficult to mock and that would be an advanced 
 * topic. Most people would agree it should be integration tested.
 * Our repository is being integration tested with the in memory database.
 * 
 * Our controller is more slim than when we started.
 * Our tests look more like unit tests. They test one thing although they may verify that methods on the mocks are called.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * We're on our way to being SOLID!
 * 
 */