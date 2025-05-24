using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerApp.Services
{
    public static class TaskEvents
    {
        public static event Action? TaskSaved;

        public static void RaiseTaskSaved()
        {
            TaskSaved?.Invoke();
        }
    }

}
