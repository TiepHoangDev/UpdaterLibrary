using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdaterLibrary
{
    public interface IUpdateExecuter
    {
        Task<string> RunUpdateAsync(UpdateParameter updateParameter);
        Task<bool> CheckForUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null);
        Task<LastestVersionInfo> GetLatestVerionAsync(UpdateParameter updateParameter);
    }
}
