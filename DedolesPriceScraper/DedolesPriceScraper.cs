using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DedolesPriceScraper
{
    public class DedolesPriceScraper
    {
        private readonly HttpClient client;
        public DedolesPriceScraper()
        {
            this.client = new HttpClient();
        }

        public Task<List<Product>> Scrape(List<ProductLink> productLinks)
        {
            return this.Scrape(productLinks, 1);
        }

        public async Task<List<Product>> Scrape(List<ProductLink> productLinks, int depth)
        {
            var tasks = productLinks
                .Select(pl => Task.Run(() => this.ScrapeProducts(pl, depth)));
            
            // Join all lists of results together
            var results = (await Task.WhenAll(tasks))
                .Aggregate((a, b) =>
            {
                a.AddRange(b);
                return a;
            });

            return results;
        }

        private async Task<List<Product>> ScrapeProducts(ProductLink productLink, int depth)
        {
            var response = await this.client.GetStringAsync(productLink.Url);

            var document = new HtmlDocument();
            document.LoadHtml(response);

            var product = new Product();

            const string NAME_SELECTOR =
                @".//*[contains(concat("" "",normalize-space(@class),"" ""),"" product-detail-actions-box__title "")]";

            var productName = document.DocumentNode
                .SelectSingleNode(NAME_SELECTOR).InnerText.Trim();
            product.Name = productName;

            const string PRICE_SELECTOR =
                @".//*[contains(concat("" "",normalize-space(@class),"" ""),"" product-detail-actions-box__price "")]";

            var priceString = document.DocumentNode
                .SelectSingleNode(PRICE_SELECTOR).InnerText.Trim();

            product.Price = priceString;

            const string SIZES_SELECTOR =
                @".//*[contains(concat("" "",normalize-space(@class),"" ""),"" product-variant-picker "")]//option";

            var sizeNodes = document.DocumentNode.SelectNodes(SIZES_SELECTOR);
            foreach (var sizeNode in sizeNodes)
            {
                var size = sizeNode.InnerText.Trim();
                var isAvailable = sizeNode.Attributes["data-content"].Value
                    .Contains("product-variant--available");

                product.Sizes[size] = isAvailable;
            }

            var results = new List<Product> { product };

            // If we reached depth 1, don't look for any more products
            if (depth == 1) return results;

            // Else continue looking for products
            // TODO: We have to keep track of already visited products
            const string SIMILAR_PRODUCTS_SELECTOR =
                @".//div[contains(concat("" "",normalize-space(@class),"" ""),"" same-design-products-list__item "")]/div[(count(preceding-sibling::*)+1) = 1]/a[(count(preceding-sibling::*)+1) = 1]";

            var similarProductNodes = document.DocumentNode.SelectNodes(SIMILAR_PRODUCTS_SELECTOR);
            if (similarProductNodes != null)
            {
                var similarProductLinks = new List<ProductLink>();
                foreach (var similarProductNode in similarProductNodes)
                {
                    var similarProductLink = new ProductLink();

                    similarProductLink.Url = similarProductNode.GetAttributeValue("href", null);

                    similarProductLinks.Add(similarProductLink);
                }

                results.AddRange(await this.Scrape(similarProductLinks, depth - 1));
            }

            return results;
        }
    }
}
