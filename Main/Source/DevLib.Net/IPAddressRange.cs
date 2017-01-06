//-----------------------------------------------------------------------
// <copyright file="IPAddressRange.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides an Internet Protocol (IP) address range.
    /// </summary>
    [Serializable]
    public class IPAddressRange : ISerializable, IEnumerable<IPAddress>
    {
        /// <summary>
        /// CIDR range: "192.168.0.0/12", "fe80::/10"
        /// </summary>
        private static readonly Regex RegexCidrRange = new Regex(@"^(?<adr>[\da-f\.:]+)/(?<maskLen>\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Single address: "127.0.0.1", ":;1"
        /// </summary>
        private static readonly Regex RegexSingleAddress = new Regex(@"^(?<adr>[\da-f\.:]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Begin to end range: "169.254.0.0-169.254.0.255"
        /// </summary>
        private static readonly Regex RegexAddressRange = new Regex(@"^(?<begin>[\da-f\.:]+)[\-–](?<end>[\da-f\.:]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Bit mask range: "192.168.0.0/255.255.255.0"
        /// </summary>
        private static readonly Regex RegexBitMaskRange = new Regex(@"^(?<adr>[\da-f\.:]+)/(?<bitmask>[\da-f\.:]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// Creates an empty range object, equivalent to "0.0.0.0/0".
        /// </summary>
        public IPAddressRange()
            : this(new IPAddress(0L))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// Creates a new range with the same start/end address (range of one)
        /// </summary>
        /// <param name="singleAddress">The single address.</param>
        public IPAddressRange(IPAddress singleAddress)
        {
            if (singleAddress == null)
            {
                throw new ArgumentNullException("singleAddress");
            }

            this.Begin = this.End = singleAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// Create a new range from a begin and end address. Throws an exception if Begin comes after End, or the addresses are not in the same family.
        /// </summary>
        /// <param name="begin">The begin IPAddress.</param>
        /// <param name="end">The end IPAddress.</param>
        public IPAddressRange(IPAddress begin, IPAddress end)
        {
            if (begin == null)
            {
                throw new ArgumentNullException("begin");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            if (begin.AddressFamily != end.AddressFamily)
            {
                throw new ArgumentException("IPAddress must be of the same address family", "end");
            }

            var beginBytes = begin.GetAddressBytes();
            var endBytes = end.GetAddressBytes();

            if (!Bitwise.LessEqual(endBytes, beginBytes))
            {
                throw new ArgumentException("Begin must be smaller than the End", "begin");
            }

            this.Begin = begin;
            this.End = end;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// Creates a range from a base address and mask bits. This can also be used with <see cref="GetSubnetMaskLength"/> to create a range based on a subnet mask.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="maskLength">Length of the mask.</param>
        public IPAddressRange(IPAddress baseAddress, int maskLength)
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException("baseAddress");
            }

            var baseAdrBytes = baseAddress.GetAddressBytes();

            if (baseAdrBytes.Length * 8 < maskLength)
            {
                throw new FormatException();
            }

            var maskBytes = Bitwise.GetBitMask(baseAdrBytes.Length, maskLength);

            baseAdrBytes = Bitwise.And(baseAdrBytes, maskBytes);

            this.Begin = new IPAddress(baseAdrBytes);
            this.End = new IPAddress(Bitwise.Or(baseAdrBytes, Bitwise.Not(maskBytes)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// </summary>
        /// <param name="ipRangeString">The ip range string.</param>
        public IPAddressRange(string ipRangeString)
        {
            var result = Parse(ipRangeString);
            this.Begin = result.Begin;
            this.End = result.End;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
        protected IPAddressRange(SerializationInfo info, StreamingContext context)
        {
            var names = new List<string>();

            foreach (var item in info)
            {
                names.Add(item.Name);
            }

            this.Begin = this.DeserializeNode(info, names, "Begin");
            this.End = this.DeserializeNode(info, names, "End");
        }

        /// <summary>
        /// Gets or sets the begin IPAddress.
        /// </summary>
        public IPAddress Begin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the end IPAddress.
        /// </summary>
        public IPAddress End
        {
            get;
            set;
        }

        /// <summary>
        /// Parses the specified ip range string.
        /// </summary>
        /// <param name="ipRangeString">The ip range string.</param>
        /// <returns>IPAddressRange instance.</returns>
        public static IPAddressRange Parse(string ipRangeString)
        {
            if (ipRangeString == null)
            {
                throw new ArgumentNullException("ipRangeString");
            }

            ipRangeString = ipRangeString.Replace(" ", string.Empty);

            var matchCidrRange = RegexCidrRange.Match(ipRangeString);

            if (matchCidrRange.Success)
            {
                var baseAdrBytes = IPAddress.Parse(matchCidrRange.Groups["adr"].Value).GetAddressBytes();
                var maskLength = int.Parse(matchCidrRange.Groups["maskLen"].Value);

                if (baseAdrBytes.Length * 8 < maskLength)
                {
                    throw new FormatException();
                }

                var maskBytes = Bitwise.GetBitMask(baseAdrBytes.Length, maskLength);
                baseAdrBytes = Bitwise.And(baseAdrBytes, maskBytes);

                return new IPAddressRange(new IPAddress(baseAdrBytes), new IPAddress(Bitwise.Or(baseAdrBytes, Bitwise.Not(maskBytes))));
            }

            var matchSingleAddress = RegexSingleAddress.Match(ipRangeString);

            if (matchSingleAddress.Success)
            {
                return new IPAddressRange(IPAddress.Parse(ipRangeString));
            }

            var matchAddressRange = RegexAddressRange.Match(ipRangeString);

            if (matchAddressRange.Success)
            {
                return new IPAddressRange(IPAddress.Parse(matchAddressRange.Groups["begin"].Value), IPAddress.Parse(matchAddressRange.Groups["end"].Value));
            }

            var matchBitMaskRange = RegexBitMaskRange.Match(ipRangeString);

            if (matchBitMaskRange.Success)
            {
                var baseAdrBytes = IPAddress.Parse(matchBitMaskRange.Groups["adr"].Value).GetAddressBytes();
                var maskBytes = IPAddress.Parse(matchBitMaskRange.Groups["bitmask"].Value).GetAddressBytes();
                baseAdrBytes = Bitwise.And(baseAdrBytes, maskBytes);

                return new IPAddressRange(new IPAddress(baseAdrBytes), new IPAddress(Bitwise.Or(baseAdrBytes, Bitwise.Not(maskBytes))));
            }

            throw new FormatException("Unknown IP range string.");
        }

        /// <summary>
        /// Try to parses the specified ip range string.
        /// </summary>
        /// <param name="ipRangeString">The ip range string.</param>
        /// <param name="ipAddressRange">The IPAddressRange object.</param>
        /// <returns>true if parse succeeded; otherwise, false.</returns>
        public static bool TryParse(string ipRangeString, out IPAddressRange ipAddressRange)
        {
            try
            {
                ipAddressRange = IPAddressRange.Parse(ipRangeString);
                return true;
            }
            catch
            {
                ipAddressRange = null;
                return false;
            }
        }

        /// <summary>
        /// Takes a subnet mask (eg, "255.255.254.0") and returns the CIDR bit length of that address. Throws an exception if the passed address is not valid as a subnet mask.
        /// </summary>
        /// <param name="subnetMask">The subnet mask to use.</param>
        /// <returns>The subnet mask length.</returns>
        public static int GetSubnetMaskLength(IPAddress subnetMask)
        {
            if (subnetMask == null)
            {
                throw new ArgumentNullException("subnetMask");
            }

            var length = Bitwise.GetBitMaskLength(subnetMask.GetAddressBytes());

            if (length == null)
            {
                throw new ArgumentException("It is not a valid subnet mask.", "subnetMask");
            }

            return length.Value;
        }

        /// <summary>
        /// Determines whether the current IPAddressRange contains the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns>true if the current IPAddressRange contains the specified ip address; otherwise, false.</returns>
        public bool Contains(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException("ipAddress");
            }

            if (ipAddress.AddressFamily != this.Begin.AddressFamily)
            {
                return false;
            }

            var adrBytes = ipAddress.GetAddressBytes();

            return
                Bitwise.GreaterEqual(this.Begin.GetAddressBytes(), adrBytes)
                && Bitwise.LessEqual(this.End.GetAddressBytes(), adrBytes);
        }

        /// <summary>
        /// Determines whether the current IPAddressRange contains the specified ip address.
        /// </summary>
        /// <param name="ipAddressString">The ip address.</param>
        /// <returns>true if the current IPAddressRange contains the specified ip address; otherwise, false.</returns>
        public bool Contains(string ipAddressString)
        {
            if (string.IsNullOrEmpty(ipAddressString))
            {
                throw new ArgumentNullException("ipAddressString");
            }

            var ipAddress = IPAddress.Parse(ipAddressString);

            if (ipAddress.AddressFamily != this.Begin.AddressFamily)
            {
                return false;
            }

            var adrBytes = ipAddress.GetAddressBytes();

            return
                Bitwise.GreaterEqual(this.Begin.GetAddressBytes(), adrBytes)
                && Bitwise.LessEqual(this.End.GetAddressBytes(), adrBytes);
        }

        /// <summary>
        /// Determines whether the current IPAddressRange contains the specified ip address range.
        /// </summary>
        /// <param name="ipAddressRange">The ip address range.</param>
        /// <returns>true if the current IPAddressRange contains the specified ip address range; otherwise, false.</returns>
        public bool Contains(IPAddressRange ipAddressRange)
        {
            if (ipAddressRange == null)
            {
                throw new ArgumentNullException("ipAddressRange");
            }

            if (this.Begin.AddressFamily != ipAddressRange.Begin.AddressFamily)
            {
                return false;
            }

            return
                Bitwise.GreaterEqual(this.Begin.GetAddressBytes(), ipAddressRange.Begin.GetAddressBytes())
                && Bitwise.LessEqual(this.End.GetAddressBytes(), ipAddressRange.End.GetAddressBytes());
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Begin", this.Begin != null ? this.Begin.ToString() : string.Empty);
            info.AddValue("End", this.End != null ? this.End.ToString() : string.Empty);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IPAddress> GetEnumerator()
        {
            var first = this.Begin.GetAddressBytes();
            var last = this.End.GetAddressBytes();

            for (var ip = first; Bitwise.GreaterEqual(ip, last); ip = Bitwise.Increment(ip))
            {
                yield return new IPAddress(ip);
            }
        }

        /// <summary>
        /// Returns the range in the format "begin-end", or as a single address if End is the same as Begin.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Begin.Equals(this.End) ? this.Begin.ToString() : string.Format("{0}-{1}", this.Begin, this.End);
        }

        /// <summary>
        /// Gets the length of the prefix.
        /// </summary>
        /// <returns>The length of the prefix.</returns>
        public int GetPrefixLength()
        {
            byte[] byteBegin = this.Begin.GetAddressBytes();
            byte[] byteEnd = this.End.GetAddressBytes();

            if (this.Begin.Equals(this.End))
            {
                return byteBegin.Length * 8;
            }

            int length = byteBegin.Length * 8;

            for (int i = 0; i < length; i++)
            {
                byte[] mask = Bitwise.GetBitMask(byteBegin.Length, i);

                if (new IPAddress(Bitwise.And(byteBegin, mask)).Equals(this.Begin))
                {
                    if (new IPAddress(Bitwise.Or(byteBegin, Bitwise.Not(mask))).Equals(this.End))
                    {
                        return i;
                    }
                }
            }

            throw new FormatException(string.Format("{0} is not a CIDR Subnet", this.ToString()));
        }

        /// <summary>
        /// Returns a CIDR string if this matches exactly a CIDR subnet.
        /// </summary>
        /// <returns>The CIDR string.</returns>
        public string ToCidrString()
        {
            return string.Format("{0}/{1}", this.Begin, this.GetPrefixLength());
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Deserializes the node.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="names">The names.</param>
        /// <param name="name">The name.</param>
        /// <returns>IPAddress instance.</returns>
        private IPAddress DeserializeNode(SerializationInfo info, List<string> names, string name)
        {
            return names.Contains(name)
                ? IPAddress.Parse(info.GetValue(name, typeof(object)).ToString())
                : new IPAddress(0L);
        }
    }
}
