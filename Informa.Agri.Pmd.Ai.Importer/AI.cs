using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Informa.Agri.Pmd.Ai.Importer
{
    public class Ai
    {
        public string Id;
        public string Title;
        public List<string> Tags;
        public string ProductType;
        public string Class;
        public string SalesAmount;
        public string SalesUnit;
        public string LaunchDate;
        public string KeyManufacturerBrand;
        public string OtherManufacturer;
        [JsonIgnore]
        public string StructureImageLink;
        public byte[] StructureImage;
        //public string Application;
        public string Timing;
        public string RateAmount;
        public string RateUnit;
        public List<string> MainCrops;
        public List<string> MainPests;
        public string MainMixturePartners;
        public string RecentHistory;
        public DateTime CreationDate;
        public DateTime LastUpdatedDate;

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
