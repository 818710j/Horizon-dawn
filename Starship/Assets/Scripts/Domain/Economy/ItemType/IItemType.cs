using Services.Reources;
using UnityEngine;

namespace Economy.ItemType
{
    public enum ItemQuality
    {
        N1,
        N2,
        N3,
        Low,
        Common,
        Medium,
        High,
        Perfect,
        Extreme
    }

    public interface IItemType
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        Sprite GetIcon(IResourceLocator resourceLocator);
        Color Color { get; }
        Price Price { get; }
        ItemQuality Quality { get; }

        void Consume(int amount);
        void Withdraw(int amount);
        int MaxItemsToConsume { get; }
        int MaxItemsToWithdraw { get; }
    }
}
