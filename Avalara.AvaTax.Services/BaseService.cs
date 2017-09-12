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
	public interface IBaseService : IDisposable
	{
		ClientContext ClientContext { get; set; }
		Profile Profile { get; }
		Security Security { get; }
		string Url { get; set; }
	}

	[DesignerCategory("code")]
	public abstract class BaseService : SoapHttpClientProtocol, IBaseService
	{
		public ClientContext ClientContext
		{
			get => _clientContext;
			set
			{
				if (_clientContext != value)
				{
					_clientContext = value;

					if (_clientContext != null)
					{
						_clientContext.PropertyChanged += (s, e) => OnClientContextChanged();
					}

					OnClientContextChanged();
				}
			}
		}
		public Profile Profile { get; set; } = new Profile();
		public Security Security { get; set; } = new Security();

		private ClientContext _clientContext = new ClientContext();

		public BaseService(ClientContext clientContext, string url)
		{
			ClientContext = clientContext;
			Url = url;
		}

		private void OnClientContextChanged()
		{
			if (_clientContext != null)
			{
				Profile.Client = _clientContext.ClientName;
				Security.UsernameToken.Username = _clientContext.Account ?? _clientContext.Username;
				Security.UsernameToken.Password.Value = _clientContext.License ?? _clientContext.Password;
			}
			else
			{
				Profile.Client = null;
				Security.UsernameToken.Username = null;
				Security.UsernameToken.Password.Value = null;
			}
		}
	}
}
