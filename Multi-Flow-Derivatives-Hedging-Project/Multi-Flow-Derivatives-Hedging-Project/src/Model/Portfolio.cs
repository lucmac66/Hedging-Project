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

namespace Multi_Flow_Derivatives_Hedging_Project.src.Model
{
	public class Portfolio
	{
		// Portfolio Composition

		Dictionary<string, double> assetQuantities;
		double freeRiskQuantity;

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
		
		// Change of asset quantities after deltas' calculations
		public void changeQuantities(double[] deltas, DateTime t)
		{
			foreach (var asset in assetQuantities)
				{
					assetQuantities[asset.Key] = deltas[testParameters.PricingParams.UnderlyingPositions[asset.Key]];
					Console.WriteLine("Key = {0}, Value = {1}", asset.Key, asset.Value);
				}
			currentTime = t;
		}

		// Change of free risk quantities after deltas' calculations
		public void changeRiskFree(double price, double[] spots)
		{ 
			freeRiskQuantity = price;
			foreach (var key in assetQuantities.Keys)
			{
				freeRiskQuantity -= spots[testParameters.PricingParams.UnderlyingPositions[key]]*assetQuantities[key];
			}
			double deltaTime = mathDateConverterconverter.ConvertToMathDistance(currentTime, maturity);
			freeRiskQuantity *= Math.Exp(rateFreeRisk * deltaTime);
			Console.WriteLine(freeRiskQuantity.ToString());
		}

		// Tool
		public double MathCurrentTime(DateTime t)
		{
			return mathDateConverterconverter.ConvertToMathDistance(startTime, t);
		}

		public double getPortfolioPrice(PastValues past)
		{
			double price = 0;
			foreach(var key in assetQuantities.Keys)
			{
				price += assetQuantities[key] * past.getPast()[currentTime][key];
			}
			double deltaTime = mathDateConverterconverter.ConvertToMathDistance(currentTime, maturity);
			return price + freeRiskQuantity / Math.Exp(rateFreeRisk * deltaTime);
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
