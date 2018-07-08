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
    public class UnitTestAccountControllerV4
    {
        [Fact]
        public async void Test_Account_Create_With_Validation()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test2");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            FBIService fbiService = new FBIService();
            AccountControllerV4 controller = new AccountControllerV4(context, fbiService);

            //act
           
            IActionResult result = await controller.Create(new Account() { Id = 1, Name = "Checking", SSN = "000-05-1120" });
            Assert.True(result != null); // the result isn't null
            Assert.True(context.Accounts.Count() == 1); // the result has the count we expect
            Assert.True(context.Accounts.First().Id == 1); // the record has the id we expect
            Assert.True(context.Accounts.First().Name == "Checking"); // it has the right name
            Assert.True(context.Accounts.First().DateCreated != null); // we saved a date
            Assert.True(context.Accounts.First().SSN == "000-05-1120");
            // test that creating a blank name returns a 422 code
            IActionResult result2 = await controller.Create(new Account() { Id = 1, Name = "" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

            // test that creating duplicate names returns 4222
            IActionResult resultUnique = await controller.Create(new Account() { Id = 2, Name = "Checking1" });
            IActionResult resultUniqueTest = await controller.Create(new Account() { Id = 2, Name = "Checking1" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

        }









/*
* This is looking better. We're injecting the FBIService now. But how to we test it 
* without call out to the FBI system every time we want to run a test?
*
*/
















        [Fact]
        public async void Test_Account_Create_With_Validation_With_FBIService_mock()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test2");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            FBIService fbiService = new FBIService();

            Mock<IFBIService> fbiServiceMock = new Mock<IFBIService>();
            fbiServiceMock.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult<bool>(true));
            AccountControllerV4 controller = new AccountControllerV4(context, fbiService);

            //act
            IActionResult result = await controller.Create(new Account() { Id = 1, Name = "Checking", SSN = "000-05-1120" });
            Assert.True(result != null); // the result isn't null
            Assert.True(context.Accounts.Count() == 1); // the result has the count we expect
            Assert.True(context.Accounts.First().Id == 1); // the record has the id we expect
            Assert.True(context.Accounts.First().Name == "Checking"); // it has the right name
            Assert.True(context.Accounts.First().DateCreated != null); // we saved a date
            Assert.True(context.Accounts.First().SSN == "000-05-1120");
            // test that creating a blank name returns a 422 code
            IActionResult result2 = await controller.Create(new Account() { Id = 1, Name = "", SSN = "000-05-1120" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

            // test that creating duplicate names returns 422
            IActionResult resultUnique = await controller.Create(new Account() { Id = 2, Name = "Checking1", SSN = "000-05-1120" });
            IActionResult resultUniqueTest = await controller.Create(new Account() { Id = 3, Name = "Checking1", SSN = "000-05-1120" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);


            // we're now able to test that if the fbi service returns false we handle it with a 400 response
            Mock<IFBIService> fbiServiceMock2 = new Mock<IFBIService>();
            fbiServiceMock2.Setup(x => x.VerifyWithFBI(It.IsAny<string>())).Returns(Task.FromResult<bool>(false));
            AccountControllerV4 controller2 = new AccountControllerV4(context, fbiServiceMock2.Object);
            IActionResult result3 = await controller2.Create(new Account() { Id = 4, Name = "CheckingNew", SSN = "000-05-1120" });
            Assert.True((result3 as StatusCodeResult).StatusCode == 400);

        }
    }
}



















/*
 * We are back tp 100% coverage. We're testing that the FBI service response is handled correctly in our system.
 * We introduced mocking which lets us pass stubbed in objects in place of the actual classes. 
 * That let's us change values and test the various outcomes.
 * 
 * Ok what's wrong with Test_Account_Create_With_Validation_With_FBIService_mock?
 * It should be obvious that we're 30 minutes into a new system and we have a 40 line unit test. 
 * How maintainable is this?
 * What happens when we add a new field to the Account table? How many places will need to change in this one method?
 * Is our unit test easy to read and diagnose failures?
 * The FBI service needs some sort of validation and testing where would that go?
 * 
 * Our controlelr is doing a lot of stuff. It's validating with the FBI service and calling the repository and validaiton.
 * 
 * 
 * 
 * 
 * It may be like a slow moving train but this application has already spiraled into chaos.
 */
