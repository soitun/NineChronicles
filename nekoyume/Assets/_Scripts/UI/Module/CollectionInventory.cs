using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Helper;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.UI.Model;
using Nekoyume.UI.Scroller;
using NUnit.Framework;
using UnityEngine;
using Material = Nekoyume.Model.Item.Material;

namespace Nekoyume.UI.Module
{
    using UniRx;
    public class CollectionInventory : MonoBehaviour
    {
        [SerializeField] private InventoryScroll scroll;
        [SerializeField] private CanvasGroup scrollCanvasGroup;

        private CollectionMaterial _requiredItem;
        private Action<InventoryItem> _onClickItem;
        private CollectionMaterial[] _requiredItems;
        private bool canSelect = true;

        private readonly List<InventoryItem> _items = new List<InventoryItem>();

        public InventoryItem SelectedItem { get; private set; }

        public void SetInventory(Action<InventoryItem> onClickItem)
        {
            _onClickItem = onClickItem;

            ReactiveAvatarState.Inventory.Subscribe(UpdateInventory).AddTo(gameObject);

            scroll.OnClick.Subscribe(SelectItem).AddTo(gameObject);
        }

        private void SetCanSelect(bool value)
        {
            canSelect = value;
            scrollCanvasGroup.interactable = value;
        }

        #region NonFungibleItems (Equipment, Costume) - select one

        public void SetRequiredItem(CollectionMaterial requiredItem)
        {
            _requiredItem = requiredItem;
            SetCanSelect(true);

            var models = GetModels(_requiredItem);
            scroll.UpdateData(models, true);
            SelectItem(models.FirstOrDefault());
        }

        private List<InventoryItem> GetModels(CollectionMaterial requiredItem)
        {
            // get from _items by required item's condition
            var row = requiredItem.Row;
            var items = _items.Where(item => item.ItemBase.Id == row.ItemId).ToList();
            if (items.First().ItemBase.ItemType == ItemType.Equipment)
            {
                items = items.Where(item =>
                    item.ItemBase is Equipment equipment &&
                    equipment.level == row.Level &&
                    equipment.Skills.Any() || !row.SkillContains).ToList();
            }

            return items;
        }

        private void SelectItem(InventoryItem item)
        {
            if (item == null || !canSelect)
            {
                return;
            }

            SelectedItem?.CollectionSelected.SetValueAndForceNotify(false);
            SelectedItem = item;
            SelectedItem.CollectionSelected.SetValueAndForceNotify(true);
            _onClickItem?.Invoke(SelectedItem);
        }

        #endregion

        #region FungibleItems (Consumable, Material) - select auto

        public void SetRequiredItems(CollectionMaterial[] requiredItem)
        {
            _requiredItems = requiredItem;
            SetCanSelect(false);

            var models = GetModels(_requiredItems);
            scroll.UpdateData(models, true);

            // Select All
            foreach (var model in models)
            {
                model.CollectionSelected.SetValueAndForceNotify(true);
            }
        }

        private List<InventoryItem> GetModels(CollectionMaterial[] requiredItems)
        {
            var models = new List<InventoryItem>();
            foreach (var requiredItem in requiredItems)
            {
                // Todo : 걍 first로?
                var items = _items.Where(item => item.ItemBase.Id == requiredItem.Row.ItemId);
                models.AddRange(items);
            }

            return models;
        }

        #endregion

        #region Update Inventory

        private void UpdateInventory(Nekoyume.Model.Item.Inventory inventory)
        {
            _items.Clear();
            if (inventory == null)
            {
                return;
            }

            foreach (var item in inventory.Items)
            {
                AddItem(item.item, item.count);
            }

            if (canSelect)
            {
                SetRequiredItem(_requiredItem);
            }
            else
            {
                SetRequiredItems(_requiredItems);
            }
        }

        private void AddItem(ItemBase itemBase, int count = 1)
        {
            if (itemBase is ITradableItem tradableItem)
            {
                var blockIndex = Game.Game.instance.Agent?.BlockIndex ?? -1;
                if (tradableItem.RequiredBlockIndex > blockIndex)
                {
                    return;
                }
            }

            InventoryItem inventoryItem;
            switch (itemBase.ItemType)
            {
                case ItemType.Consumable:
                    var consumable = (Consumable)itemBase;
                    if (TryGetConsumable(consumable, out inventoryItem))
                    {
                        inventoryItem.Count.Value += count;
                    }
                    else
                    {
                        inventoryItem = new InventoryItem(itemBase, count, false, true);
                        _items.Add(inventoryItem);
                    }

                    break;
                case ItemType.Costume:
                case ItemType.Equipment:
                    inventoryItem = new InventoryItem(
                        itemBase,
                        count,
                        !Util.IsUsableItem(itemBase),
                        false);
                    _items.Add(inventoryItem);
                    break;
                case ItemType.Material:
                    var material = (Material)itemBase;
                    if (TryGetMaterial(material, out inventoryItem))
                    {
                        inventoryItem.Count.Value += count;
                    }
                    else
                    {
                        inventoryItem = new InventoryItem(itemBase, count, false, false);
                        _items.Add(inventoryItem);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TryGetMaterial(Material material, out InventoryItem model)
        {
            model = _items.FirstOrDefault(item =>
                item.ItemBase is Material m && m.ItemId.Equals(material.ItemId));

            return model != null;
        }

        private bool TryGetConsumable(Consumable consumable, out InventoryItem model)
        {
            model = _items.FirstOrDefault(item => item.ItemBase.Id.Equals(consumable.Id) &&
                                                  item.ItemBase.ItemType == ItemType.Consumable);

            return model != null;
        }

        #endregion
    }
}
