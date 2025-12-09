using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string ReturnUrl { get; set; }
    }
}
