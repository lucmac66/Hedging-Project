using ModelConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterInfo;
using ParserTools;
using CsvHelper;
using TimeHandler;
using System.Collections;
using Grpc;
using ParameterInfo.JsonUtils;

namespace Multi_Flow_Derivatives_Hedging_Project.src.Model
{
	public class Portfolio
	{
		// Portfolio Composition

		Dictionary<string, double> assetQuantities;
		double freeRiskQuantity;
		double oldPrice;

		string output;

		// Dates
		DateTime maturity;
		DateTime currentTime;
		DateTime startTime;

		//Market Parameters
		TestParameters testParameters;
		double rateFreeRisk;

		//Tool
		IMathDateConverter mathDateConverterconverter;

		public Portfolio(Parser parser) 
		{
			this.output = "";
			assetQuantities = new Dictionary<string, double>();
			freeRiskQuantity = 0;
			testParameters = parser.GetTestParameters();
			currentTime = testParameters.PayoffDescription.CreationDate;
			startTime = testParameters.PayoffDescription.CreationDate;
			maturity = testParameters.PayoffDescription.PaymentDates.Last().Date;
			rateFreeRisk = testParameters.AssetDescription.CurrencyRates.First().Value;
			mathDateConverterconverter = new MathDateConverter(testParameters.NumberOfDaysInOneYear);

			foreach (var key in testParameters.PricingParams.UnderlyingPositions.Keys)
			{
				assetQuantities[key] = 0;
			}
		}
		
		// Change of asset quantities after deltas' calculations and freeRiskQuantity
		public void ChangeAllQuantities(DateTime t, PastValues past, double[] newDeltas)
		{
			freeRiskQuantity = this.oldPrice;
			foreach (var key in assetQuantities.Keys)
			{
				assetQuantities[key] = newDeltas[testParameters.PricingParams.UnderlyingPositions[key]];
				freeRiskQuantity -= past.GetPast()[t][key] * assetQuantities[key];
			}
			double deltaTime = mathDateConverterconverter.ConvertToMathDistance(t, maturity);
			freeRiskQuantity *= Math.Exp(rateFreeRisk * deltaTime);
			SetPortfolioPrice(past);
			currentTime = t;
		}

		public void FirstChangeAllQuantities(DateTime t, PastValues past, double[] newDeltas, double price)
		{
			this.oldPrice = price;
			freeRiskQuantity = this.oldPrice;
			foreach (var key in assetQuantities.Keys)
			{
				assetQuantities[key] = newDeltas[testParameters.PricingParams.UnderlyingPositions[key]];
				freeRiskQuantity -= past.GetPast()[t][key] * assetQuantities[key];
			}
			double deltaTime = mathDateConverterconverter.ConvertToMathDistance(t, maturity);
			freeRiskQuantity *= Math.Exp(rateFreeRisk * deltaTime);
			SetPortfolioPrice(past);
		}

		// Tool
		public double MathCurrentTime(DateTime t)
		{
			return mathDateConverterconverter.ConvertToMathDistance(startTime, t);
		}

		public void SetPortfolioPrice(PastValues past)
		{
			double price = 0;
			foreach(var key in assetQuantities.Keys)
			{
				price += assetQuantities[key] * past.GetPast()[currentTime][key];
			}
			double deltaTime = mathDateConverterconverter.ConvertToMathDistance(currentTime, maturity);
			this.oldPrice = price + freeRiskQuantity / Math.Exp(rateFreeRisk * deltaTime);
		}

		public void SetupOutput(GrpcClient client)
		{
			OutputData outputData = new OutputData()
			{
				OutputDate = currentTime,
				PriceStdDev = client.GetStdPrice(),
				Price = client.GetPrice(),
				PortfolioValue = this.oldPrice,
				Delta = client.GetDeltas(),
				DeltaStdDev = client.GetStdDeltas()
			};
			output += JsonIO.ToJson(outputData) + "\n";
		}

		public void ExportCsv()
		{
			File.WriteAllText("output.json", output);
		}

		//Getters
		public DateTime GetStart()
		{
			return startTime;
		}
		public DateTime GetMaturity()
		{
			return maturity;
		}
		public double GetFreeRiskQuantity()
		{
			return freeRiskQuantity;
		}

		public Dictionary<string, double> GetAssetQuantities()
		{
			return assetQuantities;
		}
	}
}
