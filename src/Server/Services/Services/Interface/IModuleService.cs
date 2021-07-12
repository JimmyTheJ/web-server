using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models.Modules;

namespace VueServer.Services.Interface
{
    public interface IModuleService
    {
        Task<IResult<IEnumerable<ModuleAddOn>>> GetActiveModulesForUser();
        Task<IResult<IEnumerable<ModuleAddOn>>> GetAllModules();
        Task<IResult<IDictionary<string, IList<ModuleAddOn>>>> GetActiveModulesForAllUsers();
        Task<IResult<IEnumerable<UserHasModuleAddOn>>> GetModulesAndFeaturesForOtherUser(string user);
        Task<IResult<bool>> AddModuleToUser(UserHasModuleAddOn userModule);
        Task<IResult<bool>> DeleteModuleFromUser(UserHasModuleAddOn userModule);

        Task<IResult<bool>> AddFeatureToUser(UserHasModuleFeature userFeature);
        Task<IResult<bool>> DeleteFeatureFromUser(UserHasModuleFeature userFeature);
        Task<IResult<bool>> DoesUserHaveModule(string user, string module);
        Task<IResult<bool>> DoesUserHaveFeature(string user, string feature);

    }
}
