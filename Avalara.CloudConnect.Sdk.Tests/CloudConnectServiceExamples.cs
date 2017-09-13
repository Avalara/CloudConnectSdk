using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalara.AvaTax;
using Avalara.AvaTax.TaxSvc;
using System.Threading;
using Avalara.AvaTax.AddressSvc;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Avalara.CloudConnect.Sdk.Tests
{
	[TestClass]
	public class CloudConnectServiceExamples
	{
		private static ICloudConnectService _cloudConnectService;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext)
		{
			CloudConnectSettings settings = new CloudConnectSettings()
			{
				AvaTaxClientContext = new ClientContext()
				{
					Username = "(YOUR USERNAME)",
					Password = "(YOUR PASSWORD)",
				},
				CloudConnectHostNameOrIPAddress = "(CLOUDCONNECT HOST NAME OR IP ADDRESS)",
			};

			_cloudConnectService = new CloudConnectService(settings);

			_cloudConnectService.StartHealthChecker();
		}

		[TestMethod]
		public void GetTax()
		{
			// arrange
			AvaTax.TaxSvc.BaseAddress originAddress = new AvaTax.TaxSvc.BaseAddress()
			{
				Line1 = "4 Yawkey Way",
				City = "Boston",
				Region = "MA",
				PostalCode = "02215",
			};

			AvaTax.TaxSvc.BaseAddress destinationAddress = new AvaTax.TaxSvc.BaseAddress()
			{
				Line1 = "1250 1st Ave S",
				City = "Seattle",
				Region = "WA",
				PostalCode = "98134",
			};

			Random random = new Random(DateTime.Now.Millisecond);

			GetTaxRequest request = TaxService.CreateGetTaxRequest(originAddress, destinationAddress, new Line[]
			{
				new Line() { Amount = random.Next(1,100), Qty = random.Next(1,5), },
			});
			request.CustomerCode = "1";
			request.DocDate = DateTime.Now;

			// act
			GetTaxResult result = _cloudConnectService.GetTax(request);

			// assert
		}

		[TestMethod]
		public void ManyGetTaxConcurrently()
		{
			// arrange
			AvaTax.TaxSvc.BaseAddress originAddress = new AvaTax.TaxSvc.BaseAddress()
			{
				Line1 = "4 Yawkey Way",
				City = "Boston",
				Region = "MA",
				PostalCode = "02215",
			};

			AvaTax.TaxSvc.BaseAddress destinationAddress = new AvaTax.TaxSvc.BaseAddress()
			{
				Line1 = "1250 1st Ave S",
				City = "Seattle",
				Region = "WA",
				PostalCode = "98134",
			};

			Random random = new Random(DateTime.Now.Millisecond);

			IEnumerable<GetTaxRequest> requests = Enumerable.Range(1, 200).Select((i) =>
			{
				GetTaxRequest request = TaxService.CreateGetTaxRequest(originAddress, destinationAddress, new Line[]
				{
					new Line() { Amount = random.Next(1,100), Qty = random.Next(1,5), },
				});
				request.CustomerCode = "1";
				request.DocDate = DateTime.Now;
				return request;
			}).ToArray();

			Stopwatch stopwatch = new Stopwatch();

			// act
			stopwatch.Start();
			IEnumerable<GetTaxResult> results = _cloudConnectService.GetTax(requests, CancellationToken.None, 0);
			stopwatch.Stop();

			Debug.WriteLine($"Requests: {requests.Count()}, Results: {results.Count()}, Success: {results.Count(r => r.ResultCode == AvaTax.TaxSvc.SeverityLevel.Success)}, Duration: {stopwatch.Elapsed}");

			// assert
		}

		[TestMethod]
		public void Validate()
		{
			// arrange
			ValidateRequest request = new ValidateRequest()
			{
				Address = new AvaTax.AddressSvc.BaseAddress()
				{
					Line1 = "1250 1st Ave S",
					City = "Seattle",
					Region = "WA",
					PostalCode = "98134",
				},
			};

			// act
			ValidateResult result = _cloudConnectService.Validate(request);

			// assert
		}
	}
}
