using Chaos.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using Xunit;

namespace Chaos.Tests
{
    public class UnitTestAccountControllerV2
    {
        [Fact]
        public async void Test_Account_Create()
        {

            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test1");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            AccountControllerV2 controller = new AccountControllerV2(context);
            Guid customerId = Guid.NewGuid();
            IActionResult result = controller.Create(new Account() { Id = 1, Name = "Checking", GlobalCustomerId = customerId });
            Assert.True(result != null);
            Assert.True(context.Accounts.Count() == 1);
            Assert.True(context.Accounts.First().Id == 1);
            Assert.True(context.Accounts.First().Name == "Checking");
            Assert.True(context.Accounts.First().DateCreated != null);
            Assert.True(context.Accounts.First().GlobalCustomerId == customerId);
        }

/*
*
* 
* 
* 
* 
* 
* What's wrong with this test?
* We're passing the database context with dependency injection which means we can verify
* in the context that the record is actually create in the database.
* We're still testing every field which creates a lot of work when fields change.
* We're still not checking validation.
* We still have the date time issue. 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
*/


        [Fact]
        public async void Test_Account_Create_With_Validation()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test2");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            AccountControllerV2 controller = new AccountControllerV2(context);

            //act
            Guid customerId = Guid.NewGuid();
            IActionResult result = controller.Create(new Account() { Id = 1, Name = "Checking", GlobalCustomerId=customerId });
            Assert.True(result != null); // the result isn't null
            Assert.True(context.Accounts.Count() == 1); // the result has the count we expect
            Assert.True(context.Accounts.First().Id == 1); // the record has the id we expect
            Assert.True(context.Accounts.First().Name == "Checking"); // it has the right name
            Assert.True(context.Accounts.First().DateCreated != null); // we saved a date
            Assert.True(context.Accounts.First().GlobalCustomerId == customerId);
            // test that creating a blank name returns a 422 code
            IActionResult result2 = controller.Create(new Account() { Id = 1, Name = "" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

            // test that creating duplicate names returns 4222
            IActionResult resultUnique = controller.Create(new Account() { Id = 2, Name = "Checking1" });
            IActionResult resultUniqueTest = controller.Create(new Account() { Id = 2, Name = "Checking1" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

        }

    }
}

/*
 * Ok we're testing validation now. 
 * We have 100% coverage on our method yay!!!
 * There are a few issues though.
 * We're still checking every field.
 * Our test has 6 asserts. Not exactly a unit test. When it breaks how long will it take to figure out which thing broke?
 * If this table has 20 fields how long would this test be? 1k lines?
 * If our method needs to call an external service how would we mock that and verify the methods are being called?
 * We still have the date time issue.  
 * We're using an In memory EntityFramework database. That means that this is technically an integration test still. 
 * 
 * 
 * 
 * 
 * 
 */
