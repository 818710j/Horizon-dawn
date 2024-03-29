using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Common;
using Constructor;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameServices.Random;
using Constructor.Ships;
using Database.Legacy;
using Game;
using Game.Exploration;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using GameModel;
using GameServices.Player;
using Zenject;
using Component = GameDatabase.DataModel.Component;
using Domain.Quests;

namespace GameServices.Economy
{
    public class LootGenerator
    {
        [Inject] private readonly ItemTypeFactory _factory;
        [Inject] private readonly Research.Research _research;
        [Inject] private readonly IRandom _random;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly HolidayManager _holidayManager;
        [Inject] private readonly PlayerSkills _playerSkills;

        public ItemTypeFactory Factory { get { return _factory; } }

        public IEnumerable<IProduct> GetCommonReward(IEnumerable<IShip> ships, int distance, Faction faction, int seed)
        {
            var random = _random.CreateRandom(seed);

            var scraps = 1;
            var money = 1;
            var stars = 0;
            var moduleLevel = Maths.Distance.ComponentLevel(distance);
            var factor = Mathf.Clamp((distance / 100), 1, 5);

            var QuestItemCount = 0;//额外奖励的数量

            foreach (var ship in ships)
            {
                scraps += ship.Scraps();
                money += (ship.Price() / 50) * factor;
                stars += (ship.Price() / 6000) * factor;

                QuestItemCount += ship.Model.Layout.CellCount;//完全根据船的格子数累加

                if (ship.Model.Category == ShipCategory.Flagship)
                {
                    var bossFaction = ship.Model.Faction;
                    foreach (var item in RandomComponents(moduleLevel + 35, random.Next(1, 2), bossFaction, random, false))
                        yield return new Product(item);

                    if (ship.ExtraThreatLevel >= DifficultyClass.Class2)
                    {
                        yield return Price.Premium(1).GetProduct(_factory);
                        foreach (var item in RandomComponents(moduleLevel + 75, random.Next(2, 3), bossFaction, random, false))
                            yield return new Product(item);
                    }

                    if (ship.ExtraThreatLevel >= DifficultyClass.Class3)
                    {
                        yield return Price.Premium(1).GetProduct(_factory);
                        foreach (var item in RandomComponents(moduleLevel + 115, random.Next(3, 4), bossFaction, random, false))
                            yield return new Product(item);
                    }
                }
                else if (ship.Model.Category == ShipCategory.SubFlagship)
                {
                    var bossFaction = ship.Model.Faction;
                    foreach (var item in RandomComponents(moduleLevel + 25, random.Next(1), bossFaction, random, false))
                        yield return new Product(item);

                    if (ship.ExtraThreatLevel >= DifficultyClass.Class2)
                    {
                        yield return Price.Premium(1).GetProduct(_factory);
                        foreach (var item in RandomComponents(moduleLevel + 65, random.Next(2), bossFaction, random, false))
                            yield return new Product(item);
                    }

                    if (ship.ExtraThreatLevel >= DifficultyClass.Class3)
                    {
                        yield return Price.Premium(1).GetProduct(_factory);
                        foreach (var item in RandomComponents(moduleLevel + 105, random.Next(3), bossFaction, random, false))
                            yield return new Product(item);
                    }
                }
                else
                {
                    foreach (var item in RandomComponents(moduleLevel, random.Next(-10, 2), faction, random, false))
                        yield return new Product(item);
                }
            }

            if (money > 0)
                yield return Price.Common(money).GetProduct(_factory);

            if (stars > 0)
                yield return Price.Premium(stars).GetProduct(_factory);

            //var toxicWaste =random.Next2(scraps/2);
            var toxicWaste = scraps / 2 + random.Next2(scraps / 2);
            if (toxicWaste > 0)
                yield return new Product(CreateArtifact(CommodityType.ToxicWaste), toxicWaste);

            scraps -= toxicWaste;
            if (scraps > 0)
                yield return new Product(CreateArtifact(CommodityType.Scraps), scraps);

            bool v = (QuestItemCount > 0);
            {
                var ResourceList = _database.QuestItemList.Where(i => i.Price > 0 && i.Price <= 10);
                var sum = ResourceList.Sum(i => 1.0f / i.Price);
                List<KeyValuePair<QuestItem, float>> QuestItemValuePairs = new List<KeyValuePair<QuestItem, float>>();
                var index = 0.0f;
                foreach (var QuestItem in ResourceList)
                {
                    index += 1f / QuestItem.Price / sum;
                    QuestItemValuePairs.Add(new KeyValuePair<QuestItem, float>(QuestItem, index));
                }
                var result = random.Next(0, 100) * 1.0f / 100;
                var min = 2f;
                var item = ResourceList.First();
                foreach (var QuestItemValuePair in QuestItemValuePairs)
                {
                    if (result <= QuestItemValuePair.Value)
                    {
                        if (QuestItemValuePair.Value < min)
                        {
                            item = QuestItemValuePair.Key;
                            min = QuestItemValuePair.Value;
                        }
                    }
                    else
                        continue;
                }
                yield return new Product(_factory.CreateArtifactItem(item), QuestItemCount);
            }

            foreach (var item in GetHolidayLoot(random))
                yield return item;
        }

