using System.Linq;
using Nekoyume.Model.Item;
using Nekoyume.TableData;
using UniRx;

namespace Nekoyume.UI.Model
{
    public class CollectionMaterial
    {
        public CollectionSheet.RequiredMaterial Row { get; }
        public int Grade { get; }
        public ItemType ItemType { get; }
        public bool HasItem { get; private set; }
        public bool EnoughCount { get; private set; }
        public bool Active { get; }

        // enough condition for active
        public bool Enough => HasItem && EnoughCount && !Active && !Registered.Value;

        public ReactiveProperty<bool> Selected { get; }

        public ReactiveProperty<bool> Focused { get; }

        public ReactiveProperty<bool> Registered { get; }

        // For Collection Scroll
        public CollectionMaterial(
            CollectionSheet.RequiredMaterial row,
            int grade,
            ItemType itemType,
            bool active)
        {
            Row = row;
            Grade = grade;
            ItemType = itemType;
            Active = active;
            Selected = new ReactiveProperty<bool>(false);
            Focused = new ReactiveProperty<bool>(false);
            Registered = new ReactiveProperty<bool>(false);
        }

        // For CollectionRegistrationPopup
        public CollectionMaterial(
            CollectionSheet.RequiredMaterial row,
            int grade,
            ItemType itemType)
        {
            Row = row;
            Grade = grade;
            ItemType = itemType;

            HasItem = true;
            EnoughCount = true;
            Active = false;

            Selected = new ReactiveProperty<bool>(false);
            Focused = new ReactiveProperty<bool>(false);
            Registered = new ReactiveProperty<bool>(false);
        }

        // scroll, active : enough x : registered = false
        // scroll, not active : enough -> set condition -> o : registered = false
        // select : enough -> !registered -> o : active = false
        // enough = !active && (hasItem && enoughCount) && !registered

        public void SetCondition(Inventory inventory)
        {
            var items = inventory.Items
                .Where(item => item.item.Id == Row.ItemId).ToArray();

            var hasItem = items.Any();
            bool enoughCount;
            switch (ItemType)
            {
                case ItemType.Equipment:
                    var equipments = items
                        .Select(item => item.item).OfType<Equipment>().ToArray();
                    hasItem &= equipments
                        .Any(item => item.Skills.Any() || !Row.SkillContains);
                    enoughCount = equipments
                        .Any(item => item.level == Row.Level);
                    break;
                case ItemType.Material:
                    enoughCount = items.Sum(item => item.count) >= Row.Count;
                    break;
                case ItemType.Consumable:
                    enoughCount = items.Length >= Row.Count;
                    break;
                default:
                    enoughCount = hasItem;
                    break;
            }

            HasItem = hasItem;
            EnoughCount = enoughCount;
        }
    }
}
