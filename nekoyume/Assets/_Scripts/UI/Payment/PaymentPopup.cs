using Nekoyume.L10n;
using System.Numerics;
using Nekoyume.UI.Module;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class PaymentPopup : PopupWidget
    {
        private enum PopupType
        {
            HasAttractAction,
            NoAttractAction,
            NoneAction,
        }

        // TODO: CostType과 동일하게 쓸 수 있을지는 확인
        private enum PaymentType
        {
            Crystal,
            SilverDust,
            GoldenDust,
            RubyDust,
            EmeraldDust,
            NCG,
            NCGStaking,
            MonsterCollection,
            RuneStoneSummonOnly, // 룬조각?
            RuneStone,
            ActionPoint,
            APPortion,
        }

#region SerializeField
        [SerializeField]
        private CostIconDataScriptableObject costIconData;

        [SerializeField]
        private Image costIcon;

        [SerializeField]
        private Image addCostIcon;

        [SerializeField]
        private TextMeshProUGUI costText;

        [SerializeField]
        private TextMeshProUGUI addCostText;

        [SerializeField]
        private GameObject addCostContainer;

        [SerializeField]
        private GameObject attractArrowObject;
        
        [SerializeField]
        private TextMeshProUGUI titleText;
        
        [SerializeField]
        private TextMeshProUGUI contentText;
        
        [SerializeField]
        private TextButton buttonYes;
        
        [SerializeField]
        private TextButton buttonNo;
        
        [SerializeField]
        private Button buttonClose;
        
        [SerializeField]
        private GameObject titleBorder;
#endregion SerializeField

        private ConfirmDelegate CloseCallback { get; set; }
        
        protected override void Awake()
        {
            base.Awake();

            buttonNo.OnClick = No;
            buttonYes.OnClick = Yes;
            buttonClose.onClick.AddListener(NoWithoutCallback);
            CloseWidget = NoWithoutCallback;
            SubmitWidget = Yes;
        }
        
        private void SetPopupType(PopupType popupType)
        {
            switch (popupType)
            {
                case PopupType.HasAttractAction:
                    buttonYes.gameObject.SetActive(true);
                    buttonNo.gameObject.SetActive(false);
                    buttonClose.gameObject.SetActive(true);
                    attractArrowObject.SetActive(true);
                    break;
                case PopupType.NoAttractAction:
                    buttonYes.gameObject.SetActive(true);
                    buttonNo.gameObject.SetActive(true);
                    buttonClose.gameObject.SetActive(false);
                    attractArrowObject.SetActive(false);
                    break;
                case PopupType.NoneAction:
                    buttonYes.gameObject.SetActive(false);
                    buttonNo.gameObject.SetActive(false);
                    buttonClose.gameObject.SetActive(true);
                    attractArrowObject.SetActive(false);
                    break;
            }
        }

#region HasAttractAction
        public void ShowAttract(
            CostType costType,
            string cost,
            string content,
            string attractMessage,
            System.Action onAttract)
        {
            SetPopupType(PopupType.HasAttractAction);
            addCostContainer.SetActive(false);
            
            costIcon.overrideSprite = costIconData.GetIcon(costType);
            var title = L10nManager.Localize("UI_TOTAL_COST");
            costText.text = cost;
            var no = L10nManager.Localize("UI_NO");
            CloseCallback = result =>
            {
                if (result == ConfirmResult.Yes)
                {
                    onAttract();
                }
            };
            Show(title, content, attractMessage, no, false);
        }

        // TODO: Remove after add world boss exception
        public void ShowAttractWithAddCost(
            string title,
            string content,
            CostType costType,
            int cost,
            CostType addCostType,
            int addCost,
            System.Action onConfirm)
        {
            SetPopupType(PopupType.HasAttractAction);
            addCostContainer.SetActive(true);
            
            costIcon.overrideSprite = costIconData.GetIcon(costType);
            costText.text = $"{cost:#,0}";

            addCostIcon.overrideSprite = costIconData.GetIcon(addCostType);
            addCostText.text = $"{addCost:#,0}";

            CloseCallback = result =>
            {
                if (result == ConfirmResult.Yes)
                {
                    onConfirm?.Invoke();
                }
            };
            Show(title, content);
        }

        public void ShowAttract(
            CostType costType,
            BigInteger cost,
            string content,
            string attractMessage,
            System.Action onAttract)
        {
            ShowAttract(costType, cost.ToString(), content, attractMessage, onAttract);
        }
#endregion HasAttractAction
        
#region NoAttractAction
        public void ShowNoAttractActionWithCheck(
            CostType costType,
            BigInteger balance,
            BigInteger cost,
            string enoughMessage,
            string insufficientMessage,
            System.Action onPaymentSucceed,
            System.Action onAttract)
        {
            SetPopupType(PopupType.NoAttractAction);
            addCostContainer.SetActive(false);
            
            var popupTitle = L10nManager.Localize("UI_TOTAL_COST");
            var enoughBalance = balance >= cost;
            costText.text = cost.ToString();
            costIcon.overrideSprite = costIconData.GetIcon(costType);

            var yes = L10nManager.Localize("UI_YES");
            var no = L10nManager.Localize("UI_NO");
            CloseCallback = result =>
            {
                if (result != ConfirmResult.Yes)
                {
                    return;
                }

                if (enoughBalance)
                {
                    onPaymentSucceed.Invoke();
                }
                else
                {
                    Close(true);
                    var attractMessage = costType == CostType.Crystal
                        ? L10nManager.Localize("UI_GO_GRINDING")
                        : L10nManager.Localize("UI_YES");
                    ShowAttract(costType, cost, insufficientMessage, attractMessage, onAttract);
                }
            };
            
            SetContent(popupTitle, enoughMessage, yes, no, false);
            Show(popupTitle, enoughMessage, yes, no, false);
        }
#endregion NoAttractAction

#region General
        private void Show(string title, string content, string labelYes = "UI_OK", string labelNo = "UI_CANCEL",
            bool localize = true)
        {
            SetContent(title, content, labelYes, labelNo, localize);

            if (!gameObject.activeSelf)
            {
                Show();
            }
        }
        
        private void SetContent(string title, string content, string labelYes = "UI_OK", string labelNo = "UI_CANCEL",
            bool localize = true)
        {
            var titleExists = !string.IsNullOrEmpty(title);
            if (localize)
            {
                if (titleExists)
                {
                    titleText.text = L10nManager.Localize(title);
                }

                contentText.text = L10nManager.Localize(content);
                buttonYes.Text = L10nManager.Localize(labelYes);
                buttonNo.Text = L10nManager.Localize(labelNo);
            }
            else
            {
                titleText.text = title;
                contentText.text = content;
                buttonYes.Text = labelYes;
                buttonNo.Text = labelNo;
            }

            titleText.gameObject.SetActive(titleExists);
            titleBorder.SetActive(titleExists);
        }
#endregion General
        

        private void Yes()
        {
            base.Close();
            CloseCallback?.Invoke(ConfirmResult.Yes);
        }

        private void No()
        {
            base.Close();
            CloseCallback?.Invoke(ConfirmResult.No);
        }

        public void NoWithoutCallback()
        {
            base.Close();
        }
    }
}
