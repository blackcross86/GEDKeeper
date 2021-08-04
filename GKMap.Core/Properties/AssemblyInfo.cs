﻿/*
 *  This file is part of the "GKMap".
 *  GKMap project borrowed from GMap.NET (by radioman).
 *
 *  Copyright (C) 2009-2018 by radioman (email@radioman.lt).
 *  This program is licensed under the FLAT EARTH License.
 */

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("GKMap.Core")]
[assembly: AssemblyDescription("GKMap Core")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyProduct("GKMap")]
[assembly: AssemblyCopyright("Copyright © 2009-2018 by radioman")]
[assembly: AssemblyVersion("1.8.0")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]
[assembly: Guid("843e1f67-489b-4454-b451-021e5c526e30")]

[assembly: InternalsVisibleTo("GKMap.WinForms, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cd251b0b8f7079914bbe3e5655d92e5427218f3f0241537a9cb7467b6da2aa5cb20915c31400800e3081d20e6454a35164600fe8bf4f846744f211e040588260cc872c78abd91b422c60071bfda5f11d251eb09f0935944b41de2a28374ad17e8c963d642310df9050e8ae0f1a2b867bcc8f035e4b353dc699cfc7125b9661ce")]
