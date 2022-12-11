using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore
{
    internal class Element
    {
        public Vector2i m_pos;
        public GameObject m_go;
        public Image m_icon;
        public Text m_amount;
        public Text m_quality;
        public Image m_equiped;
        public Image m_queued;
        public GameObject m_selected;
        public Image m_noteleport;
        public UITooltip m_tooltip;
        public GuiBar m_durability;
        public bool m_used;
    }
}