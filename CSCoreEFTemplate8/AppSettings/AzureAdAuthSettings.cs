using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.AppSettings
{
    public class AzureAdAuthSettings
    {
        //"https://login.microsoftonline.com/e5944de6-0f0c-4434-9e16-e7b929c2032e", //replace with correct azureAdTenendtId
        public String Authority { get; set; }
        //"cae597d5-3463-4855-8e58-847d8bf6f26a", //replace with correct azure ClientId
        public String ClientId { get; set; }
    }
}
