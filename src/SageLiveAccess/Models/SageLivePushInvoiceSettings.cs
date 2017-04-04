namespace SageLiveAccess.Models
{
    public class SageLivePushInvoiceSettings
    {
        public readonly string _legislationId;
        public readonly string _companyName;

        public SageLivePushInvoiceSettings( string legislationId, string companyName )
        {
            this._companyName = companyName;
            this._legislationId = legislationId;
        }
    }
}
