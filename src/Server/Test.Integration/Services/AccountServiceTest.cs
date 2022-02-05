using Moq;
using VueServer.Services.Account;

namespace VueServer.Test.Integration.Services
{
    public class AccountServiceTest
    {
        private readonly Mock<AccountService> AccountService;

        public AccountServiceTest()
        {
            AccountService = new Mock<AccountService>();
            //AccountService.Setup<>
        }


    }
}
