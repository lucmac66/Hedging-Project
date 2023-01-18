using System.Threading.Tasks;
using CsvHelper;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using GrpcPricing.Protos;
using ModelConverter;
namespace Grpc
{
	public class GrpcClient
	{
		public GrpcPricer.GrpcPricerClient client { get; set; }
		public MathParameters parameters { get; set; }

		public double[] deltas;
		public double[] stdDeltas;
		public double price;
		public double stdPrice;

		public void Connexion()
		{
			var httpHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using var channel = GrpcChannel.ForAddress("http://localhost:50051",
				new GrpcChannelOptions { HttpHandler = httpHandler });
			this.client = new GrpcPricer.GrpcPricerClient(channel);

		}

		public GrpcClient(MathParameters parameters)
		{
			price = 0;
			stdPrice = 0;
			deltas = new double[parameters.MathPaymentDates.Length];
			stdDeltas = new double[parameters.MathPaymentDates.Length];
			// Je sais si on a vraiment besoin de parameters
			this.parameters = parameters;
			Connexion();
		}

		public void Request(RepeatedField<PastLines> past, bool isMonitoringDate, double t)
		{
			// Initialize Pricing Input
			PricingInput input = new PricingInput();
			input.Past.Add(past);
			input.MonitoringDateReached = isMonitoringDate;
			input.Time = t;

			// Request to gRPC Server
			var output = client.PriceAndDeltas(input);

			// Set new values
			price = output.Price;
			stdPrice = output.PriceStdDev;
			for (int i = 0; i < deltas.Length; i++)
			{
				deltas[i] = output.Deltas[i];
				stdDeltas[i] = output.DeltasStdDev[i];
			}

		}

		//Getters

		public double getPrice()
		{
			return price;
		}

		public double getStdPrice()
		{
			return stdPrice;
		}

		public double[] getDeltas()
		{
			return deltas;
		}
		public double[] getStdDeltas()
		{
			return stdDeltas;
		}

		static void Main(string[] args)
		{ 
		}
	}
}