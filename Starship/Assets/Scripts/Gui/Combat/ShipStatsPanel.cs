using Combat.Component.Ship;
using Combat.Unit;
using Gui.Controls;
using Gui.Windows;
using Services.Gui;
using Services.Reources;
using UnityEngine;
using UnityEngine.UI;
using ViewModel;
using Zenject;

namespace Gui.Combat
{
    [RequireComponent(typeof(AnimatedWindow))]
    public class ShipStatsPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] private ProgressBar _armorPoints;
        [SerializeField] private ProgressBar _structurePoints;
        [SerializeField] private ProgressBar _shieldPoints;
        [SerializeField] private ProgressBar _energyPoints;
        [SerializeField] private Image _icon;
        [SerializeField] private SelectShipPanelItemViewModel _shipItem;
        [SerializeField] private GameObject _fireResistIcon;
        [SerializeField] private GameObject _energyResistIcon;
        [SerializeField] private GameObject _kineticResistIcon;
        [SerializeField] private GameObject _structureResistIcon;
        [SerializeField] private Text _fireResistText;
        [SerializeField] private Text _energyResistText;
        [SerializeField] private Text _kineticResistText;
        [SerializeField] private Text _structureResistText;

        public void Close()
        {
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }

        public void Open(IShip ship)
        {
            if (!ship.IsActive())
                return;

            GetComponent<AnimatedWindow>().Open();

            if (_ship == ship)
                return;

            _ship = ship;

            if (_icon)
                _icon.sprite = _resourceLocator.GetSprite(ship.Specification.Stats.IconImage) ?? _resourceLocator.GetSprite(ship.Specification.Stats.ModelImage);

            _shipItem.SetLevel(ship.Specification.Type.Level);
            _shipItem.SetClass(ship.Specification.Type.Class);

            UpdateResistance();

            _hasShield = _ship.Stats.Shield.Exists;
            _hasArmor = _ship.Stats.Armor.Exists;
            _hasStructure = _ship.Stats.Structure.Exists;

            _shieldPoints.gameObject.SetActive(_hasShield);
            _armorPoints.gameObject.SetActive(_hasArmor);
            _structurePoints.gameObject.SetActive(_hasStructure);
        }

        private void UpdateResistance()
        {
            var resistance = _ship.Stats.Resistance;

            if (_fireResistIcon != null)
            {
                var active = resistance.Heat > 0.01f;
                _fireResistIcon.gameObject.SetActive(active);
                _fireResistText.gameObject.SetActive(active);
                if (active)
                    _fireResistText.text = Mathf.RoundToInt(resistance.Heat * 100) + "%";
            }

            if (_energyResistIcon != null)
            {
                var active = resistance.Energy > 0.01f;
                _energyResistIcon.gameObject.SetActive(active);
                _energyResistText.gameObject.SetActive(active);
                if (active)
                    _energyResistText.text = Mathf.RoundToInt(resistance.Energy * 100) + "%";
            }

            if (_kineticResistIcon != null)
            {
                var active = resistance.Kinetic > 0.01f;
                _kineticResistIcon.gameObject.SetActive(active);
                _kineticResistText.gameObject.SetActive(active);
                if (active)
                    _kineticResistText.text = Mathf.RoundToInt(resistance.Kinetic * 100) + "%";
            }

            if (_structureResistIcon != null)
            {
                var active = resistance.Structure > 0.01f;
                _structureResistIcon.gameObject.SetActive(active);
                _structureResistText.gameObject.SetActive(active);
                if (active)
                    _structureResistText.text = Mathf.RoundToInt(resistance.Structure * 100) + "%";
            }
        }

        private void Update()
        {
            if (!_ship.IsActive())
            {
                Close();
                return;
            }

            _updateResistanceCooldown -= Time.deltaTime;
            if (_updateResistanceCooldown <= 0)
            {
                _updateResistanceCooldown = 0.5f;
                UpdateResistance();
            }

            var total = 0f;
            if (_hasStructure) total += _ship.Stats.Structure.MaxValue;
            if (_hasArmor) total += _ship.Stats.Armor.MaxValue;
            if (_hasShield) total += _ship.Stats.Shield.MaxValue;

            var structure = _structurePoints ? _ship.Stats.Structure.Value : 0;
            var armor = _hasArmor ? _ship.Stats.Armor.Value : 0;
            var shield = _hasShield ? _ship.Stats.Shield.Value : 0;

            if (_hasStructure)
            {
                _structurePoints.X0 = 0;
                _structurePoints.X1 = structure / total;
                _structurePoints.SetAllDirty();
            }
            if (_hasArmor)
            {
                _armorPoints.X0 = structure / total;
                _armorPoints.X1 = (armor + structure) / total;
                _armorPoints.SetAllDirty();
            }
            if (_hasShield)
            {
                _shieldPoints.X0 = (armor + structure) / total;
                _shieldPoints.X1 = (armor + structure + shield) / total;
                _shieldPoints.SetAllDirty();
            }

            var energy = _ship.Stats.Energy.Percentage;
            if (!Mathf.Approximately(_energyPoints.X1, energy))
            {
                _energyPoints.X1 = energy;
                _energyPoints.SetAllDirty();
            }
        }

        private float _updateResistanceCooldown;
        private bool _hasShield;
        private bool _hasArmor;
        private bool _hasStructure;
        private IShip _ship;
    }
}
