using BitMono.API.Protecting.Injection;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Runtime.CompilerServices;

namespace BitMono.Core.Protecting.Injection
{
    public class Injector : IInjector
    {
        public FieldDef InjectInvisibleArray(ModuleDefMD moduleDefMD, TypeDef typeDef, byte[] data, string name)
        {
            var importer = new Importer(moduleDefMD);
            var valueTypeRef = importer.Import(typeof(ValueType));
            var classWithLayout = new TypeDefUser("<>c", valueTypeRef);
            classWithLayout.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            classWithLayout.ClassLayout = new ClassLayoutUser(1, (uint)data.Length);
            var compilerGeneratedAttribute = InjectCompilerGeneratedAttribute(moduleDefMD);
            classWithLayout.CustomAttributes.Add(compilerGeneratedAttribute);

            typeDef.NestedTypes.Add(classWithLayout);

            var fieldWithRVA = new FieldDefUser("dummy", new FieldSig(classWithLayout.ToTypeSig()), FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRVA);
            fieldWithRVA.InitialValue = data;
            classWithLayout.Fields.Add(fieldWithRVA);

            var byteArrayRef = importer.Import(typeof(byte[]));
            var fieldInjectedArray = new FieldDefUser(name, new FieldSig(byteArrayRef.ToTypeSig()), FieldAttributes.Static | FieldAttributes.Assembly);
            classWithLayout.Fields.Add(fieldInjectedArray);

            var systemByte = importer.Import(typeof(byte));
            var initializeArrayMethod = importer.Import(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
            {
                typeof(Array),
                typeof(RuntimeFieldHandle)
            }));

            var cctor = classWithLayout.FindOrCreateStaticConstructor();
            var cctorBodyInstructions = cctor.Body.Instructions;
            cctorBodyInstructions.Insert(0, new Instruction(OpCodes.Ldc_I4, data.Length));
            cctorBodyInstructions.Insert(1, new Instruction(OpCodes.Newarr, systemByte));
            cctorBodyInstructions.Insert(2, new Instruction(OpCodes.Dup));
            cctorBodyInstructions.Insert(3, new Instruction(OpCodes.Ldtoken, fieldWithRVA));
            cctorBodyInstructions.Insert(4, new Instruction(OpCodes.Call, initializeArrayMethod));
            cctorBodyInstructions.Insert(5, new Instruction(OpCodes.Stsfld, fieldInjectedArray));

            cctor.Body.SimplifyAndOptimizeBranches();
            return fieldInjectedArray;
        }
        public TypeDef CreateInvisibleType(ModuleDefMD moduleDefMD, string name = null)
        {
            var invislbeTypeDef = new TypeDefUser(name ?? "<PrivateImplementationDetails>", moduleDefMD.CorLibTypes.Object.ToTypeDefOrRef());
            InjectCompilerGeneratedAttribute(moduleDefMD, invislbeTypeDef);
            invislbeTypeDef.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            return invislbeTypeDef;
        }
        public TypeDef CreateInvisibleValueType(ModuleDefMD moduleDefMD, string name = null)
        {
            var importer = new Importer(moduleDefMD);
            var invislbeTypeDef = new TypeDefUser(name ?? "<PrivateImplementationDetails>", importer.Import(typeof(ValueType)));
            InjectCompilerGeneratedAttribute(moduleDefMD, invislbeTypeDef);
            invislbeTypeDef.IsAbstract = false;
            invislbeTypeDef.IsSealed = false;
            invislbeTypeDef.IsBeforeFieldInit = false;
            return invislbeTypeDef;
        }
        public TypeDef InjectInvisibleValueType(ModuleDefMD moduleDefMD, TypeDef typeDef, string name = null)
        {
            var result = CreateInvisibleValueType(moduleDefMD, name);
            typeDef.NestedTypes.Add(result);
            return result;
        }
        public CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefMD moduleDefMD, TypeDef typeDef = null)
        {
            var compilerGeneratedAttributeType = moduleDefMD.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", nameof(CompilerGeneratedAttribute));
            var compilerGeneratedCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void), compilerGeneratedAttributeType);
            var compilerGeneratedAttribute = new CustomAttribute(compilerGeneratedCtor);
            if (typeDef != null)
            {
                typeDef.CustomAttributes.Add(compilerGeneratedAttribute);
            }
            return compilerGeneratedAttribute;
        }
        public CustomAttribute InjectAttributeWithContent(ModuleDefMD moduleDefMD, string @namespace, string @name, string text)
        {
            var attributeRef = moduleDefMD.CorLibTypes.GetTypeRef(@namespace, @name);
            var attributeCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void, moduleDefMD.CorLibTypes.String), attributeRef);
            var customAttribute = new CustomAttribute(attributeCtor);
            customAttribute.ConstructorArguments.Add(new CAArgument(moduleDefMD.CorLibTypes.String, text));
            moduleDefMD.CustomAttributes.Add(customAttribute);
            return customAttribute;
        }
        public CustomAttribute InjectAttribute(ModuleDefMD moduleDefMD, string @namespace, string @name)
        {
            var attributeRef = moduleDefMD.CorLibTypes.GetTypeRef(@namespace, @name);
            var attributeCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void), attributeRef);
            var customAttribute = new CustomAttribute(attributeCtor);
            moduleDefMD.CustomAttributes.Add(customAttribute);
            return customAttribute;
        }
    }
}