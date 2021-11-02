using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace DedolesPriceScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Ensure cyrillic characters are output correctly.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var productsCSV = Properties.Resources.Products;

            var productLinks = ParseCSV(productsCSV);
            int depth = 1;

            var scraper = new DedolesPriceScraper();

            Console.WriteLine($"Running scraper for {productLinks.Count} products at depth {depth}...");
            var products = await scraper.Scrape(productLinks, depth);

            Console.WriteLine($"Completed scrape with {products.Count} results.");
            Console.WriteLine();
            foreach (var product in products.OrderBy(p => p.Name))
            {
                Console.WriteLine(product);
            }
        }

        private static List<ProductLink> ParseCSV(string csv)
        {
            using (var reader = new StringReader(csv))
            {
                using (TextFieldParser parser = new TextFieldParser(reader))
                {
                    var productLinks = new List<ProductLink>();

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Read column names
                    var fieldNames = parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        ProductLink productLink = new ProductLink();

                        var fields = parser.ReadFields();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var fieldName = fieldNames[i];
                            var field = fields[i];

                            switch (fieldName.ToLower())
                            {
                                case "name":
                                    productLink.Name = field;
                                    break;
                                case "url":
                                    productLink.Url = field;
                                    break;
                                default:
                                    throw new Exception($"Unrecognized field name: {fieldName}");
                            }
                        }

                        productLinks.Add(productLink);
                    }

                    return productLinks;
                }
            }
        }
    }
}
