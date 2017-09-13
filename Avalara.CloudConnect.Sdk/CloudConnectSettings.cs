using Avalara.AvaTax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalara.CloudConnect
{
	/// <summary>
	/// CloudConnect settings.
	/// </summary>
	public class CloudConnectSettings
	{
		/// <summary>
		/// Gets or sets AvaTax client context.
		/// </summary>
		public ClientContext AvaTaxClientContext { get; set; } = new ClientContext();

		/// <summary>
		/// Gets CloudConnect base URI.
		/// </summary>
		public string CloudConnectBaseUri => $"http{(CloudConnectSsl ? "s" : "")}://{CloudConnectHostNameOrIPAddress}:{(CloudConnectSsl ? CloudConnectPortOnSsl : CloudConnectPortOnNonSsl)}";

		/// <summary>
		/// Gets or sets CloudConnect host name or IP address.
		/// </summary>
		public string CloudConnectHostNameOrIPAddress { get; set; }

		/// <summary>
		/// CloudConnect port when <see cref="CloudConnectSsl"/> is false. Default is 8080.
		/// </summary>
		public int CloudConnectPortOnNonSsl { get; set; } = 8080;

		/// <summary>
		/// CloudConnect port when <see cref="CloudConnectSsl"/> is true. Default is 8084.
		/// </summary>
		public int CloudConnectPortOnSsl { get; set; } = 8084;

		/// <summary>
		/// Gets or sets whether to use SSL with CloudConnect. Default is false.
		/// </summary>
		public bool CloudConnectSsl { get; set; }

		/// <summary>
		/// Gets remote base URI. Default is https://avatax.avalara.net.
		/// </summary>
		public string RemoteBaseUri => $"https://{RemoteHostNameOrIPAddress}";

		/// <summary>
		/// Gets or sets remote host name or IP address. Default is avatax.avalara.net.
		/// </summary>
		public string RemoteHostNameOrIPAddress { get; set; } = TaxService.DefaultUri.Host;

		/// <summary>
		/// Gets or sets CloudConnect service mode. Default is CloudConnectThenRemote.
		/// </summary>
		public ServiceMode ServiceMode { get; set; } = ServiceMode.CloudConnectThenRemote;
	}
}
