using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Domain.Dtos
{
    // Models/InvoiceDto.cs
    public class InvoiceDto
    {
        /// <summary>
        /// The customer's contact phone number.
        /// </summary>
        public string? CustomerContactNumber { get; set; }

        /// <summary>
        /// The unique number identifying the customer (e.g., "00003", "10294817", "71210488").
        /// </summary>
        public string? CustomerNumber { get; set; }

        /// <summary>
        /// The unique identifier for the invoice (e.g., "2021153", "2136014", "78574843").
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// The date the invoice was created or transaction occurred (e.g., "11/25/25", "11/18/2025").
        /// </summary>
        public string? InvoiceDate { get; set; }

        /// <summary>
        /// The total amount due for the invoice, including taxes and charges (e.g., 209.75, 1383.70).
        /// </summary>
        public decimal? TotalAmountDue { get; set; }

        /// <summary>
        /// A list of all products included in the invoice.
        /// </summary>
        public List<InvoiceLineDetailsDto>? invoiceLineDetails { get; set; }
    }

    public class InvoiceLineDetailsDto
    {
        public string? item { get; set; }
        public List<ProductDetailDto>? productDetails { get; set; }
    }

    public class ProductDetailDto
    {
        /// <summary>
        /// The description or name of the product (e.g., "RESOLVE URINE DESTROYER").
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// The SKU or item number (e.g., "10051945830000400011").
        /// </summary>
        public string? ProductID { get; set; }

        /// <summary>
        /// The quantity purchased (e.g., 1.0000).
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// The unit of measure (e.g., "EA" for Each, "CS" for Case, "PK" for Pack).
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// The price per unit (e.g., $32.58).
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// The extended price for this line item (e.g., $32.58).
        /// </summary>
        public decimal? TotalPrice { get; set; }
    }

    public class TextLine
    {
        public List<ExtractedWord> Words { get; set; } = new();
        public decimal Top => Words.Count > 0 ? Words.Min(w => w.top) : 0;
        public decimal Bottom => Words.Count > 0 ? Words.Max(w => w.bottom) : 0;
        public decimal X0 => Words.Count > 0 ? Words.Min(w => w.x0) : 0;
        public string FullText => string.Join(" ", Words.Select(w => w.Text));
    }

    // Models/ExtractedWord.cs
    public class PageWords
    {
        public int Page_Number { get; set; }
        public List<ExtractedWord> Words { get; set; }
    }
    public class ExtractedWord
    {
        public string Text { get; set; } = string.Empty;
        public decimal x0 { get; set; }
        public decimal x1 { get; set; }
        public decimal top { get; set; }
        public decimal bottom { get; set; }
        public decimal doctop { get; set; }
        public bool upright { get; set; }
        public decimal height { get; set; }
        public decimal width { get; set; }
        public string direction { get; set; } = string.Empty;
    }

}
