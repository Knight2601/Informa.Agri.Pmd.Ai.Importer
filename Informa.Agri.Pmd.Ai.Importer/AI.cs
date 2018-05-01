using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json;

namespace Informa.Agri.Pmd.Ai.Importer
{
    [ElasticsearchType(Name = "ai")]
    public class Ai
    {
        [Keyword]
        public string Id { get; set; }
        [Keyword]
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        [Keyword]
        public string ProductType { get; set; }
        [Keyword]
        public string Class { get; set; }
        [Text]
        public string SalesAmount { get; set; }
        [Text]
        public string SalesUnit { get; set; }
        [Keyword]
        public string LaunchDate { get; set; }
        [Keyword]
        public string KeyManufacturerBrand { get; set; }
        [Keyword]
        public string OtherManufacturer { get; set; }
        [Text(Ignore = true)]
        [JsonIgnore]
        public string StructureImageLink { get; set; }
        [Text]
        public byte[] StructureImage { get; set; }
        [Text]
        public string Timing { get; set; }
        [Text]
        public string RateAmount { get; set; }
        [Text]
        public string RateUnit { get; set; }
        public List<string> MainCrops { get; set; }
        public List<string> MainPests { get; set; }
        [Keyword]
        public string MainMixturePartners { get; set; }
        [Keyword]
        public string RecentHistory { get; set; }
        [Date]
        public DateTime CreationDate { get; set; }
        [Date]
        public DateTime LastUpdatedDate { get; set; }

        public bool InProduction { get; set; }

        public Ai()
        {
            MainCrops = new List<string>();
            MainPests = new List<string>();
            CreationDate = DateTime.UtcNow;
            LastUpdatedDate = DateTime.MinValue;
            StructureImage = new byte[0];
            Tags = new List<string>();
        }

        public Ai(string id, string title, string productType, string @class, string salesAmount, string salesUnit, string launchDate, string keyManufacturerBrand, string otherManufacturer, string structureImageLink, byte[] structureImage, string timing, string rateAmount, string rateUnit, List<string> mainCrops, List<string> mainPests, string mainMixturePartners, string recentHistory, DateTime creationDate, DateTime lastUpdatedDate)
        {
            Id = id;
            Title = title;
            ProductType = productType;
            Class = @class;
            SalesAmount = salesAmount;
            SalesUnit = salesUnit;
            LaunchDate = launchDate;
            KeyManufacturerBrand = keyManufacturerBrand;
            OtherManufacturer = otherManufacturer;
            StructureImageLink = structureImageLink;
            StructureImage = structureImage;
            Timing = timing;
            RateAmount = rateAmount;
            RateUnit = rateUnit;
            MainCrops = mainCrops;
            MainPests = mainPests;
            MainMixturePartners = mainMixturePartners;
            RecentHistory = recentHistory;
            CreationDate = creationDate;
            LastUpdatedDate = lastUpdatedDate;
        }
    }
}
