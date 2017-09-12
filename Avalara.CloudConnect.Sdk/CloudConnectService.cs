using Avalara.AvaTax;
using Avalara.AvaTax.AddressSvc;
using Avalara.AvaTax.TaxSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalara.CloudConnect
{
	/// <summary>
	/// CloudConnect service.
	/// </summary>
	public interface ICloudConnectService : IDisposable
	{
		/// <summary>
		/// CloudConnect address service.
		/// </summary>
		IAddressService CloudConnectAddressService { get; }

		/// <summary>
		/// CloudConnect health checker.
		/// </summary>
		ICloudConnectHealthChecker CloudConnectHealthChecker { get; }

		/// <summary>
		/// CloudConnect tax service.
		/// </summary>
		ITaxService CloudConnectTaxService { get; }

		/// <summary>
		/// Indicates whether CloudConnect service is running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Remote address service.
		/// </summary>
		IAddressService RemoteAddressSevice { get; }

		/// <summary>
		/// Remote tax service.
		/// </summary>
		ITaxService RemoteTaxService { get; }

		/// <summary>
		/// Gets or sets CloudConnect service mode. Default is CloudConnectThenRemote.
		/// </summary>
		ServiceMode ServiceMode { get; set; }

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		GetTaxResult GetTax(GetTaxRequest request);

		/// <summary>
		/// Starts CloudConnect service which starts the CloudConnect health checker. (Non-blocking)
		/// </summary>
		void Start();

		/// <summary>
		/// Stops CloudConnect service which stops the CloudConnect health checker.
		/// </summary>
		void Stop();

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		ValidateResult Validate(ValidateRequest request);
	}

	/// <summary>
	/// CloudConnect service.
	/// </summary>
	public class CloudConnectService : ICloudConnectService
	{
		/// <summary>
		/// CloudConnect address service.
		/// </summary>
		public IAddressService CloudConnectAddressService { get; private set; }

		/// <summary>
		/// CloudConnect health checker.
		/// </summary>
		public ICloudConnectHealthChecker CloudConnectHealthChecker { get; private set; }

		/// <summary>
		/// CloudConnect tax service.
		/// </summary>
		public ITaxService CloudConnectTaxService { get; private set; }

		/// <summary>
		/// Indicates whether CloudConnect service is running.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Remote address service.
		/// </summary>
		public IAddressService RemoteAddressSevice { get; private set; }

		/// <summary>
		/// Remote tax service.
		/// </summary>
		public ITaxService RemoteTaxService { get; private set; }

		/// <summary>
		/// Gets or sets CloudConnect service mode. Default is CloudConnectThenRemote.
		/// </summary>
		public ServiceMode ServiceMode { get; set; } = ServiceMode.CloudConnectThenRemote;

		private bool _objectDisposed;

		/// <summary>
		/// Instantiates a new instance of CloudConnect service.
		/// </summary>
		/// <param name="clientContext"></param>
		/// <param name="cloudConnectHostNameOrIPAddress"></param>
		/// <param name="remoteHostNameOrIPAddress"></param>
		/// <param name="cloudConnectSsl"></param>
		public CloudConnectService(ClientContext clientContext, string cloudConnectHostNameOrIPAddress, string remoteHostNameOrIPAddress, bool cloudConnectSsl)
		{
			CloudConnectHealthChecker = new CloudConnectHealthChecker(cloudConnectHostNameOrIPAddress);
			CloudConnectAddressService = new AddressService(clientContext, $"http{(cloudConnectSsl ? "s" : "")}://{cloudConnectHostNameOrIPAddress}:808{(cloudConnectSsl ? "4" : "0")}{AddressService.DefaultUri.AbsolutePath}");
			CloudConnectTaxService = new TaxService(clientContext, $"http{(cloudConnectSsl ? "s" : "")}://{cloudConnectHostNameOrIPAddress}:808{(cloudConnectSsl ? "4" : "0")}{TaxService.DefaultUri.AbsolutePath}");
			RemoteAddressSevice = new AddressService(clientContext, $"https://{remoteHostNameOrIPAddress}{AddressService.DefaultUri.AbsolutePath}");
			RemoteTaxService = new TaxService(clientContext, $"https://{remoteHostNameOrIPAddress}{TaxService.DefaultUri.AbsolutePath}");
		}

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public GetTaxResult GetTax(GetTaxRequest request)
		{
			GetTaxResult result = null;

			if (ServiceMode <= ServiceMode.CloudConnectThenRemote && CloudConnectHealthChecker.IsHealthy)
			{
				try
				{
					result = CloudConnectTaxService.GetTax(request);
				}
				catch (Exception) when (ServiceMode == ServiceMode.CloudConnectThenRemote) { }

				if (IsGetTaxResultCodeOk(result) || ServiceMode == ServiceMode.CloudConnectOnly)
				{
					return result;
				}
			}
			else if (ServiceMode == ServiceMode.CloudConnectOnly)
			{
				throw new Exception("Using only CloudConnect but it is not healthy.");
			}

			try
			{
				result = RemoteTaxService.GetTax(request);
			}
			catch (Exception) when (ServiceMode == ServiceMode.RemoteThenCloudConnect) { }

			if (IsGetTaxResultCodeOk(result) || ServiceMode != ServiceMode.RemoteThenCloudConnect || !CloudConnectHealthChecker.IsHealthy)
			{
				return result;
			}


			return CloudConnectTaxService.GetTax(request);
		}

		/// <summary>
		/// Starts CloudConnect service which starts the CloudConnect health checker. (Non-blocking)
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
			IsRunning = true;

			CloudConnectHealthChecker.Start();
		}

		/// <summary>
		/// Stops CloudConnect service which stops the CloudConnect health checker.
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

			CloudConnectHealthChecker.Stop();

			IsRunning = true;
		}

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public ValidateResult Validate(ValidateRequest request)
		{
			ValidateResult result = null;

			if (ServiceMode <= ServiceMode.CloudConnectThenRemote && CloudConnectHealthChecker.IsHealthy)
			{
				try
				{
					result = CloudConnectAddressService.Validate(request);
				}
				catch (Exception) when (ServiceMode == ServiceMode.CloudConnectThenRemote) { }

				if (IsValidateResultCodeOk(result) || ServiceMode == ServiceMode.CloudConnectOnly)
				{
					return result;
				}
			}
			else if (ServiceMode == ServiceMode.CloudConnectOnly)
			{
				throw new Exception("Using only CloudConnect but it is not healthy.");
			}

			try
			{
				result = RemoteAddressSevice.Validate(request);
			}
			catch (Exception) when (ServiceMode == ServiceMode.RemoteThenCloudConnect) { }

			if (IsValidateResultCodeOk(result) || ServiceMode != ServiceMode.RemoteThenCloudConnect || !CloudConnectHealthChecker.IsHealthy)
			{
				return result;
			}

			return CloudConnectAddressService.Validate(request);
		}

		private static bool IsValidateResultCodeOk(ValidateResult result) => result != null && (result.ResultCode == AvaTax.AddressSvc.SeverityLevel.Success || result.ResultCode == AvaTax.AddressSvc.SeverityLevel.Warning);

		private static bool IsGetTaxResultCodeOk(GetTaxResult result) => result != null && (result.ResultCode == AvaTax.TaxSvc.SeverityLevel.Success || result.ResultCode == AvaTax.TaxSvc.SeverityLevel.Warning);

		#region IDisposable

		~CloudConnectService()
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
				if (CloudConnectHealthChecker != null)
				{
					try { CloudConnectHealthChecker.Dispose(); }
					catch { }
					CloudConnectHealthChecker = null;
				}

				_objectDisposed = true;
			}
		}

		#endregion
	}
}
