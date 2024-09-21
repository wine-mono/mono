//
// CustomSink.cs
//
// Authors:
//         Bernhard Kölbl <bkoelbl@codeweavers.com>
//
// Copyright (C) 2024 Bernhard Kölbl for CodeWeavers (https://www.codeweavers.com)
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
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.IO;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	public abstract class BaseCustomSink : BaseChannelObjectWithProperties, IClientChannelSink, IServerChannelSink, IMessageSink
	{
		private IClientChannelSink	nextClientChannelSink;
		private IServerChannelSink	nextServerChannelSink;
		private IMessageSink nextMessageSink;

		public BaseCustomSink ()
		{
		}

		public void SetNextSink (object nextSink)
		{
			this.nextClientChannelSink = nextSink as IClientChannelSink;
			this.nextServerChannelSink = nextSink as IServerChannelSink;
			this.nextMessageSink = nextSink as IMessageSink;
		}

		//Client
		IClientChannelSink IClientChannelSink.NextChannelSink 
		{ 
			get { return this.nextClientChannelSink; }
		}

		void IClientChannelSink.AsyncProcessRequest (IClientChannelSinkStack sinkStack, 
			IMessage msg, ITransportHeaders headers, Stream stream) 
		{
			object state = null;
			ProcessRequest(msg, headers, ref stream, ref state);

			sinkStack.Push (this, state);
			nextClientChannelSink.AsyncProcessRequest (sinkStack, msg, headers, stream);
		}

		void IClientChannelSink.AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack, object state, 
			ITransportHeaders headers, Stream stream)
		{
			ProcessResponse (null, headers, ref stream, state);
			sinkStack.AsyncProcessResponse (headers, stream);
		}

		Stream IClientChannelSink.GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			return nextClientChannelSink.GetRequestStream (msg, headers);
		}

		void IClientChannelSink.ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, 
			Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			object state = null;
			ProcessRequest (msg, requestHeaders, ref requestStream, ref state);

			nextClientChannelSink.ProcessMessage (msg, requestHeaders, requestStream, 
				out responseHeaders, out responseStream);

			ProcessResponse (null, responseHeaders, ref responseStream, state);
		}

		//Server
		IServerChannelSink IServerChannelSink.NextChannelSink 
		{
			get { return this.nextServerChannelSink; }
		}

		void IServerChannelSink.AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack,
			object state, IMessage msg, ITransportHeaders headers, Stream stream)
		{
			ProcessResponse (msg, headers, ref stream, state);
			sinkStack.AsyncProcessResponse (msg, headers, stream);
		}

		Stream IServerChannelSink.GetResponseStream (IServerResponseChannelSinkStack sinkStack,
			object state, IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		ServerProcessing IServerChannelSink.ProcessMessage (IServerChannelSinkStack sinkStack,
			IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
			out IMessage responseMsg, out ITransportHeaders responseHeaders, 
			out Stream responseStream)
		{
			object state = null;
			ProcessRequest (requestMsg, requestHeaders, ref requestStream, ref state);

			sinkStack.Push (this, state);

			ServerProcessing processing =  nextServerChannelSink.ProcessMessage (sinkStack,
				requestMsg, requestHeaders, requestStream, 
				out responseMsg, out responseHeaders,out responseStream);

			if (processing == ServerProcessing.Complete)
			{
				ProcessResponse (responseMsg, responseHeaders, ref responseStream, state);
			}

			return processing;
		}

		IMessageCtrl IMessageSink.AsyncProcessMessage (IMessage msg, IMessageSink replySink) 
		{
			object state = null;
			Stream dummyStream = null;
			ProcessRequest (msg, null, ref dummyStream, ref state);
			ReplySink myReplySink = new ReplySink (replySink, this, state);

			return nextMessageSink.AsyncProcessMessage (msg, myReplySink);
		}

		IMessage IMessageSink.SyncProcessMessage(IMessage reqMsg) 
		{
			object state = null;
			Stream dummyStream = null;
			ProcessRequest (reqMsg, null, ref dummyStream, ref state);
			IMessage respMsg = nextMessageSink.SyncProcessMessage (reqMsg);
			dummyStream = null;
			ProcessResponse (respMsg, null, ref dummyStream, state);
			return respMsg;
		}

		IMessageSink IMessageSink.NextSink 
		{
			get { return this.nextMessageSink; }
		}

		private class ReplySink : IMessageSink
		{
			IMessageSink nextSink;
			BaseCustomSink parentSink;
			object state;

			public ReplySink (IMessageSink nextSink, BaseCustomSink parentSink, object state)
			{
				this.nextSink = nextSink;
				this.parentSink = parentSink;
				this.state = state;
			}

			IMessageCtrl IMessageSink.AsyncProcessMessage (IMessage msg, IMessageSink replySink) 
			{
				throw new Exception ("ReplySink.AsyncProcessMessage should never be called!");
			}

			IMessage IMessageSink.SyncProcessMessage (IMessage reqMsg) 
			{
				Stream dummyStream = null;
				parentSink.ProcessResponse (reqMsg, null, ref dummyStream, this.state);
				return nextSink.SyncProcessMessage (reqMsg);
			}

			IMessageSink IMessageSink.NextSink 
			{
				get { return this.nextSink; }
			}
		}

		protected virtual void ProcessRequest (IMessage message, ITransportHeaders headers, ref Stream stream, ref object state)
		{
		}

		protected virtual void ProcessResponse (IMessage message, ITransportHeaders headers, ref Stream stream, object state)
		{
		}
	}

	//Custom sink with tests
	internal class SimpleCustomClientSink : BaseCustomSink
	{
		protected override void ProcessRequest (IMessage message, ITransportHeaders headers, ref Stream stream, ref object state)
		{
		}

		protected override void ProcessResponse (IMessage message, ITransportHeaders headers, ref Stream stream, object state)
		{
			// Basic tests
			Assert.AreEqual ("System.Runtime.Remoting.Channels.ChunkedMemoryStream", stream.GetType().ToString(), "Stream Type");

			Assert.AreEqual (true, stream.CanRead, "CanRead");
			Assert.AreEqual (true, stream.CanWrite, "CanWrite");
			Assert.AreEqual (true, stream.CanSeek, "CanSeek");

			Assert.AreEqual (0, stream.Position, "Stream Position");
			Assert.IsTrue (0 < stream.Length, "Length not greater than 0");

			//Read first half, afterwards the remaining part.
			int len = (int)stream.Length, halfLen = (int)stream.Length/2;
			int read;

			byte[] arr = new byte[halfLen];
			read = stream.Read (arr, 0, halfLen);

			Assert.AreEqual (halfLen, read, "Half length read");
			Assert.AreEqual (stream.Position, read, "Halfway stream position");

			int remaining = len-(halfLen);
			arr = new byte[remaining];
			read = stream.Read (arr, 0, remaining);

			Assert.AreEqual (remaining, read, "Remaining length read");
			Assert.AreEqual (stream.Length, stream.Position, "Position at end #1");

			read = stream.Read (arr, 0, 10);
			Assert.AreEqual (0, read, "Reading past stream length");
			Assert.AreEqual (stream.Length, stream.Position, "Position at end #2");

			//Read stream not starting at the beginning
			stream.Position = halfLen;
			read = stream.Read (arr, 0, remaining);

			Assert.AreEqual (remaining, read, "Remaining length read");
			Assert.AreEqual (stream.Length, stream.Position, "Position at end #3");

			//Read less bytes than array length
			stream.Position = 0;

			arr = new byte[5] { 0xff, 0xfe, 0xfd, 0xfc, 0xfb };
			read = stream.Read (arr, 0, 2);

			for (int i = 2; i < 5; ++i)
				Assert.AreEqual ((byte)(0xff - i), arr[i]);

			//Read with offset
			stream.Position = 0;
			arr = new byte[5];

			try
			{
				read = stream.Read (arr, 4, 2);
				Assert.Fail("ArgumentException not thrown");
			} catch (ArgumentException) {}

			Assert.AreEqual(0, stream.Position);

			read = stream.Read (arr, 2, 3);

			for (int i = 0; i < 2; ++i)
				Assert.AreEqual (0x00, arr[i]);
			
			Assert.AreNotEqual (0x00, arr[3]);

			//Read more bytes than array length
			try
			{
				arr = new byte[1];
				stream.Read (arr, 0, len);
				Assert.Fail("ArgumentException not thrown");
			}
			catch (ArgumentException) {}

			//Try read more bytes than the stream is long
			stream.Position = 0;
			arr = new byte[len + 10];

			read = stream.Read (arr, 0, len);

			Assert.AreNotEqual ((byte)'\0', arr[len-1], "Last read byte");
			Assert.AreEqual ((byte)'\0', arr[len], "Byte beyond stream");
			Assert.AreEqual (stream.Length, read, "Bytes read");

			//Change stream length
			try
			{
				stream.SetLength(len + 100);
				Assert.Fail("NotSupportedException not thrown");
			}
			catch (NotSupportedException) {}

			//Reset position for further use by the remoting stack
			stream.Position = 0;
		}
	}

	internal class SimpleCustomServerSink : BaseCustomSink
	{
		protected override void ProcessRequest (IMessage message, ITransportHeaders headers, ref Stream stream, ref object state)
		{
		}

		protected override void ProcessResponse (IMessage message, ITransportHeaders headers, ref Stream stream, object state)
		{
		}
	}

	public abstract class BaseCustomSinkProvider
	{
		protected SinkProviderData data;
		protected Type customSinkType;

		public BaseCustomSinkProvider (IDictionary properties, ICollection providerData)
		{
			customSinkType = Type.GetType ((string)properties["customSinkType"]);
			if (customSinkType == null)
			{
				throw new Exception ($"Could not load type {(string)properties["customSinkType"]}");
			}

			if (!customSinkType.IsSubclassOf (typeof(BaseCustomSink)))
			{
				throw new Exception ("Custom sink type does not inherit from BaseCustomSink");
			}
		}
	}

	public class CustomClientSinkProvider : BaseCustomSinkProvider, IClientChannelSinkProvider
	{
		private IClientChannelSinkProvider nextProvider;

		public CustomClientSinkProvider (IDictionary properties, ICollection providerData) 
			: base (properties, providerData)
		{
		}

		public IClientChannelSinkProvider Next
		{
			get { return this.nextProvider; }
			set { this.nextProvider = value; }
		}

		public IClientChannelSink CreateSink (IChannelSender channel, string url, object remoteChannelData) 
		{
			IClientChannelSink next = this.nextProvider.CreateSink (channel, url, remoteChannelData);
			BaseCustomSink sink = (BaseCustomSink)Activator.CreateInstance (this.customSinkType);

			sink.SetNextSink (next);
			return sink;
		}
	}

	public class CustomServerSinkProvider : BaseCustomSinkProvider, IServerChannelSinkProvider
	{
		private IServerChannelSinkProvider nextProvider;

		public CustomServerSinkProvider (IDictionary properties, ICollection providerData)
			: base (properties, providerData)
		{
		}

		public IServerChannelSinkProvider Next
		{
			get { return this.nextProvider; }
			set { this.nextProvider = value; }
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel) 
		{
			BaseCustomSink sink = (BaseCustomSink)Activator.CreateInstance (customSinkType);
			IServerChannelSink next = nextProvider.CreateSink (channel);

			sink.SetNextSink (next);
			return sink;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
		}
	}
}
