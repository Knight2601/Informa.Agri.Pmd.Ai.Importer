using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Aspose.Words;
using Aspose.Words.Saving;
using HtmlAgilityPack;
using Nest;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using License = Aspose.Words.License;

namespace Informa.Agri.Pmd.Ai.Importer
{
    class Program
    {
        private const int LogFileSizeLimitBytes = 1073741824;
        private const int RetainedLogFileCountLimit = 31;
        private static IElasticClient _client;

        static void Main(string[] args)
        {
            _client = new ElasticClient(new Uri(ConfigurationManager.AppSettings["ElasticsearchBaseUri"]));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Console(LogEventLevel.Verbose)
                .WriteTo.RollingFile(new JsonFormatter(), ConfigurationManager.AppSettings["JsonLogFilePathFormat"],
                    fileSizeLimitBytes: LogFileSizeLimitBytes, retainedFileCountLimit: RetainedLogFileCountLimit)
                .CreateLogger();

            var license = new License();
            license.SetLicense(".\\Aspose.Total.lic");

            var b = DoWork("A-Z.docx");

            Console.WriteLine("Any key to exit...");
            Console.ReadLine();
        }

        private static bool DoWork(string file)
        {
            Log.Information($"Reading {file}");

            var d = new Document($".\\source\\{file}");

            Log.Information($"{d.PageCount} pages read in from {file}.");

            var htmlXPathDocument = ReadIn(ProcessDocumentToHtml(d));

            ProcessXDocument(htmlXPathDocument);

            return true;
        }

        private static void ProcessXDocument(HtmlDocument doc)
        {
            var listOf = new List<Ai>();
            var nodes = doc.DocumentNode.Descendants("table")
                .Select(y => y.Descendants()
                    .Where(x => x.Name == "tr"))
                .ToList();
            var idxPrefix = "AI-";
            var idx = 0;
            foreach (var node in nodes) // table's
            {
                var thisAi = new Ai();
                var counter = 0;
                thisAi.Id = $"{idxPrefix}{idx}";
                foreach (var child in node.Select(x => x.Descendants().Where(xx => xx.Name == "td")).ToList()) // td's
                {

                    foreach (var c in child)
                    {
                        ProcessTd(c, ref thisAi, ref counter);
                        counter++;
                    }
                }
                listOf.Add(thisAi);
                idx++;
            }

//            WriteOut(listOf);

            var readyToGoDocs = JsonConvert.SerializeObject(listOf);

            File.WriteAllText("output.json", readyToGoDocs);

            Console.WriteLine($"output.json written.");

            var result = BulkUpload(listOf);
        }

        private static bool BulkUpload(List<Ai> readyToGoDocs)
        {

            using (var bulkAll = _client.BulkAll(readyToGoDocs, b => b
                .Index("pmcd-ai-index")
                .BackOffRetries(2)
                .BackOffTime("20s")
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(6)
                .Size(1000)))
            {
                var waitHandle = new CountdownEvent(1);

                bulkAll.Subscribe(new BulkAllObserver(
                    onNext: (b) => { Console.Write("x"); },
                    onError: (e) => { throw e; },
                    onCompleted: () => waitHandle.Signal()
                ));

                waitHandle.Wait();
            }


            Log.Information("Written to ES");
            return true;
        }

        private static void WriteOut(List<Ai> listOf)
        {
            foreach (var ai in listOf)
            {
                Console.WriteLine($"---AI---");
                Console.WriteLine($"Id: {ai.Id}");
                Console.WriteLine($"Title: {ai.Title}");
                Console.WriteLine($"ProductType: {ai.ProductType}");
                Console.WriteLine($"Class: {ai.Class}");
                Console.WriteLine($"SalesAmount: {ai.SalesAmount}");
                Console.WriteLine($"SalesUnit: {ai.SalesUnit}");
                Console.WriteLine($"LaunchDate: {ai.LaunchDate}");
                Console.WriteLine($"KeyManufacturerBrand: {ai.KeyManufacturerBrand}");
                Console.WriteLine($"OtherManufacturer: {ai.OtherManufacturer}");
                Console.WriteLine($"StructureImageLink: {ai.StructureImageLink}");
                Console.WriteLine($"StructureImageBytes: {ai.StructureImage.LongLength} bytes");
                Console.WriteLine($"Timing: {ai.Timing}");
                Console.WriteLine($"RateAmount: {ai.RateAmount}");
                Console.WriteLine($"RateUnit: {ai.RateUnit}");
                Console.WriteLine($"MainCrops: {string.Join("|", ai.MainCrops)}");
                Console.WriteLine($"MainPests: {string.Join("|", ai.MainPests)}");
                Console.WriteLine($"MainMixturePartners: {ai.MainMixturePartners}");
                Console.WriteLine($"RecentHistory: {ai.RecentHistory}");
                Console.WriteLine($"CreationDate: {ai.CreationDate}");
            }
        }

