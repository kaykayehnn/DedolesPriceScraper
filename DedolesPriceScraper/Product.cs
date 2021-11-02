using System.Collections.Generic;
using System.Linq;

namespace DedolesPriceScraper
{
    public class Product
    {
        public Product()
        {
            this.Sizes = new Dictionary<string, bool>();
        }

        public string Name { get; set; }
        public string Price { get; set; }
        public Dictionary<string,bool> Sizes { get; set; }

        public override string ToString()
        {
            // Get only available sizes
            var availableSizes = this.Sizes
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key);
            var sizesString = string.Join(", ", availableSizes);

            return $"{this.Name}: {this.Price} ({sizesString})";
        }
    }
}
