using MejorNX.HLE.Horizon.Service.acc;
using MejorNX.HLE.Horizon.Service.am;
using MejorNX.HLE.Horizon.Service.apm;
using MejorNX.HLE.Horizon.Service.aud;
using MejorNX.HLE.Horizon.Service.fspsrv;
using MejorNX.HLE.Horizon.Service.hid;
using MejorNX.HLE.Horizon.Service.lm;
using MejorNX.HLE.Horizon.Service.ns;
using MejorNX.HLE.Horizon.Service.nv;
using MejorNX.HLE.Horizon.Service.pctl;
using MejorNX.HLE.Horizon.Service.set;
using MejorNX.HLE.Horizon.Service.sm;
using MejorNX.HLE.Horizon.Service.ssl;
using MejorNX.HLE.Horizon.Service.vi;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service
{
    public static class Factory
    {
        public static ICommand GetService(string name)
        {
            switch (name)
            {
                case "acc:u0":      return new IAccountServiceForApplication();
                case "aoc:u":       return new IAddOnContentManager();
                case "appletAE":    return new IAllSystemAppletProxiesService();
                case "appletOE":    return new IApplicationProxyService();
                case "apm":         return new IManager();
                case "apm:p":       return new IManager();
                case "audout:u":    return new IAudioOutManager();
                case "fsp-srv":     return new IFileSystemProxy();
                case "hid":         return new IHidServer();
                case "lm":          return new ILogService();
                case "nvdrv":       return new INvDrvServices();
                case "nvdrv:a":     return new INvDrvServices();
                case "sm:":         return new IUserInterface();
                case "pctl:a":      return new IParentalControlServiceFactory();
                case "set":         return new ISettingsServer();
                case "ssl":         return new ISslService();
                case "vi:m":        return new IManagerRootService();
                case "vi:s":        return new IManagerRootService();
                case "vi:u":        return new IApplicationRootService();
                default: throw new NotImplementedException(name);
            }
        }
    }
}
