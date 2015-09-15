//-----------------------------------------------------------------------
// <copyright file="ZipVersionNeededValues.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    internal enum ZipVersionNeededValues : ushort
    {
        Default = (ushort)10,
        Deflate = (ushort)20,
        ExplicitDirectory = (ushort)20,
        Zip64 = (ushort)45,
    }
}
