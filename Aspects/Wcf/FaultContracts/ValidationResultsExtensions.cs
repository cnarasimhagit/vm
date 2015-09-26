﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace vm.Aspects.Wcf.FaultContracts
{
    /// <summary>
    /// Contains extension methods for <see cref="ValidationResult"/>
    /// </summary>
    public static class ValidationResultsExtensions
    {
        /// <summary>
        /// Copies the properties of a given <see cref="ValidationResult"/> object to the <paramref name="element"/>.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="element">The element.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="element"/> or <paramref name="result"/> are <see langword="null"/>.
        /// </exception>
        public static void CopyTo(
            this ValidationResult result,
            ValidationFaultElement element)
        {
            if (result==null)
                throw new ArgumentNullException("result");
            if (element==null)
                throw new ArgumentNullException("element");

            element.Message = result.Message;
            element.Key     = result.Key;
            element.Tag     = result.Tag;

            if (result.Target != null)
            {
                Type targetType = result.Target as Type;

                element.TargetTypeName = targetType!=null 
                                            ? targetType.Name 
                                            : result.Target.GetType().Name;
            }

            element.NestedValidationElements = new List<ValidationFaultElement>();
            result.NestedValidationResults.CopyTo(element.NestedValidationElements);
        }

        /// <summary>
        /// Copies a sequence of <see cref="ValidationResult"/> objects to <paramref name="elements"/>.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="elements">The elements.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="elements"/> or <paramref name="results"/> are <see langword="null"/>.
        /// </exception>
        public static void CopyTo(
            this IEnumerable<ValidationResult> results,
            ICollection<ValidationFaultElement> elements)
        {
            if (results==null)
                throw new ArgumentNullException("results");
            if (elements==null)
                throw new ArgumentNullException("elements");

            foreach (var r in results)
                if (r != null)
                    elements.Add(new ValidationFaultElement(r));
        }
    }
}
