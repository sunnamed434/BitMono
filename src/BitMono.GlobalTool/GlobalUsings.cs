﻿global using Autofac;
global using BitMono.CLI.Modules;
global using BitMono.Host;
global using BitMono.CLI;
global using BitMono.Host.Modules;
global using BitMono.Obfuscation;
global using System;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Threading;
global using System.Threading.Tasks;
global using BitMono.Host.Extensions;
global using BitMono.Obfuscation.Files;
global using BitMono.Obfuscation.Starter;
global using BitMono.Shared.Models;
global using BitMono.Utilities.Paths;
global using CommandLine;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Pocket.Extensions;
global using Serilog;
global using Serilog.Configuration;
global using ILogger = Serilog.ILogger;