using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBrightDNN;

namespace OS_40F_PostNL
{
    public class EventProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.EventInterface 
    {

        public override NBrightInfo ValidateCartBefore(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartAfter(NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemBefore(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo ValidateCartItemAfter(NBrightInfo cartItemInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterCartSave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterCategorySave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterProductSave(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterSavePurchaseData(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforeOrderStatusChange(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterOrderStatusChange(NBrightInfo nbrightInfo)
        {
            // TODO:
            // if setting is true
            // - check if status set to SHIPPED
            // - if so, create new label
            // - send email with label
            var info = ProviderUtils.GetProviderSettings();

            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentOK(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterPaymentOK(NBrightInfo nbrightInfo)
        {
            // TODO: Implement
            // if setting is true
            // - creation of the label
            // - send email with label
            var info = ProviderUtils.GetProviderSettings();

            return nbrightInfo;
        }

        public override NBrightInfo BeforePaymentFail(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterPaymentFail(NBrightInfo nbrightInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo BeforeSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo AfterSendEmail(NBrightInfo nbrightInfo, string emailsubjectrexkey)
        {
            throw new NotImplementedException();
        }
    }
}
