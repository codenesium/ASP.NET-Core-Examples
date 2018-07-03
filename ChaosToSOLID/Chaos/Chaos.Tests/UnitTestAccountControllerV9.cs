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

namespace Chaos.Tests
{
    public class UnitTestAccountControllerV9
    {
        [Fact]
        public async void Test_Account_Create_200()
        {
            //setup
            Mock<IAccountServiceV9> serviceMock = new Mock<IAccountServiceV9>();
            serviceMock.Setup(x => x.Create(It.IsAny<AccountRequestModelV9>())).Returns(Task.FromResult(new ActionResultV9(true, default(object))));
            AccountControllerV9 controller = new AccountControllerV9(serviceMock.Object);

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
            AccountControllerV9 controller = new AccountControllerV9(serviceMock.Object);

            //act
            IActionResult result = await controller.Create(new AccountRequestModelV9());

            //verify
            result.Should().BeOfType<StatusCodeResult>();
            (result as StatusCodeResult).StatusCode.Should().Be(422);
            serviceMock.Verify(x => x.Create(It.IsAny<AccountRequestModelV9>()));
        }


        [Fact]
        public void Test_Account_Get_200()
        {
            //setup
            Mock<AccountServiceV9> serviceMock = new Mock<AccountServiceV9>();
            AccountControllerV9 controller = new AccountControllerV9(serviceMock.Object);
            Assert.True(false);
        }

        [Fact]
        public void Test_Account_Get_404()
        {
            //setup
            Mock<AccountServiceV9> serviceMock = new Mock<AccountServiceV9>();
            AccountControllerV9 controller = new AccountControllerV9(serviceMock.Object);

            Assert.True(false);
        }

        [Fact]
        public void Test_AccountService_Create_Success()
        {

            Assert.True(false);
        }

        [Fact]
        public void Test_AccountService_Create_ValidationFailure()
        {

            Assert.True(false);
        }

        [Fact]
        public void Test_AccountService_Create_FBIService_Failure()
        {

            Assert.True(false);
        }

        [Fact]
        public void Test_AccountService_Get_Found()
        {

            Assert.True(false);
        }

        [Fact]
        public void Test_AccountService_Get_Not_Found()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_Mapper_MapRequestToEntity()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_Mapper_MapEntityToResponse()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_Name_NotEmpty_Valid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_Name_NotEmpty_Invalid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_NameUnique_Valid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_NameUnique_Invalid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_GlobalCustomerId_NotEmpty_Valid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_GlobalCustomerId_NotEmpty_Invalid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_Token_NotEmpty_Valid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_ModelValidator_Token_NotEmpty_Invalid()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_Repository_Create()
        {
            Assert.True(false);
        }

        [Fact]
        public void Test_Repository_Find()
        {
            Assert.True(false);
        }

    }
}


/*
 * At this point in time we have what we expect to be be required to test out app. 
 * 
 * 
 * 
 * 
 */