        private static void ProcessTd(HtmlNode td, ref Ai ai, ref int counter)
        { // we're in
            var child = td.FirstChild;
            if (child != null)
            {
                switch (counter)
                {
                    case 0:
                        ai.Title = child.InnerText.Trim();
                        ai.Tags.Add(child.InnerText.Trim());
                        Inform(ai.Title);
                        break;
                    case 3:
                        ai.SalesUnit = child.InnerText.Substring(child.InnerText.IndexOf("(", StringComparison.Ordinal) + 1,
                            child.InnerText.IndexOf(")", StringComparison.Ordinal) - child.InnerText.IndexOf("(", StringComparison.Ordinal) - 1).Trim();
                        break;
                    case 5:
                        ai.ProductType = child.InnerText;
                        break;
                    case 6:
                        ai.Class = child.InnerText;
                        break;
                    case 7:
                        ai.SalesAmount = child.InnerText;
                        break;
                    case 8:
                        ai.LaunchDate = child.InnerText;
                        break;
                    case 12:
                        ai.KeyManufacturerBrand = child.InnerText;
                        break;
                    case 13:
                        ai.OtherManufacturer = child.InnerText;
                        break;
                    case 11:
                        var img = child.ParentNode.SelectSingleNode("p/img");
                        if (img != null)
                        {
                            ai.StructureImageLink = img.GetAttributeValue("src", "");
                            try
                            {
                                ai.StructureImage = File.ReadAllBytes(ai.StructureImageLink);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            ai.StructureImageLink = child.ParentNode.InnerText.Replace("Structure","").Replace("&#xa0;", "").Trim();
                        }


                        break;
                    case 15:
                        ai.Timing = child.ParentNode.SelectNodes("p/span").Count == 2 ? child.ParentNode.InnerText.Replace("Timing:", "").Trim() : child.ParentNode.NextSibling.InnerText.Trim();
                        break;
                    default:
   //                     ai.MainCrops.Add(child.InnerText);
                        if (child.InnerText.Contains("Main Mixture Partners"))
                        {
                            ai.MainMixturePartners = $"{child.InnerText} ".Substring(23).Trim();
                        }
                        else if (child.InnerText.Contains("Recent History:"))
                        {
                            var paragraphs = child.ParentNode.SelectNodes("p");
                            foreach (var paragraph in paragraphs)
                            {
                                if (!paragraph.InnerText.StartsWith("Recent History"))
                                {
                                    ai.RecentHistory += paragraph.InnerText + "\n\n";
                                }
                            }
                        }
                        else if (child.ParentNode.InnerText.Trim().StartsWith("Rate "))
                        {
                            ai.RateAmount = child.InnerText.Substring(child.InnerText.IndexOf(":", StringComparison.Ordinal)+1).Trim();
                            ai.RateUnit = child.InnerText.Substring(child.InnerText.IndexOf("(", StringComparison.Ordinal)+1,
                                child.InnerText.IndexOf(")", StringComparison.Ordinal) - child.InnerText.IndexOf("(", StringComparison.Ordinal)-1).Trim();
                        }
                        else if (child.InnerText.Trim() == "Main Crops")
                        {
                            HtmlNode current = child.ParentNode.ParentNode.NextSibling.SelectNodes("td").First();
                            for(var i = 0; i < 10; i++)
                            {
                                if (current.FirstChild.InnerText.Trim()
                                    .StartsWith("Main Mixture") || current.FirstChild.InnerText.Trim()
                                        .StartsWith("Recent History")) continue;
                                var index = 0;
                                foreach (var n in current.ParentNode.ChildNodes)
                                {
                                    if (index == 0)
                                    {
                                        ai.MainCrops.Add(n.InnerText);
                                    }
                                    else
                                    {
                                        ai.MainPests.Add(n.InnerText);
                                    }
                                    index++;
                                }

                                current = current.ParentNode.NextSibling.SelectNodes("td").First();
                            }
                        }

                        break;


                }
            }
        }

        private static void Inform(string aiTitle)
        {
            Log.Information($"  working on {aiTitle}");
        }

        private static HtmlDocument ReadIn(string htmlFileName)
        {
            var doc = new HtmlDocument();
            doc.Load(htmlFileName);
            return doc;
        }

        private static string ProcessDocumentToHtml(Document document)
        {
            var outputHtml = "output.html";
            document.Save(outputHtml, new HtmlSaveOptions()
            {
                AllowNegativeIndent = true,
                ColorMode = ColorMode.Normal,
                CssClassNamePrefix = "AI-",
                DmlEffectsRenderingMode = DmlEffectsRenderingMode.Simplified,
                DmlRenderingMode = DmlRenderingMode.DrawingML,
                ImageResolution = 120,
                ImagesFolder = "output"
            });

            return outputHtml;
        }
    }
}
