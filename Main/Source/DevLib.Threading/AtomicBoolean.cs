//-----------------------------------------------------------------------
// <copyright file="AtomicBoolean.cs" company="Yu Guan Corporation">
//     Copyright (c) Yu Guan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Threading
{
    using System.Threading;

    /// <summary>
    /// Provides non-blocking, thread-safe access to a boolean value.
    /// </summary>
    public struct AtomicBooleanValue
    {
        /// <summary>
        /// Field UnSet.
        /// </summary>
        private const int UnSet = 0;

        /// <summary>
        /// Field Set.
        /// </summary>
        private const int Set = 1;

        /// <summary>
        /// Field _flag.
        /// </summary>
        private int _flag;

        /// <summary>
        /// Gets or sets the boolean value represented by this instance.
        /// </summary>
        public bool Value
        {
            get
            {
                return this._flag == Set;
            }

            set
            {
                this.Exchange(value);
            }
        }

        /// <summary>
        /// Gets new AtomicBooleanValue instance from a boolean value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>AtomicBooleanValue instance.</returns>
        public static AtomicBooleanValue FromValue(bool value)
        {
            AtomicBooleanValue result = new AtomicBooleanValue();
            result.Value = value;

            return result;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="AtomicBooleanValue"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="value">The AtomicBooleanValue instance.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator bool(AtomicBooleanValue value)
        {
            return value.Value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Boolean"/> to <see cref="AtomicBooleanValue"/>.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator AtomicBooleanValue(bool value)
        {
            return AtomicBooleanValue.FromValue(value);
        }

        /// <summary>
        /// Compares two boolean values, replaces expected value with new value.
        /// </summary>
        /// <param name="expectedValue">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the destination value.</param>
        /// <returns>The original value in expected.</returns>
        public bool CompareAndExchange(bool expectedValue, bool newValue)
        {
            int newTemp = newValue ? Set : UnSet;
            int expectedTemp = expectedValue ? Set : UnSet;

            return Interlocked.CompareExchange(ref this._flag, newTemp, expectedTemp) == expectedTemp;
        }

        /// <summary>
        /// Try to atomically change the value from false to true.
        /// </summary>
        /// <returns>true if set succeeded, false otherwise.</returns>
        public bool TrySet()
        {
            return !this.Exchange(true);
        }

        /// <summary>
        /// Try to atomically change the value from false to true.
        /// </summary>
        /// <remarks>
        /// This method is lighter than TrySet() in the case the AtomicBoolean is already set to true because it first uses a normal read to check the value.
        /// </remarks>
        /// <returns>true if set succeeded, false otherwise.</returns>
        public bool TryRelaxedSet()
        {
            return this._flag == UnSet && !this.Exchange(true);
        }

        /// <summary>
        /// Sets a boolean to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="newValue">The new value to set this instance.</param>
        /// <returns>The original value of this instance.</returns>
        public bool Exchange(bool newValue)
        {
            int newTemp = newValue ? Set : UnSet;

            return Interlocked.Exchange(ref this._flag, newTemp) == Set;
        }

        /// <summary>
        /// Determines whether the specified AtomicBooleanValue is equal to this instance.
        /// </summary>
        /// <param name="value">The AtomicBooleanValue to compare with this instance.</param>
        /// <returns>true if the specified AtomicBooleanValue is equal to this instance; otherwise, false.</returns>
        public bool Equals(AtomicBooleanValue value)
        {
            return this._flag == value._flag;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>true if the specified <see cref="System.Object" /> is equal to this instance; otherwise, false.</returns>
        public override bool Equals(object value)
        {
            return value is AtomicBooleanValue
                ? this.Equals((AtomicBooleanValue)value)
                : false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this._flag.GetHashCode();
        }
    }

    /// <summary>
    /// Provides non-blocking, thread-safe access to a boolean value.
    /// </summary>
    public class AtomicBoolean
    {
        /// <summary>
        /// Field UnSet.
        /// </summary>
        private const int UnSet = 0;

        /// <summary>
        /// Field Set.
        /// </summary>
        private const int Set = 1;

        /// <summary>
        /// Field _flag.
        /// </summary>
        private int _flag;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicBoolean"/> class.
        /// </summary>
        public AtomicBoolean()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicBoolean"/> class.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public AtomicBoolean(bool value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the boolean value represented by this instance.
        /// </summary>
        public bool Value
        {
            get
            {
                return this._flag == Set;
            }

            set
            {
                this.Exchange(value);
            }
        }

        /// <summary>
        /// Gets new AtomicBoolean instance from a boolean value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>AtomicBoolean instance.</returns>
        public static AtomicBoolean FromValue(bool value)
        {
            AtomicBoolean result = new AtomicBoolean(value);
            return result;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="AtomicBoolean"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="value">The AtomicBoolean instance.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator bool(AtomicBoolean value)
        {
            return value.Value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Boolean"/> to <see cref="AtomicBoolean"/>.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator AtomicBoolean(bool value)
        {
            return AtomicBoolean.FromValue(value);
        }

        /// <summary>
        /// Compares two boolean values, replaces expected value with new value.
        /// </summary>
        /// <param name="expectedValue">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the destination value.</param>
        /// <returns>The original value in expected.</returns>
        public bool CompareAndExchange(bool expectedValue, bool newValue)
        {
            int newTemp = newValue ? Set : UnSet;
            int expectedTemp = expectedValue ? Set : UnSet;

            return Interlocked.CompareExchange(ref this._flag, newTemp, expectedTemp) == expectedTemp;
        }

        /// <summary>
        /// Try to atomically change the value from false to true.
        /// </summary>
        /// <returns>true if set succeeded, false otherwise.</returns>
        public bool TrySet()
        {
            return !this.Exchange(true);
        }

        /// <summary>
        /// Try to atomically change the value from false to true.
        /// </summary>
        /// <remarks>
        /// This method is lighter than TrySet() in the case the AtomicBoolean is already set to true because it first uses a normal read to check the value.
        /// </remarks>
        /// <returns>true if set succeeded, false otherwise.</returns>
        public bool TryRelaxedSet()
        {
            return this._flag == UnSet && !this.Exchange(true);
        }

        /// <summary>
        /// Sets a boolean to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="newValue">The new value to set this instance.</param>
        /// <returns>The original value of this instance.</returns>
        public bool Exchange(bool newValue)
        {
            int newTemp = newValue ? Set : UnSet;

            return Interlocked.Exchange(ref this._flag, newTemp) == Set;
        }

        /// <summary>
        /// Determines whether the specified AtomicBoolean is equal to this instance.
        /// </summary>
        /// <param name="value">The AtomicBoolean to compare with this instance.</param>
        /// <returns>true if the specified AtomicBoolean is equal to this instance; otherwise, false.</returns>
        public bool Equals(AtomicBoolean value)
        {
            return this._flag == value._flag;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>true if the specified <see cref="System.Object" /> is equal to this instance; otherwise, false.</returns>
        public override bool Equals(object value)
        {
            var atomicBoolean = value as AtomicBoolean;

            if (atomicBoolean != null)
            {
                return this.Equals(atomicBoolean);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this._flag.GetHashCode();
        }
    }
}
