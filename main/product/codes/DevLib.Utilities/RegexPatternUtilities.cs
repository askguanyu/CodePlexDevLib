//-----------------------------------------------------------------------
// <copyright file="RegexPatternUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    /// <summary>
    /// RegexPattern Utilities
    /// </summary>
    public class RegexPatternUtilities
    {
        public const string Alpha = @"^[a-zA-Z]*$";
        public const string AlphaUpperCase = @"^[A-Z]*$";
        public const string AlphaLowerCase = @"^[a-z]*$";
        public const string AlphaNumeric = @"^[a-zA-Z0-9]*$";
        public const string AlphaNumericSpace = @"^[a-zA-Z0-9 ]*$";
        public const string AlphaNumericSpaceDash = @"^[a-zA-Z0-9 \-]*$";
        public const string AlphaNumericSpaceDashUnderscore = @"^[a-zA-Z0-9 \-_]*$";
        public const string AlphaNumericSpaceDashUnderscorePeriod = @"^[a-zA-Z0-9\. \-_]*$";

        public const string Numeric = @"^\-?[0-9]*\.?[0-9]*$";
        public const string Hex = "^[0-9a-fA-F]+$";
        public const string SocialSecurity = @"\d{3}[-]?\d{2}[-]?\d{4}";
        public const string Email = @"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$";
        public const string Url = @"^^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
        public const string ZipCodeUS = @"\d{5}";
        public const string ZipCodeUSWithFour = @"\d{5}[-]\d{4}";
        public const string ZipCodeUSWithFourOptional = @"\d{5}([-]\d{4})?";
        public const string PhoneUS = @"\d{3}[-]?\d{3}[-]?\d{4}";
    }
}
