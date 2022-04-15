using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Peon.Bothers;

namespace Peon.Modules
{
    public unsafe struct PtrCharaSelectListMenu
    {
        public AtkUnitBase* Pointer;

        public static implicit operator PtrCharaSelectListMenu(IntPtr ptr)
            => new() { Pointer = Module.Cast<AtkUnitBase>(ptr) };

        public static implicit operator bool(PtrCharaSelectListMenu ptr)
            => ptr.Pointer != null;

        private AtkComponentNode* ListNode
            => (AtkComponentNode*) Pointer->RootNode->ChildNode->PrevSiblingNode;

        private AtkComponentList* List
            => (AtkComponentList*) ListNode->Component;

        public string[] CharacterNames()
        {
            var      list = List;
            string[] ret  = new string[list->ListLength];
            for (var i = 0; i < list->ListLength; ++i)
                ret[i] = Module.TextNodeToString(list->ItemRendererList[i].AtkComponentListItemRenderer->AtkComponentButton.ButtonTextNode);

            return ret;
        }

        public int CharacterIndex(string name)
        {
            var      list = List;
            string[] ret  = new string[list->ListLength];
            for (var i = 0; i < list->ListLength; ++i)
                if (name == Module.TextNodeToString(list->ItemRendererList[i].AtkComponentListItemRenderer->AtkComponentButton.ButtonTextNode))
                    return i;

            return -1;
        }

        public bool Select(int idx)
            => Module.ClickList(Pointer, ListNode, idx, 5 + idx, EventType.Click);

        public bool Select(CompareString name)
            => Module.ClickList(Pointer, ListNode, a => name.Matches(Module.TextNodeToString(a->AtkComponentButton.ButtonTextNode)), 5, EventType.Click);
    }
}
