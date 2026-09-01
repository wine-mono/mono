// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
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
using System.ServiceModel.Channels;

namespace System.ServiceModel {
	[MonoTODO]
	public class NetHttpBinding : HttpBindingBase {
		BinaryMessageEncodingBindingElement binary_message_encoding_binding_element;
		ReliableSessionBindingElement session;
		OptionalReliableSession reliable_session;
		NetHttpMessageEncoding message_encoding;
		BasicHttpSecurity basic_http_security;

		public NetHttpBinding ()
			: this(BasicHttpSecurityMode.None)
		{
		}

		public NetHttpBinding (BasicHttpSecurityMode securityMode)
			: base()
		{
			this.message_encoding = NetHttpMessageEncoding.Binary;
			this.binary_message_encoding_binding_element = new BinaryMessageEncodingBindingElement() { MessageVersion = MessageVersion.Soap12WSAddressing10 };
			this.session = new ReliableSessionBindingElement();
			this.reliable_session = new OptionalReliableSession(this.session);
			this.basic_http_security = new BasicHttpSecurity();
			this.basic_http_security.Mode = securityMode;
		}

		public NetHttpBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}
		
		public NetHttpBinding (
			BasicHttpSecurityMode securityMode, bool reliableSessionEnabled)
		{
			throw new NotImplementedException ();
		}

		public NetHttpMessageEncoding MessageEncoding {
			get { return this.message_encoding; }
			set { throw new NotImplementedException (); }
		}

		public OptionalReliableSession ReliableSession { get; set; }

		public BasicHttpSecurity Security {
			get { return this.basic_http_security; }
			set { throw new NotImplementedException (); }
		}

		public WebSocketTransportSettings WebSocketSettings {
			get { throw new NotImplementedException (); }
		}

		internal override BasicHttpSecurity BasicHttpSecurity {
			get { return this.basic_http_security; }
		}

		public override BindingElementCollection CreateBindingElements ()
		{
			// return collection of BindingElements
			BindingElementCollection bindingElements = new BindingElementCollection();

			// order of BindingElements is important
			// add session
			if (this.reliable_session.Enabled)
			{
				bindingElements.Add(this.session);
			}

			// add security (*optional)
			SecurityBindingElement messageSecurity = this.BasicHttpSecurity.CreateMessageSecurity();
			if (messageSecurity != null)
			{
				bindingElements.Add(messageSecurity);
			}

			// add encoding
			switch (this.MessageEncoding)
			{
				case NetHttpMessageEncoding.Text:
				case NetHttpMessageEncoding.Mtom:
					throw new NotImplementedException ();
					break;
				default:
					bindingElements.Add(this.binary_message_encoding_binding_element);
					break;
			}

			// add transport (http or https)
			bindingElements.Add(this.GetTransport());

			return bindingElements.Clone();
		}

		public bool ShouldSerializeReliableSession ()
		{
			throw new NotImplementedException ();
		}
		
		public bool ShouldSerializeSecurity ()
		{
			throw new NotImplementedException ();
		}
		
		
		
	}
}