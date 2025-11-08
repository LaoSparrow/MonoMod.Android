using System;
using System.Diagnostics.CodeAnalysis;
using static MonoMod.Core.Interop.CoreCLR;

namespace MonoMod.Core.Platforms.Runtimes
{
    [SuppressMessage("Performance", "CA1852", Justification = "This type will be derived for .NET 11.")]
    internal class Core100Runtime : Core90Runtime
    {
        public Core100Runtime(ISystem system, IArchitecture arch) : base(system, arch) { }
        
        // src/coreclr/inc/jiteeversionguid.h line 46
        // 7a8cbc56-9e19-4321-80b9-a0d2c578c945
        private static readonly Guid JitVersionGuid = new(
            0x7a8cbc56,
            0x9e19,
            0x4321,
            0x80, 0xb9, 0xa0, 0xd2, 0xc5, 0x78, 0xc9, 0x45
        );
        
        protected override Guid ExpectedJitVersion => JitVersionGuid;

        protected override int VtableIndexICorJitInfoAllocMem => V100.ICorJitInfoVtable.AllocMemIndex;
        protected override int ICorJitInfoFullVtableCount => V100.ICorJitInfoVtable.TotalVtableCount;
    }
}