using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalara.CloudConnect
{
	/// <summary>
	/// CloudConnect service mode.
	/// </summary>
	public enum ServiceMode
	{
		/// <summary>
		/// Use CloudConnect only.
		/// </summary>
		CloudConnectOnly,

		/// <summary>
		/// Use CloudConnect but if failures occur retry on remote.
		/// </summary>
		CloudConnectThenRemote,

		/// <summary>
		/// Use remote only.
		/// </summary>
		RemoteOnly,

		/// <summary>
		/// Use remote but if failures occur retry on CloudConnect.
		/// </summary>
		RemoteThenCloudConnect,
	}
}
