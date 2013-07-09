//-----------------------------------------------------------------------
// <copyright file="Action.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    internal delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
}
