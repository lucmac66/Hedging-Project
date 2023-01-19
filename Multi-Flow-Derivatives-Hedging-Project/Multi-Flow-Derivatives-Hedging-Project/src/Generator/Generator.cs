using MarketData;
using MarketDataGeneration;
using ParameterInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Flow_Derivatives_Hedging_Project.src.Generator
{

    public class Generator
    {
        MarketInfo mktInfo;

        public Generator(MarketInfo infos)
        {
            this.mktInfo = infos;
        }

        // Generate new values in past
        public List<ShareValue> GenerateValues(TestParameters parameters)
        {
            List<ShareValue> values = ShareValueGenerator.Create(parameters, mktInfo);
            return values;
        }

        public void GenerateCSV(TestParameters parameters, string pathCSV)
        {
            ShareValueGenerator.Create(parameters, mktInfo, pathCSV);
        }
    }
}
