using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Avalara.AvaTax
{
	[XmlRoot("Security", IsNullable = false, Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	public class Security : SoapHeader
	{
		public UsernameToken UsernameToken { get; set; } = new UsernameToken();
	}
}
