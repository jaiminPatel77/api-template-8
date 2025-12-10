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
      InvoiceDto ExtractInvoiceDataAsync(IFormFile jsonFile);
    }

    public class InvoiceExtractionService : IInvoiceExtractionService
    {
     private const decimal LineTolerance = 5.0m;
        private const decimal HeaderValueSearchDepth = 50.0m; // How far down to look for header values
        private const decimal ColumnGapTolerance = 20.0m; // Proximity for split header words

        public InvoiceDto ExtractInvoiceDataAsync(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            List<PageWords>? pages;

            using (var stream = file.OpenReadStream())
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                pages = JsonSerializer.Deserialize<List<PageWords>>(stream, options);
            }

            if (pages is null || pages.Count == 0)
            {
                return new InvoiceDto();
            }

            var invoice = new InvoiceDto
            {
                invoiceLineDetails = new List<InvoiceLineDetailsDto>()
            };

            // 1. Extract Main Headers (Account, Invoice #, Date)
            // Typically found on the first page
            var firstPage = pages.FirstOrDefault(p => p.Page_Number == 1);
            if (firstPage != null)
            {
                invoice.CustomerNumber = ExtractHeaderValue(firstPage.Words, "ACCOUNT", "NUMBER");
                invoice.InvoiceNumber = ExtractHeaderValue(firstPage.Words, "INVOICE", "NUMBER");
                invoice.InvoiceDate = ExtractHeaderValue(firstPage.Words, "INVOICE", "DATE");
            }

            // 2. Extract Table Data (Loop all pages)
            foreach (var page in pages)
            {
                ExtractTableDataFromPage(page, invoice);
            }

            return invoice;
        }

        private string? ExtractHeaderValue(List<ExtractedWord> words, string firstWord, string secondWord)
        {
            // Find the phrase
            var phraseStart = words.FirstOrDefault(w => w.Text.Equals(firstWord, StringComparison.OrdinalIgnoreCase));
            
            if (phraseStart is null) return null;

            // Check adjacent word
            var phraseEnd = words.FirstOrDefault(w => 
                w.Text.Equals(secondWord, StringComparison.OrdinalIgnoreCase) && 
                w.top >= phraseStart.top - LineTolerance && 
                w.top <= phraseStart.top + LineTolerance &&
                w.x0 >= phraseStart.x1 && 
                (w.x0 - phraseStart.x1) <= ColumnGapTolerance);

            if (phraseEnd is null) return null;

            // Search for value strictly BELOW the phrase found.
            // Based on JSON sample provided, values are vertically aligned under headers.
            // We define a bounding box below the found header.
            decimal searchTop = phraseStart.bottom;
            decimal searchBottom = phraseStart.bottom + HeaderValueSearchDepth;
            decimal searchLeft = phraseStart.x0 - 10; // Allow slight skew left
            decimal searchRight = phraseEnd.x1 + 20;  // Allow wider value

            var valueWord = words
                .Where(w => 
                    w.top >= searchTop && 
                    w.top <= searchBottom &&
                    w.x0 >= searchLeft &&
                    w.x0 <= searchRight)
                .OrderBy(w => w.top) // Closest one below
                .ThenBy(w => w.x0)
                .FirstOrDefault();

            return valueWord?.Text;
        }

        private void ExtractTableDataFromPage(PageWords page, InvoiceDto invoice)
        {
            if (invoice.invoiceLineDetails is null) return;

            // 1. Check for "INVOICE LINE DETAILS" trigger
            if (!HasPhrase(page.Words, "INVOICE", "LINE", "DETAILS"))
            {
                return; // Skip page (summary or cover)
            }

            // 2. Determine Column Boundaries dynamically based on headers
            var columnMap = GetColumnBoundaries(page.Words);
            if (columnMap is null) return; // Couldn't find table headers

            // 3. Group words into lines
            var lines = GroupWordsIntoLines(page.Words);

            // 4. Iterate lines
            // Filter: Only process lines vertically below "INVOICE LINE DETAILS"
            // and above "STORAGE LOCATION RECAP"
            decimal tableStartTop = GetPhraseBottom(page.Words, "INVOICE", "LINE", "DETAILS") ?? 0;
            decimal tableEndTop = GetPhraseTop(page.Words, "STORAGE", "LOCATION", "RECAP(N)") ?? decimal.MaxValue;

            InvoiceLineDetailsDto? currentSection = invoice.invoiceLineDetails.LastOrDefault();

            foreach (var line in lines)
            {
                decimal lineTop = line.First().top;

                // Skip lines outside table area
                if (lineTop <= tableStartTop || lineTop >= tableEndTop) continue;

                // Skip header row repeats (if the line contains header keywords)
                if (IsHeaderRow(line)) continue;

                // Determine classification
                if (IsSectionHeader(line, columnMap))
                {
                    currentSection = new InvoiceLineDetailsDto
                    {
                        item = string.Join(" ", line.Select(w => w.Text)),
                        productDetails = new List<ProductDetailDto>()
                    };
                    invoice.invoiceLineDetails.Add(currentSection);
                }
                else if (IsProductRow(line, columnMap, out var newProduct))
                {
                    if (currentSection is null)
                    {
                        // Fallback if no section header found yet, creating a default one
                        currentSection = new InvoiceLineDetailsDto { item = "General", productDetails = new List<ProductDetailDto>() };
                        invoice.invoiceLineDetails.Add(currentSection);
                    }
                    currentSection.productDetails?.Add(newProduct!);
                }
                else if (IsMultiLineDescription(line, columnMap))
                {
                    // Append to previous product in current section
                    if (currentSection?.productDetails is not null && currentSection.productDetails.Count > 0)
                    {
                        var lastProduct = currentSection.productDetails.Last();
                        string extraDesc = string.Join(" ", line.Select(w => w.Text));
                        
                        if (!string.IsNullOrEmpty(lastProduct.ProductName))
                            lastProduct.ProductName += " " + extraDesc;
                        else
                            lastProduct.ProductName = extraDesc;
                    }
                }
            }
        }

        // --- Geometric Helpers ---

        private class ColumnBoundaries
        {
            public decimal DescX0 { get; set; }
            public decimal DescX1 { get; set; }
            public decimal ProdNumX0 { get; set; }
            public decimal ProdNumX1 { get; set; }
            public decimal PackX0 { get; set; }
            public decimal PackX1 { get; set; }
            public decimal UnitX0 { get; set; }
            public decimal UnitX1 { get; set; }
            public decimal UnitPriceX0 { get; set; }
            public decimal UnitPriceX1 { get; set; }
            public decimal ExtPriceX0 { get; set; }
            public decimal ExtPriceX1 { get; set; }
        }

        private ColumnBoundaries? GetColumnBoundaries(List<ExtractedWord> words)
        {
            // Find header words to establish X coordinates
            var prodNum = FindWord(words, "PRODUCT", "NUMBER"); // "PRODUCT NUMBER" is often split
            var desc = FindWord(words, "DESCRIPTION");
            var pack = FindWord(words, "PACK"); // "PACK SIZE"
            var unit = FindWord(words, "PRICING"); // "PRICING UNIT"
            var uPrice = FindWord(words, "UNIT"); // "UNIT PRICE" (ambiguous with PRICING UNIT, check context)
            // Refined search for Unit Price to differentiate from Pricing Unit
            var uPriceSpecific = words.FirstOrDefault(w => w.Text == "PRICE" && w.x0 > (unit?.x1 ?? 0)); 
            
            var extPrice = FindWord(words, "EXTENDED"); // "EXTENDED PRICE"

            if (desc is null) return null; // Description is mandatory for mapping

            return new ColumnBoundaries
            {
                // Logic: Start of Desc to Start of Pack Size (approx)
                DescX0 = desc.x0 - 10, 
                DescX1 = pack?.x0 ?? (desc.x1 + 250), // Fallback if Pack missing

                // Logic: Product Number column
                ProdNumX0 = (prodNum?.x0 ?? 140) - 10,
                ProdNumX1 = (prodNum?.x1 ?? 190) + 10,

                // Logic: Pack
                PackX0 = (pack?.x0 ?? 480) - 10,
                PackX1 = (pack?.x1 ?? 515) + 10,

                // Unit
                UnitX0 = (unit?.x0 ?? 620) - 10,
                UnitX1 = (unit?.x1 ?? 655) + 10,

                // Unit Price
                UnitPriceX0 = (uPriceSpecific?.x0 ?? 675) - 30, // Go left a bit for the '$'
                UnitPriceX1 = (uPriceSpecific?.x1 ?? 710) + 10,

                // Extended Price (Total)
                ExtPriceX0 = (extPrice?.x0 ?? 720) - 10,
                ExtPriceX1 = (extPrice?.x1 ?? 765) + 10
            };
        }

        private bool IsSectionHeader(List<ExtractedWord> line, ColumnBoundaries cols)
        {
            // Rule 1: At least one word in far-left (x < 100)
            bool hasFarLeft = line.Any(w => w.x0 < 100);
            if (!hasFarLeft) return false;

            // Rule 2: No words inside ProductID or UnitPrice boundaries
            bool hasProdId = line.Any(w => w.x0 >= cols.ProdNumX0 && w.x1 <= cols.ProdNumX1);
            bool hasPrice = line.Any(w => w.x0 >= cols.UnitPriceX0 && w.x1 <= cols.UnitPriceX1);

            if (hasProdId || hasPrice) return false;

            // Rule 3: Single group (implicit by grouping logic, but essentially implies the text is usually headers like DRY, FROZEN)
            return true;
        }

        private bool IsMultiLineDescription(List<ExtractedWord> line, ColumnBoundaries cols)
        {
            // Rule 1: Every word falls inside Description OR is far left (indentation variance)
            // But strictly, Description Only rows usually don't have IDs or Prices.
            
            // Check required EMPTY fields
            bool hasProdId = line.Any(w => w.x0 >= cols.ProdNumX0 && w.x1 <= cols.ProdNumX1);
            bool hasPrice = line.Any(w => w.x0 >= cols.UnitPriceX0 && w.x1 <= cols.UnitPriceX1);
            bool hasTotal = line.Any(w => w.x0 >= cols.ExtPriceX0 && w.x1 <= cols.ExtPriceX1);
            bool hasUnit = line.Any(w => w.x0 >= cols.UnitX0 && w.x1 <= cols.UnitX1);

            if (hasProdId || hasPrice || hasTotal || hasUnit) return false;

            // Ensure there is actually text in the description area
            bool hasDescText = line.Any(w => w.x1 >= cols.DescX0); 
            
            return hasDescText;
        }

        private bool IsProductRow(List<ExtractedWord> line, ColumnBoundaries cols, out ProductDetailDto? product)
        {
            product = null;

            // Get words in specific columns
            var prodIdWords = line.Where(w => w.x0 >= cols.ProdNumX0 && w.x1 <= cols.ProdNumX1).OrderBy(w => w.x0).ToList();
            var descWords = line.Where(w => w.x0 >= cols.DescX0 && w.x1 < cols.PackX0).OrderBy(w => w.x0).ToList();
            var packWords = line.Where(w => w.x0 >= cols.PackX0 && w.x1 <= cols.PackX1).OrderBy(w => w.x0).ToList();
            var unitWords = line.Where(w => w.x0 >= cols.UnitX0 && w.x1 <= cols.UnitX1).OrderBy(w => w.x0).ToList();
            var unitPriceWords = line.Where(w => w.x0 >= cols.UnitPriceX0 && w.x1 <= cols.UnitPriceX1).OrderBy(w => w.x0).ToList();
            var totalWords = line.Where(w => w.x0 >= cols.ExtPriceX0 && w.x1 <= cols.ExtPriceX1).OrderBy(w => w.x0).ToList();

            // Validation: Must NOT contain non-numeric noise in Product ID (e.g. "NUMBER")
            string prodIdRaw = string.Join("", prodIdWords.Select(w => w.Text));
            if (string.IsNullOrWhiteSpace(prodIdRaw) || prodIdWords.Any(w => w.Text.Contains("NUMBER", StringComparison.OrdinalIgnoreCase)))
                return false;

            // Validation: Must have a price or quantity to be a valid product row usually
            if (!unitPriceWords.Any() && !totalWords.Any()) return false;

            product = new ProductDetailDto
            {
                ProductID = prodIdRaw,
                ProductName = string.Join(" ", descWords.Select(w => w.Text)),
                Unit = string.Join(" ", unitWords.Select(w => w.Text)),
                Quantity = ParseDecimal(string.Join("", packWords.Select(w => w.Text))), // Often Pack size is complex (4/125), treating as text might be better but DTO asks for decimal. 
                // Note: DTO asks for Decimal Quantity. Usually "Pack Size" is "4/125 EA". Actual ordered quantity is usually in a column to the left of "Pack Size". 
                // Based on prompt "PACK SIZE (maps to Quantity)". I will try to parse the first number found.
                UnitPrice = ParseCurrency(string.Join("", unitPriceWords.Select(w => w.Text))),
                TotalPrice = ParseCurrency(string.Join("", totalWords.Select(w => w.Text)))
            };

            // Attempt to parse quantity from Pack Size string (e.g. "2" from "2/150") or standard int
            // However, looking at the layout, there are "ORD" columns to the left. 
            // The prompt specifically says "PACK SIZE (maps to Quantity)". I will obey the prompt strictly.
            // If text is "4/125", simple decimal parse fails. 
            product.Quantity = ExtractFirstDecimal(string.Join(" ", packWords.Select(w => w.Text)));

            return true;
        }

        private decimal? ParseCurrency(string text)
        {
            var clean = text.Replace("$", "").Replace(",", "").Trim();
            if (decimal.TryParse(clean, out decimal result)) return result;
            return null;
        }

        private decimal? ExtractFirstDecimal(string text)
        {
            // Naive extraction for things like "24/16.9" -> 24
            if (string.IsNullOrWhiteSpace(text)) return null;
            var parts = text.Split(new[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && decimal.TryParse(parts[0], out decimal d)) return d;
            return null;
        }

        private decimal? ParseDecimal(string text)
        {
            if (decimal.TryParse(text, out decimal d)) return d;
            return null;
        }

        private bool IsHeaderRow(List<ExtractedWord> line)
        {
            var text = string.Join(" ", line.Select(w => w.Text)).ToUpper();
            return text.Contains("PRODUCT NUMBER") || 
                   text.Contains("EXTENDED PRICE") || 
                   text.Contains("DESCRIPTION") ||
                   text.Contains("ORD SHP");
        }

        private List<List<ExtractedWord>> GroupWordsIntoLines(List<ExtractedWord> words)
        {
            var sortedWords = words.OrderBy(w => w.top).ToList();
            var lines = new List<List<ExtractedWord>>();

            foreach (var word in sortedWords)
            {
                bool added = false;
                foreach (var line in lines)
                {
                    // Check vertical alignment overlap
                    decimal lineAvgTop = line.Average(w => w.top);
                    if (Math.Abs(word.top - lineAvgTop) <= LineTolerance)
                    {
                        line.Add(word);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    lines.Add(new List<ExtractedWord> { word });
                }
            }

            // Sort left-to-right within lines
            foreach (var line in lines)
            {
                line.Sort((a, b) => a.x0.CompareTo(b.x0));
            }

            return lines;
        }

        private ExtractedWord? FindWord(List<ExtractedWord> words, string text)
        {
            return words.FirstOrDefault(w => w.Text.Equals(text, StringComparison.OrdinalIgnoreCase));
        }

        private ExtractedWord? FindWord(List<ExtractedWord> words, string text1, string text2)
        {
            // Find text1 immediately followed by text2
            var w1s = words.Where(w => w.Text.Equals(text1, StringComparison.OrdinalIgnoreCase));
            foreach (var w1 in w1s)
            {
                var w2 = words.FirstOrDefault(w => 
                    w.Text.Equals(text2, StringComparison.OrdinalIgnoreCase) && 
                    Math.Abs(w.top - w1.top) < LineTolerance &&
                    w.x0 > w1.x1 && (w.x0 - w1.x1) < ColumnGapTolerance);
                
                if (w2 != null) return w1;
            }
            return null;
        }

        private bool HasPhrase(List<ExtractedWord> words, params string[] phrases)
        {
            // Simple check if the sequence exists loosely on the page (vertical proximity)
            // Used for Page Filtering
            for(int i = 0; i < words.Count - phrases.Length; i++)
            {
                bool match = true;
                for(int j=0; j < phrases.Length; j++)
                {
                    if (!words[i+j].Text.Equals(phrases[j], StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
            return false;
        }

        private decimal? GetPhraseBottom(List<ExtractedWord> words, params string[] phrases)
        {
            // Logic to find exact coordinate of a multi-word phrase
            // Used to define table start
            var sorted = words.OrderBy(w => w.top).ThenBy(w => w.x0).ToList();
            
            // This is a simplified finder for "INVOICE LINE DETAILS"
            // We assume they appear sequentially in the list or strictly adjacent
            for (int i = 0; i < sorted.Count - (phrases.Length - 1); i++)
            {
                if (sorted[i].Text.Equals(phrases[0], StringComparison.OrdinalIgnoreCase))
                {
                    bool fullMatch = true;
                    decimal maxBottom = sorted[i].bottom;
                    
                    for (int j = 1; j < phrases.Length; j++)
                    {
                        var next = sorted[i + j];
                        // Check adjacency
                        if (!next.Text.Equals(phrases[j], StringComparison.OrdinalIgnoreCase) ||
                            Math.Abs(next.top - sorted[i].top) > LineTolerance * 2) 
                        {
                            fullMatch = false;
                            break;
                        }
                        if (next.bottom > maxBottom) maxBottom = next.bottom;
                    }

                    if (fullMatch) return maxBottom;
                }
            }
            return null;
        }

        private decimal? GetPhraseTop(List<ExtractedWord> words, params string[] phrases)
        {
            // Similar to GetPhraseBottom, but returns the Top Y
            var sorted = words.OrderBy(w => w.top).ThenBy(w => w.x0).ToList();
            for (int i = 0; i < sorted.Count - (phrases.Length - 1); i++)
            {
                if (sorted[i].Text.Equals(phrases[0], StringComparison.OrdinalIgnoreCase))
                {
                    bool fullMatch = true;
                    decimal minTop = sorted[i].top;

                    for (int j = 1; j < phrases.Length; j++)
                    {
                        var next = sorted[i + j];
                        if (!next.Text.Equals(phrases[j], StringComparison.OrdinalIgnoreCase))
                        {
                            fullMatch = false;
                            break;
                        }
                    }
                    if (fullMatch) return minTop;
                }
            }
            return null;
        }
    }
}