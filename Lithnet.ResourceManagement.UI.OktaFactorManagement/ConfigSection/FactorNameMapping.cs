using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Lithnet.ResourceManagement.UI.OktaFactorManagement
{
    public class FactorNameMapping : ConfigurationElement
    {
        private const string PropFactorID = "factor-id";
        private const string PropDisplayName = "display-name";

        [ConfigurationProperty(FactorNameMapping.PropFactorID, IsRequired = true)]
        public string FactorID => (string)this[FactorNameMapping.PropFactorID];

        [ConfigurationProperty(FactorNameMapping.PropDisplayName, IsRequired = true)]
        public string DisplayName => (string)this[FactorNameMapping.PropDisplayName];
    }
}