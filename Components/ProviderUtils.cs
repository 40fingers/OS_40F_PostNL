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
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using NBrightBuy.render;
using NBrightCore.common;
using NBrightCore.TemplateEngine;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OS_40F_PostNL.Components;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Encoding = System.Text.Encoding;
using Logger = Nevoweb.DNN.NBrightBuy.Components.Logger;

namespace OS_40F_PostNL
{
    public static class ProviderUtils
    {
        #region GetProviderSettings
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("os_40f_postnl", "OS_40F_POSTNL", Utils.GetCurrentCulture());
            return info;
        }
        #endregion

        private static string ApiHostname(bool useProd) => useProd
            ? "api.postnl.nl"
            : "api-sandbox.postnl.nl";

        public static bool CreateLabelOnPaymentOK()
        {
            var info = GetProviderSettings();
            return info.GetXmlPropertyBool("genxml/checkbox/createlabelonpayment");
        }
        public static bool CreateLabelOnShipped()
        {
            var info = GetProviderSettings();
            return info.GetXmlPropertyBool("genxml/checkbox/sendlabelonstatusshipped");
        }

        public static NBrightInfo CreateLabel(NBrightInfo orderInfo)
        {
            Logger.Debug($"CreateLabel for order itemid {orderInfo.ItemID}");
            var orderData = new OrderData(orderInfo.ItemID);
            var osSettings = new StoreSettings(orderInfo.PortalId);
            var prvSettings = GetProviderSettings();

            var recipient = UserController.Instance.GetUser(orderInfo.PortalId, orderInfo.UserId);

            var postdata = new CreateShipmentModel();
            postdata.Customer.Address.AddressType = "02";
            postdata.Customer.Address.CompanyName = osSettings.Get(StoreSettingKeys.storecompany);
            postdata.Customer.Address.Street = osSettings.Get(StoreSettingKeys.storeaddressline1);
            postdata.Customer.Address.City = osSettings.Get(StoreSettingKeys.storecity);
            postdata.Customer.Address.Zipcode = osSettings.Get(StoreSettingKeys.storepostcode);
            postdata.Customer.Address.Countrycode = osSettings.Get(StoreSettingKeys.storecountry);

            postdata.Customer.CustomerCode = prvSettings.GetXmlProperty("genxml/textbox/customercode");
            postdata.Customer.CustomerNumber = prvSettings.GetXmlProperty("genxml/textbox/customernumber");
            postdata.Customer.CollectionLocation = prvSettings.GetXmlProperty("genxml/textbox/collectionlocation");
            postdata.Customer.Email = osSettings.Get(StoreSettingKeys.supportemail);
            if(string.IsNullOrEmpty(postdata.Customer.Email)) postdata.Customer.Email = osSettings.Get(StoreSettingKeys.salesemail);
            if(string.IsNullOrEmpty(postdata.Customer.Email)) postdata.Customer.Email = osSettings.Get(StoreSettingKeys.manageremail);
            if(string.IsNullOrEmpty(postdata.Customer.Email)) postdata.Customer.Email = osSettings.Get(StoreSettingKeys.adminemail);

            var shipoption = orderInfo.GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/rblshippingoptions");
            var shipaddress = shipoption == "1" ? orderData.GetBillingAddress() : orderData.GetShippingAddress();
            postdata.Shipments.Add(new Shipment());
            postdata.Shipments[0].Addresses.Add(new Address()
            {
                AddressType = "01",
                FirstName = shipaddress.GetXmlProperty("genxml/textbox/firstname"),
                Name = shipaddress.GetXmlProperty("genxml/textbox/lastname"),
                Street = $"{shipaddress.GetXmlProperty("genxml/textbox/street")} {shipaddress.GetXmlProperty("genxml/textbox/unit")}".Trim(),
                Zipcode = shipaddress.GetXmlProperty("genxml/textbox/postalcode"),
                City = shipaddress.GetXmlProperty("genxml/textbox/city"),
                Countrycode = shipaddress.GetXmlProperty("genxml/dropdownlist/country"),
            });
            postdata.Shipments[0].Contacts.Add(new Contact()
            {
                ContactType = "01",
                Email = string.IsNullOrEmpty(shipaddress.GetXmlProperty("genxml/textbox/email")) ? recipient.Email : shipaddress.GetXmlProperty("genxml/textbox/email"),
            });

            var baseurl = $"https://{ApiHostname(prvSettings.GetXmlPropertyBool("genxml/checkbox/useproduction"))}/v1/shipment";

            Logger.Debug($"requesting label from {baseurl}, with payload\r\n{JsonConvert.SerializeObject(postdata, Formatting.None)}");

            var webReq = (HttpWebRequest)WebRequest.Create(baseurl);
            webReq.Method = "POST";
            webReq.Headers.Add("apikey", prvSettings.GetXmlProperty("genxml/textbox/key", true));
            webReq.ContentType = "application/json";
            webReq.AddRequestContent(JsonConvert.SerializeObject(postdata));

            HttpWebResponse webResp = null;
            try
            {
                webResp = (HttpWebResponse)webReq.GetResponse();
            }
            catch (WebException e)
            {
                Logger.Error($"CreateLabel Exception for {orderInfo.ItemID}", e);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                webResp = (HttpWebResponse)e.Response;
            }
            var respText = webResp?.GetResponseStream().ReadAllText() ?? $"{{ \"Errors\": \"Response was empty, with status {webResp.StatusCode}\"}}";
            Logger.Debug($"PostNL Response: {respText}");

            var resp = JObject.Parse(respText);

            if (resp["Errors"] != null)
            {
                var msg = $"PostNL returned errors for order-itemid {orderInfo.ItemID}: {respText}";
                Logger.Error(msg);
                orderData.AddAuditMessage(msg, "postnl", UserController.Instance.GetCurrentUserInfo()?.Username, bool.TrueString);
                orderData.Save();
                return orderData.PurchaseInfo;
            }
            // we only request one label, so no need to process more in the response either
            var respModel = new CreateShipmentResponse();
            if (resp["ResponseShipments"] != null)
            {
                var respShipment = (JObject)((JArray)resp["ResponseShipments"])[0];
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel", "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/barcode", respShipment["Barcode"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/content", ((JArray)respShipment["Labels"])?[0]?["Content"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/labeltype", respShipment["Labeltype"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/outputtype", respShipment["OutputType"]?.Value<string>() ?? "");
                orderData.Save();
                Logger.Debug($"PostNL Label saved to order");
            }

            return orderData.GetInfo();
        }

