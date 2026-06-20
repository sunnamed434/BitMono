global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
#if NET6_0_OR_GREATER
global using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
#else
global using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
#endif
global using NullGuard;
global using BitMono.Shared.Extensions;