        public IEnumerable<IProduct> GetSocialShareReward()
        {
            yield return Price.Premium(10).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetAdReward()
        {
            yield return Price.Premium(1).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetHolidayLoot(System.Random random)
        {
            if (_holidayManager.IsChristmas)
            {
                if (random.Percentage(33))
                    yield return new Product(_factory.CreateCurrencyItem(Currency.Snowflakes));
            }
        }

        public IEnumerable<IProduct> GetMeteoriteLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner*150);

            yield return new Product(CreateArtifact(CommodityType.Minerals), 1 + random.Next2(20*quality/100));
            if (random.Percentage(5))
                yield return new Product(CreateArtifact(CommodityType.Gems), 1 + random.Next2(5 * quality / 100));
            if (random.Percentage(5))
                yield return new Product(CreateArtifact(CommodityType.PreciousMetals), 1 + random.Next2(5 * quality / 100));

            var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Meteorite);
            if (resources != null)
            {
                var resource = resources.RandomElement(random);
                foreach (var item in resource.ExplorationLoot)
                    if (random.Percentage(item.Chance))
                        yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
            }
        }

        public IEnumerable<IProduct> GetOutpostLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 150);

            yield return new Product(CreateArtifact(CommodityType.Scraps), 1 + random.Next2(20 * quality / 100));

            if (random.Percentage(quality/10))
            {
                var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }

            for (var i = 0; i < random.Next(quality/40); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));

            yield return new Product(_factory.CreateCurrencyItem(Currency.Credits), Maths.Distance.Credits(level) + random.Next2(Maths.Distance.Credits(level) * quality));

            if (random.Percentage(quality/5))
                yield return new Product(_factory.CreateResearchItem(faction));

            var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Outpost);
            if (resources != null)
            {
                var resource = resources.RandomElement(random);
                foreach (var item in resource.ExplorationLoot)
                    if (random.Percentage(item.Chance))
                        yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
            }

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 50).GetProduct(_factory);
        }

        public IEnumerable<IProduct> GetHiveLoot(int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 150);

            yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(5 * quality / 100));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            if (random.Percentage(20 + quality / 10))
                yield return new Product(RandomComponent(level, _database.ExplorationSettings.InfectedPlanetFaction, random, true));
            
            if (random.Percentage(quality/5))
                yield return new Product(RandomFactionShip(level, _database.ExplorationSettings.InfectedPlanetFaction, random));

            yield return new Product(_factory.CreateCurrencyItem(Currency.Credits), Maths.Distance.Credits(level) + random.Next2(Maths.Distance.Credits(level) * quality / 3 ));

            if (random.Percentage(quality/10))
            {
                var tech = _research.GetAvailableTechs((_database.ExplorationSettings.InfectedPlanetFaction)).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 60).GetProduct(_factory);

            var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Hive);
            if (resources != null)
            {
                var resource = resources.RandomElement(random);
                foreach (var item in resource.ExplorationLoot)
                    if (random.Percentage(item.Chance))
                        yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
            }
        }

        public IEnumerable<IProduct> GetPlanetResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 150);

            if (planetType == PlanetType.Gas)
            {
                yield return new Product(CreateArtifact(CommodityType.ToxicWaste), 1 + random.Next2(100 * quality / 100));
                if (random.Percentage(30))
                    yield return new Product(_factory.CreateFuelItem(), 1 + random.Next2(5 * quality / 100));

                var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Gas);
                if (resources != null)
                {
                    var resource = resources.RandomElement(random);
                    foreach (var item in resource.ExplorationLoot)
                        if (random.Percentage(item.Chance))
                            yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
                }
            }
            else
            {
                yield return new Product(CreateArtifact(CommodityType.Minerals), 1 + random.Next2(20 * quality / 100));
                if (random.Percentage(5))
                    yield return new Product(CreateArtifact(CommodityType.Gems), 1 + random.Next2(5 * quality / 100));
                if (random.Percentage(5))
                    yield return new Product(CreateArtifact(CommodityType.PreciousMetals), 1 + random.Next2(5 * quality / 100));

                var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Minerals);
                if (resources != null)
                {
                    var resource = resources.RandomElement(random);
                    foreach (var item in resource.ExplorationLoot)
                        if (random.Percentage(item.Chance))
                            yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
                }
            }

        }

        public IEnumerable<IProduct> GetPlanetRareResources(PlanetType planetType, Faction faction, int level, int seed)
        {
            return GetPlanetResources(planetType, faction, level, seed);
        }

        public IEnumerable<IProduct> GetContainerLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 150);

            yield return new Product(_factory.CreateCurrencyItem(Currency.Credits), Maths.Distance.Credits(level) + random.Next2(Maths.Distance.Credits(level)*quality / 6 ));

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 80).GetProduct(_factory);

            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Alloys), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Polymers), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(10))
                yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(10 * quality / 100));

            for (var i = 0; i < random.Next(quality/50); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));

            var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.Container);
            if (resources != null)
            {
                var resource = resources.RandomElement(random);
                foreach (var item in resource.ExplorationLoot)
                    if (random.Percentage(item.Chance))
                        yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
            }

            if (random.Percentage(quality / 20))
            {
                var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }
        }

        public IEnumerable<IProduct> GetShipWreckLoot(Faction faction, int level, int seed)
        {
            var random = new System.Random(seed);
            var quality = Mathf.RoundToInt(_playerSkills.PlanetaryScanner * 150);

            yield return new Product(CreateArtifact(CommodityType.Scraps), 1 + random.Next2(50*quality/100));

            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Alloys), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(30))
                yield return new Product(CreateArtifact(CommodityType.Polymers), 1 + random.Next2(20 * quality / 100));
            if (random.Percentage(20))
                yield return new Product(_factory.CreateFuelItem(), 1 + random.Next2(10 * quality / 100));

            if (random.Percentage(quality/5))
                yield return new Product(_factory.CreateResearchItem(faction));

            yield return new Product(_factory.CreateCurrencyItem(Currency.Credits), Maths.Distance.Credits(level) + random.Next2(Maths.Distance.Credits(level) * quality / 6 ));

            yield return Price.Premium(1 + random.Next(2 + quality + level) / 80).GetProduct(_factory);

            for (var i = 0; i < random.Next(quality / 50); ++i)
                yield return new Product(RandomComponent(level, faction, random, true));

            var resources = _database.ExplorationSettings.ExplorationResource.Where(item => item.Type == ExplorationType.ShipWreck);
            if (resources != null)
            {
                var resource = resources.RandomElement(random);
                foreach (var item in resource.ExplorationLoot)
                    if (random.Percentage(item.Chance))
                        yield return new Product(_factory.CreateArtifactItem(item.QuestItem), 1 + random.Next2(item.MaxAmount));
            }

            if (random.Percentage(quality / 15))
            {
                var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }
        }

        public IEnumerable<IProduct> GetStarBaseSpecialReward(Region region)
        {
            yield return new Product(_factory.CreateResearchItem(region.Faction), Mathf.FloorToInt(3f + region.BaseDefensePower / 4f));

            if (region.IsPirateBase)
            {
                var random = _random.CreateRandom(region.Id);

                yield return Price.Premium(Mathf.Min(10, 1 + region.MilitaryPower / 30)).GetProduct(_factory);
                foreach (var faction in _database.FactionList.Visible().RandomUniqueElements(4, random))
                    yield return new Product(_factory.CreateResearchItem(faction), Mathf.Min(10, 1 + region.MilitaryPower / 30));

                if (random.Percentage(30))
                {
                    var tech = _research.GetAvailableTechs(region.Faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
                    if (tech != null)
                        yield return new Product(_factory.CreateBlueprintItem(tech));
                }
            }
        }

        //public IEnumerable<IProduct> GetCommonPlanetReward(Faction faction, int level, System.Random random, float successChances)
        //{
        //    if (random.NextFloat() < successChances * successChances && random.Percentage(7))
        //        yield return Price.Premium(1).GetProduct(_factory);

        //    if (random.NextFloat() < successChances * successChances && random.Percentage(2))
        //    {
        //        var tech = _research.GetAvailableTechs(faction).Where(item => item.Hidden || item.Price <= 10).RandomElement(random);
        //        if (tech != null)
        //            yield return new Product(_factory.CreateBlueprintItem(tech));
        //    }

        //    if (System.DateTime.UtcNow.IsEaster())
        //        if (random.NextFloat() < successChances * successChances && random.Percentage(2))
        //            yield return new Product(_factory.CreateShipItem(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("fns3_mk2"))).Unlocked()));
        //}

        public IEnumerable<IProduct> GetSpaceWormLoot(int level, int seed)
        {
            var random = _random.CreateRandom(seed);
            yield return new Product(CreateArtifact(CommodityType.Artifacts), 1 + random.Next2(level));
            yield return Price.Premium(5 + random.Next2(level / 20)).GetProduct(_factory);

            if (random.Percentage(30))
            {
                var tech = _research.GetAvailableTechs(Faction.Undefined).Where(item => item.Price <= 15).RandomElement(random);
                if (tech != null)
                    yield return new Product(_factory.CreateBlueprintItem(tech));
            }
        }

        public IEnumerable<IProduct> GetRuinsRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return Price.Common(5 * Maths.Distance.Credits(level)).GetProduct(_factory);
            yield return new Product(_factory.CreateFuelItem(), random.Next(1,2));

            if (random.Next(3) == 0)
            {
                var itemLevel = Mathf.Max(6, level / 2);
                var companions = _database.SatelliteList.Where(item => item.Layout.CellCount <= itemLevel && item.SizeClass != SizeClass.Titan);
                foreach (var item in companions.Where(item => item.SizeClass != SizeClass.Titan).RandomUniqueElements(1, random))
                    yield return new Product(_factory.CreateSatelliteItem(item));
            }

            foreach (var item in RandomComponents(Maths.Distance.ComponentLevel(level) + 35, random.Next(1, 5), Faction.Undefined, random, false))
                yield return new Product(item);

            var quantity = random.Next(3);
            if (quantity > 0)
                yield return Price.Premium(quantity).GetProduct(_factory);

            yield return new Product(_factory.CreateResearchItem(_database.GalaxySettings.AbandonedStarbaseFaction));
        }

        public IEnumerable<IProduct> GetXmasRewards(int level, int seed)
        {
            var random = _random.CreateRandom(seed);

            yield return new Price(random.Range(level/5 + 15, level/5 + 30), Currency.Snowflakes).GetProduct(_factory);

            var items = _database.ComponentList.CommonAndRare().LevelLessOrEqual(level + 50)
                .RandomElements(random.Range(5, 10), random).Select(item =>
                    ComponentInfo.CreateRandomModification(item, random, ModificationQuality.P3));

            if (random.Percentage(10))
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(96))))); // xmas bomb
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(215))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(220))))); // drone bay
            if (random.Percentage(5) && level > 50)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(219))))); // drone bay
            if (random.Percentage(5) && level > 100)
                yield return new Product(_factory.CreateComponentItem(new ComponentInfo(_database.GetComponent(new ItemId<Component>(213))))); // holy cannon

            foreach (var item in items)
                yield return new Product(_factory.CreateComponentItem(item));
        }

        public IEnumerable<IProduct> GetDailyReward(int day, int level, int seed)
        {
            if (day <= 0)
                yield break;

            yield return new Price(Mathf.Min(day*1000, 10000), Currency.Credits).GetProduct(_factory);

            yield return new Price(Mathf.Min(day*10, 100), Currency.Stars).GetProduct(_factory);

            if (day % 2 == 0)
                yield return new Product(_factory.CreateFuelItem(), Mathf.Min(30, 10*day/2));
            else if (day % 3 == 0)
                yield return new Product(_factory.CreateResearchItem(_database.FactionList.Visible().AtDistance(level).RandomElement(new System.Random(seed))), Mathf.Min(5,day/4));
            else if (day % 5 == 0)
                yield return Price.Premium(Mathf.Min(5,day/5)).GetProduct(_factory);

            if (day > 3)
            {
                var quality = (ComponentQuality)Mathf.Min(day/3, (int)ComponentQuality.P4);
                var component = ComponentInfo.CreateRandom(_database, level, Faction.Undefined, _random.CreateRandom(seed), false, quality);
                yield return new Product(_factory.CreateComponentItem(component));
            }
        }

        public IItemType GetRandomComponent(int distance, Faction faction, int seed, bool allowRare)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponent(distance, faction, random, allowRare);
        }

        public IEnumerable<IItemType> GetRandomComponents(int distance, int count, Faction faction, int seed, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P4)
        {
            var random = _random.CreateRandom(seed);
            return RandomComponents(distance, count, faction, random, allowRare, maxQuality);
        }

        public IItemType GetRandomFactionShip(int distance, Faction faction, int seed)
        {
            var random = _random.CreateRandom(seed);
            return RandomFactionShip(distance, faction, random);
        }

        public DamagedShipItem GetRandomDamagedShip(int distance, int seed)
        {
            var random = _random.CreateRandom(seed);

            var value = random.Next(distance);
            var ships = value > 20 ? _database.ShipBuildList.Available().NormalShips() : _database.ShipBuildList.Available().Common();
            var ship = ships.Playable().LimitLevel(value).OfFaction(Faction.Undefined, distance/2).RandomElements(1, random).First();

            return (DamagedShipItem)_factory.CreateDamagedShipItem(ship, random.Next());
        }

        private IItemType RandomFactionShip(int distance, Faction faction, System.Random random)
        {
            var ships = _database.ShipBuildList.Available().Common().Playable().OfFaction(faction).LimitLevel(distance).ToArray();
            return ships.Length > 0 ? _factory.CreateShipItem(new CommonShip(ships[random.Next(ships.Length)])) : null;
        }

        private IEnumerable<IItemType> RandomComponents(int distance, int count, Faction faction, System.Random random, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P4)
        {
            for (var i = 0; i < count; ++i)
                yield return _factory.CreateComponentItem(ComponentInfo.CreateRandom(_database, distance, faction, random, allowRare, maxQuality));
        }

        private IItemType RandomComponent(int distance, Faction faction, System.Random random, bool allowRare, ComponentQuality maxQuality = ComponentQuality.P4)
        {
            return _factory.CreateComponentItem(ComponentInfo.CreateRandom(_database, distance, faction, random, allowRare, maxQuality));
        }

        private IItemType CreateArtifact(CommodityType commodityType)
        {
            var artifact = _database.GetQuestItem(new ItemId<QuestItem>((int)commodityType));
            return _factory.CreateArtifactItem(artifact);
        }
    }
}
