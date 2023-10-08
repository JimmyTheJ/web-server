using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models.Modules;

namespace VueServer.Modules.Core.Services.Module
{
    public interface IModuleService
    {
        Task<IServerResult<IEnumerable<string>>> GetEnabledModules();
        Task<IServerResult<IEnumerable<ModuleAddOn>>> GetActiveModulesForUser();
        Task<IServerResult<IEnumerable<ModuleAddOn>>> GetAllModules();
        Task<IServerResult<IDictionary<string, IList<ModuleAddOn>>>> GetActiveModulesForAllUsers();
        Task<IServerResult<IEnumerable<UserHasModuleAddOn>>> GetModulesAndFeaturesForOtherUser(string user);
        Task<IServerResult<bool>> AddModuleToUser(UserHasModuleAddOn userModule);
        Task<IServerResult<bool>> DeleteModuleFromUser(UserHasModuleAddOn userModule);

        Task<IServerResult<bool>> AddFeatureToUser(UserHasModuleFeature userFeature);
        Task<IServerResult<bool>> DeleteFeatureFromUser(UserHasModuleFeature userFeature);
        Task<IServerResult<bool>> DoesUserHaveModule(string user, string module);
        Task<IServerResult<bool>> DoesUserHaveFeature(string user, string feature);

    }
}
