using System.Diagnostics;
using System.Threading.Tasks;

namespace UpdaterLibrary
{
    public interface IUpdateExecuter
    {
        Task<UpdatingJob> RunUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null);
        Task<bool> CheckForUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null);
        Task<LastestVersionInfo> GetLatestVerionAsync(UpdateParameter updateParameter);
    }

    public class UpdatingJob
    {
        public bool? IsSuccess { get; set; }
        public bool IsComplete { get; set; }
        public string MessageError { get; set; }
        public Process ProcessReplaceFile { get; set; }

        public void WaitUpdateComplete()
        {
            if (IsComplete) return;
            ProcessReplaceFile?.WaitForExit();
            IsComplete = true;
        }
    }
}
