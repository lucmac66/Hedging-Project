using ParserTools;
using ParameterInfo;
using Grpc;
using ModelConverter;
using Multi_Flow_Derivatives_Hedging_Project.src.Model;
using Google.Protobuf.Collections;
using GrpcPricing.Protos;
using TimeHandler;

class Hedger
{
	Parser parser;
	Portfolio portfolio;
	GrpcClient client;
	PastValues pastValues;
	static void Main(string[] args)
	{
		/*if (args.Length != 2) {
			Console.WriteLine("This Financial App need two arguments");
			return;
		}*/
		String jsonName = args[0];
		String pastValuesDocName = args[1];
		PastValues pastValues = new PastValues(pastValuesDocName);
		Parser parser = new Parser(jsonName);
		GrpcClient client = new GrpcClient(parser.GetMathParameters());
		Portfolio portfolio = new Portfolio(parser);
		Hedger hedger = new Hedger(parser, portfolio, client, pastValues);
		hedger.RebalanceAll();
		Console.WriteLine("End of Rebalancing");
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
		DateTime selectDate = this.portfolio.GetStart();
		DateTime maturity = this.portfolio.GetMaturity();

		//First Rebalance
		RebalanceOnce(date, pastValues.Convert());
		
		while(date <= maturity){
			// Condtion : delta between two Rebalances is greater than the period 
			if (DayCount.CountBusinessDays(date, selectDate)> parser.GetTestParameters().RebalancingOracleDescription.Period)
			{
				// Select of the next business day
				if (DayCount.IsBusinessDay(selectDate))
				{
					RebalanceOnce(selectDate, pastValues.Convert());
					date = selectDate;
				}
				else
				{
					RebalanceOnce(DayCount.NextBusinessDay(selectDate), pastValues.Convert());
				}
			}
			selectDate = date.AddDays(1);
		}
	}

	// One Rebalance Process
	public void RebalanceOnce(DateTime dateT, RepeatedField<PastLines> past)
	{
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