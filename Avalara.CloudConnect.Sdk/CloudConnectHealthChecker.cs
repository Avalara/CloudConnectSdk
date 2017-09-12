using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Avalara.CloudConnect
{
	/// <summary>
	/// CloudConnect health checker
	/// </summary>
	public interface ICloudConnectHealthChecker : IDisposable
	{
		/// <summary>
		/// Last exception if health check fails.
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		/// Host name or IP address.
		/// </summary>
		string HostNameOrIPAddress { get; }

		/// <summary>
		/// Gets or sets health check interval (in milliseconds). Default is 1000 (1 second).
		/// </summary>
		int Interval { get; set; }

		/// <summary>
		/// Indicates whether CloudConnect system is healthy (i.e. online and AvaTax is running).
		/// </summary>
		bool IsHealthy { get; }

		/// <summary>
		/// Indicates whether health check is running on its interval.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Starts checking health on defined interval. (Non-blocking)
		/// </summary>
		void Start();

		/// <summary>
		/// Stops checking health.
		/// </summary>
		void Stop();
	}

	/// <summary>
	/// CloudConnect health checker
	/// </summary>
	public class CloudConnectHealthChecker : ICloudConnectHealthChecker
	{
		/// <summary>
		/// Last exception if health check fails.
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		/// Host name or IP address.
		/// </summary>
		public string HostNameOrIPAddress { get; private set; }

		/// <summary>
		/// Gets or sets health check interval (in milliseconds). Default is 1000 (1 second).
		/// </summary>
		public int Interval
		{
			get => _interval;
			set
			{
				_interval = value;
				if (IsRunning)
				{
					_timer.Change(_interval, _interval);
				}
			}
		}

		/// <summary>
		/// Indicates whether CloudConnect is healthy (i.e. online and AvaTax is running).
		/// </summary>
		public bool IsHealthy { get; private set; }

		/// <summary>
		/// Indicates whether health check is running on its interval.
		/// </summary>
		public bool IsRunning { get; private set; }

		private bool _checkingHealth;
		private HttpClient _httpClient = new HttpClient();
		private int _interval;
		private bool _objectDisposed;
		private Timer _timer;

		/// <summary>
		/// Instantiates a new instance of a CloudConnect health checker.
		/// </summary>
		/// <param name="hostNameOrIPAddress"></param>
		public CloudConnectHealthChecker(string hostNameOrIPAddress)
		{
			HostNameOrIPAddress = hostNameOrIPAddress;
			_interval = 1000;
		}

		/// <summary>
		/// Starts checking health on defined interval. (Non-blocking)
		/// </summary>
		public void Start()
		{
			if (_objectDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			if (IsRunning)
			{
				return;
			}

			_timer = new Timer(CheckHealth, null, 0, Interval);

			IsRunning = true;
		}

		/// <summary>
		/// Stops checking health.
		/// </summary>
		public void Stop()
		{
			if (_objectDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			if (!IsRunning)
			{
				return;
			}

			using (WaitHandle waitHandle = new AutoResetEvent(false))
			{
				_timer.Dispose(waitHandle);
				waitHandle.WaitOne();
			}

			_timer = null;
			Exception = null;
			IsHealthy = false;
			IsRunning = false;
		}

		private void CheckHealth(object userState)
		{
			if (_checkingHealth)
			{
				return;
			}
			_checkingHealth = true;

			try
			{
				using (HttpResponseMessage response = _httpClient.GetAsync($"http://{HostNameOrIPAddress}:30009/calc").Result)
				{
					if ((IsHealthy = response.StatusCode == System.Net.HttpStatusCode.OK) != true)
					{
						Exception = new Exception($"HTTP status code {response.StatusCode.ToString()}.");
					}
				}
			}
			catch (Exception ex)
			{
				Exception = ex;
			}

			_checkingHealth = false;
		}

		#region IDisposable

		~CloudConnectHealthChecker()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes of object and its resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of object and its resources.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!_objectDisposed && disposing)
			{
				if (IsRunning)
				{
					Stop();
				}

				if (_httpClient != null)
				{
					try { _httpClient.Dispose(); }
					catch { }
					_httpClient = null;
				}

				if (_timer != null)
				{
					try { _timer.Dispose(); }
					catch { }
					_timer = null;
				}

				_objectDisposed = true;
			}
		}

		#endregion
	}
}
