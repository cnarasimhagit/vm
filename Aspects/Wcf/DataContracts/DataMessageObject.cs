﻿using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Facilities;
using vm.Aspects.Validation;

namespace vm.Aspects.Wcf.DataContracts
{
    /// <summary>
    /// Messages (contracts) base class. Standardizes on <see cref="T:vm.Aspects.Validation.IValidatable"/>.
    /// This is just another type of DTO. Prefer <see cref="DataTransferObject"/> where possible.
    /// </summary>
    [MessageContract(WrapperNamespace = "urn:service:vm.Aspects.Wcf.DataContracts")]
    public abstract class DataMessageObject : IValidatable
    {
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
    }
}
