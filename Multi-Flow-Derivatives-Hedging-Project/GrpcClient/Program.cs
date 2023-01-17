using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcPricing.Protos;

var httpHandler = new HttpClientHandler();
// Return `true` to allow certificates that are untrusted/invalid
httpHandler.ServerCertificateCustomValidationCallback =
	HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
using var channel = GrpcChannel.ForAddress("http://localhost:5079",
	new GrpcChannelOptions { HttpHandler = httpHandler });
var client = new GrpcPricer.GrpcPricerClient(channel);
