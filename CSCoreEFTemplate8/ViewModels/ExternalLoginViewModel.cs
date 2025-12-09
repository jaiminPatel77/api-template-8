using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.ViewModels
{
    public class ExternalLoginViewModel : LoginBaseModel
    {
        public String LoginProvider { get; set; }
        public String ProviderKey { get; set; }
        public String Email { get; set; }
        public String Token { get; set; }
        public String FullName { get; set; }
    }
}
