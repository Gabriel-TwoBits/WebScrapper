using HtmlAgilityPack;
using System.Text.RegularExpressions;
//using CsvHelper;

// WARNING!! The headers must be changed to each specifc website, at the functions:
// GetObjects and GetLink
// they can be found as strings, at the lines 74, 78 and 114.

namespace ScrapperConsole 
{
    class Program
    {
        static void Main()
        {
            //example url used: https://books.toscrape.com/catalogue/category/books/travel_2/index.html;

            Console.WriteLine("Please, insert a valid URL:");
            string url = Console.ReadLine();
            bool urlCheck = ValidateUrl(url);

            Console.Clear();

            if (!urlCheck)
            {
                Console.WriteLine("Invalid Url! Please insert another one!\n\n");
                Main();
            }
            else
            {
                var doc = GetDocument(url);
                List<string> links = GetLinks(url);
                List<Object> objects = GetObjects(links);

                /*
                foreach (var link in links)
                {
                    Console.WriteLine(link);
                }
                */

                foreach (var obj in objects)
                {
                    Console.WriteLine(obj.Title + "\n$" + obj.Price + "\n\n");
                }
            }
        }

        // checks if the inserted URL is valid
        static bool ValidateUrl(string url)
        {
            var urlRegex = new Regex(
            @"^(https?|ftps?):\/\/(?:[a-zA-Z0-9]" +
            @"(?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}" +
            @"(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}" +
            @"|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?" +
            @"(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$",
            RegexOptions.IgnoreCase);

            urlRegex.Matches(url);

            return urlRegex.IsMatch(url);
        }

        // function to get the objects from specifc nodes and subnodes
        private static List<Object> GetObjects(List<string> links)
        {
            var objects = new List<Object>();
            foreach (var link in links)
            {
                var doc = GetDocument(link);
                var temp_obj = new Object();

                //string inside the SelectSingleNode must be refering to the object header
                temp_obj.Title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;

                // the xpath must contain a class as specific as possible, avoiding the program
                // to go for the wrong class
                var xpath = "//*[@class=\"col-sm-6 product_main\"]/*[@class=\"price_color\"]";
                var price_raw = doc.DocumentNode.SelectSingleNode(xpath).InnerText;

                temp_obj.Price = Extract_Price(price_raw);
                objects.Add(temp_obj);
            }

            return objects;
        }

        // this function turns a string element in a double element, being perfect to extract and show prices
        // it works better if you're creating a sheet based on the extracted data

        static double Extract_Price(string raw)
        {
            //the Regex formula excludes undesired characters from the string
            var reg = new Regex(@"[\d\.,]+", RegexOptions.Compiled);
            var m = reg.Match(raw);

            if (!m.Success)
            {
                return 0;
            }

            double final = Convert.ToDouble(m.Value);

            return final;
        }

        // function made to get all the url's from the main url used. those links will be recursively
        // analyzed by only 1 layer, but can be extended for more.
        static List<string> GetLinks(string url)
        {

            var doc = GetDocument(url);
            //nodes headers must be changed to each specific website
            var linkNodes = doc.DocumentNode.SelectNodes("//h3/a");

            //Uri is absolutely necessary to make the url absolute in the following "foreach"
            var baseUri = new Uri(url);
            var links = new List<string>();

            foreach (var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;
                link = new Uri(baseUri, link).AbsoluteUri;
                links.Add(link);
            }

            //at the end, the function return a list of strings, which are the URLs
            return links;
        }

        // simply catch the raw HTML file, returning the HtmlDocument from the HtmlAgilityPack
        static HtmlDocument GetDocument(string url) 
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            
            return doc;
        }


        /*
         * Failed attempt to convert into a .csv file.
         * 
        private static void ExportCsv(List<Object> data)
        {
            using (var writer = new StreamWriter("data.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }
        */
    }
}