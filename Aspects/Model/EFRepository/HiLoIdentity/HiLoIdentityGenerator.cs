﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    /// <summary>
    /// Class HiLoIdentityGenerator. Implements the Hi-Lo algorithm for generating unique ID-s.
    /// </summary>
    [MetadataType(typeof(HiLoIdentityGeneratorMetadata))]
    public partial class HiLoIdentityGenerator : BaseDomainEntity, IEquatable<HiLoIdentityGenerator>
    {
        #region Constants
        /// <summary>
        /// The maximum length of the property <see cref="EntitySetName"/>: 100.
        /// </summary>
        public const int EntitySetNameMaxLength = 100;

#if DEBUG
        /// <summary>
        /// The default value of the maximum value of the low value: DEBUG - 100, RELEASE - 1000.
        /// </summary>
        public const int DefaultMaxLowValue = 100;
#else
        /// <summary>
        /// The default value of the maximum value of the low value: DEBUG - 100, RELEASE - 1000.
        /// </summary>
        public const int DefaultMaxLowValue = 1000;
#endif
        #endregion

        /// <summary>
        /// The maximum high value beyond which ID-s cannot be generated.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public long MaxHighValue { get; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoIdentityGenerator"/> class.
        /// </summary>
        public HiLoIdentityGenerator()
            : this(null, DefaultMaxLowValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoIdentityGenerator" /> class.
        /// </summary>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="maxLowValue">The max low value.</param>
        /// <param name="initialHighValue">The initial high value.</param>
        public HiLoIdentityGenerator(
            string entitySetName,
            int maxLowValue = DefaultMaxLowValue,
            long initialHighValue = 1L)
        {
            if (entitySetName != null  &&  !RegularExpression.CSharpIdentifier.IsMatch(entitySetName))
                throw new ArgumentException("Invalid entity set name.", nameof(entitySetName));
            if (maxLowValue <= 0)
                throw new ArgumentException("The parameter must be positive.", nameof(maxLowValue));
            if (initialHighValue < 0L)
                throw new ArgumentException("The parameter cannot be negative.", nameof(initialHighValue));

            EntitySetName = entitySetName;
            MaxLowValue   = maxLowValue;
            HighValue     = initialHighValue;
            MaxHighValue  = long.MaxValue/MaxLowValue;
        }
        #endregion

        #region Persisted properties
        /// <summary>
        /// Gets the name of the entities set.
        /// </summary>
        /// <value>
        /// The name of the entities set.
        /// </value>
        public string EntitySetName { get; set; }

        /// <summary>
        /// Gets the next value of the high value.
        /// </summary>
        public long HighValue { get; set; }

        /// <summary>
        /// Gets the low value.
        /// </summary>
        public int LowValue { get; set; }

        /// <summary>
        /// Gets the maximum value of the low value.
        /// </summary>
        public int MaxLowValue { get; set; }
        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is already associated with an entity set.
        /// </summary>
        public override bool HasIdentity => !string.IsNullOrWhiteSpace(EntitySetName);

        #region Methods
        /// <summary>
        /// Increments the high value and initializes the low value.
        /// I.e. Moves the generator to the next locked range of values.
        /// </summary>
        internal void IncrementHighValue()
        {
            unchecked
            {
                if (HighValue+1 >= MaxHighValue)
                    throw new InvalidOperationException(
                        $"FATAL ERROR: the HighValue of the ID generator for entity set {EntitySetName} has reached the maximum value.");

                HighValue++;
                LowValue = 0;
            }
        }

        /// <summary>
        /// Gets the next available value for an entity id. If the low values are exhausted the method will return -1L.
        /// </summary>
        /// <returns>The next available value for an entity id.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "n/a")]
        public long GetId()
        {
            if (HighValue <= 1L || LowValue >= MaxLowValue)
                return -1L;

            return unchecked((HighValue-1)*MaxLowValue + ++LowValue);
        }
        #endregion

        #region Identity rules implementation.
        #region IEquatable<HiLoIdentityGenerator> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="HiLoIdentityGenerator"/> to compare with the current object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/>  if <paramref name="other"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="other"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/>  if the current object and the <paramref name="other"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(HiLoIdentityGenerator)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public virtual bool Equals(HiLoIdentityGenerator other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;

            return EntitySetName == other.EntitySetName;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="HiLoIdentityGenerator"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="HiLoIdentityGenerator"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> cannot be cast to <see cref="HiLoIdentityGenerator"/>, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/>  if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/>  if the current object and the <paramref name="obj"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(HiLoIdentityGenerator)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as HiLoIdentityGenerator);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="HiLoIdentityGenerator"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="HiLoIdentityGenerator"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            unchecked
            {
                hashCode = Constants.HashMultiplier * hashCode + EntitySetName.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="HiLoIdentityGenerator"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(HiLoIdentityGenerator)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(HiLoIdentityGenerator left, HiLoIdentityGenerator right)
            => left is null ? right is null : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="HiLoIdentityGenerator"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(HiLoIdentityGenerator)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(HiLoIdentityGenerator left, HiLoIdentityGenerator right)
            => !(left==right);
        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="BaseDomainEntity"/> to compare with this object.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise
        /// <see langword="true"/>  if <paramref name="other"/> refers to <c>this</c> object, otherwise
        /// <see langword="true"/>  if <i>the business identities</i> of the current object and the <paramref name="other"/> are equal by value,
        /// e.g. <c>BusinessKeyProperty == other.BusinessKeyProperty</c>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(HiLoIdentityGenerator)"/> method and its overloads as well as the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public override bool Equals(BaseDomainEntity other) => Equals(other as HiLoIdentityGenerator);
    }
}
