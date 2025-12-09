using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.ViewModels
{
    public class FacebookAuthViewModel : LoginBaseModel
    {
        [Required]
        public string AccessToken { get; set; }
    }
}
