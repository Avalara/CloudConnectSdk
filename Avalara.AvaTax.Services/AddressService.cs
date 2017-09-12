using Avalara.AvaTax.AddressSvc;
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
	public interface IAddressService : IBaseService
	{
		ValidateResult Validate(ValidateRequest ValidateRequest);
	}

	[WebServiceBinding(Name = "AddressSvcSoap", Namespace = "http://avatax.avalara.com/services")]
	[DesignerCategory("code")]
	[XmlInclude(typeof(BaseResult))]
	[DebuggerStepThrough]
	public class AddressService : BaseService, IAddressService
	{
		public static Uri DefaultUri = new Uri("https://avatax.avalara.net/Address/AddressSvc.asmx");

		public AddressService(ClientContext clientContext, string url)
			: base(clientContext, url)
		{
		}

		[SoapDocumentMethod("http://avatax.avalara.com/services/Validate", ParameterStyle = SoapParameterStyle.Wrapped, RequestNamespace = "http://avatax.avalara.com/services", ResponseNamespace = "http://avatax.avalara.com/services", Use = SoapBindingUse.Literal)]
		[SoapHeader("Security")]
		[SoapHeader("Profile")]
		public ValidateResult Validate(ValidateRequest ValidateRequest)
		{
			if (ValidateRequest == null)
			{
				throw new ArgumentNullException(nameof(ValidateRequest));
			}

			return this.Invoke("Validate", new object[] { ValidateRequest })[0] as ValidateResult;
		}
	}
}
