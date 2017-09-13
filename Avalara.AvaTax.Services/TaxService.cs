using Avalara.AvaTax.TaxSvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Avalara.AvaTax
{
	public interface ITaxService : IBaseService, IDisposable
	{
		GetTaxResult GetTax(GetTaxRequest GetTaxRequest);
	}

	[WebServiceBinding(Name = "TaxSvcSoap", Namespace = "http://avatax.avalara.com/services")]
	[DesignerCategory("code")]
	[XmlInclude(typeof(BaseResult))]
	[DebuggerStepThrough]
	public class TaxService : BaseService, ITaxService
	{
		public static Uri DefaultUri = new Uri("https://avatax.avalara.net/Tax/TaxSvc.asmx");

		public TaxService(ClientContext clientContext, string url)
			: base(clientContext, url)
		{
		}

		[SoapDocumentMethod("http://avatax.avalara.com/services/GetTax", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://avatax.avalara.com/services", ResponseNamespace = "http://avatax.avalara.com/services", Use = SoapBindingUse.Literal)]
		[SoapHeader("Security")]
		[SoapHeader("Profile")]
		public GetTaxResult GetTax(GetTaxRequest GetTaxRequest)
		{
			if (GetTaxRequest == null)
			{
				throw new ArgumentNullException(nameof(GetTaxRequest));
			}

			return this.Invoke("GetTax", new object[] { GetTaxRequest })[0] as GetTaxResult;
		}

		public static GetTaxRequest CreateGetTaxRequest(BaseAddress originAddress, BaseAddress destinationAddress, IEnumerable<Line> lines)
		{
			if (originAddress == null)
			{
				throw new ArgumentNullException(nameof(originAddress));
			}
			else if (destinationAddress == null)
			{
				throw new ArgumentNullException(nameof(destinationAddress));
			}
			else if (lines == null)
			{
				throw new ArgumentNullException(nameof(lines));
			}

			originAddress.AddressCode = originAddress.GetHashCode().ToString();
			destinationAddress.AddressCode = destinationAddress.GetHashCode().ToString();

			GetTaxRequest getTaxRequest = new GetTaxRequest()
			{
				Addresses = new BaseAddress[] { originAddress, destinationAddress },
				DestinationCode = destinationAddress.AddressCode,
				DocType = DocumentType.SalesOrder,
				Lines = lines.ToArray(),
				OriginCode = originAddress.AddressCode,
			};

			int lineNo = 0;
			foreach (Line line in getTaxRequest.Lines)
			{
				line.DestinationCode = getTaxRequest.DestinationCode;
				line.No = (++lineNo).ToString();
				line.OriginCode = getTaxRequest.OriginCode;
			}

			return getTaxRequest;
		}
	}
}
