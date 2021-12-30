using System;
using Dalamud.Game.Network;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Peon
{
    public static class ProgramHelper
    {
        public static void NetworkDetour(IntPtr data, ushort opCode, uint sourceId, uint targetId, NetworkMessageDirection direction)
        {
            switch(opCode)
            {
                case 382:
                case 730:
                    break;
                default:
                    return;
            }
            PluginLog.Information($"{opCode} {sourceId} {targetId} {direction}");
        }

        public static bool OpcodePrinterEnabled = false;

        public static void AddOpcodePrinter()
        {
            if (OpcodePrinterEnabled)
                return;
            Dalamud.Network.NetworkMessage += NetworkDetour;
            OpcodePrinterEnabled = true;
        }

        public static void DisableOpcodePrinter()
        {
            if (!OpcodePrinterEnabled)
                return;
            Dalamud.Network.NetworkMessage -= NetworkDetour;
            OpcodePrinterEnabled           =  false;
        }

        public static void Dispose()
        {
            DisableOpcodePrinter();
        }

        public static void ScanSig(string sig)
        {
            try
            {
                var ptr = Dalamud.SigScanner.ScanText(sig);
                if (ptr != IntPtr.Zero)
                    Dalamud.Chat.Print(
                        $"Found \"{sig}\" at 0x{ptr:X16}, offset +0x{GetOffset(ptr):X}");
            }
            catch (Exception)
            {
                Dalamud.Chat.Print($"Could not find \"{sig}\"");
            }
        }

        public static void ScanStaticSig(string sig)
        {
            try
            {
                var ptr = Dalamud.SigScanner.GetStaticAddressFromSig(sig);
                if (ptr != IntPtr.Zero)
                    Dalamud.Chat.Print(
                        $"Found static address for \"{sig}\" at 0x{ptr:X16}, offset +0x{GetOffset(ptr):X}");
            }
            catch (Exception)
            {
                Dalamud.Chat.Print($"Could not find \"{sig}\"");
            }
        }

        public static unsafe void PrintAgent(AgentId id)
        {
            var address = (IntPtr)((UIModule*)Dalamud.GameGui.GetUIModule())->GetAgentModule()->GetAgentByInternalId(id);
            Dalamud.Chat.Print(
                $"Agent {id} found at 0x{address:X16}, offset +0x{GetOffset(address):X}");
        }

        public static void PrintAgent(int id)
            => PrintAgent((AgentId)id);

        public static ulong GetOffset(IntPtr absoluteAddress)
            => (ulong) (absoluteAddress.ToInt64() - Dalamud.SigScanner.Module.BaseAddress.ToInt64());

        public static void PrintOffset(IntPtr absoluteAddress)
            => Dalamud.Chat.Print($"0x{absoluteAddress:X16}, offset +0x{GetOffset(absoluteAddress):X}");

        public static IntPtr GetAbsoluteAddress(int offset)
            => Dalamud.SigScanner.Module.BaseAddress + offset;

        public static void PrintAbsolute(int offset)
            => Dalamud.Chat.Print($"0x{GetAbsoluteAddress(offset):X16}, offset +0x{offset:X}");
    }
}
