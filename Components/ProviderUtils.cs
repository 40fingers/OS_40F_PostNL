using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_40F_PostNL
{
    public class ProviderUtils
    {
        #region GetProviderSettings
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("os_40f_postnlpayment", "OS_40F_POSTNLPAYMENT", Utils.GetCurrentCulture());
            return info;
        }
        #endregion
    }
}