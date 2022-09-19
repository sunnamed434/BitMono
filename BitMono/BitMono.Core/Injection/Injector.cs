using BitMono.API.Injection;
using BitMono.Core.Extensions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BitMono.Core.Injection
{
    public class Injector : IInjector
    {
        public FieldDef InjectArrayInGlobalType(ModuleDefMD moduleDefMD, byte[] injectedData, string injectedName)
        {
            Importer importer = new Importer(moduleDefMD);
            ITypeDefOrRef valueTypeRef = importer.Import(typeof(ValueType));
            TypeDef classWithLayout = new TypeDefUser("<>c", valueTypeRef);
            classWithLayout.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            classWithLayout.ClassLayout = new ClassLayoutUser(1, (uint)injectedData.Length);
            var compilerGeneratedAttribute = InjectCompilerGeneratedAttribute(moduleDefMD);
            classWithLayout.CustomAttributes.Add(compilerGeneratedAttribute);

            moduleDefMD.GlobalType.NestedTypes.Add(classWithLayout);

            FieldDef fieldWithRVA = new FieldDefUser("dummy", new FieldSig(classWithLayout.ToTypeSig()), FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRVA);
            fieldWithRVA.InitialValue = injectedData;
            classWithLayout.Fields.Add(fieldWithRVA);

            ITypeDefOrRef byteArrayRef = importer.Import(typeof(byte[]));
            FieldDef fieldInjectedArray = new FieldDefUser(injectedName, new FieldSig(byteArrayRef.ToTypeSig()), FieldAttributes.Static | FieldAttributes.Assembly);
            classWithLayout.Fields.Add(fieldInjectedArray);

            /*
              ldc.i4     XXXsizeofarrayXXX
              newarr     [mscorlib]System.Byte
              dup
              ldtoken    field valuetype className fieldName
              call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array, valuetype [mscorlib]System.RuntimeFieldHandle)
              stsfld     uint8[] bla
             */
            ITypeDefOrRef systemByte = importer.Import(typeof(byte));
            IMethod initArray = importer.Import(typeof(RuntimeHelpers).GetMethod("InitializeArray", new Type[]
            {
                typeof(Array),
                typeof(RuntimeFieldHandle)
            }));

            MethodDef cctor = classWithLayout.FindOrCreateStaticConstructor();
            IList<Instruction> instrs = cctor.Body.Instructions;
            instrs.Insert(0, new Instruction(OpCodes.Ldc_I4, injectedData.Length));
            instrs.Insert(1, new Instruction(OpCodes.Newarr, systemByte));
            instrs.Insert(2, new Instruction(OpCodes.Dup));
            instrs.Insert(3, new Instruction(OpCodes.Ldtoken, fieldWithRVA));
            instrs.Insert(4, new Instruction(OpCodes.Call, initArray));
            instrs.Insert(5, new Instruction(OpCodes.Stsfld, fieldInjectedArray));

            cctor.Body.SimplifyAndOptimizeBranches();

            return fieldInjectedArray;
        }
        public TypeDef CreateInvisibleType(ModuleDefMD moduleDefMD, string name = null)
        {
            var inivislbeTypeDef = new TypeDefUser(name ?? "<PrivateImplementationDetails>");
            InjectCompilerGeneratedAttribute(moduleDefMD, inivislbeTypeDef);
            inivislbeTypeDef.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            inivislbeTypeDef.BaseType = moduleDefMD.CorLibTypes.Object.ToTypeDefOrRef();
            return inivislbeTypeDef;
        }
        public CustomAttribute InjectCompilerGeneratedAttribute(ModuleDefMD moduleDefMD, TypeDef typeDef = null)
        {
            TypeRef compilerGeneratedAttributeType = moduleDefMD.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", nameof(CompilerGeneratedAttribute));
            MemberRefUser compilerGeneratedCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void), compilerGeneratedAttributeType);
            var compilerGeneratedAttribute = new CustomAttribute(compilerGeneratedCtor);
            if (typeDef != null)
            {
                typeDef.CustomAttributes.Add(compilerGeneratedAttribute);
            }
            return compilerGeneratedAttribute;
        }
        public void InjectAttributeWithContent(ModuleDefMD moduleDefMD, string @namespace, string @name, string text)
        {
            TypeRef attributeRef = moduleDefMD.CorLibTypes.GetTypeRef(@namespace, @name);

            MemberRefUser attributeCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void, moduleDefMD.CorLibTypes.String), attributeRef);
            CustomAttribute customAttribute = new CustomAttribute(attributeCtor);
            customAttribute.ConstructorArguments.Add(new CAArgument(moduleDefMD.CorLibTypes.String, text));
            moduleDefMD.CustomAttributes.Add(customAttribute);
        }
        public void InjectAttribute(ModuleDefMD moduleDefMD, string @namespace, string @name)
        {
            TypeRef attributeRef = moduleDefMD.CorLibTypes.GetTypeRef(@namespace, @name);

            MemberRefUser attributeCtor = new MemberRefUser(moduleDefMD, ".ctor", MethodSig.CreateInstance(moduleDefMD.CorLibTypes.Void), attributeRef);
            moduleDefMD.CustomAttributes.Add(new CustomAttribute(attributeCtor));
        }
    }
}