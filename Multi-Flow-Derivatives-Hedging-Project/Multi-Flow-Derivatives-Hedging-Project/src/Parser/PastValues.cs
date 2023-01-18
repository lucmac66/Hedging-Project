using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using GrpcPricing.Protos;
using MarketData;
using ParameterInfo;
using MarketDataGeneration;

namespace ParserTools
{
	public class PastValues
	{
		List<DataFeed> past;
		MarketInfo mktInfo;

		public PastValues(String docName)
		{
			this.past = MarketDataReader.ReadDataFeeds(docName);
			this.mktInfo = new MarketInfo();
		}

		// Getter
		public List<DataFeed> getPast() { return past; }

		// Tool that convert pastValues datas to Repeadted<PastLines> format
		public RepeatedField<PastLines> Convert()
		{
			RepeatedField<PastLines> past = new RepeatedField<PastLines>();
			foreach(var datafeed in past)
			{
				PastLines pastLines = new PastLines();
				foreach(var data in datafeed.Value)
				{
					pastLines.Value.Add(data);
				}
				past.Add(pastLines);
			}
			return past;
		}

		public void GenerateValues(TestParameters parameters)
		{
			List<ShareValue> values = ShareValueGenerator.Create(parameters, mktInfo);
			foreach(var value in values)
			{
				if(past.Contains(value.DateOfPrice))
			}
		}
	}
}
