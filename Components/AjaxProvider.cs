using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;

namespace OS_40F_PostNL
{
    public class AjaxProvider : AjaxInterface
    {
        public override string Ajaxkey { get; set; }

        public override string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
            var objCtrl = new NBrightBuyController();

            var strOut = "OS_40F_PostNL Ajax Error";

            if (PluginUtils.CheckPluginSecurity(PortalSettings.Current.PortalId, "OS_40F_PostNL"))
            {
                // NOTE: The paramCmd MUST start with the plugin ref. in lowercase. (links ajax provider to cmd)
                switch (paramCmd)
                {
                    case "os_40f_postnl_savesettings":
                        strOut = objCtrl.SavePluginSinglePageData(context);
                        break;
                    case "os_40f_postnl_selectlang":
                        objCtrl.SavePluginSinglePageData(context);
                        var nextlang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang");
                        var info = objCtrl.GetPluginSinglePageData("os_40f_postnl_settings", "OS_40F_POSTNL_SETTINGS", nextlang);
                        strOut = NBrightBuyUtils.RazorTemplRender("settingsfields.cshtml", 0, "", info, "/DesktopModules/NBright/OS_40F_PostNL", "config", nextlang,
                            StoreSettings.Current.Settings());
                        break;
                }
            }

            return strOut;
        }

        public override void Validate()
        {
        }
    }
}