using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VRCP.Log;

namespace VRCP.Application
{
    internal static class ApplicationHandler
    {
        public static string GetGuid() => ApplicationHandler.APP_GUID;

        // tiny asynchronous wrapper, couldnt be bothered with synchronous op
        public static IPromise PreventMultipleInstance()
        {
            Promise promise = new Promise();
            ApplicationHandler.Internal_PreventMultipleInstance(promise);
            return promise;
        }

        // https://stackoverflow.com/a/54640621
        private static void Internal_PreventMultipleInstance(Promise p)
        {
            var applicationId = ApplicationHandler.APP_GUID;
            // Under Windows this is:
            //      C:\Users\SomeUser\AppData\Local\Temp\ 
            // Linux this is:
            //      /tmp/
            var temporaryDirectory = Path.GetTempPath();

            // Application ID (Make sure this guid is different accross your different applications!
            var applicationGuid = applicationId + ".process-lock";

            // file that will serve as our lock
            var fileFulePath = Path.Combine(temporaryDirectory, applicationGuid);

            try
            {
                // Prevents other processes from reading from or writing to this file
                var _InstanceLock = new FileStream(fileFulePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _InstanceLock.Lock(0, 0);
                Logger.Trace("Aquired lock");

                // TODO: investigate why we need a reference to file stream. Without this GC releases the lock!
                System.Timers.Timer t = new System.Timers.Timer()
                {
                    Interval = 50,
                    Enabled = true,
                };
                t.Elapsed += (a, b) =>
                {
                    try
                    {
                        _InstanceLock.Lock(0, 0);
                        p.Resolve();
                    }
                    catch (Exception ex) // errors after resolve for some reason
                    {
                        // if we already resolved this no need to reject it
                        if (p.CurState != PromiseState.Resolved)
                            p.Reject(ex);
                    }
                };
                t.Start();
            }
            catch (Exception ex)
            {
                p.Reject(ex);
            }
        }

        private const string APP_GUID = "61eb7492-c33e-456b-8fd7-59ab2eb9e9d4";
    }
}
