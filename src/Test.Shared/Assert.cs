using System;
using System.Collections.Generic;

namespace Test.Shared
{
    /// <summary>
    /// Exception thrown when a Touchstone assertion fails.
    /// Touchstone treats a thrown exception as a failed test case.
    /// </summary>
    public sealed class AssertionException : Exception
    {
        /// <summary>
        /// Instantiate a new assertion exception.
        /// </summary>
        /// <param name="message">Failure message.</param>
        public AssertionException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Lightweight, framework-agnostic assertion helpers used by the Touchstone descriptors.
    /// Every failure throws an <see cref="AssertionException"/> so it is reported identically
    /// through the console runner, xUnit, and NUnit.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="condition">Condition that must hold.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void True(bool condition, string message)
        {
            if (!condition)
                throw new AssertionException("Expected condition to be true: " + message);
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="condition">Condition that must not hold.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void False(bool condition, string message)
        {
            if (condition)
                throw new AssertionException("Expected condition to be false: " + message);
        }

        /// <summary>
        /// Assert that two integers are equal.
        /// </summary>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void Equal(int expected, int actual, string message)
        {
            if (expected != actual)
                throw new AssertionException($"Expected {expected} but got {actual}: {message}");
        }

        /// <summary>
        /// Assert that two strings are equal.
        /// </summary>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void Equal(string expected, string actual, string message)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new AssertionException($"Expected '{expected}' but got '{actual}': {message}");
        }

        /// <summary>
        /// Assert that a reference is not null.
        /// </summary>
        /// <param name="value">Value that must not be null.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void NotNull(object value, string message)
        {
            if (value == null)
                throw new AssertionException("Expected non-null value: " + message);
        }

        /// <summary>
        /// Assert that a string is neither null nor empty.
        /// </summary>
        /// <param name="value">Value that must have content.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void NotEmpty(string value, string message)
        {
            if (string.IsNullOrEmpty(value))
                throw new AssertionException("Expected non-empty string: " + message);
        }

        /// <summary>
        /// Assert that a string starts with the supplied prefix.
        /// </summary>
        /// <param name="prefix">Expected prefix.</param>
        /// <param name="value">Value to inspect.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void StartsWith(string prefix, string value, string message)
        {
            if (value == null || !value.StartsWith(prefix, StringComparison.Ordinal))
                throw new AssertionException($"Expected '{value}' to start with '{prefix}': {message}");
        }

        /// <summary>
        /// Assert that a string has the expected length.
        /// </summary>
        /// <param name="expectedLength">Expected length.</param>
        /// <param name="value">Value to measure.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void Length(int expectedLength, string value, string message)
        {
            if (value == null)
                throw new AssertionException("Expected non-null value when checking length: " + message);
            if (value.Length != expectedLength)
                throw new AssertionException($"Expected length {expectedLength} but got {value.Length} for '{value}': {message}");
        }

        /// <summary>
        /// Assert that every character of a string is contained in the supplied allowed set.
        /// </summary>
        /// <param name="value">Value to inspect.</param>
        /// <param name="allowed">Set of allowed characters.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void AllCharsIn(string value, HashSet<char> allowed, string message)
        {
            if (value == null)
                throw new AssertionException("Expected non-null value when validating characters: " + message);

            foreach (char c in value)
            {
                if (!allowed.Contains(c))
                    throw new AssertionException($"Character '{c}' in '{value}' is not in the allowed set: {message}");
            }
        }

        /// <summary>
        /// Assert that invoking an action throws an exception assignable to the supplied type.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action expected to throw.</param>
        /// <param name="message">Message describing the expectation.</param>
        public static void Throws<TException>(Action action, string message) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new AssertionException(
                    $"Expected {typeof(TException).Name} but got {ex.GetType().Name} ('{ex.Message}'): {message}");
            }

            throw new AssertionException($"Expected {typeof(TException).Name} but no exception was thrown: {message}");
        }
    }
}
