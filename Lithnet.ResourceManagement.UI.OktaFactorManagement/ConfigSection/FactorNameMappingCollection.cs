using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Lithnet.ResourceManagement.UI.OktaFactorManagement
{
    public class FactorNameMappingCollection : ConfigurationElementCollection
    {
        public FactorNameMapping this[int index]
        {
            get => (FactorNameMapping) this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FactorNameMapping();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FactorNameMapping) element).FactorID;
        }
    }
}