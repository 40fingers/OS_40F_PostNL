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
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OS_40F_PostNL.Components;

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
            ? "api-sandbox.postnl.nl"
            : "api.postnl.nl";

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

            var shipaddress = orderData.GetShippingAddress();
            postdata.Shipments.Add(new Shipment());
            postdata.Shipments[0].Addresses.Add(new Address()
            {
                AddressType = "02",
                FirstName = recipient.FirstName,
                Name = recipient.LastName,
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

            var webReq = (HttpWebRequest)WebRequest.Create(baseurl);
            webReq.Method = "POST";
            webReq.Headers.Add("apikey", prvSettings.GetXmlProperty("genxml/textbox/key", true));
            webReq.ContentType = "application/json";
            webReq.AddRequestContent(JsonConvert.SerializeObject(postdata));

            var webResp = (HttpWebResponse)webReq.GetResponse();
            var respText = webResp.GetResponseStream().ReadAllText();
            var resp = JObject.Parse(respText);

            if (resp.ContainsKey("Errors"))
            {
                Logger.Error($"PostNL returned errors for order-itemid {orderInfo.ItemID}: {respText}");
                return "";
            }
            // we only request one label, so no need to process more in th eresponse either
            var respModel = new CreateShipmentResponse();
            if (resp.ContainsKey("ResponseShipments"))
            {
                var respShipment = (JObject)((JArray)resp["ResponseShipments"])[0];
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel", "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/barcode", respShipment["Barcode"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/content", ((JArray)respShipment["Labels"])?[0]?["Content"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/labeltype", respShipment["Labeltype"]?.Value<string>() ?? "");
                orderData.GetInfo().SetXmlProperty("genxml/postnllabel/outputtype", respShipment["OutputType"]?.Value<string>() ?? "");
                orderData.Save();
            }

            return orderData.GetInfo();
        }

        public static void AddRequestContent(this WebRequest webReq, JToken jtoken)
        {
            webReq.AddRequestContent(jtoken.ToString(Formatting.Indented));
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

    }
}