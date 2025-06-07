using Mono.Cecil.Cil;
using MonoMod.Core.Platforms;
using MonoMod.Utils;
using System;
using Xunit;
using Xunit.Abstractions;


namespace MonoMod.UnitTest
{
    public unsafe sealed class CalliTests : TestBase
    {
        public CalliTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public void CalliS()
        {
            if (PlatformDetection.Runtime is RuntimeKind.Mono)
            {
                LightCalli<random_struct>();
            }
            else
            {
                Calli<random_struct>();
            }
        }
        [Fact]
        public void CalliO()
        {
            if (PlatformDetection.Runtime is RuntimeKind.Mono)
            {
                LightCalli<random_class>();
            }
            else
            {
                Calli<random_class>();
            }
        }
        delegate T Helper<T>(T o, ref T r, T[] a, T* p) where T : new();
        public static unsafe T Ret<T>(T o, ref T r, T[] a, T* p) where T : new()
        {
            Assert.Equal(typeof(T), o.GetType());
            Assert.Equal(typeof(T[]), a.GetType());
            Assert.Equal(r, *p);
            r = new T();
            Assert.Equal(r, *p);
            return o;
        }
        unsafe void Calli<T>() where T : new()
        {
            T obj = new();
            Ret(obj, ref obj, [obj], (T*)Unsafe.AsPointer(ref obj));

            Type type = typeof(T);
            var method = ((Helper<T>)Ret).Method;
            using var pin = PlatformTriple.Current.PinMethodIfNeeded(method);
            var i = method.MethodHandle.GetFunctionPointer();
            using DynamicMethodDefinition dmd = new("a", null, [typeof(nint), type]);
            var il = dmd.GetILProcessor();
            var c = new Mono.Cecil.CallSite(dmd.Module.ImportReference(type));
            c.Parameters.Add(new(dmd.Module.ImportReference(type)));
            c.Parameters.Add(new(dmd.Module.ImportReference(type.MakeByRefType())));
            c.Parameters.Add(new(dmd.Module.ImportReference(type.MakeArrayType())));
            c.Parameters.Add(new(dmd.Module.ImportReference(type.MakePointerType())));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarga, 1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newarr, type);
            il.Emit(OpCodes.Ldarga, 1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Calli, c);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);
            //Switches.SetSwitchValue(Switches.DMDType, "cecil");
            var ret = dmd.Generate();
            PlatformTriple.Current.Compile(ret);
            ret.Invoke(null, [i, Activator.CreateInstance(type, [32])]);
        }
        static void Light(object u)
        {
            throw new NotImplementedException("MonoMod.Test.Intended" + u.ToString());
        }
        unsafe void LightCalli<T>() where T : new()
        {

            Type type = typeof(T);
            var method = ((Delegate)Light).Method;
            using var pin = PlatformTriple.Current.PinMethodIfNeeded(method);
            var i = PlatformTriple.Current.GetNativeMethodBody(method);
            using DynamicMethodDefinition dmd = new("a", null, [typeof(nint), type]);
            var il = dmd.GetILProcessor();
            var c = new Mono.Cecil.CallSite(dmd.Module.TypeSystem.Void);
            c.Parameters.Add(new(dmd.Module.ImportReference(type.MakeArrayType())));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newarr, type);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Calli, c);
            il.Emit(OpCodes.Ret);
            //Switches.SetSwitchValue(Switches.DMDType, "cecil");
            var ret = dmd.Generate();
            PlatformTriple.Current.Compile(ret);
            try
            {
                Extensions.CreateDelegate<Action<nint, T>>(ret)(i, (T)Activator.CreateInstance(type, [32]));
            }
            catch (NotImplementedException ex)
            {
                if (ex.Message.StartsWith("MonoMod.Test.Intended", StringComparison.InvariantCulture))
                {
                    return;
                }
            }
            Assert.Fail("should be unreachable");
        }
        public struct random_struct(int v)
        {
            int u = v;
            public random_struct() : this(0)
            {

            }
        }
        public class random_class(int v)
        {
            int u = v;
            public random_class() : this(0)
            {
            }
        }
    }
}
