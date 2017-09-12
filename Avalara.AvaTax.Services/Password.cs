using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Avalara.AvaTax
{
	public class Password
	{
		[XmlAttribute]
		public string Type { get; set; }
		[XmlText]
		public string Value { get; set; }
	}
}
