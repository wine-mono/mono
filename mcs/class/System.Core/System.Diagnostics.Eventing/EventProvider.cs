using System;
using System.Security;

namespace System.Diagnostics.Eventing
{
	public class EventProvider : IDisposable
	{
		public EventProvider (Guid providerGuid) { }

		public virtual void Close () { }

		[SecurityCritical]
		public static Guid CreateActivityId ()
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		~EventProvider ()
		{
			Dispose (false);
		}

		public enum WriteEventErrorCode
		{
			NoError,
			NoFreeBuffers,
			EventTooBig
		}

		public static WriteEventErrorCode GetLastWriteEventError ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEnabled (byte level, long keywords)
		{
			return false;
		}

		public bool IsEnabled ()
		{
			return false;
		}

		public static void SetActivityId (ref Guid id) { }

		public bool WriteEvent (ref EventDescriptor eventDescriptor, params object[] eventPayload)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public bool WriteEvent (ref EventDescriptor eventDescriptor, string data)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public bool WriteEvent (ref EventDescriptor eventDescriptor, int dataCount, IntPtr data)
		{
			throw new NotImplementedException ();
		}

		public bool WriteMessageEvent (string eventMessage)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public bool WriteMessageEvent (string eventMessage, byte eventLevel, long eventKeywords)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public bool WriteTransferEvent (ref EventDescriptor eventDescriptor, Guid relatedActivityId, params object[] eventPayload)
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public bool WriteTransferEvent (ref EventDescriptor eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
		{
			throw new NotImplementedException ();
		}
	}
}
