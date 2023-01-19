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
		if ((args.Length > 3) && (args.Length < 1)) {
			Console.WriteLine("This Financial App need one or two arguments");
			return;
		}
		else
		{
			String jsonName;
			if (args.Length == 1)
			{
				jsonName = args[0];
				parser = new Parser(jsonName);
				var random = new Random();
				MarketInfo mktInfo = new MarketInfo();
				double[] randomSpots= new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
				double[] randomTrends = new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
				for(int i = 0; i < randomSpots.Length; i++)
				{
					randomSpots[i] = 15 * random.NextDouble() + 5;
					randomTrends[i] = 4 * random.NextDouble() -2;
				}
				mktInfo.Trends = randomTrends;
				mktInfo.InitialSpots = randomSpots;
				Generator gen = new Generator(mktInfo);
				past = new PastValues(gen.GenerateValues(parser.GetTestParameters()));
			}
			else
			{
				if (args.Length == 2)
				{
					jsonName= args[0];
					parser = new Parser(jsonName);
					String csvName = args[1];
					past = new PastValues(csvName, parser.GetTestParameters());
				}
				else
				// Run with -g command : indication that we want to create a new MarketData csv File
				{
					if (args[0] == "-g")
					{
						jsonName = args[1];
						parser = new Parser(jsonName);
						var random = new Random();
						MarketInfo mktInfo = new MarketInfo();
						double[] randomSpots = new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
						double[] randomTrends = new double[parser.GetTestParameters().PricingParams.UnderlyingPositions.Count];
						for (int i = 0; i < randomSpots.Length; i++)
						{
							randomSpots[i] = 15 * random.NextDouble() + 5;
							randomTrends[i] = 4 * random.NextDouble() - 2;
						}
						mktInfo.Trends = randomTrends;
						mktInfo.InitialSpots = randomSpots;
						Generator gen = new Generator(mktInfo);
						gen.GenerateCSV(parser.GetTestParameters(), args[2]);
						past = new PastValues(args[2], parser.GetTestParameters());
					}
					else
					{
						Console.WriteLine("3 arguments situation : first argument need to be '-g' because it's the run with creation of csv file");
						return;
					}
				}

				
			}
		}
		GrpcClient client = new GrpcClient(parser.GetMathParameters());
		Portfolio portfolio = new Portfolio(parser);
		Hedger hedger = new Hedger(parser, portfolio, client, past);
		hedger.RebalanceAll();
		portfolio.ExportCsv();
		Console.WriteLine("CSV output is generated");
	}

	public Hedger(Parser parser, Portfolio portfolio, GrpcClient client, PastValues pastValues)
	{
		this.parser = parser;
		this.portfolio = portfolio;
		this.client = client;
		this.pastValues = pastValues;
	}

	// method that determine if it's 
	public void RebalanceAll()
	{
		//Important Dates
		DateTime date = this.portfolio.GetStart();
		DateTime selectDate = this.portfolio.GetStart().AddDays(1);
		DateTime maturity = this.portfolio.GetMaturity();

		//First Rebalance
		RebalanceOnce(date, pastValues);
		
		while(selectDate <= maturity){
			// Condtion : delta between two Rebalances is greater than the period 
			if (DayCount.CountBusinessDays(date, selectDate)> parser.GetTestParameters().RebalancingOracleDescription.Period)
			{
				// Select of the next business day
				if (DayCount.IsBusinessDay(selectDate))
				{
					RebalanceOnce(selectDate, pastValues);
					date = selectDate;
				}
				else
				{
					RebalanceOnce(DayCount.NextBusinessDay(selectDate), pastValues);
				}
			}
			selectDate = selectDate.AddDays(1);
		}
	}

	// One Rebalance Process
	public void RebalanceOnce(DateTime dateT, PastValues pastValues)
	{
		List<DateTime> days = new List<DateTime>();
		foreach( var dates in parser.GetTestParameters().PayoffDescription.PaymentDates.ToArray()){
			if(dates < dateT) { days.Add(dates); }
		}
		days.Add(dateT);
		RepeatedField<PastLines> past = pastValues.Convert(days);
		bool isMonitoringDate;
		if (parser.GetTestParameters().PayoffDescription.PaymentDates.Contains(dateT))
		{
			isMonitoringDate = true;
		}
		else { 
			isMonitoringDate = false;
		}
		client.Request(past, isMonitoringDate, this.portfolio.MathCurrentTime(dateT));
		if(dateT == this.portfolio.GetStart())
		{
			portfolio.FirstChangeAllQuantities(dateT, pastValues, client.GetDeltas(), client.GetPrice());
		}
		else
		{
			portfolio.ChangeAllQuantities(dateT, pastValues, client.GetDeltas());
		}
		
		portfolio.SetupOutput(client);
	}
}