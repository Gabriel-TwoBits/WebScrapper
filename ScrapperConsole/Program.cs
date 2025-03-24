using HtmlAgilityPack;
using System.Text.RegularExpressions;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace ScrapperConsole 
{
    class Program
    {
        static void Main()
        {
            string url = "https://books.toscrape.com/catalogue/category/books/travel_2/index.html";
            var doc = GetDocument(url);
            List<string> links = GetLinks(url);
            List<Object> objects = GetObjects(links);

            foreach (var link in links)
            {
                Console.WriteLine(link);
            }

            foreach (var obj in objects)
            {
                Console.WriteLine(obj.Title + "\n$" + obj.Price + "\n");
            }
        }
        private static List<Object> GetObjects(List<string> links)
        {
            var objects = new List<Object>();
            foreach (var link in links)
            {
                var doc = GetDocument(link);
                var temp_obj = new Object();

                temp_obj.Title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;

                var xpath = "//*[@class=\"col-sm-6 product_main\"]/*[@class=\"price_color\"]";
                var price_raw = doc.DocumentNode.SelectSingleNode(xpath).InnerText;

                temp_obj.Price = Extract_Price(price_raw);
                objects.Add(temp_obj);    
            }

            return objects;
        }

        static double Extract_Price(string raw)
        {
            var reg = new Regex(@"[\d\.,]+", RegexOptions.Compiled);
            var m = reg.Match(raw);

            if (!m.Success)
            {
                return 0;
            }

            double final = Convert.ToDouble(m.Value);

            return final;
        }

        static List<string> GetLinks(string url)
        {

            var doc = GetDocument(url);
            var linkNodes = doc.DocumentNode.SelectNodes("//h3/a");

            var baseUri = new Uri(url);
            var links = new List<string>();

            foreach (var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;
                link = new Uri(baseUri, link).AbsoluteUri;
                links.Add(link);
            }

            return links;
        }

        static HtmlDocument GetDocument(string url) 
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            
            return doc;
        }
    }
}