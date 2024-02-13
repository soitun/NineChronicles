using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Game.Controller;
using Nekoyume.L10n;
using Nekoyume.Model.Collection;
using Nekoyume.Model.Item;
using Nekoyume.UI.Model;
using Nekoyume.UI.Module;
using UnityEngine;
using UnityEngine.UI;
using Material = Nekoyume.Model.Item.Material;

namespace Nekoyume.UI
{
    using UniRx;
    public class CollectionRegistrationPopup : PopupWidget
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private CollectionStat collectionStat;
        [SerializeField] private CanvasGroup collectionStatCanvasGroup;
        [SerializeField] private CollectionItemView[] collectionItemViews;
        [SerializeField] private ConditionalButton registrationButton;
        [SerializeField] private CollectionInventory collectionInventory;
        [SerializeField] private EquipmentTooltip equipmentTooltip;

        private readonly Dictionary<CollectionMaterial, ICollectionMaterial> _registeredItems = new();
        private CollectionMaterial _focusedRequiredItem;
        private Action<List<ICollectionMaterial>> _registerMaterials;

        private bool canRegister;

        protected override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() =>
            {
                AudioController.PlayClick();
                CloseWidget.Invoke();
            });
            CloseWidget = () =>
            {
                Close();
            };

            registrationButton.OnSubmitSubject
                .Subscribe(_ => OnClickRegisterButton())
                .AddTo(gameObject);

            collectionInventory.SetInventory(OnClickInventoryItem);
        }

        private void OnClickRegisterButton()
        {
            if (canRegister)
            {
                RegisterItem(collectionInventory.SelectedItem);
            }
            else
            {
                RegisterMaterials();
            }
        }

        private void OnClickInventoryItem(InventoryItem item)
        {
            if (!canRegister)
            {
                return;
            }

            ShowItemTooltip(item);
        }

        private void RegisterMaterials()
        {
            var registeredItems = _registeredItems.Values.ToList();
            _registerMaterials?.Invoke(registeredItems);
            CloseWidget.Invoke();
        }

        #region NonFungibleCollectionMaterial (Equipment, Costume)

        private void RegisterItem(InventoryItem item)
        {
            ICollectionMaterial collectionMaterialItem;
            switch (item.ItemBase)
            {
                case Equipment equipment:
                    collectionMaterialItem = new NonFungibleCollectionMaterial
                    {
                        ItemId = equipment.Id,
                        ItemCount = 1,
                        NonFungibleId = equipment.NonFungibleId,
                        Level = equipment.level,
                        SkillContains = equipment.Skills.Any()
                    };
                    break;
                case Costume costume:
                    collectionMaterialItem = new NonFungibleCollectionMaterial
                    {
                        ItemId = item.ItemBase.Id,
                        ItemCount = 1,
                        NonFungibleId = costume.NonFungibleId,
                        SkillContains = false,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _registeredItems[_focusedRequiredItem] = collectionMaterialItem;
            _focusedRequiredItem.Registered.SetValueAndForceNotify(true);

            // Focus next required item or Activate
            var notRegisteredItem = _registeredItems
                .FirstOrDefault(registeredItem => registeredItem.Value == null).Key;
            if (notRegisteredItem != null)
            {
                FocusItem(notRegisteredItem);
            }
            else
            {
                RegisterMaterials();
            }
        }

        private void ShowItemTooltip(InventoryItem item)
        {
            if (item.ItemBase is null)
            {
                return;
            }

            equipmentTooltip.Show(item, string.Empty, false, null);
            equipmentTooltip.OnEnterButtonArea(true);
        }

        private void FocusItem(CollectionMaterial collectionMaterial)
        {
            if (collectionMaterial == null || !canRegister)
            {
                return;
            }

            _focusedRequiredItem?.Focused.SetValueAndForceNotify(false);
            _focusedRequiredItem = collectionMaterial;
            _focusedRequiredItem.Focused.SetValueAndForceNotify(true);

            collectionInventory.SetRequiredItem(_focusedRequiredItem);

            var count = _registeredItems.Count(registeredItem => registeredItem.Value == null);
            registrationButton.Text = count == 1
                ? L10nManager.Localize("UI_REGISTER")
                : L10nManager.Localize("UI_ACTIVATE");
        }

        #endregion

        // For NonFungibleCollectionMaterial (Equipment, Costume)
        public void ShowForNonFungibleMaterial(
            CollectionModel model,
            Action<List<ICollectionMaterial>> register,
            bool ignoreShowAnimation = false)
        {
            collectionStat.Set(model);
            _registerMaterials = register;
            SetCanRegister(true);

            _registeredItems.Clear();
            foreach (var material in model.Materials)
            {
                var data = new CollectionMaterial(material.Row, material.Grade, material.ItemType);

                _registeredItems.Add(data, null);
            }

            var requiredItems = _registeredItems.Keys.ToArray();
            for (var i = 0; i < collectionItemViews.Length; i++)
            {
                collectionItemViews[i].gameObject.SetActive(i < requiredItems.Length);
                if (i >= requiredItems.Length)
                {
                    continue;
                }

                collectionItemViews[i].Set(requiredItems[i], FocusItem);
            }

            FocusItem(requiredItems.First());

            base.Show(ignoreShowAnimation);
        }

        // For FungibleCollectionMaterial (Consumable, Material)
        // fungible 하기 때문에 Inventory와 연동 없이 바로 등록
        public void ShowForFungibleMaterial(
            CollectionModel model,
            Action<List<ICollectionMaterial>> register,
            bool ignoreShowAnimation = false)
        {
            collectionStat.Set(model);
            _registerMaterials = register;
            SetCanRegister(false);

            _registeredItems.Clear();
            foreach (var material in model.Materials)
            {
                var required = new CollectionMaterial(material.Row, material.Grade, material.ItemType);
                var registered = new FungibleCollectionMaterial
                {
                    ItemId = material.Row.ItemId,
                    ItemCount = material.Row.Count,
                };

                _registeredItems.Add(required, registered);
            }

            var requiredItems = _registeredItems.Keys.ToArray();
            for (var i = 0; i < collectionItemViews.Length; i++)
            {
                collectionItemViews[i].gameObject.SetActive(i < requiredItems.Length);
                if (i >= requiredItems.Length)
                {
                    continue;
                }

                collectionItemViews[i].Set(requiredItems[i], null);
                requiredItems[i].Focused.SetValueAndForceNotify(true);
            }

            collectionInventory.SetRequiredItems(requiredItems);
            equipmentTooltip.Close();
            registrationButton.Text = L10nManager.Localize("UI_ACTIVATE");

            base.Show(ignoreShowAnimation);
        }

        private void SetCanRegister(bool value)
        {
            canRegister = value;
            collectionStatCanvasGroup.interactable = value;
        }
    }
}
