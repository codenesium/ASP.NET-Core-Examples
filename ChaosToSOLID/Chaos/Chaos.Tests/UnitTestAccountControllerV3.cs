using Chaos.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using Xunit;

namespace Chaos.Tests
{
    public class UnitTestAccountControllerV3
    {
        [Fact]
        public async void Test_Account_Create_With_Validation_Cant_Test_FBI_Service()
        {
            //setup
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase("test2");
            var context = new ApplicationDbContext(optionsBuilder.Options);
            AccountControllerV3 controller = new AccountControllerV3(context);

            //act
            Guid customerId = Guid.NewGuid();
            IActionResult result = await controller.Create(new Account() { Id = 1, Name = "Checking", GlobalCustomerId=customerId });
            Assert.True(result != null); // the result isn't null
            Assert.True(context.Accounts.Count() == 1); // the result has the count we expect
            Assert.True(context.Accounts.First().Id == 1); // the record has the id we expect
            Assert.True(context.Accounts.First().Name == "Checking"); // it has the right name
            Assert.True(context.Accounts.First().DateCreated != null); // we saved a date
            Assert.True(context.Accounts.First().GlobalCustomerId == customerId);
            // test that creating a blank name returns a 422 code
            IActionResult result2 = await controller.Create(new Account() { Id = 1, Name = "" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);

            // test that creating duplicate names returns 4222
            IActionResult resultUnique = await controller.Create(new Account() { Id = 2, Name = "Checking1" });
            IActionResult resultUniqueTest = await controller.Create(new Account() { Id = 2, Name = "Checking1" });
            Assert.True((result2 as StatusCodeResult).StatusCode == 422);


            //How can we test that our FBI service worked or failed? We really can't.
        }

    }
}




/*
 * The problem with how we designed this is that there's no way to test the call to the FBI service.
 * We need to inject it to be able to test it.
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