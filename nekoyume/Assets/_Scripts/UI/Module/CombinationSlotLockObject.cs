#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Nekoyume.Blockchain;
using Nekoyume.Game.Controller;
using Nekoyume.L10n;
using Nekoyume.State;
using Nekoyume.TableData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Model
{
    public class CombinationSlotLockObject : MonoBehaviour
    {
        private Button _button = null!;
        
        // TODO: 동적으로 이미지 변경?
        // private Image _costImage;
        
        [SerializeField]
        private TMP_Text costText = null!;
        
        // TODO: 별도 enum만들기 싫은데 다른 방법이 잘 안보임
        private CostType _costType;
        private UnlockCombinationSlotCostSheet.Row? _data;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                AudioController.PlayClick();
                ShowPaymentPopup();
            });
        }

        private void ShowPaymentPopup()
        {
            if (_data == null)
            {
                NcDebug.LogError("TableData is null");
                return;
            }
            
            Widget.Find<PaymentPopup>().Show(
                _costType,
                GetBalance(),
                GetCost(),
                GetEnoughCostMessageString(),
                L10nManager.Localize("UI_NOT_ENOUGH_CRYSTAL"),
                () =>
                {
                    ActionManager.Instance.UnlockCombinationSlot(_data.SlotId);
                },
                OnAttractInPaymentPopup);
        }

        private void OnAttractInPaymentPopup()
        {
            // TODO: 이후 재화 관련 팝업에서 처리
        }

#region GetBalance
        private BigInteger GetBalance()
        {
            var inventory = States.Instance.CurrentAvatarState.inventory;
            return _costType switch
            {
                CostType.Crystal => States.Instance.CrystalBalance.MajorUnit,
                CostType.NCG => States.Instance.GoldBalanceState.Gold.MajorUnit,
                CostType.GoldDust => inventory.GetMaterialCount((int)_costType),
                CostType.RubyDust => inventory.GetMaterialCount((int)_costType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private BigInteger GetCost()
        {
            if (_data != null)
            {
                return _costType switch
                {
                    CostType.Crystal => _data.CrystalPrice,
                    CostType.NCG => _data.NcgPrice,
                    CostType.GoldDust => _data.GoldenDustPrice,
                    CostType.RubyDust => _data.RubyDustPrice,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            NcDebug.LogError("TableData is null");
            return BigInteger.Zero;
        }
#endregion GetBalance

#region EnoughCostMessage
        private string GetEnoughCostMessageString()
        {
            if (_data != null)
            {
                return _costType switch
                {
                    CostType.Crystal => GetEnoughFavCostMessageString(_data.CrystalPrice),
                    CostType.NCG => GetEnoughFavCostMessageString(_data.NcgPrice),
                    CostType.GoldDust => GetEnoughMaterialCostMessageString(),
                    CostType.RubyDust => GetEnoughMaterialCostMessageString(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            NcDebug.LogError("TableData is null");
            return string.Empty;
        }
        
        private string GetEnoughFavCostMessageString(BigInteger cost)
        {
            // TODO: 신규 키 추가
            var usageMessage = L10nManager.Localize("UI_DRAW_ADVANCED_BUFF");
            var favText = _costType switch
            {
                CostType.Crystal => "UI_CRYSTAL",
                CostType.NCG => "UI_NCG",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return L10nManager.Localize(
                "UI_CONFIRM_PAYMENT_CURRENCY_FORMAT",
                cost,
                favText,
                usageMessage);
        }
        
        private string GetEnoughMaterialCostMessageString()
        {
            // TODO: 
            return _costType switch
            {
                CostType.GoldDust => "UI_CONFIRM_PAYMENT_CURRENCY_FORMAT",
                CostType.RubyDust => "UI_CONFIRM_PAYMENT_CURRENCY_FORMAT",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private string GetNotEnoughCostMessageString()
        {
            return _costType switch
            {
                CostType.Crystal => L10nManager.Localize("UI_NOT_ENOUGH_CRYSTAL"),
                CostType.NCG => L10nManager.Localize("UI_NOT_ENOUGH_NCG"),
                CostType.GoldDust => L10nManager.Localize("UI_NOT_ENOUGH_GOLD_DUST"),
                CostType.RubyDust => L10nManager.Localize(""), // TODO: 신규 키 추가
                _ => throw new ArgumentOutOfRangeException()
            };
        }
#endregion EnoughCostMessage

        public void SetData(UnlockCombinationSlotCostSheet.Row data)
        {
            _data = data;
            if (data.CrystalPrice > 0)
            {
                _costType = CostType.Crystal;
            }
            else if (data.GoldenDustPrice > 0)
            {
                _costType = CostType.GoldDust;
            }
            else if (data.RubyDustPrice > 0)
            {
                _costType = CostType.RubyDust;
            }
            else if (data.NcgPrice > 0)
            {
                _costType = CostType.NCG;
            }
            
            costText.text = GetCost().ToString();
        }
    }
}
