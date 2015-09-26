﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// A lightweight WCF service client class based on <see cref="T:ChannelFactory{T}"/>, includes interceptor behavior.
    /// </summary>
    /// <typeparam name="TContract">The service interface.</typeparam>
    public abstract class InterceptorLightClient<TContract> : LightClientBase<TContract>, ICallIntercept where TContract : class
    {
        /// <summary>
        /// Gets the proxy of the service.
        /// </summary>
        public TContract Proxy { get; private set; }

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
        /// Initializes a new instance of the <see cref="T:InterceptorLightClient{T}"/> class
        /// from an endpoint configuration section given by the <paramref name="endpointConfigurationName"/> and a remote address.
        /// If <paramref name="endpointConfigurationName"/> is <see langword="null" />, empty or consist of whitespace characters
        /// the constructor will try to resolve the binding from the schema in the given remote address from the <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint section of the configuration files.</param>
        /// <param name="remoteAddress">
        /// The remote address. If the remote address is <see langword="null" /> or empty
        /// the constructor will try to use the address in the endpoint configuration.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string endpointConfigurationName,
            string remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
            Contract.Requires<ArgumentException>(
                !string.IsNullOrWhiteSpace(endpointConfigurationName) &&
                !string.IsNullOrWhiteSpace(remoteAddress), "At least one of the parameters must be not null, empty or consist of whitespace characters only.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            string identity)
            : base(remoteAddress, identityType, identity)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorLightClient{TContract}" /> class (creates the channel factory).
        /// </summary>
        /// <param name="remoteAddress">The remote address of the service.</param>
        /// <param name="identityType">
        /// Type of the identity: can be <see cref="ServiceIdentity.Certificate" /> or <see cref="ServiceIdentity.Rsa" />.
        /// </param>
        /// <param name="certificate">The identifying certificate.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate)
            : base(remoteAddress, identityType, certificate)
        {
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                               ||
                identityType == ServiceIdentity.Certificate &&  certificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  certificate!=null,
                "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            string identity)
            : base(binding, remoteAddress, identityType, identity)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                                           ||
                identityType == ServiceIdentity.Dns  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Rsa  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Upn  &&  !string.IsNullOrWhiteSpace(identity)  ||
                identityType == ServiceIdentity.Spn  &&  !string.IsNullOrWhiteSpace(identity),
                "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected InterceptorLightClient(
            Binding binding,
            string remoteAddress,
            ServiceIdentity identityType,
            X509Certificate2 certificate)
            : base(binding, remoteAddress, identityType, certificate)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));
            Contract.Requires<ArgumentNullException>(remoteAddress != null, nameof(remoteAddress));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(remoteAddress), "The argument \"remoteAddress\" cannot be null, empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(
                identityType == ServiceIdentity.None                               ||
                identityType == ServiceIdentity.Certificate &&  certificate!=null  ||
                identityType == ServiceIdentity.Rsa         &&  certificate!=null,
                "Invalid combination of identity parameters.");

            ChannelFactory.Endpoint.Behaviors.Add(new InterceptorBehavior(this));
            Proxy = ChannelFactory.CreateChannel();
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// Disposes the object graph.
        /// </summary>
        protected override void DisposeObjectGraph()
        {
            DisposeCommunicationObject(Proxy as ICommunicationObject);
            base.DisposeObjectGraph();
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(Proxy != null);
        }
    }
}
