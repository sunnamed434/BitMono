global using AsmResolver;
global using AsmResolver.DotNet;
global using AsmResolver.DotNet.Cloning;
global using AsmResolver.DotNet.Signatures;
global using AsmResolver.PE.DotNet.Cil;
global using BitMono.API.Configuration;
global using BitMono.API.Protecting;
global using BitMono.API.Protecting.Analyzing;
global using BitMono.API.Protecting.Contexts;
global using BitMono.API.Protecting.Pipeline;
global using BitMono.API.Protecting.Renaming;
global using BitMono.API.Protecting.Resolvers;
global using BitMono.Core.Extensions;
global using BitMono.Core.Protecting.Analyzing;
global using BitMono.Core.Protecting.Attributes;
global using BitMono.Core.Protecting.Injection;
global using BitMono.Shared.Models;
global using BitMono.Utilities.Extensions.AsmResolver;
global using Microsoft.Extensions.Configuration;
global using MoreLinq;
global using Newtonsoft.Json;
global using NullGuard;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Threading.Tasks;
global using System.Xml.Serialization;
global using FieldAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.FieldAttributes;
global using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.TypeAttributes;