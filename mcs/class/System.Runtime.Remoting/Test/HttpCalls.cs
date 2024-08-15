//
// MonoTests.Remoting.HttpCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	
	[TestFixture]
	public class HttpSyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	[TestFixture]
	public class HttpAsyncCallTest : AsyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

/*
	//[TestFixture]
	public class HttpReflectionCallTest : ReflectionCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	//[TestFixture]
	public class HttpDelegateCallTest : DelegateCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}
	
	//[TestFixture]
	public class HttpBinarySyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	// Needs separate tests.
	[TestFixture]
	public class HttpCustomSyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpCustomChannelManager ();
		}
	}
*/

	[TestFixture]
	public class HttpCustomAsyncCallTest : AsyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpCustomChannelManager ();
		}
	}

	[Serializable]
	public class HttpChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			Hashtable options = new Hashtable ();
			options ["timeout"] = 10000; // 10s
			return new HttpClientChannel (options, null);
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			return new HttpChannel (0);
		}
	}
	
	[Serializable]
	public class HttpBinaryChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			Hashtable options = new Hashtable ();
			options ["timeout"] = 10000; // 10s
			options ["name"] = "binary http channel";
			return new HttpClientChannel (options,  new BinaryClientFormatterSinkProvider ());
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			return new HttpChannel (0);
		}
	}

	[Serializable]
	public class HttpCustomChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			Hashtable channelOptions = new Hashtable ();
			channelOptions ["timeout"] = 5000; // 5s
			channelOptions ["name"] = "http channel with custom sink";

			Hashtable providerOptions = new Hashtable ();
			providerOptions["customSinkType"] = "MonoTests.Remoting.SimpleCustomClientSink";

			BinaryClientFormatterSinkProvider sinkProvider = new BinaryClientFormatterSinkProvider ();
			sinkProvider.Next = new CustomClientSinkProvider (providerOptions, new Hashtable ());
			return new HttpClientChannel (channelOptions,  sinkProvider);
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			Hashtable channelOptions = new Hashtable ();
			channelOptions ["port"] = "0";

			Hashtable providerOptions = new Hashtable ();
			providerOptions["customSinkType"] = "MonoTests.Remoting.SimpleCustomServerSink";

			BinaryServerFormatterSinkProvider sinkProvider = new BinaryServerFormatterSinkProvider ();
			sinkProvider.Next = new CustomServerSinkProvider (providerOptions, new Hashtable ());
			return new HttpChannel (channelOptions, (IClientChannelSinkProvider)null, (IServerChannelSinkProvider)sinkProvider);
		}
	}
}

