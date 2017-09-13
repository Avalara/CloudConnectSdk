using Avalara.AvaTax;
using Avalara.AvaTax.AddressSvc;
using Avalara.AvaTax.TaxSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
		/// Gets CloudConnect service settings.
		/// </summary>
		CloudConnectSettings Settings { get; }

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		GetTaxResult GetTax(GetTaxRequest request);

		/// <summary>
		/// Get tax concurrently.
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="concurrentRequests">Limits the number of concurrent requests. If it is 0 or less, there is no limit on the number of concurrently running requests.</param>
		/// <returns></returns>
		IEnumerable<GetTaxResult> GetTax(IEnumerable<GetTaxRequest> requests, CancellationToken cancellationToken, int concurrentRequests);

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<GetTaxResult> GetTaxAsync(GetTaxRequest request, CancellationToken cancellationToken);

		/// <summary>
		/// Starts CloudConnect health checker. (Non-blocking)
		/// </summary>
		void StartHealthChecker();

		/// <summary>
		/// Stops CloudConnect health checker.
		/// </summary>
		void StopHealthChecker();

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		ValidateResult Validate(ValidateRequest request);

		/// <summary>
		/// Validate concurrently.
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="concurrentRequests">Limits the number of concurrent requests. If it is 0 or less, there is no limit on the number of concurrently running requests.</param>
		/// <returns></returns>
		IEnumerable<ValidateResult> Validate(IEnumerable<ValidateRequest> requests, CancellationToken cancellationToken, int concurrentRequests);

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<ValidateResult> ValidateAsync(ValidateRequest request, CancellationToken cancellationToken);
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
		/// Gets CloudConnect service settings.
		/// </summary>
		public CloudConnectSettings Settings { get; private set; }

		private bool _objectDisposed;

		/// <summary>
		/// Instantiates a new instance of CloudConnect service.
		/// </summary>
		/// <param name="settings"></param>
		public CloudConnectService(CloudConnectSettings settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));

			CreateServices();
		}

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public GetTaxResult GetTax(GetTaxRequest request)
		{
			GetTaxResult result = null;

			if (Settings.ServiceMode <= ServiceMode.CloudConnectThenRemote && (!IsRunning || CloudConnectHealthChecker.IsHealthy))
			{
				try
				{
					result = CloudConnectTaxService.GetTax(request);
				}
				catch (Exception) when (Settings.ServiceMode == ServiceMode.CloudConnectThenRemote) { }

				if (IsGetTaxResultCodeOk(result) || Settings.ServiceMode == ServiceMode.CloudConnectOnly)
				{
					return result;
				}
			}
			else if (Settings.ServiceMode == ServiceMode.CloudConnectOnly)
			{
				throw new Exception("Using only CloudConnect but it is not healthy or health checker is not running.");
			}

			try
			{
				result = RemoteTaxService.GetTax(request);
			}
			catch (Exception) when (Settings.ServiceMode == ServiceMode.RemoteThenCloudConnect) { }

			if (IsGetTaxResultCodeOk(result) || Settings.ServiceMode != ServiceMode.RemoteThenCloudConnect || (IsRunning && !CloudConnectHealthChecker.IsHealthy))
			{
				return result;
			}


			return CloudConnectTaxService.GetTax(request);
		}

		/// <summary>
		/// Get tax concurrently.
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="concurrentRequests">Limits the number of concurrent requests. If it is 0 or less, there is no limit on the number of concurrently running requests.</param>
		/// <returns></returns>
		public IEnumerable<GetTaxResult> GetTax(IEnumerable<GetTaxRequest> requests, CancellationToken cancellationToken, int concurrentRequests)
		{
			List<GetTaxResult> results = new List<GetTaxResult>();

			ParallelOptions parallelOptions = new ParallelOptions()
			{
				CancellationToken = cancellationToken,
				MaxDegreeOfParallelism = concurrentRequests < 1 ? -1 : concurrentRequests,
			};

			try
			{
				Parallel.ForEach(requests, parallelOptions, (r) =>
				{
					results.Add(GetTax(r));
				});
			}
			catch (OperationCanceledException) { }

			return results;
		}

		/// <summary>
		/// Gets tax.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetTaxResult> GetTaxAsync(GetTaxRequest request, CancellationToken cancellationToken)
		{
			return Task.Run(() => GetTax(request), cancellationToken);
		}

		/// <summary>
		/// Starts CloudConnect health checker.
		/// </summary>
		public void StartHealthChecker()
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
		/// Stops CloudConnect health checker.
		/// </summary>
		public void StopHealthChecker()
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
		/// Validate concurrently.
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="concurrentRequests">Limits the number of concurrent requests. If it is 0 or less, there is no limit on the number of concurrently running requests.</param>
		/// <returns></returns>
		public IEnumerable<ValidateResult> Validate(IEnumerable<ValidateRequest> requests, CancellationToken cancellationToken, int concurrentRequests)
		{
			List<ValidateResult> results = new List<ValidateResult>();

			ParallelOptions parallelOptions = new ParallelOptions()
			{
				CancellationToken = cancellationToken,
				MaxDegreeOfParallelism = concurrentRequests < 1 ? -1 : concurrentRequests,
			};

			try
			{
				Parallel.ForEach(requests, parallelOptions, (r) =>
				{
					results.Add(Validate(r));
				});
			}
			catch (OperationCanceledException) { }

			return results;
		}

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public ValidateResult Validate(ValidateRequest request)
		{
			ValidateResult result = null;

			if (Settings.ServiceMode <= ServiceMode.CloudConnectThenRemote && (!IsRunning || CloudConnectHealthChecker.IsHealthy))
			{
				try
				{
					result = CloudConnectAddressService.Validate(request);
				}
				catch (Exception) when (Settings.ServiceMode == ServiceMode.CloudConnectThenRemote) { }

				if (IsValidateResultCodeOk(result) || Settings.ServiceMode == ServiceMode.CloudConnectOnly)
				{
					return result;
				}
			}
			else if (Settings.ServiceMode == ServiceMode.CloudConnectOnly)
			{
				throw new Exception("Using only CloudConnect but it is not healthy or health checker is not running.");
			}

			try
			{
				result = RemoteAddressSevice.Validate(request);
			}
			catch (Exception) when (Settings.ServiceMode == ServiceMode.RemoteThenCloudConnect) { }

			if (IsValidateResultCodeOk(result) || Settings.ServiceMode != ServiceMode.RemoteThenCloudConnect || (IsRunning && !CloudConnectHealthChecker.IsHealthy))
			{
				return result;
			}

			return CloudConnectAddressService.Validate(request);
		}

		/// <summary>
		/// Validates address.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<ValidateResult> ValidateAsync(ValidateRequest request, CancellationToken cancellationToken)
		{
			return Task.Run(() => Validate(request), cancellationToken);
		}

		private static bool IsGetTaxResultCodeOk(GetTaxResult result) => result != null && (result.ResultCode == AvaTax.TaxSvc.SeverityLevel.Success || result.ResultCode == AvaTax.TaxSvc.SeverityLevel.Warning);

		private static bool IsValidateResultCodeOk(ValidateResult result) => result != null && (result.ResultCode == AvaTax.AddressSvc.SeverityLevel.Success || result.ResultCode == AvaTax.AddressSvc.SeverityLevel.Warning);

		private void CreateServices()
		{
			CloudConnectHealthChecker = new CloudConnectHealthChecker(Settings.CloudConnectHostNameOrIPAddress);
			CloudConnectAddressService = new AddressService(Settings.AvaTaxClientContext, $"{Settings.CloudConnectBaseUri}{AddressService.DefaultUri.AbsolutePath}");
			CloudConnectTaxService = new TaxService(Settings.AvaTaxClientContext, $"{Settings.CloudConnectBaseUri}{TaxService.DefaultUri.AbsolutePath}");
			RemoteAddressSevice = new AddressService(Settings.AvaTaxClientContext, $"{Settings.RemoteBaseUri}{AddressService.DefaultUri.AbsolutePath}");
			RemoteTaxService = new TaxService(Settings.AvaTaxClientContext, $"{Settings.RemoteBaseUri}{TaxService.DefaultUri.AbsolutePath}");
		}

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
				DisposeServices();

				_objectDisposed = true;
			}
		}

		private void DisposeServices()
		{
			if (CloudConnectHealthChecker != null)
			{
				try { CloudConnectHealthChecker.Dispose(); }
				catch { }
				CloudConnectHealthChecker = null;
			}

			if (CloudConnectAddressService != null)
			{
				try { CloudConnectAddressService.Dispose(); }
				catch { }
				CloudConnectAddressService = null;
			}

			if (CloudConnectTaxService != null)
			{
				try { CloudConnectTaxService.Dispose(); }
				catch { }
				CloudConnectTaxService = null;
			}

			if (RemoteAddressSevice != null)
			{
				try { RemoteAddressSevice.Dispose(); }
				catch { }
				RemoteAddressSevice = null;
			}

			if (RemoteTaxService != null)
			{
				try { RemoteTaxService.Dispose(); }
				catch { }
				RemoteTaxService = null;
			}
		}

		#endregion
	}
}
