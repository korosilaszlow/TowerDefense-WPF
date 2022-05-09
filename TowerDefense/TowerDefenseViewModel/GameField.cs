using System;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Az <see cref="INotifyPropertyChanged"/> interface-t megvalósító osztály, egy cellát reprezentál rácsban
    /// </summary>
    public class GameField : ViewModelBase
    {
        #region fields
        private int row, col;
        private CellType cellType;

        // 1 - there is a soldier here
        // 0 - no soldier here
        //-1 - dead soldier
        private int hasPlayer1AttackSoldier;
        private int hasPlayer2AttackSoldier;
        private int hasPlayer1TankSoldier;
        private int hasPlayer2TankSoldier;

        private bool showExplosion;
        private int towerLevel;
        #endregion
        #region events
        /// <summary>Akkor lesz kiváltva, ha végrehajtódik a <see cref="ClickCellCommand"/></summary>
        public event EventHandler<EventArgs> Clicked;
        #endregion
        #region commands
        /// <summary>Ezen a commandon keresztül lehet jelezni azt, hogy erre a cellára rákattintottak</summary>
        public DelegateCommand ClickCellCommand { get; set; }
        #endregion
        #region constructor
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="row">A cella sorának indexe a rácsban</param>
        /// <param name="col">A cella oszlopának indexe a rácsban</param>
        /// <param name="cellType">A cella típusa, alapértelmezetten mezőség</param>
        public GameField(int row, int col, CellType cellType = CellType.Plain)
        {
            Row = row;
            Col = col;
            CellType = cellType;
            Player1AttackSoldierInfo = 0;
            Player2AttackSoldierInfo = 0;
            Player1TankSoldierInfo = 0;
            Player2TankSoldierInfo = 0;
            ShowExplosion = false;

            ClickCellCommand = new DelegateCommand(
                new Action<object>(
                (_) =>
                {
                    Clicked?.Invoke(this, EventArgs.Empty);
                })
            );

            if (IsTower)
            {
                TowerLevel = 1;
            }
            else
            {
                TowerLevel = 0;
            }
        }
        #endregion
        #region properties
        /// <summary>A cella sorának indexe a rácsban </summary>
        public int Row
        {
            get => row;
            set
            {
                row = value;
                OnPropertyChanged();
            }
        }
        /// <summary>A cella oszlopának indexe a rácsban</summary>
        public int Col
        {
            get => col;
            set
            {
                col = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BGImagePath));
            }
        }
        /// <summary>A cella típusa</summary>
        public CellType CellType
        {
            get => cellType;
            set
            {
                cellType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BGImagePath));
                OnPropertyChanged(nameof(IsTower));
            }
        }
        /// <summary>Jelöli, hogy van-e kék <see cref="AttackSoldier"/> ezen a cellán. 
        /// Értékek: Van: 1 ; Nincs: 0; Ebben a szimulációs tick-ben halt meg egy itt levő: -1
        /// </summary>
        public int Player1AttackSoldierInfo
        {
            get => hasPlayer1AttackSoldier;
            set
            {
                hasPlayer1AttackSoldier = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Jelöli, hogy van-e vörös <see cref="AttackSoldier"/> ezen a cellán. 
        /// Értékek: Van: 1 ; Nincs: 0; Ebben a szimulációs tick-ben halt meg egy itt levő: -1
        /// </summary>
        public int Player2AttackSoldierInfo
        {
            get => hasPlayer2AttackSoldier;
            set
            {
                hasPlayer2AttackSoldier = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Jelöli, hogy van-e kék <see cref="TankSoldier"/> ezen a cellán. 
        /// Értékek: Van: 1 ; Nincs: 0; Ebben a szimulációs tick-ben halt meg egy itt levő: -1
        /// </summary>
        public int Player1TankSoldierInfo
        {
            get => hasPlayer1TankSoldier;
            set
            {
                hasPlayer1TankSoldier = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Jelöli, hogy van-e vörös <see cref="TankSoldier"/> ezen a cellán. 
        /// Értékek: Van: 1 ; Nincs: 0; Ebben a szimulációs tick-ben halt meg egy itt levő: -1
        /// </summary>
        public int Player2TankSoldierInfo
        {
            get => hasPlayer2TankSoldier;
            set
            {
                hasPlayer2TankSoldier = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Visszaadja, hogy ezen cella típusa torony-e. 
        /// Értékek: igaz: 1; hamis: 0
        /// </summary>
        public bool IsTower
        {
            get
            {
                return CellType == CellType.Player1DamageTower
                    || CellType == CellType.Player2DamageTower
                    || CellType == CellType.Player1RangedTower
                    || CellType == CellType.Player2RangedTower
                    || CellType == CellType.Player1SupportTower
                    || CellType == CellType.Player2SupportTower;
            }
        }
        /// <summary>
        /// Ha a cella típusa torony, visszaadja a hozzárendelt szintet
        /// </summary>
        public int TowerLevel
        {
            get => towerLevel;
            set
            {
                towerLevel = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Ha a <see cref="CellType"/>=<c>CellType.Plain</c>, akkor jelzi, hogy erre a cellára tüzelt-e torony
        /// Ha a <see cref="IsTower"/>=<c>True</c>, azaz torony típusú a cella, akkor azt jelzi, hogy tüzelt-e ez a torony
        /// Értékek: igaz: 1; hamis: 0
        /// </summary>
        public bool ShowExplosion
        {
            get => showExplosion;
            set
            {
                showExplosion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BGImagePath));
            }
        }
        /// <summary>
        /// Megadja, hogy milyen háttere legyen a cellának fájlútvonalként
        /// </summary>
        public string BGImagePath
        {
            get
            {
                switch (cellType)
                {
                    case CellType.Plain:
                        return showExplosion ? "../../../resources/plain_burning.jpg"
                                             : "../../../resources/plain.jpg";
                    case CellType.Water:
                        return "../../../resources/water.jpg";
                    case CellType.Mountain:
                        return "../../../resources/mountain.jpg";
                    case CellType.Player1Castle:
                        return "../../../resources/castle1.jpg";
                    case CellType.Player2Castle:
                        return "../../../resources/castle2.jpg";
                    case CellType.Player1RangedTower:
                        return "../../../resources/ranged_tower1.jpg";
                    case CellType.Player1SupportTower:
                        return "../../../resources/support_tower1.jpg";
                    case CellType.Player1DamageTower:
                        return "../../../resources/damage_tower1.jpg";
                    case CellType.Player2RangedTower:
                        return "../../../resources/ranged_tower2.jpg";
                    case CellType.Player2SupportTower:
                        return "../../../resources/support_tower2.jpg";
                    case CellType.Player2DamageTower:
                        return "../../../resources/damage_tower2.jpg";
                    default:
                        return "../../../resources/plain.jpg";
                }
            }
        }

        #endregion
        #region public methods
        /// <summary>
        /// Manuálisan kiváltja minden Property-re a <see cref="PropertyChanged"/> eseményt
        /// </summary>
        public void RaisePropertyChanged()
        {
            OnPropertyChanged(nameof(TowerLevel));
            OnPropertyChanged(nameof(BGImagePath));
            OnPropertyChanged(nameof(Player1AttackSoldierInfo));
            OnPropertyChanged(nameof(Player1TankSoldierInfo));
            OnPropertyChanged(nameof(Player2AttackSoldierInfo));
            OnPropertyChanged(nameof(Player2TankSoldierInfo));
            OnPropertyChanged(nameof(CellType));
            OnPropertyChanged(nameof(Row));
            OnPropertyChanged(nameof(Col));
        }
        #endregion
    }

}
