using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdaterLibrary
{
    public interface IUpdateExecuter
    {
        Task<string> CheckForUpdate(UpdateParameter info);
    }
}
