using System.Collections.Generic;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameServices.Player;

namespace GameModel
{
    namespace Quests
	{
		public class BlackMarketPlayerInventory : IInventory
		{
		    public BlackMarketPlayerInventory(PlayerResources playerResources, ItemTypeFactory itemTypeFactory, bool normal = false)
		    {
		        _playerResources = playerResources;
		        _itemTypeFactory = itemTypeFactory;
				_normal= normal;
		    }

			public void Refresh() {}

			public IEnumerable<IProduct> Items
			{
				get
				{
				    if (CurrencyExtensions.PremiumCurrencySell&&!_normal)
				    {
				        var itemType = _itemTypeFactory.CreateCurrencyItem(Currency.Stars);
                        if (_playerResources.Stars > 0)
                            yield return new PlayerProduct(_playerResources, itemType, itemType.MaxItemsToWithdraw);
                    }
                }
			}
			
			//public int Money { get { return Game.Session.Player.Money; } }

		    private readonly PlayerResources _playerResources;
		    private readonly ItemTypeFactory _itemTypeFactory;

			private bool _normal;

		}
	}
}
