using System.ComponentModel;

namespace Avalara.AvaTax
{
	public class ClientContext : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public string Account
		{
			get => _account;
			set
			{
				_account = value;
				OnPropertyChanged(nameof(Account));
			}
		}

		public string ClientName
		{
			get => _clientName;
			set
			{
				_clientName = value;
				OnPropertyChanged(nameof(ClientName));
			}
		}

		public string License
		{
			get => _license;
			set
			{
				_license = value;
				OnPropertyChanged(nameof(License));
			}
		}

		public string Password
		{
			get => _password;
			set
			{
				_password = value;
				OnPropertyChanged(nameof(Password));
			}
		}

		public string Username
		{
			get => _username;
			set
			{
				_username = value;
				OnPropertyChanged(nameof(Username));
			}
		}

		private string _account;
		private string _clientName = "Avalara CloudConnect SDK";
		private string _license;
		private string _password;
		private string _username;

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
