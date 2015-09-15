//-----------------------------------------------------------------------
// <copyright file="KeySpec.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Input
{
    /// <summary>
    /// KeySpec struct.
    /// </summary>
    internal struct KeySpec
    {
        /// <summary>
        /// Key code.
        /// </summary>
        public ushort KeyCode;

        /// <summary>
        /// Is extended key.
        /// </summary>
        public bool IsExtended;

        /// <summary>
        /// Key name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySpec" /> struct.
        /// </summary>
        /// <param name="keyCode">Key code.</param>
        /// <param name="isExtended">Is extended key.</param>
        /// <param name="name">Key name.</param>
        public KeySpec(ushort keyCode, bool isExtended, string name)
        {
            this.KeyCode = keyCode;
            this.IsExtended = isExtended;
            this.Name = name;
        }
    }
}
