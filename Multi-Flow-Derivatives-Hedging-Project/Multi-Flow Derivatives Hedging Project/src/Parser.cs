using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarketData;
using ParameterInfo;

namespace Multi_Flow_Derivatives_Hedging_Project
{
	public class Parser
	{
		public DataFeed marketData;
		public TestParameters parameters;

		public Parser(String marketDataDocName, String parametersDocName)
		{
			this.parameters = ParameterInfo.JsonUtils.JsonIO.FromJson(parametersDocName);
		}
	}
}
