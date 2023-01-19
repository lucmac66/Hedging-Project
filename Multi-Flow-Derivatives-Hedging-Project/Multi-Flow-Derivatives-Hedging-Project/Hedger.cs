using ParserTools;
using ParameterInfo;
using Grpc;
using ModelConverter;
using Multi_Flow_Derivatives_Hedging_Project.src.Model;
using Google.Protobuf.Collections;
using GrpcPricing.Protos;
using TimeHandler;
using MarketDataGeneration;
using Multi_Flow_Derivatives_Hedging_Project.src.Generator;
using ParameterInfo;
using MultiCashFlow.Common.ParameterInfo;

class Hedger
{
	Parser parser;
	Portfolio portfolio;
	GrpcClient client;
	PastValues pastValues;
	static void Main(string[] args)
	{
		Parser parser;
		PastValues past;
		if ((args.Length > 2) && (args.Length < 1)) {
			Console.WriteLine("This Financial App need one or two argumentsq");
			return;
		}
		else
		{
			String jsonName = args[0];
			parser = new Parser(jsonName);
			if (args.Length == 1)
			{
				var random = new Random();
				MarketInfo mktInfo = new MarketInfo();
				double[] randomSpots= new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
				double[] randomTrends = new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
				for(int i = 0; i < randomSpots.Length; i++)
				{
					randomSpots[i] = 2 * random.NextDouble() + 10;
					randomTrends[i] = 0.5 * random.NextDouble();
				}
				mktInfo.Trends = randomTrends;
				mktInfo.InitialSpots = randomSpots;
				Generator gen = new Generator(mktInfo);
				past = new PastValues(gen.GenerateValues(parser.GetTestParameters()));
			}
			else
			{
				String csvName = args[1];
				past = new PastValues(csvName, parser.GetTestParameters());
			}
		}
		GrpcClient client = new GrpcClient(parser.GetMathParameters());
		Portfolio portfolio = new Portfolio(parser);
		Hedger hedger = new Hedger(parser, portfolio, client, past);
		hedger.RebalanceAll();
		Console.WriteLine("End of Rebalancing");
		OutputData output = new OutputData();
	}

	public Hedger(Parser parser, Portfolio portfolio, GrpcClient client, PastValues pastValues)
	{
		this.parser = parser;
		this.portfolio = portfolio;
		this.client = client;
		this.pastValues = pastValues;
	}

	public void RebalanceAll()
	{
		//Important Dates
		DateTime date = this.portfolio.GetStart();
		DateTime selectDate = this.portfolio.GetStart().AddDays(1);
		DateTime maturity = this.portfolio.GetMaturity();
		//First Rebalance
		RebalanceOnce(date, pastValues.Convert(date));
		
		while(selectDate <= maturity){
			// Condtion : delta between two Rebalances is greater than the period 
			if (DayCount.CountBusinessDays(date, selectDate)> parser.GetTestParameters().RebalancingOracleDescription.Period)
			{
				// Select of the next business day
				if (DayCount.IsBusinessDay(selectDate))
				{
					RebalanceOnce(selectDate, pastValues.Convert(selectDate));
					date = selectDate;
				}
				else
				{
					RebalanceOnce(DayCount.NextBusinessDay(selectDate), pastValues.Convert(selectDate));
				}
			}
			selectDate = selectDate.AddDays(1);
		}
	}

	// One Rebalance Process
	public void RebalanceOnce(DateTime dateT, RepeatedField<PastLines> past)
	{
		Console.WriteLine(dateT);
		bool isMonitoringDate;
		if (parser.GetTestParameters().PayoffDescription.PaymentDates.Contains(dateT))
		{
			isMonitoringDate = true;
		}
		else { 
			isMonitoringDate = false;
		}
		client.Request(past, isMonitoringDate, this.portfolio.MathCurrentTime(dateT));
		portfolio.changeQuantities(client.getDeltas(), dateT);
		portfolio.changeRiskFree(client.getPrice(), past.Last().Value.ToArray());
	}
}