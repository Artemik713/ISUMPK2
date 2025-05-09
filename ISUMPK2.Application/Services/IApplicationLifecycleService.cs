using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface IApplicationLifecycleService
    {
        Task ExitApplication();
        Task RestartApplication();
        bool IsShuttingDown { get; }
    }
}
