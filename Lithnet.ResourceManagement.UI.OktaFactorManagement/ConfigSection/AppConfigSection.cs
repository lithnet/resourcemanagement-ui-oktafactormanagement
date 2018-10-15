using System;
using System.Configuration;
using System.Reflection;

namespace Lithnet.ResourceManagement.UI.OktaFactorManagement
{
    internal class AppConfigurationSection : ConfigurationSection
    {
        private static AppConfigurationSection current;

        internal static AppConfigurationSection CurrentConfig
        {
            get
            {
                if (AppConfigurationSection.current == null)
                {
                    AppConfigurationSection.current = AppConfigurationSection.GetConfiguration();
                }

                return AppConfigurationSection.current;
            }
        }

        internal static AppConfigurationSection GetConfiguration()
        {
            AppConfigurationSection section = (AppConfigurationSection)ConfigurationManager.GetSection("lithnetOktaFactorManagement");

            if (section == null)
            {
                section = new AppConfigurationSection();
            }

            return section;
        }

        public string OktaApiKey
        {
            get => ConfigurationManager.AppSettings["okta-api-key"] ?? ConfigurationManager.AppSettings["okta-factor-reset-api-key"];
        }

        public string OktaDomain
        {
            get => ConfigurationManager.AppSettings["okta-tenant"];
        }

        [ConfigurationProperty("oktaIDAttributeName", IsRequired = false)]
        public string OktaIDAttributeName
        {
            get => (string)this["oktaIDAttributeName"];
            set => this["oktaIDAttributeName"] = value;
        }

        [ConfigurationProperty("displayNameAttributeName", IsRequired = false, DefaultValue = "DisplayName")]
        public string DisplayNameAttributeName
        {
            get => (string)this["displayNameAttributeName"];
            set => this["displayNameAttributeName"] = value;
        }

        [ConfigurationProperty("readFactorsAuthZAttributeName", IsRequired = false)]
        public string ReadFactorsAuthZAttributeName
        {
            get => (string)this["readFactorsAuthZAttributeName"];
            set => this["readFactorsAuthZAttributeName"] = value;
        }

        [ConfigurationProperty("writeFactorsAuthZAttributeName", IsRequired = false)]
        public string WriteFactorsAuthZAttributeName
        {
            get => (string)this["writeFactorsAuthZAttributeName"];
            set => this["writeFactorsAuthZAttributeName"] = value;
        }


        [ConfigurationProperty("collapseOktaVerifyFactors", IsRequired = false, DefaultValue = true)]
        public bool CollapseOktaVerifyFactors
        {
            get => (bool)this["collapseOktaVerifyFactors"];
            set => this["collapseOktaVerifyFactors"] = value;
        }

        [ConfigurationProperty("factor-name-mapping", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FactorNameMappingCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public FactorNameMappingCollection FactorNameMapping => (FactorNameMappingCollection)base["factor-name-mapping"];
    }
}