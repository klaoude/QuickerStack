using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace QuickerStack
{
    internal class Element
    {
        // Token: 0x04000010 RID: 16
        public Vector2i m_pos;

        // Token: 0x04000011 RID: 17
        public GameObject m_go;

        // Token: 0x04000012 RID: 18
        public Image m_icon;

        // Token: 0x04000013 RID: 19
        public Text m_amount;

        // Token: 0x04000014 RID: 20
        public Text m_quality;

        // Token: 0x04000015 RID: 21
        public Image m_equiped;

        // Token: 0x04000016 RID: 22
        public Image m_queued;

        // Token: 0x04000017 RID: 23
        public GameObject m_selected;

        // Token: 0x04000018 RID: 24
        public Image m_noteleport;

        // Token: 0x04000019 RID: 25
        public UITooltip m_tooltip;

        // Token: 0x0400001A RID: 26
        public GuiBar m_durability;

        // Token: 0x0400001B RID: 27
        public bool m_used;
    }
}
