using ConfidoSoft.Data.Domain.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Namespace matches your project structure
namespace ConfidoSoft.Data.Services.Extraction
{
    public interface IInvoiceExtractionService
    {
      InvoiceDto ProcessInvoice(IFormFile jsonFile);
    }

    public class InvoiceExtractionService : IInvoiceExtractionService
    {
       public InvoiceDto ProcessInvoice(IFormFile file)
        {
            var invoiceDto = new InvoiceDto();
            return invoiceDto;
        }
    }
}