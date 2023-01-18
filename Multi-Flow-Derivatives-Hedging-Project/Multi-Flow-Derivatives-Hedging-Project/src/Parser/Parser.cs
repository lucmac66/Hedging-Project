using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketData;
using ParameterInfo;
using ModelConverter;

namespace ParserTools
{
	public class Parser
	{
		TestParameters parameters;
		MathParameters mathParameters;

		public Parser(String testDocName)
		{
			this.parameters = ParameterInfo.JsonUtils.JsonIO.FromJson(testDocName);
			this.mathParameters = Converter.Extract(parameters);

		}

		// Getters
		public MathParameters GetMathParameters()
		{
			return this.mathParameters;
		}
		public TestParameters GetTestParameters()
		{
			return this.parameters;
		}
	}
}
