using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var url = $"{this.Request.Scheme}://{this.Request.Host}";
            if (this.Request.PathBase.HasValue)
            {
                url += this.Request.PathBase.Value;
            }
            url += "/swagger";
            return this.Redirect(url);
        }
    }
}
