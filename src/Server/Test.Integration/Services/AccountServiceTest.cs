﻿using Moq;
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
