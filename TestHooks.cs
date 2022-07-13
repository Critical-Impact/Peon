using System;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using ImGuiNET;
using Peon.SeFunctions;
using Peon.Utility;

namespace Peon;

public static unsafe class HookManagerExtension
{
    private delegate IntPtr Delegate3Ptr_Ptr(IntPtr a1, IntPtr a2, IntPtr a3);
    private delegate void   Delegate3PtrInt(IntPtr a1, IntPtr a2, IntPtr a3, int a4);
    private delegate void   Delegate3Ptr(IntPtr a1, IntPtr a2, IntPtr a3);
    private delegate void   Delegate2PtrLong(IntPtr a1, IntPtr a2, ulong a3);
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
    private delegate ulong  CRC(IntPtr chars);

    private delegate void DelegateFishing(IntPtr a1, int a2, byte a3, ushort a4, byte a5, byte a6, byte a7, byte a8, byte a9, byte a10,
        byte a11, byte a12);

    private delegate void GmpDelegate(IntPtr a1, IntPtr a2, float a3, IntPtr a4, ushort a5, byte a6);

    private delegate bool ActionDelegate(ulong actionType, byte type, ulong actionID, ulong targetID, ulong a4, uint a5, ulong a6, IntPtr a7, uint a8, uint a9, uint a10, IntPtr a11);
    private delegate IntPtr Test(IntPtr a1, IntPtr a2, IntPtr a3, uint a4);

    private delegate IntPtr CreateCharacterBase(uint a1, IntPtr a2, IntPtr a3, byte a4);
    private delegate void   PapTest(IntPtr a1, IntPtr a2, IntPtr a3, uint a4, uint a5, IntPtr a6, IntPtr a7);
    public static void SetHooks(this HookManager hooks)
    {
        //hooks.Create<PapTest>("pap1", 0xd9fe60, true, null, ReceiveEventData);
    }


    private static unsafe void PrintFashionUpdate(IntPtr a1, IntPtr a2, ulong _)
    {
        var s = string.Empty;
        for (var i = 0; i < 8; ++i)
            s += $"{*(byte*)(a2 + i)} ";
        PluginLog.Information(s);
    }

    private static unsafe void PrintFashionUpdatePacket(IntPtr a1)
    {
        var s = string.Empty;
        for (var i = 0; i < 32; ++i)
            s += $"{*(byte*)(a1 + i)} ";
        PluginLog.Information(s);
    }

    private static unsafe void PrintSpearInfo(IntPtr a1, IntPtr a2, IntPtr a3)
    {
        foreach (var offset in new[]
                 {
                     0x28C,
                     0x2A8,
                     0x2C4,
                 })
        {
            var s = string.Empty;
            for (var i = 0; i < 12; ++i)
                s += i switch
                {
                    1 => $"{(*(byte*)(a1 + offset + i) == 0 ? "NQ" : "HQ")}",
                    2 => $"{*(byte*)(a1 + offset + i) switch { 1 => "Small", 2 => "Average", 3 => "Large", _ => "Unknown" }} ",
                    _ => $"{*(byte*)(a1 + offset + i)} ",
                };
            PluginLog.Information($"Row {offset:x}: {s}");
        }
    }

    private static unsafe void PrintCRC(IntPtr ptr, ulong ret)
    {
        var s = Marshal.PtrToStringUTF8(ptr);
        PluginLog.Information($"{ret & 0xFFFFFFFF:X} {ret >> 32:X} {s}");
    }

    private static unsafe bool IncRefCondition(IntPtr a1, IntPtr a2, uint a3)
        => ((ResourceHandle*)a1)->FileName.ToString().EndsWith(".cmp");

    private static void FindResourcePost(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4)
    {
        var ext = $"{(char)*((byte*)a2 + 0x3)}{(char)*((byte*)a2 + 0x2)}{(char)*((byte*)a2 + 0x1)}";
        PluginLog.Information($"{*(uint*)a1} - {*(uint*)a3} - {ext}: {a4:X16}");
    }

    private static bool ResourceUnloadCondition(IntPtr a1, IntPtr a2)
    {
        var counter = *(uint*)(a2 + 0xAC);
        if (counter > 1)
            return false;

        var ext    = $"{(char)*((byte*)a2 + 0xE)}{(char)*((byte*)a2 + 0xD)}{(char)*((byte*)a2 + 0xC)}";
        var length = *(uint*)(a2 + 0x58);
        var file   = length > 15 ? Marshal.PtrToStringAnsi(*(IntPtr*)(a2 + 0x48)) : Marshal.PtrToStringAnsi(a2 + 0x48);
        PluginLog.Information($"Resource Unload Counter {counter} {ext} {length} {file}");
        return true;
    }

    private static bool EventDataCondition(IntPtr ptr, ushort eventType, int which, IntPtr source, IntPtr value)
        => eventType == 3;

    private static OnAddonReceiveEventDelegate ReceiveEventData = (_, _, _, data, eventInfo) =>
    {
        PluginLog.Information(
            $"Helper: [0] = {*(ulong*)(data + 0):X}, [1] = {*(IntPtr*)(data + 8):X}, [2] = {*(IntPtr*)(data + 0x10):X}, [3] = {*(ulong*)(data + 0x18):X}, [4] = {*(ulong*)(data + 0x20):X}, [5] = {*(ulong*)(data + 0x28):X}, [6] = {*(IntPtr*)(data + 0x30):X}, [7] = {*(IntPtr*)(data + 0x38):X}");
        PluginLog.Information(
            $"Helper: [0] = {*(ulong*)(eventInfo + 0):X}, [1] = {*(IntPtr*)(eventInfo + 8):X}, [2] = {*(IntPtr*)(eventInfo + 0x10):X}");
    };
}
