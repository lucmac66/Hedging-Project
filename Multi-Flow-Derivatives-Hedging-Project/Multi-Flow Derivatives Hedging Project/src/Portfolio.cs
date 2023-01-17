using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Flow_Derivatives_Hedging_Project
{


	public class Portfolio
	{
		private double[] assetQuantities { get; }

		private double freeRiskQuantity {get;}

		public Portfolio(double[] assetQuantities, double freeRiskqQuantity)
		{
			this.assetQuantities = assetQuantities;
			this.freeRiskQuantity= freeRiskqQuantity;
		}
	}
}
