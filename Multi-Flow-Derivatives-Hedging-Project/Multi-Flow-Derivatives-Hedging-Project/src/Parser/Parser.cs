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
			String jsonText = "";
			String text = File.ReadAllText(testDocName);
			foreach (var line in text){
				jsonText+= line;
			}
			Console.WriteLine(jsonText);
			this.parameters = ParameterInfo.JsonUtils.JsonIO.FromJson(jsonText);
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
