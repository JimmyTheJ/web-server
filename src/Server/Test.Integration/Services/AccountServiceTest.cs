using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Services.Concrete;

namespace VueServer.Test.Integration.Services
{
    public class AccountServiceTest
    {
        private Mock<AccountService> AccountService;

        public AccountServiceTest()
        {
            AccountService = new Mock<AccountService>();
            //AccountService.Setup<>
        }


    }
}
