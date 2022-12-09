using dnlib.DotNet;
using NullGuard;
using System;
using System.Collections.Generic;

namespace BitMono.Core.Protecting.Helpers
{
    /// <summary>
    ///     Context of the injection process.
    /// </summary>
    public class InjectContext : ImportMapper
    {
        /// <summary>
        ///     The mapping of origin definitions to injected definitions.
        /// </summary>
        public readonly Dictionary<IMemberRef, IMemberRef> DefMap = new Dictionary<IMemberRef, IMemberRef>();

        /// <summary>
        ///     The module which source type originated from.
        /// </summary>
        public readonly ModuleDef OriginModule;

        /// <summary>
        ///     The module which source type is being injected to.
        /// </summary>
        public readonly ModuleDef TargetModule;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InjectContext" /> class.
        /// </summary>
        /// <param name="module">The origin module.</param>
        /// <param name="target">The target module.</param>
        public InjectContext(ModuleDef module, ModuleDef target)
        {
            OriginModule = module;
            TargetModule = target;
            Importer = new Importer(target, ImporterOptions.TryToUseTypeDefs, new GenericParamContext(), this);
        }

        /// <summary>
        ///     Gets the importer.
        /// </summary>
        /// <value>The importer.</value>
        public Importer Importer { get; }

        /// <inheritdoc />
        [return: AllowNull]
        public override ITypeDefOrRef Map(ITypeDefOrRef source)
        {
            if (DefMap.TryGetValue(source, out var mappedRef))
                return mappedRef as ITypeDefOrRef;

            // check if the assembly reference needs to be fixed.
            if (source is TypeRef sourceRef)
            {
                var targetAssemblyRef = TargetModule.GetAssemblyRef(sourceRef.DefinitionAssembly.Name);
                if (!(targetAssemblyRef is null) && !string.Equals(targetAssemblyRef.FullName, source.DefinitionAssembly.FullName, StringComparison.Ordinal))
                {
                    // We got a matching assembly by the simple name, but not by the full name.
                    // This means the injected code uses a different assembly version than the target assembly.
                    // We'll fix the assembly reference, to avoid breaking anything.
                    var fixedTypeRef = new TypeRefUser(sourceRef.Module, sourceRef.Namespace, sourceRef.Name, targetAssemblyRef);
                    return Importer.Import(fixedTypeRef);
                }
            }
            return null;
        }

        /// <inheritdoc />
        [return: AllowNull]
        public override IMethod Map(MethodDef source)
        {
            if (DefMap.TryGetValue(source, out var mappedRef))
                return mappedRef as IMethod;
            return null;
        }

        /// <inheritdoc />
        [return: AllowNull]
        public override IField Map(FieldDef source)
        {
            if (DefMap.TryGetValue(source, out var mappedRef))
                return mappedRef as IField;
            return null;
        }

        [return: AllowNull]
        public override MemberRef Map(MemberRef source)
        {
            if (DefMap.TryGetValue(source, out var mappedRef))
                return mappedRef as MemberRef;
            return null;
        }
    }
}