﻿using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading;
using vm.Aspects.Wcf.FaultContracts.Metadata;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// This is the base class for all services faults. Mirrors the Exception class. 
    /// Note that the fields StackTrace and Source are transferred in 
    /// DEBUG mode only.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}:: {Message}")]
    [MetadataType(typeof(FaultMetadata))]
    public class Fault
    {
        #region Properties
        /// <summary>
        /// Gets or sets the handling instance ID.
        /// <c>PostHandlingAction.ThrowNewException</c> causes the exception handling application block to throw a new exception of type 
        /// FaultException{XyzFault} containing the same <c>handlingInstanceID</c> after the entire chain of handlers runs.
        /// This would help to track the fault caught at the client down to the original exception logged by the WCF service.
        /// </summary>
        [DataMember]
        public Guid HandlingInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the fault's message
        /// </summary>
        [DataMember]
        public virtual string Message { get; set; }

        /// <summary>
        /// Gets or sets the text of the messages of the inner exception(s) of the original exception that caused this fault.
        /// </summary>
        [DataMember]
        public string InnerExceptionsMessages { get; protected set; }
        #endregion

        /// <summary>
        /// Appends the text of the related exception's inner exception(s) to the Message property.
        /// </summary>
        /// <remarks>
        /// Note that this method has dummy getter and also the property is not marked with DataMemberAttribute. By default the fault exception handler from 
        /// Enterprise Library will copy the properties of the exception to the fault's properties - property for property matched by name. Here the setter 
        /// will extract recursively the needed textual information from the inner exception(s) and will append it to the property 
        /// <see cref="P:InnerExceptionsMessages"/>.
        /// </remarks>
        public Exception InnerException
        {
            get
            {
                Contract.Ensures(Contract.Result<Exception>() == null);

                return null;
            }
            set
            {
                if (value != null)
                    InnerExceptionsMessages = value.Message;
            }
        }

#if DEBUG
        #region Debug only properties and constructor
        /// <summary>
        /// Gets or sets the user who experienced the fault.
        /// </summary>
        /// <value>The user login ID.</value>
        [DataMember]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine where the exception happened.
        /// </summary>
        [DataMember]
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the process where the exception happened.
        /// </summary>
        [DataMember]
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the process where the exception happened.
        /// </summary>
        [DataMember]
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the thread on which the exception happened.
        /// </summary>
        [DataMember]
        public int ThreadId { get; set; }

        /// <summary>
        /// Gets or sets a collection of key-value pairs that provide additional, user-defined information about the exception.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="It is a DTO.")]
        public IDictionary Data { get; set; }

        /// <summary>
        /// String representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        [DataMember]
        public string StackTrace { get; set; }

        /// <summary>
        /// The name of the application or the object that causes the error.
        /// </summary>
        [DataMember]
        public string Source { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance with some default data in DEBUG mode only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fault()
        {
            var process = Process.GetCurrentProcess();

            ProcessId	= process.Id;
            ProcessName = process.ProcessName;
            ThreadId	= Thread.CurrentThread.ManagedThreadId;
            MachineName = Environment.MachineName;
            User		= Environment.UserName;
        }
        #endregion
#endif

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(0);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that display the value(s) of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that display the value(s) of this instance.
        /// </returns>
        public string ToString(int indentLevel)
        {
            return this.DumpString(indentLevel);
        }
        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(InnerException == null);
        }
    }
}
