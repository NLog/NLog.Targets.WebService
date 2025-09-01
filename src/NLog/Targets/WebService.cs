// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Web.Services.Protocols;

using NLog.Config;
using System.IO;
using System.Xml;
using System.Net;

namespace NLog.Targets
{
    /// <summary>
    /// Calls the specified webservice on each logging message. NOT OPERATIONAL YET.
    /// </summary>
    [Target("WebServiceCall")]
    public sealed class WebServiceTarget: MethodCallTargetBase
    {
        const string soapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

        /// <summary>
        /// Web service protocol
        /// </summary>
        public enum WebServiceProtocol
        {
            /// <summary>
            /// SOAP 1.1
            /// </summary>
            Soap11,

            /// <summary>
            /// SOAP 1.2
            /// </summary>
            Soap12,

            /// <summary>
            /// HTTP POST
            /// </summary>
            HttpPost,

            /// <summary>
            /// HTTP GET
            /// </summary>
            HttpGet,
        }

        private string _methodName = null;
        private string _url = null;
        private string _namespace = null;
        private WebServiceProtocol _protocol = WebServiceProtocol.Soap11;

        /// <summary>
        /// Web service URL.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; } 
        }

        /// <summary>
        /// Web service method name.
        /// </summary>
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        /// <summary>
        /// Web service namespace.
        /// </summary>
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// The protocol to be used when calling web service.
        /// </summary>
        [System.ComponentModel.DefaultValue("Soap11")]
        public WebServiceProtocol Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="parameters">Parameters to be passed.</param>
        protected override void DoInvoke(object[]parameters)
        {
            switch (_protocol)
            {
                case WebServiceProtocol.Soap11:
                    InvokeSoap11(parameters);
                    break;

                case WebServiceProtocol.Soap12:
                    InvokeSoap12(parameters);
                    break;

                case WebServiceProtocol.HttpGet:
                    InvokeHttpGet(parameters);
                    break;

                case WebServiceProtocol.HttpPost:
                    InvokeHttpPost(parameters);
                    break;
            }
            //_client.DoInvoke(MethodName, parameters);
        }

        private void InvokeSoap11(object[] parameters)
        {
            WebClient client = new WebClient();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";

            string soapAction;

            if (Namespace.EndsWith("/"))
                soapAction = "SOAPAction: " + Namespace + MethodName;
            else
                soapAction = "SOAPAction: " + Namespace + "/" + MethodName;

            using (Stream s = request.GetRequestStream())
            {
                using (XmlTextWriter xtw = new XmlTextWriter(s, System.Text.Encoding.UTF8))
                {
                    xtw.WriteStartElement("soap", "Envelope", soapEnvelopeNamespace);
                    xtw.WriteStartElement("Body", soapEnvelopeNamespace);
                    xtw.WriteStartElement(MethodName, Namespace);
                    for (int i = 0; i < Parameters.Count; ++i)
                    {
                        xtw.WriteElementString(Parameters[i].Name, Convert.ToString(parameters[i]));
                    }
                    xtw.WriteEndElement();
                    xtw.WriteEndElement();
                    xtw.WriteEndElement();
                }
            }
            using (WebResponse response = request.GetResponse())
            {
            }
        }

        private void InvokeSoap12(object[] parameters)
        {
            throw new NotSupportedException();
        }

        private void InvokeHttpGet(object[] parameters)
        {
            throw new NotSupportedException();
        }

        private void InvokeHttpPost(object[] parameters)
        {
            throw new NotSupportedException();
        }
    }
}
