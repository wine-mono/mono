//
// Copyright (C) 2024 Esme Povirk for CodeWeavers
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;

namespace System.Deployment.Application {

	public sealed class ApplicationDeployment
	{
		private ApplicationDeployment ()
		{
		}

		public Uri ActivationUri
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public static ApplicationDeployment CurrentDeployment
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public Version CurrentVersion
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public string DataDirectory
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public bool IsFirstRun
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public static bool IsNetworkDeployed
		{
			get
			{
				return false;
			}
		}

		public DateTime TimeOfLastUpdateCheck
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public string UpdatedApplicationFullName
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public Version UpdatedVersion
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public Uri UpdateLocation
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public UpdateCheckInfo CheckForDetailedUpdate ()
		{
			return CheckForDetailedUpdate (true);
		}

		public UpdateCheckInfo CheckForDetailedUpdate (bool persistUpdateCheckResult)
		{
			throw new NotImplementedException ();
		}

		public bool CheckForUpdate ()
		{
			return CheckForUpdate (true);
		}

		public bool CheckForUpdate (bool persistUpdateCheckResult)
		{
			throw new NotImplementedException ();
		}

		public void CheckForUpdateAsync ()
		{
			throw new NotImplementedException ();
		}

		public void CheckForUpdateAsyncCancel ()
		{
			throw new NotImplementedException ();
		}

		public void DownloadFileGroup (string groupName)
		{
			throw new NotImplementedException ();
		}

		public void DownloadFileGroupAsync (string groupName)
		{
			throw new NotImplementedException ();
		}

		public void DownloadFileGroupAsync (string groupName, object userState)
		{
			throw new NotImplementedException ();
		}

		public void DownloadFileGroupAsyncCancel (string groupName)
		{
			throw new NotImplementedException ();
		}

		public bool IsFileGroupDownloaded (string groupName)
		{
			throw new NotImplementedException ();
		}

		public bool Update ()
		{
			throw new NotImplementedException ();
		}

		public void UpdateAsync ()
		{
			throw new NotImplementedException ();
		}

		public void UpdateAsyncCancel ()
		{
			throw new NotImplementedException ();
		}

		public event CheckForUpdateCompletedEventHandler CheckForUpdateCompleted;
		public event DeploymentProgressChangedEventHandler CheckForUpdateProgressChanged;
		public event DownloadFileGroupCompletedEventHandler DownloadFileGroupCompleted;
		public event DeploymentProgressChangedEventHandler DownloadFileGroupProgressChanged;
		public event AsyncCompletedEventHandler UpdateCompleted;
		public event DeploymentProgressChangedEventHandler UpdateProgressChanged;
	}
}
