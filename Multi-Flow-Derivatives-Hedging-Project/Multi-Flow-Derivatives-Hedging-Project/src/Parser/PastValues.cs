using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using GrpcPricing.Protos;
using Google.Protobuf.Collections;
using MarketData;
using ParameterInfo;
using MarketDataGeneration;
using System.Collections.Generic;

namespace ParserTools
{
	public class PastValues
	{
		SortedDictionary<DateTime, Dictionary<String, double>> past;
		MarketInfo mktInfo;

		public PastValues(String docName)
		{
			past = new SortedDictionary<DateTime, Dictionary<String, double>>();
			var pastValues = MarketDataReader.ReadDataFeeds(docName);
			foreach(var value in pastValues)
			{
				past.Add(value.Date, value.SpotList);
			}
			this.mktInfo = new MarketInfo();
		}

		// Getter
		public SortedDictionary<DateTime, Dictionary<String, double>> getPast() { return past; }

		// Tool that convert pastValues datas to Repeadted<PastLines> format
		public RepeatedField<PastLines> Convert()
		{
			RepeatedField<PastLines> pastConverted = new RepeatedField<PastLines>();
			foreach(var datafeed in past.Values)
			{
				PastLines pastLines = new PastLines();
				foreach(var data in datafeed)
				{
					pastLines.Value.Add(data.Value);
				}
				pastConverted.Add(pastLines);
			}
			return pastConverted;
		}

		// Generate new values in past
		public void GenerateValues(TestParameters parameters)
		{
			List<ShareValue> values = ShareValueGenerator.Create(parameters, this.mktInfo);
			foreach(var value in values)
			{
				if (past.ContainsKey(value.DateOfPrice))
				{
					if (past[value.DateOfPrice].ContainsKey(value.Id))
					{
						past[value.DateOfPrice][value.Id] = value.Value;
					}
					else
					{
						past[value.DateOfPrice].Add(value.Id, value.Value);
					}
				}
				else
				{
					Dictionary<String, double> dict = new Dictionary<string, double>();
					dict.Add(value.Id, value.Value);
					past.Add(value.DateOfPrice, dict);
				}
			}			
		}

		public void GenerateCSV(TestParameters parameters, String pathCSV)
		{
			ShareValueGenerator.Create(parameters, this.mktInfo, pathCSV);
		}
	}
}
