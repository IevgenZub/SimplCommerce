namespace SimplCommerce.Module.PaymentStripe.ViewModels
{
    public class PortmoneCallback
    {
        public string BILL_AMOUNT { get; set; }
        public string SHOPORDERNUMBER { get; set; }
        public string APPROVALCODE { get; set; }
        public string RECEIPT_URL { get; set; }
        public string TOKEN { get; set; }
        public string CARD_PAYMENT_SYSTEM { get; set; }
        public string CARD_LAST_DIGITS { get; set; }
        public string CARD_MASK { get; set; }
        public string DESCRIPTION { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string RESULT { get; set; }
    }
}
