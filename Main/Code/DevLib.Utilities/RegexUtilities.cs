//-----------------------------------------------------------------------
// <copyright file="RegexUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    /// <summary>
    /// Regex Utilities.
    /// </summary>
    public static class RegexUtilities
    {
        /// <summary>
        /// Const Field Alpha.
        /// </summary>
        public const string Alpha = @"^[a-zA-Z]*$";

        /// <summary>
        /// Const Field AlphaUpperCase.
        /// </summary>
        public const string AlphaUpperCase = @"^[A-Z]*$";

        /// <summary>
        /// Const Field AlphaLowerCase.
        /// </summary>
        public const string AlphaLowerCase = @"^[a-z]*$";

        /// <summary>
        /// Const Field AlphaNumeric.
        /// </summary>
        public const string AlphaNumeric = @"^[a-zA-Z0-9]*$";

        /// <summary>
        /// Const Field AlphaNumericSpace.
        /// </summary>
        public const string AlphaNumericSpace = @"^[a-zA-Z0-9 ]*$";

        /// <summary>
        /// Const Field AlphaNumericSpaceDash.
        /// </summary>
        public const string AlphaNumericSpaceDash = @"^[a-zA-Z0-9 \-]*$";

        /// <summary>
        /// Const Field AlphaNumericSpaceDashUnderscore.
        /// </summary>
        public const string AlphaNumericSpaceDashUnderscore = @"^[a-zA-Z0-9 \-_]*$";

        /// <summary>
        /// Const Field AlphaNumericSpaceDashUnderscorePeriod.
        /// </summary>
        public const string AlphaNumericSpaceDashUnderscorePeriod = @"^[a-zA-Z0-9\. \-_]*$";

        /// <summary>
        /// Const Field Numeric.
        /// </summary>
        public const string Numeric = @"^\-?[0-9]*\.?[0-9]*$";

        /// <summary>
        /// Const Field Hex.
        /// </summary>
        public const string Hex = "^[0-9a-fA-F]+$";

        /// <summary>
        /// Const Field SocialSecurity.
        /// </summary>
        public const string SocialSecurity = @"\d{3}[-]?\d{2}[-]?\d{4}";

        /// <summary>
        /// Const Field Email.
        /// </summary>
        public const string Email = @"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$";

        /// <summary>
        /// Const Field Url.
        /// </summary>
        public const string Url = @"^^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";

        /// <summary>
        /// Const Field ZipCodeUS.
        /// </summary>
        public const string ZipCodeUS = @"\d{5}";

        /// <summary>
        /// Const Field ZipCodeUSWithFour.
        /// </summary>
        public const string ZipCodeUSWithFour = @"\d{5}[-]\d{4}";

        /// <summary>
        /// Const Field ZipCodeUSWithFourOptional.
        /// </summary>
        public const string ZipCodeUSWithFourOptional = @"\d{5}([-]\d{4})?";

        /// <summary>
        /// Const Field PhoneUS.
        /// </summary>
        public const string PhoneUS = @"\d{3}[-]?\d{3}[-]?\d{4}";
    }
}
