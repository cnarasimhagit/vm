﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using vm.Aspects.Wcf.FaultContracts.Metadata;

using ValidationResult = Microsoft.Practices.EnterpriseLibrary.Validation.ValidationResult;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Mirrors ValidationResult.
    /// </summary>
    [DataContract(Namespace="urn:vm.Aspects.Wcf")]
    [DebuggerDisplay("{GetType().Name, nq}: {Message}")]
    [MetadataType(typeof(ValidationFaultElementMetadata))]
    public sealed class ValidationFaultElement
    {
        #region Properties
        /// <summary>
        /// Gets or sets a message describing the failure.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the type name of the object to which the validation rule was applied.
        /// </summary>
        [DataMember]
        public string TargetTypeName { get; set; }

        /// <summary>
        /// Gets or sets a name describing the location of the validation result.
        /// </summary>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value characterizing the result.
        /// </summary>
        [DataMember]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the nested validation results for a composite failed validation.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="It is a DTO.")]
        public ICollection<ValidationFaultElement> NestedValidationElements { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFaultElement"/> class.
        /// </summary>
        public ValidationFaultElement()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFaultElement"/> class with data from <see cref="ValidationResult"/> object.
        /// </summary>
        /// <param name="result">The <see cref="ValidationResult"/> object to copy data from.</param>
        public ValidationFaultElement(
            ValidationResult result)
        {
            NestedValidationElements = new List<ValidationFaultElement>();

            if (result!=null)
                result.CopyTo(this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(0);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="indentLevel">The indent level.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public string ToString(int indentLevel)
        {
            return this.DumpString(indentLevel);
        }
        #endregion
    }
}
