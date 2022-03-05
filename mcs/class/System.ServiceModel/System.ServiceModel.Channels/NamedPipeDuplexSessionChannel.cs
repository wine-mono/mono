// 
// NamedPipeDuplexSessionChannel.cs
// 
// Author: 
//	Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels.NetTcp;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class NamedPipeDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{
		class NamedPipeDuplexSession : DuplexSessionBase
		{
			NamedPipeDuplexSessionChannel owner;

			internal NamedPipeDuplexSession (NamedPipeDuplexSessionChannel owner)
			{
				this.owner = owner;
			}

			public override TimeSpan DefaultCloseTimeout {
				get { return TimeSpan.MaxValue; }
			}

			public override void Close (TimeSpan timeout)
			{
			}
		}

		bool is_service_side;
		TcpBinaryFrameManager frame;
		PipeStream stream;
		
		public NamedPipeDuplexSessionChannel (ChannelFactoryBase factory, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			var npstream = new NamedPipeClientStream (".", Via.LocalPath.Substring (1).Replace ('/', '\\'), PipeDirection.InOut);
			npstream.Connect ();
			stream = npstream;
			Encoder = new BinaryMessageEncoder (); // FIXME
			Session = new NamedPipeDuplexSession (this);
			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, stream, false) {
				Encoder = this.Encoder,
				Via = this.Via };
			frame.ProcessPreambleInitiator ();
			frame.ProcessPreambleAckInitiator ();
		}
		
		public NamedPipeDuplexSessionChannel (ChannelListenerBase listener, MessageEncoder encoder, NamedPipeServerStream npstream)
			: base (listener)
		{
			is_service_side = true;
			stream = npstream;
			Encoder = encoder;
			Session = new NamedPipeDuplexSession (this);
			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, stream, true) {
				Encoder = this.Encoder };
			frame.ProcessPreambleRecipient ();
			frame.ProcessPreambleAckRecipient ();
		}

		public IDuplexSession Session {
			get; private set;
		}

		public MessageEncoder Encoder { get; private set; }

		public override void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}
		
		public override void Send (Message message, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			if (!is_service_side) {
				if (message.Headers.To == null)
					message.Headers.To = RemoteAddress.Uri;
			}

			// Logger.LogMessage (MessageLogSourceKind.TransportSend, ref message, BindingElement.MaxReceivedMessageSize);

			frame.WriteSizedMessage (message);
		}
		
		public override bool TryReceive (TimeSpan timeout, out Message message)
		{
			ThrowIfDisposedOrNotOpen ();

			message = frame.ReadSizedMessage ();

			// Logger.LogMessage (MessageLogSourceKind.TransportReceive, ref message, info.BindingElement.MaxReceivedMessageSize);

			return true;
		}
		
		public override bool WaitForMessage (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			throw new NotImplementedException ();
		}
		
		// CommunicationObject
		
		[MonoTODO]
		protected override void OnAbort ()
		{
		}

		protected override void OnClose (TimeSpan timeout)
		{
		}
		
		protected override void OnOpen (TimeSpan timeout)
		{
		}
	}
}
