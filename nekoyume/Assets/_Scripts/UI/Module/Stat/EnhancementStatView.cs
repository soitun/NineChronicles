using Nekoyume.EnumType;
using TMPro;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class EnhancementStatView : StatView
    {
        public Image arrowImage;
        public TextMeshProUGUI afterText;

        public override void Show(StatType statType, int before)
        {
            arrowImage.enabled = false;
            afterText.enabled = false;
            base.Show(statType, before);
        }

        public void Show(StatType statType, int before, int after)
        {
            Show(statType.ToString(), before.ToString(), after.ToString());
        }

        public void Show(string statType, string before, string after)
        {
            arrowImage.enabled = true;
            afterText.enabled = true;
            afterText.text = after;
            Show(statType, before);
        }

        public override void Hide()
        {
            arrowImage.enabled = false;
            afterText.enabled = false;
            base.Hide();
        }
    }
}
