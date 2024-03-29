using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using GameServices.Player;
using GameServices.Random;
using Services.IAP;
using Market = Model.Regulations.Market;

namespace GameModel
{
    namespace Quests
	{
		public class BlackMarketInventory : IInventory
		{
			public BlackMarketInventory(Galaxy.Star star, ItemTypeFactory itemTypeFactory, ProductFactory productFactory, PlayerSkills playerSkills, IRandom random, IInAppPurchasing iapPurchasing, IDatabase database, bool normal = false)
			{
				_starId = star.Id;
				_level = star.Level;
			    _random = random;
			    _itemTypeFactory = itemTypeFactory;
			    _productFactory = productFactory;
                _inAppPurchasing = iapPurchasing;
			    _playerSkills = playerSkills;
			    _database = database;

				_normal = normal;
				Money = 10000;
			}

			public void Refresh() { _items = null; }

			public IEnumerable<IProduct> Items
			{
				get
				{
					if (_items == null)
					{
						var random = _random.CreateRandom(_starId);
						var pricescale = _playerSkills.PriceScale;
						var extraGoods = _playerSkills.HasMasterTrader ? 1 : 0;
						_items = new List<IProduct>();
						if (_normal)
						{
							foreach (var item in _database.QuestItemList.Where(item => item.InMarket).RandomUniqueElements(random.Next(3, 5 + 3 * extraGoods), random))
								_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateArtifactItem(item), random.Next(item.MinAmount, item.MaxAmount), _starId, Market.TechRenewalTime, 2f * pricescale));
						}
						else
						{

							_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateFuelItem(), 100 + extraGoods * 50, _starId, Market.FuelRenewalTime, 2f * pricescale));

							foreach (var id in _database.FactionList.Visible().Where(faction => faction.WanderingShipsDistance <= _level).RandomUniqueElements(random.Next(2, 5 + extraGoods), random))
								_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateResearchItem(id), random.Next(5, 20), _starId, Market.TechRenewalTime, pricescale));

							if (CurrencyExtensions.PremiumCurrencyAllowed)
								_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateCurrencyItem(Currency.Stars), random.Next(10, 1000 + 5 * extraGoods), _starId, Market.StarsRenewalTime, 1.25f * pricescale));

							foreach (var ship in _database.ShipBuildList.Playable().NormalShips().Where(item => item.Ship.Faction.WanderingShipsDistance <= _level).RandomUniqueElements(random.Next(extraGoods + 2, 6), random))
								_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateShipItem(new CommonShip(ship), true), 1, _starId, Market.ShipRenewalTime, pricescale));

							var componentCount = random.Next(7, 14 + 8 * extraGoods);
							for (var i = 0; i < componentCount; ++i)
								_items.Add(_productFactory.CreateRandomComponentProduct(_starId, i, _level + 75, _level > 50 ? ComponentQuality.P4 : ComponentQuality.P1, Faction.Undefined, true, Market.RareComponentRenewalTime, true, 1.25f * pricescale));

							if (extraGoods > 0)
								_items.Add(_productFactory.CreateRandomComponentProduct(_starId, componentCount, _level + 75, _level > 50 ? ComponentQuality.P4 : ComponentQuality.P1, Faction.Undefined, true, Market.RareComponentRenewalTime, false, 1.5f * pricescale));

							foreach (var item in _database.SatelliteList.Where(item => item.SizeClass != SizeClass.Titan).RandomUniqueElements(random.Next(extraGoods + 3), random))
								_items.Add(_productFactory.CreateRenewableMarketProduct(_itemTypeFactory.CreateSatelliteItem(item, true), 1, _starId, Market.SatelliteRenewalTime, pricescale));

							//if (Model.Regulations.Time.IsCristmas && random.Next(3) == 0)
							//	_items.Add(_productFactory.CreateRenewableMarketProduct(new XmaxBoxItem(random.Next(), _starId), 1, _starId, Market.GiftBoxRenewalTime, 1));

							_items.AddRange(_inAppPurchasing.GetAvailableProducts().Select(item => _productFactory.CreateMarketProduct(item, 1, 0)));
						}
					}
					
					return _items.Where(item => item.Quantity > 0);
				}
			}
			
			public int Money { get; private set; }
			
			private readonly int _starId;
			private readonly int _level;
			private readonly IRandom _random;
			private List<IProduct> _items;
		    private readonly IInAppPurchasing _inAppPurchasing;
		    private readonly ItemTypeFactory _itemTypeFactory;
		    private readonly ProductFactory _productFactory;
		    private readonly PlayerSkills _playerSkills;
		    private readonly IDatabase _database;

			private bool _normal;
		}
	}
}
