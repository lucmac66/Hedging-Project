using Multi_Flow_Derivatives_Hedging_Project;
using System;
using MarketData;
using ParameterInfo;


namespace Multi_Flow_Derivatives_Hedging_Project
{
	public class HedgerEngine
	{
		private Portfolio portfolio;
		private Parser parser;

		public HedgerEngine(Portfolio portfolio, Parser parser)
		{
			this.portfolio = portfolio;
			this.parser = parser;
		}
	}
}
