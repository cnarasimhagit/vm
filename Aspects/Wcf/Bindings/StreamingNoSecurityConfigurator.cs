﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class StreamingConfigurator. Configures the bindings for transferring streams (big amounts of data).
    /// </summary>
    public class StreamingNoSecurityConfigurator : RequestResponseNoSecurityConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "StreamingNoSecurity";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingNoSecurityConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public StreamingNoSecurityConfigurator(
            Lazy<IConfigurationProvider> config)
            : base(config)
        {
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Unity will dispose it.")]
        public override Binding Configure(
            BasicHttpBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MessageEncoding         = WSMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize  = Constants.StreamingBasicHttpMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            BasicHttpsBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MessageEncoding         = WSMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize  = Constants.StreamingBasicHttpMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MessageEncoding         = NetHttpMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize  = Constants.StreamingBasicHttpMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpsBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MessageEncoding         = NetHttpMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize  = Constants.StreamingBasicHttpMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            WebHttpBinding binding)
        {
            base.Configure(binding);

            binding.ContentTypeMapper       = null;
            binding.TransferMode            = TransferMode.Streamed;
            binding.MaxReceivedMessageSize  = Constants.StreamingMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            WSHttpBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetNamedPipeBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MaxReceivedMessageSize  = Constants.StreamingMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetTcpBinding binding)
        {
            base.Configure(binding);

            binding.TransferMode            = TransferMode.Streamed;
            binding.MaxReceivedMessageSize  = Constants.StreamingMaxMessageSize;
            binding.MaxBufferSize           = Constants.StreamingMaxBufferSize;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            NetMsmqBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            MsmqIntegrationBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }
    }
}
