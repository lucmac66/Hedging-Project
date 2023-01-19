using System.Threading.Tasks;
using CsvHelper;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcPricing.Protos;
using ModelConverter;
namespace Grpc
{
	public class GrpcClient
	{
		// Try to keep the connection
		GrpcPricer.GrpcPricerClient client;
		// Server outputs
		public double[] deltas;
		public double[] stdDeltas;
		public double price;
		public double stdPrice;

		public GrpcPricer.GrpcPricerClient Connexion()
		{
			var httpHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using var channel = GrpcChannel.ForAddress("http://localhost:50051",
				new GrpcChannelOptions { HttpHandler = httpHandler });
			var client = new GrpcPricer.GrpcPricerClient(channel);
			return client;

		}

		public GrpcClient(MathParameters parameters)
		{
			price = 0;
			stdPrice = 0;
			deltas = new double[parameters.MathPaymentDates.Length];
			stdDeltas = new double[parameters.MathPaymentDates.Length];
			var httpHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using var channel = GrpcChannel.ForAddress("http://localhost:50051",
				new GrpcChannelOptions { HttpHandler = httpHandler });
			this.client = new GrpcPricer.GrpcPricerClient(channel);
		}

		public void Request(RepeatedField<PastLines> past, bool isMonitoringDate, double t)
		{
			// Initialize Pricing Input
			PricingInput input = new PricingInput();
			input.Past.Add(past);
			input.MonitoringDateReached = isMonitoringDate;
			input.Time = t;

			// Request to gRPC Server
			var httpHandler = new HttpClientHandler();
			// Return `true` to allow certificates that are untrusted/invalid
			httpHandler.ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			using var channel = GrpcChannel.ForAddress("http://localhost:50051",
				new GrpcChannelOptions { HttpHandler = httpHandler });
			this.client = new GrpcPricer.GrpcPricerClient(channel);
			var output = this.client.PriceAndDeltas(input);
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
		public double GetPrice()
		{
			return price;
		}

		public double GetStdPrice()
		{
			return stdPrice;
		}

		public double[] GetDeltas()
		{
			return deltas;
		}
		public double[] GetStdDeltas()
		{
			return stdDeltas;
		}

		static void Main(string[] args)
		{ 
		}
	}
}