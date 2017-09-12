using System.Web.Services.Protocols;

namespace Avalara.AvaTax
{
	public class UsernameToken : SoapHeader
	{
		public Password Password { get; set; } = new Password();
		public string Username;
	}
}