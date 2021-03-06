﻿using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A lightweight WCF service client class based on <see cref="ChannelFactory{T}"/>, includes interceptor behavior.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public abstract class InterceptorLightClient<TContract> : LightClient<TContract>, ICallIntercept where TContract : class
    {
        #region ICallIntercept
        /// <summary>
        /// Invoked before the call.
        /// </summary>
        /// <param name="request">The request.</param>
        public virtual void PreInvoke(ref Message request)
        {
        }

        /// <summary>
        /// Invoked after the call.
        /// </summary>
        /// <param name="reply">The reply.</param>
        public virtual void PostInvoke(ref Message reply)
        {
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">
        /// The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType = ServiceIdentity.None,
            string identity = null,
            string messagingPattern = null)
            : base(remoteAddress, identityType, identity, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{T}"/> class
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName"/> and a remote address.
        /// If <paramref name="endpointConfigurationName"/> is <see langword="null" />, empty or consist of whitespace characters
        /// the constructor will try to resolve the binding from the schema in the given remote address from the <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint section of the configuration files.</param>
        /// <param name="remoteAddress">
        /// The remote address. If the remote address is <see langword="null" /> or empty
        /// the constructor will try to use the address in the endpoint configuration.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string endpointConfigurationName,
            string remoteAddress,
            string messagingPattern = null)
            : base(endpointConfigurationName, remoteAddress, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(remoteAddress, identityType, certificate, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(remoteAddress, identityClaim, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Dns" />, <see cref="ServiceIdentity.Spn" />, <see cref="ServiceIdentity.Upn" />, or 
        /// <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="identity">
        /// The identifier in the case of <see cref="ServiceIdentity.Dns" /> should be the DNS name of specified by the service's certificate or machine.
        /// If the identity type is <see cref="ServiceIdentity.Upn" /> - use the UPN of the service identity; if <see cref="ServiceIdentity.Spn" /> - use the SPN and if
        /// <see cref="ServiceIdentity.Rsa" /> - use the RSA key.
        /// </param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, identity, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="certificate">The identifying certificate.</param>
        /// <param name="messagingPattern">
        /// The messaging pattern defining the configuration of the connection. If <see langword="null"/>, empty or whitespace characters only, 
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute"/> if present,
        /// otherwise will apply the default messaging pattern fro the transport.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityType, certificate, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="binding">A binding instance.</param>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="messagingPattern">The messaging pattern defining the configuration of the connection. If <see langword="null" />, empty or whitespace characters only,
        /// the constructor will try to resolve the pattern from the interface's attribute <see cref="MessagingPatternAttribute" /> if present,
        /// otherwise will apply the default messaging pattern fro the transport.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            Claim identityClaim,
            string messagingPattern = null)
            : base(binding, remoteAddress, identityClaim, messagingPattern)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
        }
        #endregion
    }
}
