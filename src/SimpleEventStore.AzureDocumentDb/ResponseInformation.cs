using System.Collections.Specialized;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace SimpleEventStore.AzureDocumentDb
{
    public class ResponseInformation
    {
        public string CurrentResourceQuotaUsage { get; private set; }

        public string MaxResourceQuota { get; private set; }

        public double RequestCharge { get; private set; }

        public NameValueCollection ResponseHeaders { get; private set; }

        public static ResponseInformation FromWriteResponse(IStoredProcedureResponse<dynamic> response)
        {
            return new ResponseInformation
            {
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge,
                ResponseHeaders = response.ResponseHeaders
            };
        }

        public static ResponseInformation FromReadResponse(IFeedResponse<DocumentDbStorageEvent> response)
        {
            return new ResponseInformation
            {
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge,
                ResponseHeaders = response.ResponseHeaders
            };
        }

        public static ResponseInformation FromSubscriptionReadResponse(IFeedResponse<Document> response)
        {
            return new ResponseInformation
            {
                CurrentResourceQuotaUsage = response.CurrentResourceQuotaUsage,
                MaxResourceQuota = response.MaxResourceQuota,
                RequestCharge = response.RequestCharge,
                ResponseHeaders = response.ResponseHeaders
            };
        }
    }
}