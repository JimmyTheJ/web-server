using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models.Modules;

namespace VueServer.Services.Interface
{
    public interface IModuleService
    {
        Task<IResult<IEnumerable<ModuleAddOn>>> GetActiveModulesForUser();

        Task<IResult<IEnumerable<ModuleAddOn>>> GetAllModules();

        Task<IResult<IEnumerable<UserHasModuleAddOn>>> GetActiveModulesForAllUsers();

        Task<IResult<UserHasModuleAddOn>> AddModuleToUser(UserHasModuleAddOn userModule);

        Task<IResult<bool>> DeleteModuleFromUser(UserHasModuleAddOn userModule);
        
    }
}
