using System.Threading.Tasks;

namespace UpdaterLibrary
{
    public interface IUpdateExecuter
    {
        Task<string> RunUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null);
        Task<bool> CheckForUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null);
        Task<LastestVersionInfo> GetLatestVerionAsync(UpdateParameter updateParameter);
    }
}
