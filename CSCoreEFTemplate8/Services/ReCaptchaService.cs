using CSCoreEFTemplate8.AppSettings;
using Microsoft.Extensions.Options;
using Neuronms.Data.Services.BLServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Services
{

    //https://developers.google.com/recaptcha/docs/verify
    //public class ReCaptchaRequest
    //{
    //    public string Secret { get; set; }
    //    public string Response { get; set; }
    //}


    public class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }

    public class ReCaptchaService
    {
        private readonly ReCaptchaSettings _reCaptchaSettings;

        public ReCaptchaService(IOptions<ReCaptchaSettings> reCaptchaSettings)
        {
            _reCaptchaSettings = reCaptchaSettings.Value;
        }

        public async Task<bool> Validate(string secretCode)
        {
            //ReCaptchaRequest reCaptchaRequest = new ReCaptchaRequest();
            //reCaptchaRequest.Secret = _reCaptchaSettings.PrivateKey;
            //reCaptchaRequest.Response = secretCode;
            string url = $"https://www.google.com/recaptcha/api/siteverify?secret={_reCaptchaSettings.PrivateKey}&response={secretCode}";
            //string url = $"https://www.google.com/recaptcha/api/siteverify";
            var captchaResponse = await HttpClientService.PostAsJsonAsync<Object, ReCaptchaResponse>(url, null);
            return captchaResponse.Success;
        }
    }
}
