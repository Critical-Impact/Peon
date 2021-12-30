using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using Peon.SeFunctions;
using Peon.Utility;

namespace Peon
{
    public static unsafe class HookManagerExtension
    {
        private delegate IntPtr Delegate3Ptr_Ptr(IntPtr a1, IntPtr a2, IntPtr a3);
        private delegate void   Delegate3PtrInt(IntPtr a1, IntPtr a2, IntPtr a3, int a4);
        private delegate IntPtr Delegate2Ptr_Ptr(IntPtr a1, IntPtr a2);
        private delegate IntPtr Delegate2PtrInt_Ptr(IntPtr a1, IntPtr a2, uint a3);
        private delegate IntPtr DelegatePtrInt_Ptr(IntPtr a1, uint a2);
        
        private delegate IntPtr Delegate4Ptr_Ptr(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);
        private delegate IntPtr DelegatePtr_Ptr(IntPtr a1);
        private delegate IntPtr Delegate_Ptr();
        private delegate IntPtr DelegatePtrByte_Ptr(IntPtr a1, byte a2);
        private delegate void   DelegatePtr(IntPtr a1);
        private delegate void   Delegate2Ptr(IntPtr a1, IntPtr a2);
        private delegate void   DelegatePtrByte(IntPtr a1, byte a2);
        private delegate void   DelegatePtrShort(IntPtr a1, ushort a2);
        private delegate void   Constructor(IntPtr a1);

        public static void SetHooks(this HookManager hooks)
        {
        }

        private static unsafe bool IncRefCondition(IntPtr a1, IntPtr a2, uint a3)
            => ((ResourceHandle*) a1)->FileName.ToString().EndsWith(".cmp");

        private static void FindResourcePost(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4)
        {
            var ext = $"{(char)*((byte*)a2 + 0x3)}{(char)*((byte*)a2 + 0x2)}{(char)*((byte*)a2 + 0x1)}";
            PluginLog.Information($"{*(uint*) a1} - {*(uint*)a3} - {ext}: {a4:X16}");
        }

        private static bool ResourceUnloadCondition(IntPtr a1, IntPtr a2)
        {
            var counter = *(uint*) (a2 + 0xAC);
            if (counter > 1)
                return false;

            var ext    = $"{(char) *((byte*) a2 + 0xE)}{(char) *((byte*) a2 + 0xD)}{(char) *((byte*) a2 + 0xC)}";
            var length = *(uint*) (a2 + 0x58);
            var file   = length > 15 ? Marshal.PtrToStringAnsi(*(IntPtr*) (a2 + 0x48)) : Marshal.PtrToStringAnsi(a2 + 0x48);
            PluginLog.Information($"Resource Unload Counter {counter} {ext} {length} {file}");
            return true;
        }

        private static bool EventDataCondition(IntPtr ptr, ushort eventType, int which, IntPtr source, IntPtr value)
            => eventType == 3;

        private static OnAddonReceiveEventDelegate ReceiveEventData = (_, _, _, data, eventInfo) =>
        {
            PluginLog.Information(
                $"Helper: [0] = {*(ulong*) (data + 0):X}, [1] = {*(IntPtr*) (data + 8):X}, [2] = {*(IntPtr*) (data + 0x10):X}, [3] = {*(ulong*) (data + 0x18):X}, [4] = {*(ulong*) (data + 0x20):X}, [5] = {*(ulong*) (data + 0x28):X}, [6] = {*(IntPtr*) (data + 0x30):X}, [7] = {*(IntPtr*) (data + 0x38):X}");
            PluginLog.Information(
                $"Helper: [0] = {*(ulong*)(eventInfo + 0):X}, [1] = {*(IntPtr*)(eventInfo + 8):X}, [2] = {*(IntPtr*)(eventInfo + 0x10):X}");
        };
    }
}
