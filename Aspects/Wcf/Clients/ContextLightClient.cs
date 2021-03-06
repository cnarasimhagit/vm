﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Claims;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A lightweight WCF service client class based on <see cref="ChannelFactory{T}" />, that leverages the base class' interceptor behavior to add a custom header to the messages.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    /// <typeparam name="THeader">The type of the header.</typeparam>
    /// <seealso cref="vm.Aspects.Wcf.Clients.InterceptorLightClient{TContract}" />
    public abstract class ContextLightClient<TContract, THeader> : InterceptorLightClient<TContract> where TContract : class
    {
        /// <summary>
        /// Gets the name of the HTTP header that should contain the custom context value.
        /// </summary>
        public abstract string HttpHeaderName { get; }

        /// <summary>
        /// Provides access to the context to the descending classes.
        /// </summary>
        protected THeader Context { get; set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or
        /// <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="identity">The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            string remoteAddress,
            ServiceIdentity identityType = ServiceIdentity.None,
            string identity = null,
            string messagingPattern = null)
            : base(remoteAddress, identityType, identity, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{T}" /> class
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName" /> and a remote address.
        /// If <paramref name="endpointConfigurationName" /> is <see langword="null" />, empty or consist of whitespace characters
        /// the constructor will try to resolve the binding from the schema in the given remote address from the <see cref="DIContainer" />.
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint section of the configuration files.</param>
        /// <param name="remoteAddress">The remote address. If the remote address is <see langword="null" /> or empty
        /// the constructor will try to use the address in the endpoint configuration.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
            : base(endpointConfigurationName, remoteAddress, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(remoteAddress, identityType, certificate, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(remoteAddress, identityClaim, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or
        /// <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="identity">The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, identity, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.</param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, certificate, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="context">The context/header.</param>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected ContextLightClient(
            THeader context,
            Binding binding,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityClaim, messagingPattern)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }
        #endregion

        /// <summary>
        /// Invoked before the call.
        /// </summary>
        /// <param name="request">The request.</param>
        public override void PreInvoke(ref Message request)
        {
            if (ChannelFactory.Endpoint.Binding is WebHttpBinding)
            {
                (request.Properties["httpRequest"] as HttpRequestMessageProperty)
                    ?.Headers
                    ?.Add(HttpHeaderName, CreateHttpHeaderValue());
            }
            else
                new CustomDataContext<THeader>(Context).AddToHeaders(request.Headers);

            base.PreInvoke(ref request);
        }

        /// <summary>
        /// Invoked after the call.
        /// </summary>
        /// <param name="reply">The reply.</param>
        public override void PostInvoke(ref Message reply)
        {
            base.PostInvoke(ref reply);
        }

        /// <summary>
        /// Creates the HTTP header value by serializing the context to a string which will become the header's value.
        /// </summary>
        /// <returns>System.String.</returns>
        string CreateHttpHeaderValue()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(
                                            typeof(THeader),
                                            new DataContractJsonSerializerSettings
                                            {
                                                DateTimeFormat            = new DateTimeFormat("o", CultureInfo.InvariantCulture),
                                                EmitTypeInformation       = EmitTypeInformation.Never,
                                                UseSimpleDictionaryFormat = true,
                                            });

                serializer.WriteObject(stream, Context);

                return Encoding.Default.GetString(stream.ToArray());
            }
        }
    }
}
