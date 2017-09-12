using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Avalara.AvaTax
{
	[XmlRoot(ElementName = "Profile", IsNullable = false, Namespace = "http://avatax.avalara.com/services")]
	[XmlType(Namespace = "http://avatax.avalara.com/services", TypeName = "Profile")]
	public class Profile : SoapHeader
	{
		public string Name { get; set; }
		public string Client { get; set; }
		public string Adapter { get; set; }
		public string Machine { get; set; }
	}
}
