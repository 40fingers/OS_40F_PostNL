using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_40F_PostNL
{
    public class EventProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.EventInterface 
    {

        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            return cartInfo;
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            return cartItemInfo;
        }

        public override NBrightInfo AfterCartSave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterProductSave(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo)
        {
            try
            {
                Logger.Debug($"OS_40F_PostNL AfterOrderStatusChange triggered. CreateLabelOnShipped={ProviderUtils.CreateLabelOnShipped()}, CreateLabelOnPaymentOK={ProviderUtils.CreateLabelOnPaymentOK()}.");
                var orderstatus = nbrightInfo.GetXmlPropertyInt("genxml/dropdownlist/orderstatus");
                Logger.Debug($"Order ItemId={nbrightInfo.ItemID}, OrderStatus={orderstatus}.");
                if ((ProviderUtils.CreateLabelOnShipped() && orderstatus == (int)OrderStatus.Shipped)
                    || (ProviderUtils.CreateLabelOnPaymentOK() && orderstatus == (int)OrderStatus.PaymentOk))
                {
                    var orderInfoWithLabel = ProviderUtils.CreateLabel(nbrightInfo);
                    ProviderUtils.SendMailWithLabel(orderInfoWithLabel);
                    return orderInfoWithLabel;
                }
            }
            catch (Exception e)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
            }
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo)
        {
            return nbrightInfo;
        }

        public override NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }

        public override NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            return nbrightInfo;
        }
    }
}
