﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Diagnostics;
using vm.Aspects.Facilities;
using vm.Aspects.Validation;

namespace vm.Aspects.Wcf.DataContracts
{
    /// <summary>
    /// WCF DTO's base class. Takes care of the forward and backward compatibility of the DTO-s and standardizes on <see cref="IValidatable"/>.
    /// Can be easily used also in a non-WCF context.
    /// </summary>
    [DataContract(Namespace = "urn:service:vm.Aspects.Wcf.DataContracts")]
    public abstract class DataTransferObject : IExtensibleDataObject, IValidatable
    {
        #region IExtensibleDataObject Members
        /// <summary>
        /// Gets or sets the structure that contains extra data. I.e. not belonging to the current version.
        /// </summary>
        /// <value>An <see cref="T:System.Runtime.Serialization.ExtensionDataObject"/> instance.</value>
        /// <returns>
        /// An <see cref="T:System.Runtime.Serialization.ExtensionDataObject"/> instance that contains data which is not recognized as belonging to the data contract.
        /// </returns>
        [Dump(false)]
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion

        #region IValidatable Members
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="ruleset">The ruleset to test validity against.</param>
        /// <param name="results">The results.</param>
        /// <returns>A list of <see cref="ValidationResult" /> objects.</returns>
        public ValidationResults Validate(
            string ruleset = "",
            ValidationResults results = null)
        {

            var validator = Facility.ValidatorFactory.CreateValidator(GetType(), ruleset);

            if (results == null)
                results = validator.Validate(this);
            else
                validator.Validate(this, results);

#if DEBUG
            if (!results.IsValid)
                Debug.WriteLine($"{ToString()}\n{results.DumpString()}");
#endif

            return results;
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => this.DumpString();

        /// <summary>
        /// Accepts a visitor object. See the G4 visitor pattern.
        /// </summary>
        /// <remarks>
        /// Each <c>DataTransferObject</c> derived type which can accept visitors should have an overload of <c>AcceptVisitor</c> with a parameter the concrete visitor type and a body
        /// <c>visitor.Visit(this);</c>. This way a concrete overload of the visitor's <c>Visit</c> method will execute on a concrete entity and would perform a type specific function.
        /// Here <c>AcceptVisitor</c> accepts any visitors but throws <see cref="T:System.NotImplementedException"/> and works as a catch all where 
        /// either the concrete visitor does not have a concrete overload for the entity or the entity does not accept the concrete visitor. 
        /// </remarks>
        /// <param name="visitor">The visitor.</param>
        /// <exception cref="NotImplementedException">
        /// Always thrown.
        /// </exception>
        public void AcceptVisitor(
            object visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));

            throw new NotImplementedException(
                        $"Either entities of type {GetType().Name} do not accept visitors of type {1} or the visitors do not have a concrete overload for {visitor.GetType().Name}.");
        }
    }
}
