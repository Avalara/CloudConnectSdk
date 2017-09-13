using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalara.AvaTax;
using Avalara.AvaTax.TaxSvc;
using System.Threading;
using Avalara.AvaTax.AddressSvc;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

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

			GetTaxRequest request = TaxService.CreateGetTaxRequest(originAddress, destinationAddress, new Line[]
			{
				new Line() { Amount = 100, Qty = 1, },
			});
			request.CustomerCode = "1";
			request.DocDate = DateTime.Now;

			// act
			GetTaxResult result = _cloudConnectService.GetTax(request);

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
