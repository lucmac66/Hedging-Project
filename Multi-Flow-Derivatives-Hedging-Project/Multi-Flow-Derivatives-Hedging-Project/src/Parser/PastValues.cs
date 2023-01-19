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
using Google.Protobuf.WellKnownTypes;

namespace ParserTools
{
	public class PastValues
	{
		SortedDictionary<DateTime, Dictionary<String, double>> past;
		public PastValues(String docName, TestParameters parameters)
		{
			this.past = new SortedDictionary<DateTime, Dictionary<String, double>>();
			List<DataFeed> pastValues = MarketDataReader.ReadDataFeeds(docName);
			foreach(var value in pastValues)
			{ 
				past.Add(value.Date, value.SpotList);
			}
		}

		public PastValues(List<ShareValue> list)
		{
			this.past = new SortedDictionary<DateTime, Dictionary<String, double>>();
			foreach (var value in list)
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
					Dictionary<string, double> dict = new Dictionary<string, double>();
					dict.Add(value.Id, value.Value);
					past.Add(value.DateOfPrice, dict);
				}
			}

		}

		// Getter
		public SortedDictionary<DateTime, Dictionary<String, double>> GetPast() { return past; }

		// Tool that convert pastValues datas to Repeadted<PastLines> format
		public RepeatedField<PastLines> Convert(List<DateTime> list)
		{;
			RepeatedField<PastLines> pastConverted = new RepeatedField<PastLines>();
			foreach (var datafeed in list) 
			{ 
				PastLines lines = new PastLines();
				foreach (var data in past[datafeed])
				{
					lines.Value.Add(data.Value);
				}
				pastConverted.Add(lines);
			}
			return pastConverted;
		}

		public void PrintDates()
		{
			foreach (var date in past.Keys) {
				Console.WriteLine(date);
			}
		}
	}
}

