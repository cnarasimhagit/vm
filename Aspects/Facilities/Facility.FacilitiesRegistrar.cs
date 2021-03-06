﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Validation;

using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Registration;

using vm.Aspects.Diagnostics;
using vm.Aspects.Threading;

namespace vm.Aspects.Facilities
{
    public static partial class Facility
    {
        /// <summary>
        /// Gets the facilities registrar instance.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new FacilitiesRegistrar();

        /// <summary>
        /// Class FacilitiesRegistrar. Registers facilities of type IClock, IGuidGenerator, ValidatorFactory, LogWriter, ExceptionManager
        /// </summary>
        internal class FacilitiesRegistrar : ContainerRegistrar
        {
            /// <summary>
            /// Resets the <see cref="ContainerRegistrar.AreRegistered" /> method. Use for testing only.
            /// </summary>
            public override void Reset(
                IUnityContainer container = null)
            {
                Debug.Assert(container == null || container == DIContainer.Root, "The facilities registrar can register the facilities only in the root container.");

                container = DIContainer.Root;

                ExceptionPolicyProvider.Registrar.Reset(container);
                LogConfigProvider.Registrar.Reset(container);
                InitializeFacilities();

                base.Reset(container);
            }

            internal static void RefreshLogger(
                LogWriter logWriter)
            {
                using (_syncLogger.WriterLock())
                    if (_logWriter.IsValueCreated)
                    {
                        Logger.Reset();
                        Logger.SetLogWriter(logWriter);

                        _logWriter = new Lazy<LogWriter>(() => { return Logger.Writer; });
                    }
            }

            /// <summary>
            /// Does the actual work of the registration.
            /// Not thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                RegisterCommon(container, registrations)
                    .RegisterTypeIfNot<IClock, Clock>(registrations, new ContainerControlledLifetimeManager())
                    .RegisterTypeIfNot<IGuidGenerator, GuidGenerator>(registrations, new ContainerControlledLifetimeManager())
                    ;
            }

            /// <summary>
            /// The inheriting types should override this method if they need to register different configuration for unit testing purposes.
            /// The default implementation calls <see cref="ContainerRegistrar.DoRegister" />.
            /// Not thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                RegisterCommon(container, registrations, true)
                    .RegisterInstanceIfNot<IClock>(registrations, new TestClock())
                    .RegisterInstanceIfNot<IGuidGenerator>(registrations, new TestGuidGenerator())
                ;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will do it")]
            static IUnityContainer RegisterCommon(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations,
                bool isTest = false)
            {
                ClassMetadataRegistrar
                    .RegisterMetadata()
                    ;

                return container
                    .RegisterInstanceIfNot<ValidatorFactory>(registrations, ValidationFactory.DefaultCompositeValidatorFactory)
                    .RegisterTypeIfNot<IConfigurationProvider, AppConfigProvider>(registrations, new ContainerControlledLifetimeManager())
                    .RegisterTypeIfNot<ExceptionManager>(registrations, new ContainerControlledLifetimeManager(), new InjectionFactory(c => ExceptionPolicyProvider.CreateExceptionManager()))
                    .RegisterTypeIfNot<LogWriter>(registrations, new ContainerControlledLifetimeManager(), new InjectionFactory(c => LogConfigProvider.CreateLogWriter(LogConfigProvider.LogConfigurationFileName, string.Empty, isTest)))
                    .UnsafeRegister(LogConfigProvider.Registrar, registrations, isTest)
                    .UnsafeRegister(ExceptionPolicyProvider.Registrar, registrations, isTest)
                    ;
            }
        }
    }
}
