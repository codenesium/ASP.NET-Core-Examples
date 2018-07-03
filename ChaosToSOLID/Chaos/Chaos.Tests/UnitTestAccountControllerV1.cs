using Chaos.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace Chaos.Tests
{
    public class UnitTestAccountControllerV1
    {
        [Fact]
        public void Test_Account_Create()
        {
            AccountControllerV1 controller = new AccountControllerV1();

            Guid customerId = Guid.NewGuid();
            IActionResult result = controller.Create(new Account() { Id = 1, Name = "Test",GlobalCustomerId=customerId });
            Assert.True(result is OkObjectResult);
            var response = (result as OkObjectResult);
            var responseItem = response.Value as Account;
            Assert.True(responseItem.Id == 1);
            Assert.True(responseItem.Name == "Test");
            Assert.True(responseItem.GlobalCustomerId == customerId);
            Assert.True(responseItem.DateCreated != null);
        }
















/*
* What's wrong with this test? It covers most of our method but it has some issues. 
* It doesn't test the validation.
* It tests the fields on the response which is very brittle. Any time we add or change a fields
* we're going to have to update every single test that references that field.
* We don't actually test that the item was added to the database. We just get something back from the controller
* and assume it's good.
* We didn't verify that anything in the method actually got called. Someone could replace
* the call to the database and just return a new object and this test would pass. 
* Our DateCreated field isn't really being tested. How can we? The create method calls
* DateTime.Now. How can we get that value?
* The customer id isn't the same in every test. Unit tests need to be deterministic. It's unlikely
* to fail because this guid is different every test run but for more complex tests it leads to 
* hard to replicate failures. 
* 
* Introduce NCrunch.
*/
    }
}