        public static void SendMailWithLabel(NBrightInfo orderInfo)
        {
            Logger.Debug($"SendMailWithLabel for order itemid {orderInfo.ItemID}");

            var orderData = new OrderData(orderInfo.ItemID);

            var portal = new PortalSettings(orderInfo.PortalId);
            var osSettings = new StoreSettings(orderInfo.PortalId);
            var prvSettings = GetProviderSettings();

            var l1 = new List<object>();
            l1.Add(orderInfo);

            var nbRazor = new NBrightRazor(l1, osSettings.Settings());
            nbRazor.FullTemplateName = "config.labelemail";
            nbRazor.TemplateName = "labelemail.cshtml";
            nbRazor.ThemeFolder = "config";
            nbRazor.Lang = orderInfo.Lang;

            var controlMapPath = portal.HomeDirectoryMapPath + "..\\..\\DesktopModules\\NBright\\OS_40F_PostNL";
            var templCtrl = new TemplateGetter(portal.HomeDirectoryMapPath, controlMapPath, "Themes\\config\\" + osSettings.ThemeFolder, "Themes\\config\\" );
            if (nbRazor.Lang == "") nbRazor.Lang = Utils.GetCurrentCulture();
            var templData = templCtrl.GetTemplateData(nbRazor.TemplateName, nbRazor.Lang);

            var ordernumber = orderInfo.GetXmlProperty("genxml/ordernumber");
            var body = RazorRender(osSettings, nbRazor, templData, "");
            var subject = $"PostNL verzendlabel voor {ordernumber}";
            var to = prvSettings.GetXmlProperty("genxml/textbox/emailforlabel");
            if(string.IsNullOrEmpty(to)) to = osSettings.Get(StoreSettingKeys.manageremail);
            if(string.IsNullOrEmpty(to)) to = osSettings.Get(StoreSettingKeys.adminemail);
            if(string.IsNullOrEmpty(to)) to = osSettings.Get(StoreSettingKeys.supportemail);
            if(string.IsNullOrEmpty(to)) to = osSettings.Get(StoreSettingKeys.salesemail);

            // save the file
            var tempfile = Path.Combine(osSettings.FolderTempMapPath, $"PostNL-{ordernumber}.pdf");
            SaveBase64EncodedFile(orderInfo.GetXmlProperty("genxml/postnllabel/content"), tempfile);
            var sendResult = Mail.SendMail(osSettings.AdminEmail, to, "", "", MailPriority.Normal, subject, MailFormat.Html, Encoding.UTF8, body, tempfile, "", "", "", "");
            if (string.IsNullOrEmpty(sendResult))
            {
                orderData.AddAuditMessage($"PostNL label created and sent to {to}", "postnl", UserController.Instance.GetCurrentUserInfo()?.Username, bool.TrueString);
                orderData.Save();
            }
            try
            {
                File.Delete(tempfile);
            }
            catch(Exception e)
            {
                Logger.Debug(e.Message);
            }
        }

        public static void AddRequestContent(this WebRequest webReq, string postData)
        {
            var encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(postData);
            // Set the content length of the string being posted.
            webReq.ContentLength = byte1.Length;
            // get the request stream
            Stream newStream = webReq.GetRequestStream();
            // write the content to the stream
            newStream.Write(byte1, 0, byte1.Length);
        }
        public static string ReadAllText(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        public static string RazorRender(StoreSettings storeSettings, Object info, string razorTempl, string templateKey, Boolean debugMode = false)
        {
            var result = "";
            try
            {
                // do razor test
                var config = new TemplateServiceConfiguration();
                config.Debug = storeSettings.DebugMode;
                config.BaseTemplateType = typeof(NBrightBuyRazorTokens<>);
                Engine.Razor = RazorEngineService.Create(config);

                var hashCacheKey = NBrightBuyUtils.GetMd5Hash(razorTempl);

                result = Engine.Razor.RunCompile(razorTempl, hashCacheKey, null, info);

            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static bool SaveBase64EncodedFile(string base64EncodedFileContents, string fileNamePath)
        {
            byte[] decodedDataAsBytes = null;

            bool success = false;

            // remove trailing ='s
            base64EncodedFileContents = base64EncodedFileContents.TrimEnd("=".ToCharArray());
            // try to convert, if it fails, add a padding =
            while (!success)
            {
                try
                {
                    decodedDataAsBytes = Convert.FromBase64String(base64EncodedFileContents);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (base64EncodedFileContents.EndsWith("====")) throw ex;

                    base64EncodedFileContents += "=";
                }
            }

            // Open file for writing
            FileStream _FileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);

            // Writes a block of bytes to this stream using data from a byte array.
            _FileStream.Write(decodedDataAsBytes, 0, decodedDataAsBytes.Length);

            // close file stream
            _FileStream.Close();

            return true;
        }

    }
}