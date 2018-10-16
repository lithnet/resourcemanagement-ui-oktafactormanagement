using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lithnet.ResourceManagement.UI.OktaFactorManagement
{
    internal class OktaFactor
    {
        public OktaFactor(JToken factor)
        {
            this.Provider = (string)factor["provider"];
            this.FactorType = (string)factor["factorType"];
            this.Status = (string)factor["status"];
            this.LastUpdated = (DateTime?)factor["lastUpdated"];
            this.ID = (string)factor["id"];
            this.IDsToReset = new List<string>();
            if (this.ID != null)
            {
                this.IDsToReset.Add(this.ID);
            }

            this.FactorTypeID = $"{this.Provider}/{this.FactorType}";
            this.DisplayName = GetFactorName(this.FactorTypeID);
            this.Skip = string.IsNullOrEmpty(this.DisplayName);
        }

        public bool Skip { get; set; }

        public string Provider { get; }

        public string FactorType { get; }

        public string FactorTypeID { get; }
        
        public string DisplayName { get; }

        public string Status { get; }

        public bool IsActive => string.Equals(this.Status, "active", StringComparison.OrdinalIgnoreCase);

        public DateTime? LastUpdated { get; }

        public string ID { get; }

        internal List<string> IDsToReset { get; }

        public bool CanReset => !string.Equals(this.Status, "NOT_SETUP", StringComparison.OrdinalIgnoreCase);

        private static string GetFactorName(string factorTypeID)
        {
            string factorid = factorTypeID;

            foreach (FactorNameMapping item in AppConfigurationSection.CurrentConfig.FactorNameMapping.OfType<FactorNameMapping>())
            {
                if (string.Equals(item.FactorID, factorid, StringComparison.OrdinalIgnoreCase))
                {
                    return item.DisplayName;
                }
            }

            return factorid;
        }
    }
}