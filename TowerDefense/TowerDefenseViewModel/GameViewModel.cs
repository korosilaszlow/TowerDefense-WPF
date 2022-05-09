using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefenseModel;
using TowerDefensePersistence;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Nézetmodell a <see cref="GameModel"/> és a nézet összekapcsolására
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        #region constants
        private const int DEFAULT_ROWS = 10;
        private const int DEFAULT_COLS = 10;
        #endregion
        #region Fields
        private GameModel model;
        private PlayerActionMode playerActionMode = PlayerActionMode.None;
        private bool active;

        private ObservableCollection<GameField> fields;
        private GameField[,] grid;
        private List<GameField> highlightedCells = new List<GameField>();
        #endregion
        #region Commands
        /// <summary>Command új játék kezdéséhez</summary>
        public DelegateCommand NewGameCommand { get; set; }
        /// <summary>Command játék betöltéséhez</summary>
        public DelegateCommand LoadGameCommand { get; set; }
        /// <summary>Command játék mentéséhez</summary>
        public DelegateCommand SaveGameCommand { get; set; }
        /// <summary>Command a súgó megnyitásához</summary>
        public DelegateCommand DisplayHelpCommand { get; set; }
        /// <summary>Command a programból való kilépéshez</summary>
        public DelegateCommand ExitCommand { get; set; }
        /// <summary>Command a játékos által végzett művelet megváltoztatására</summary>
        public DelegateCommand ChangePlayerActionMode { get; set; }
        /// <summary>Command a térképszerkesztőbe való átlépésre</summary>
        public DelegateCommand EditorModeCommand { get; set; }
        /// <summary>Command a játékos által végzett művelet megváltoztatására</summary>
        public DelegateCommand EndTurnCommand { get; set; }
        /// <summary>Command támadó egység vásárlására</summary>
        public DelegateCommand BuyAttackSoldierCommand { get; set; }
        /// <summary>Command tank egység vásárlására</summary>
        public DelegateCommand BuyTankSoldierCommand { get; set; }
        /// <summary>Command a szimuláció szüneteltetésére</summary>
        public DelegateCommand PauseCommand { get; set; }

        #endregion
        #region Properties
        /// <summary>A cellarács sorainak száma </summary>
        public int GridRows
        {
            get
            {
                return model.Rows;
            }
        }
        /// <summary>A cellarács oszlopainak száma </summary>
        public int GridCols
        {
            get
            {
                return model.Cols;
            }
        }
        /// <summary>A jelenlegi játékfázis</summary>
        public GamePhase GamePhase
        {
            get
            {
                return model.GamePhase;
            }
        }
        /// <summary>A cellák egy megfigyelhető gyűjteménye</summary>
        public ObservableCollection<GameField> Fields
        {
            get => fields;
            set
            {
                fields = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Tárolja, hogy jelenleg az ehhez az objektumhoz tartozó nézet aktív-e a képernyőn</summary>
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Kényelmi property, a <see cref="IsSimulation"/> negáltja</summary>
        public bool NotSimulation { get => !IsSimulation; }
        /// <summary>Visszaadja, hogy jelenleg harci szimuláció folyik-e</summary>
        public bool IsSimulation { get => GamePhase == GamePhase.Simulation; }
        /// <summary>Visszaadja, hogy tudnak-e a játékosok műveletet végezni</summary>
        public bool CanInteractAsPlayer
        {
            get => !(GameIsOver || IsSimulation);
        }
        /// <summary>Visszaadja, hogy lehet-e használni a szünet gombot</summary>
        public bool CanUsePauseButton
        {
            get => !GameIsOver && IsSimulation;
        }
        /// <summary>Visszaadja, a kék játékos kastályának életét</summary>
        public int Player1CastleHP
        {
            get
            {
                return model.Player1CastleHP;
            }
        }
        /// <summary>Visszaadja, a vörös játékos kastályának életét</summary>
        public int Player2CastleHP
        {
            get
            {
                return model.Player2CastleHP;
            }
        }
        /// <summary>Visszaadja, a kék játékos egységeit string-ként formázva</summary>
        public string Player1UnitsText
        {
            get
            {
                var (attack, tank) = model.Player1Soldiers;
                return String.Format("{0} : {1}", attack, tank);
            }
        }
        /// <summary>Visszaadja, a vörös játékos egységeit string-ként formázva</summary>
        public string Player2UnitsText
        {
            get
            {
                var (attack, tank) = model.Player2Soldiers;
                return String.Format("{0} : {1}", attack, tank);
            }
        }
        /// <summary>Visszaadja, a kék játékos pénzét</summary>
        public int Player1Money
        {
            get
            {
                return model.Player1Money;
            }
        }
        /// <summary>Visszaadja, a vörös játékos pénzét</summary>
        public int Player2Money
        {
            get
            {
                return model.Player2Money;
            }
        }
        /// <summary>Visszaadja, hogy vége van-e a játéknak</summary>
        public bool GameIsOver
        {
            get
            {
                return model.GameIsOver;
            }

        }
        /// <summary>Visszaadja a távolsági torony építési költségét</summary>
        public int RangedTowerCost
        {
            get
            {
                return RangedTower.BuildCost();
            }
        }
        /// <summary>Visszaadja a sebzőtorony építési költségét</summary>
        public int DamageTowerCost
        {
            get
            {
                return DamageTower.BuildCost();
            }
        }
        /// <summary>Visszaadja a segédtorony építési költségét</summary>
        public int SupportTowerCost
        {
            get
            {
                return SupportTower.BuildCost();
            }
        }
        /// <summary>Visszaadja a támadó egység vásárlási költségét</summary>
        public int AttackSoldierCost
        {
            get
            {
                return AttackSoldier.BuildCost();
            }
        }
        /// <summary>Visszaadja a tank egység vásárlási költségét</summary>
        public int TankSoldierCost
        {
            get
            {
                return TankSoldier.BuildCost();
            }
        }
        /// <summary>Visszaadja egész számként elkódolva, hogy jelenleg milyen műveletet végez a soron lévő játékos</summary>
        public int PlayerActionModeNum
        {
            get => (int)playerActionMode;
            set
            {
                playerActionMode = (PlayerActionMode)value;
                OnPropertyChanged();
            }
        }
        /// <summary>Visszaadja string-ként, hogy hanyadik lépésnél jár a szimuláció</summary>
        public string StepCounterText
        {
            get
            {
                if (GamePhase == GamePhase.Simulation)
                {
                    return "Lépésszám: " + model.Step;
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>Visszaadja, hogy igaz-e, hogy jelenleg szimuláció van, de az szüneteltetve van</summary>
        public bool SimulationIsPaused { get => model.IsInPausedState; }

        #endregion
        #region Events
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott mentésre</summary>
        public event EventHandler<EventArgs> SaveGame;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott betöltésre</summary>
        public event EventHandler<EventArgs> LoadGame;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott az új játék-ra</summary>
        public event EventHandler<CancelEventArgs> NewGame;
        /// <summary>Jelzi, ha a nézetben a felhasználó érvénytelen műveletet próbált végrehajtani</summary>
        public event EventHandler<UserInteractionEventArgs> InvalidAction;
        /// <summary>Jelzi, ha hiba történt</summary>
        public event EventHandler<UserInteractionEventArgs> Error;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott a szerkesztőre való váltásra</summary>
        public event EventHandler<EventArgs> SwitchToEditor;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott a súgóra</summary>
        public event EventHandler<EventArgs> DisplayHelp;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott a kilépésre</summary>
        public event EventHandler<EventArgs> ExitProgram;
        /// <summary>Jelzi, ha a felhasználó megerősítendő műveletet akar végezni</summary>
        public event EventHandler<UserInteractionEventArgs> PlayerTransaction;
        /// <summary>Jelzi, ha a felhasználó rányomott egy cellára információ listázása céljából</summary>
        public event EventHandler<UserInteractionEventArgs> CellInfoDisplay;
        /// <summary>Jelzi, ha a vége a játéknak</summary>
        public event EventHandler<UserInteractionEventArgs> GameOver;

        #endregion
        #region Constructor
        /// <summary>
        /// Konstruktor, ami összeköti a megadott modellt a nézetmodellel
        /// </summary>
        /// <param name="model">A kezelni kívánt játékmodell-objektum</param>
        public GameViewModel(GameModel model)
        {

            this.model = model;
            model.NewGame(DEFAULT_ROWS, DEFAULT_COLS);
            model.GameOver += GameModel_GameOver;
            model.EndOfSimulation += GameModel_EndOfSimulation;
            model.GridUpdated += GameModel_GridUpdated;

            this.Active = false;



            NewGameCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    CancelEventArgs e = new CancelEventArgs();
                    NewGame?.Invoke(this, e);
                    if (!e.Cancel)
                    {
                        model.NewGame();
                        InitializeFields();
                    }
                })
            );
            LoadGameCommand = new DelegateCommand(
                new Action<object>((_) => LoadGame?.Invoke(this, EventArgs.Empty))
            );
            SaveGameCommand = new DelegateCommand(
                new Action<object>((_) => SaveGame?.Invoke(this, EventArgs.Empty))
            );
            DisplayHelpCommand = new DelegateCommand(
                new Action<object>((_) => DisplayHelp?.Invoke(this, EventArgs.Empty))
            );
            ExitCommand = new DelegateCommand(
                new Action<object>((_) => ExitProgram?.Invoke(this, EventArgs.Empty))
            );
            ChangePlayerActionMode = new DelegateCommand(
                new Action<object>((param) =>
                {
                    var mode_temp = playerActionMode;
                    switch (param as string)
                    {
                        case "BuildRangedTower":
                            playerActionMode = PlayerActionMode.BuildRangedTower; break;
                        case "BuildDamageTower":
                            playerActionMode = PlayerActionMode.BuildDamageTower; break;
                        case "BuildSupportTower":
                            playerActionMode = PlayerActionMode.BuildSupportTower; break;
                        case "UpgradeTower":
                            playerActionMode = PlayerActionMode.UpgradeTower; break;
                        case "RemoveTower":
                            playerActionMode = PlayerActionMode.RemoveTower; break;
                        default:
                            playerActionMode = PlayerActionMode.None; break;
                    }
                    if (mode_temp == playerActionMode)
                    {
                        playerActionMode = PlayerActionMode.None;
                    }
                    OnPropertyChanged(nameof(PlayerActionModeNum));
                })
            );

            EditorModeCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    SwitchToEditor?.Invoke(this, EventArgs.Empty);
                    Active = false;
                })
            );
            EndTurnCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    model.NextGamePhase();
                    PlayerActionModeNum = (int)PlayerActionMode.None;
                    OnPropertyChanged(nameof(GamePhase));
                    OnPropertyChanged(nameof(CanInteractAsPlayer));
                    OnPropertyChanged(nameof(NotSimulation));
                    OnPropertyChanged(nameof(IsSimulation));
                    OnPropertyChanged(nameof(CanUsePauseButton));
                })
            );
            BuyAttackSoldierCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    switch (model.CanBuyAttackSoldier())
                    {
                        case GridExceptionEnum.NO_PROBLEM:
                            model.BuyAttackSoldier();
                            break;
                        case GridExceptionEnum.MONEY:
                            InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ehhez nincs elég aranyad."));
                            break;
                        case GridExceptionEnum.INVALID_GAME_STATE:
                            InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez jelenleg nem lehetséges."));
                            break;
                        default:
                            Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
                            break;
                    }

                })
            );
            BuyTankSoldierCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    switch (model.CanBuyTankSoldier())
                    {
                        case GridExceptionEnum.NO_PROBLEM:
                            model.BuyTankSoldier();
                            break;
                        case GridExceptionEnum.MONEY:
                            InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ehhez nincs elég aranyad."));
                            break;
                        case GridExceptionEnum.INVALID_GAME_STATE:
                            InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez jelenleg nem lehetséges."));
                            break;
                        default:
                            Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
                            break;
                    }
                })
            );

            PauseCommand = new DelegateCommand(
                (_) =>
                {
                    if (GamePhase != GamePhase.Simulation) { return; }
                    if (SimulationIsPaused)
                    {
                        model.UnPauseSimulation();
                    }
                    else
                    {
                        model.PauseSimulation();
                    }
                    OnPropertyChanged(nameof(SimulationIsPaused));
                }
            );

            InitializeFields();
        }
        #endregion
        #region Async methods
        /// <summary>
        /// A megadott fájlútvonalra aszinkron elmenti a játékot
        /// </summary>
        /// <param name="fname">A mentés helye fájlnévként</param>
        /// <returns>Az aszinkron híváshoz tartozó <see cref="Task"/></returns>
        public async Task Save(string path)
        {
            await model.SaveGame(path);
        }
        /// <summary>
        /// A megadott fájlútvonalról aszinkron betölti a játékot
        /// </summary>
        /// <param name="fname">A betöltés forrása fájlnévként</param>
        /// <returns>Az aszinkron híváshoz tartozó <see cref="Task"/></returns>
        public async Task Load(string path)
        {
            await model.LoadGame(path);
            InitializeFields();

        }
        #endregion
        #region Private methods
        private void InitializeFields()
        {
            if (fields != null)
            {
                foreach (var cell in fields)
                {
                    cell.Clicked -= GameField_Clicked;
                }
            }

            grid = new GameField[model.Rows, model.Cols];

            fields = new ObservableCollection<GameField>();

            for (int i = 0; i < model.Rows; i++)
            {
                for (int j = 0; j < model.Cols; j++)
                {

                    grid[i, j] = new GameField(i, j, model[i, j].GetCellType());
                    grid[i, j].Clicked += GameField_Clicked;
                    if (model[i, j] is Tower)
                    {
                        grid[i, j].TowerLevel = (model[i, j] as Tower).Level;
                    }
                    else if (model[i, j] is SoldierContainer)
                    {
                        SoldierContainer sc = model[i, j] as SoldierContainer;
                        grid[i, j].Player1AttackSoldierInfo = sc.Player1Soldiers.Any(x => x is AttackSoldier) ? 1 : 0;
                        grid[i, j].Player1TankSoldierInfo = sc.Player1Soldiers.Any(x => x is TankSoldier) ? 1 : 0;
                        grid[i, j].Player2AttackSoldierInfo = sc.Player2Soldiers.Any(x => x is AttackSoldier) ? 1 : 0;
                        grid[i, j].Player2TankSoldierInfo = sc.Player2Soldiers.Any(x => x is TankSoldier) ? 1 : 0;
                    }
                    fields.Add(grid[i, j]);
                }
            }

            OnPropertyChanged(nameof(Fields));
            OnPropertyChanged(nameof(GridRows));
            OnPropertyChanged(nameof(GridCols));
            OnPropertyChanged(nameof(Active));
            OnPropertyChanged(nameof(GamePhase));
            OnPropertyChanged(nameof(Player1CastleHP));
            OnPropertyChanged(nameof(Player2CastleHP));
            OnPropertyChanged(nameof(Player1UnitsText));
            OnPropertyChanged(nameof(Player2UnitsText));
            OnPropertyChanged(nameof(Player1Money));
            OnPropertyChanged(nameof(Player2Money));
            OnPropertyChanged(nameof(GameIsOver));
            OnPropertyChanged(nameof(CanInteractAsPlayer));
            OnPropertyChanged(nameof(NotSimulation));
            OnPropertyChanged(nameof(IsSimulation));
            OnPropertyChanged(nameof(CanUsePauseButton));
            OnPropertyChanged(nameof(SimulationIsPaused));
            foreach (var c in fields)
            {
                c.RaisePropertyChanged();
            }

        }

        private void DisplayCellInfo(GameField gameField)
        {
            if (GamePhase == GamePhase.Simulation && !SimulationIsPaused)
            {
                PauseCommand.Execute(null);
            }
            string msg;
            StringBuilder sb = new StringBuilder();
            switch (gameField.CellType)
            {
                case CellType.Plain:
                    Plain p = model[gameField.Row, gameField.Col] as Plain;
                    sb.AppendLine("Mezőség: ");
                    int addedSoldiers = 0;

                    foreach (Soldier s in p.Player1Soldiers)
                    {
                        addedSoldiers++;
                        sb.AppendLine(String.Format("Kék {0} egység, {1} életerő", (s is AttackSoldier) ? "sebző" : "tank", s.HitPoints));
                    }

                    foreach (Soldier s in p.Player2Soldiers)
                    {
                        addedSoldiers++;
                        sb.AppendLine(String.Format("Vörös {0} egység, {1} életerő", (s is AttackSoldier) ? "sebző" : "tank", s.HitPoints));
                    }

                    if (0 == addedSoldiers)
                    {
                        sb.AppendLine("Ezen a cellán jelenleg nincs katonai egység.");
                    }

                    break;
                case CellType.Water:
                    sb.AppendLine("Állóvíz");
                    break;
                case CellType.Mountain:
                    sb.AppendLine("Hegység");
                    break;
                case CellType.Player1Castle:
                    Castle c = model[gameField.Row, gameField.Col] as Castle;
                    sb.AppendLine("Kék kastély: ");
                    sb.AppendLine(String.Format("Kastély életereje: {0}", c.Hitpoints));

                    addedSoldiers = 0;

                    foreach (Soldier s in c.Player1Soldiers)
                    {
                        addedSoldiers++;
                        sb.AppendLine(String.Format("Kék {0} egység, {1} életerő", (s is AttackSoldier) ? "sebző" : "tank", s.HitPoints));
                    }

                    if (0 == addedSoldiers)
                    {
                        sb.AppendLine("Ezen a cellán jelenleg nincs katonai egység.");
                    }

                    break;
                case CellType.Player2Castle:
                    c = model[gameField.Row, gameField.Col] as Castle;
                    sb.AppendLine("Vörös kastély: ");
                    sb.AppendLine(String.Format("Kastély életereje: {0}", c.Hitpoints));

                    addedSoldiers = 0;

                    foreach (Soldier s in c.Player2Soldiers)
                    {
                        addedSoldiers++;
                        sb.AppendLine(String.Format("Vörös {0} egység, {1} életerő", (s is AttackSoldier) ? "sebző" : "tank", s.HitPoints));
                    }

                    if (0 == addedSoldiers)
                    {
                        sb.AppendLine("Ezen a cellán jelenleg nincs katonai egység.");
                    }

                    break;
                case CellType.Player1RangedTower:
                    sb.AppendLine("Kék távtorony: ");
                    break;
                case CellType.Player1SupportTower:
                    sb.AppendLine("Kék segédtorony: ");
                    break;
                case CellType.Player1DamageTower:
                    sb.AppendLine("Kék sebzőtorony: ");
                    break;
                case CellType.Player2RangedTower:
                    sb.AppendLine("Vörös távtorony: ");
                    break;
                case CellType.Player2SupportTower:
                    sb.AppendLine("Vörös segédtorony: ");
                    break;
                case CellType.Player2DamageTower:
                    sb.AppendLine("Vörös sebzőtorony: ");
                    break;
                default:
                    break;
            }

            if (gameField.IsTower)
            {
                Tower t = model[gameField.Row, gameField.Col] as Tower;
                string upgradeMessage = t.IsMaxLevel() ? "Ez a torony már elérte a maximális szintet."
                                                       : String.Format("Következő szintre fejlesztés költsége: {0} arany", t.UpgradeCost());
                sb.Append(String.Format("Szint: {0}\nSebzés lövésenként: {1}\nHatótáv: {2} cella\nEgy lépésben ledaható lövések: {3}\n{4}\n",
                          t.Level,
                          t.Damage,
                          t.Range,
                          t.TargetCount,
                          upgradeMessage));
            }

            msg = sb.ToString();
            CellInfoDisplay?.Invoke(this, new UserInteractionEventArgs(msg));
        }

        private void TryUpgradeTower(int row, int col)
        {
            switch (model.CanUpgradeTower(row, col))
            {

                case GridExceptionEnum.MONEY:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Nincs elég aranyad ehhez."));
                    break;
                case GridExceptionEnum.INVALID_CELL:
                    if (model[row, col] is Tower)
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Csak a saját tornyaidat fejlesztheted."));
                    }
                    else
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Csak tornyot tudsz fejleszteni."));
                    }
                    break;
                case GridExceptionEnum.INVALID_GAME_STATE:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez a művelet most nem hajtható végre."));
                    break;
                case GridExceptionEnum.TOWER_MAX_LEVEL:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez a torony már elérte a maximális szintet."));
                    break;
                case GridExceptionEnum.NO_PROBLEM:
                    var transactionEventArgs = new UserInteractionEventArgs(String.Format("Torony fejlesztése: {0} arany",
                                                                        (model[row, col] as Tower).UpgradeCost()));
                    PlayerTransaction?.Invoke(this, transactionEventArgs);
                    if (transactionEventArgs.Cancel)
                    {
                        break;
                    }
                    model.UpgradeTower(row, col);
                    break;
                default:
                    throw new Exception("Ismeretlen hiba lépett fel.");
            }



        }

        private void TryRemoveTower(int row, int col)
        {
            switch (model.CanRemoveTower(row, col))
            {
                case GridExceptionEnum.INVALID_CELL:
                    if (model[row, col] is Tower)
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Csak a saját tornyaidat rombolhatod le."));
                    }
                    else
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Csak tornyot tudsz rombolni."));
                    }
                    break;
                case GridExceptionEnum.INVALID_GAME_STATE:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez a művelet most nem hajtható végre."));
                    break;
                case GridExceptionEnum.NO_PROBLEM:
                    var transactionEventArgs = new UserInteractionEventArgs(String.Format("Torony rombolása: {0} arany visszajár",
                                                                        (model[row, col] as Tower).RemovalGain()));
                    PlayerTransaction?.Invoke(this, transactionEventArgs);
                    if (transactionEventArgs.Cancel)
                    {
                        break;
                    }
                    model.RemoveTower(row, col);
                    break;
                default:
                    throw new Exception("Ismeretlen hiba lépett fel.");
            }
        }

        private void TowerBuildInvalidAction(GridExceptionEnum exc)
        {
            switch (exc)
            {
                case GridExceptionEnum.MONEY:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Nincs elég aranyad ehhez."));
                    break;
                case GridExceptionEnum.INVALID_CELL:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Csak üres cellára tudsz tornyot építeni."));
                    break;
                case GridExceptionEnum.INVALID_GAME_STATE:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ez a művelet most nem hajtható végre."));
                    break;
                case GridExceptionEnum.SOLDIER_PATH_CUT:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("A lerakott torony elvágná a katonák útját a két vár között."));
                    break;
                case GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("A torony túl közel lenne az ellenfél várához"));
                    break;
                case GridExceptionEnum.NO_NEARBY_ALLY_TOWER:
                    InvalidAction?.Invoke(this, new UserInteractionEventArgs("Túl messze vagy a bázisodtól: nincs a közelben se tornyod, se a várad."));
                    break;
                default:
                    throw new Exception("Ismeretlen hiba lépett fel.");

            }
        }
        private void TryBuildRangedTower(int row, int col)
        {
            GridExceptionEnum exc = model.CanBuildRangedTower(row, col);
            if (exc == GridExceptionEnum.NO_PROBLEM)
            {
                UserInteractionEventArgs transactionEventArgs = new UserInteractionEventArgs(String.Format("Távolsági torony vásárlás: {0} arany",
                                                                                                   RangedTowerCost));
                PlayerTransaction?.Invoke(this, transactionEventArgs);
                if (!transactionEventArgs.Cancel)
                {
                    model.PlaceRangedTower(row, col);
                }
            }
            else
            {
                TowerBuildInvalidAction(exc);
            }
        }

        private void TryBuildDamageTower(int row, int col)
        {
            GridExceptionEnum exc = model.CanBuildDamageTower(row, col);
            if (exc == GridExceptionEnum.NO_PROBLEM)
            {
                UserInteractionEventArgs transactionEventArgs = new UserInteractionEventArgs(String.Format("Sebzőtorony vásárlás: {0} arany",
                                                                                                       DamageTowerCost));
                PlayerTransaction?.Invoke(this, transactionEventArgs);
                if (!transactionEventArgs.Cancel)
                {
                    model.PlaceDamageTower(row, col);
                }
            }
            else
            {
                TowerBuildInvalidAction(exc);
            }
        }

        private void TryBuildSupportTower(int row, int col)
        {
            GridExceptionEnum exc = model.CanBuildSupportTower(row, col);
            if (exc == GridExceptionEnum.NO_PROBLEM)
            {
                UserInteractionEventArgs transactionEventArgs = new UserInteractionEventArgs(String.Format("Segédtorony vásárlás: {0} arany",
                                                                                                       SupportTowerCost));
                PlayerTransaction?.Invoke(this, transactionEventArgs);
                if (!transactionEventArgs.Cancel)
                {
                    model.PlaceSupportTower(row, col);
                }
            }
            else
            {
                TowerBuildInvalidAction(exc);
            }
        }

        private void ResetHighlighteCells()
        {
            foreach (var cell in highlightedCells)
            {
                cell.ShowExplosion = false;
                if (cell.Player1AttackSoldierInfo == -1) { cell.Player1AttackSoldierInfo = 0; }
                if (cell.Player2AttackSoldierInfo == -1) { cell.Player2AttackSoldierInfo = 0; }
                if (cell.Player1TankSoldierInfo == -1) { cell.Player1TankSoldierInfo = 0; }
                if (cell.Player2TankSoldierInfo == -1) { cell.Player2TankSoldierInfo = 0; }
            }
            highlightedCells.Clear();
        }
        #endregion
        #region Handler methods
        private void GameModel_GameOver(object sender, GameOverEventArgs args)
        {
            OnPropertyChanged(nameof(NotSimulation));
            OnPropertyChanged(nameof(IsSimulation));
            OnPropertyChanged(nameof(CanInteractAsPlayer));
            OnPropertyChanged(nameof(CanUsePauseButton));
            string msg = null;
            switch (args.Winner)
            {
                case PlayerType.Player1:
                    msg = "A kék játékos győzött";
                    break;
                case PlayerType.Player2:
                    msg = "A piros játékos győzött";
                    break;
                case PlayerType.None:
                    msg = "Döntetlen: mindkét vár egyszerre semmisült meg";
                    break;
            }
            Task.Run(() => GameOver?.Invoke(this, new UserInteractionEventArgs(msg)));
        }
        private void GameModel_EndOfSimulation(object sender, EventArgs args)
        {
            ResetHighlighteCells();
            OnPropertyChanged(nameof(GamePhase));
            OnPropertyChanged(nameof(Player1Money));
            OnPropertyChanged(nameof(Player2Money));
            OnPropertyChanged(nameof(CanInteractAsPlayer));
            OnPropertyChanged(nameof(NotSimulation));
            OnPropertyChanged(nameof(IsSimulation));
            OnPropertyChanged(nameof(StepCounterText));
            OnPropertyChanged(nameof(CanUsePauseButton));
        }
        private void GameModel_GridUpdated(object sender, GridUpdatedEventArgs args)
        {
            ResetHighlighteCells();

            foreach (var update in args.Updates)
            {
                var cell = grid[update.Cell.Row, update.Cell.Col];
                switch (update.CellUpdateType)
                {
                    case CellUpdateType.Fired:
                        cell.ShowExplosion = true;
                        highlightedCells.Add(cell);
                        break;
                    case CellUpdateType.Hit:
                        cell.ShowExplosion = true;
                        highlightedCells.Add(cell);
                        break;
                    case CellUpdateType.Changed:
                        cell.CellType = update.Cell.GetCellType();
                        if (update.Cell is Tower)
                        {
                            cell.TowerLevel = (update.Cell as Tower).Level;
                        }
                        else
                        {
                            cell.TowerLevel = 0;
                        }
                        break;
                    case CellUpdateType.Upgraded:
                        cell.TowerLevel = (update.Cell as Tower).Level;
                        break;
                    case CellUpdateType.SoldierEntered:
                    case CellUpdateType.SoldierLeft:
                        SoldierContainer sc = update.Cell as SoldierContainer;
                        cell.Player1AttackSoldierInfo = sc.Player1Soldiers.Any(x => x is AttackSoldier) ? 1 : 0;
                        cell.Player1TankSoldierInfo = sc.Player1Soldiers.Any(x => x is TankSoldier) ? 1 : 0;
                        cell.Player2AttackSoldierInfo = sc.Player2Soldiers.Any(x => x is AttackSoldier) ? 1 : 0;
                        cell.Player2TankSoldierInfo = sc.Player2Soldiers.Any(x => x is TankSoldier) ? 1 : 0;
                        break;
                    case CellUpdateType.SoldierDied:
                        sc = update.Cell as SoldierContainer;
                        cell.Player1AttackSoldierInfo = sc.Player1DeadSoldiers.Any(x => x is AttackSoldier) ? -1 : cell.Player1AttackSoldierInfo;
                        cell.Player1TankSoldierInfo = sc.Player1DeadSoldiers.Any(x => x is TankSoldier) ? -1 : cell.Player1TankSoldierInfo;
                        cell.Player2AttackSoldierInfo = sc.Player2DeadSoldiers.Any(x => x is AttackSoldier) ? -1 : cell.Player2AttackSoldierInfo;
                        cell.Player2TankSoldierInfo = sc.Player2DeadSoldiers.Any(x => x is TankSoldier) ? -1 : cell.Player2TankSoldierInfo;
                        break;
                }
            }

            OnPropertyChanged(nameof(Fields));
            OnPropertyChanged(nameof(GamePhase));
            OnPropertyChanged(nameof(Player1CastleHP));
            OnPropertyChanged(nameof(Player2CastleHP));
            OnPropertyChanged(nameof(Player1UnitsText));
            OnPropertyChanged(nameof(Player2UnitsText));
            OnPropertyChanged(nameof(Player1Money));
            OnPropertyChanged(nameof(Player2Money));
            OnPropertyChanged(nameof(StepCounterText));
        }

        private void GameField_Clicked(object sender, EventArgs args)
        {


            if (GamePhase == GamePhase.Simulation || playerActionMode == PlayerActionMode.None)
            {
                DisplayCellInfo(sender as GameField);
                return;
            }


            int row = (sender as GameField).Row,
                col = (sender as GameField).Col;
            try
            {
                switch (playerActionMode)
                {
                    case PlayerActionMode.BuildRangedTower:
                        TryBuildRangedTower(row, col);
                        break;

                    case PlayerActionMode.BuildDamageTower:
                        TryBuildDamageTower(row, col);
                        break;
                    case PlayerActionMode.BuildSupportTower:
                        TryBuildSupportTower(row, col);
                        break;
                    case PlayerActionMode.UpgradeTower:
                        TryUpgradeTower(row, col);
                        break;
                    case PlayerActionMode.RemoveTower:
                        TryRemoveTower(row, col);
                        break;
                    default:
                        throw new Exception("Ismeretlen hiba lépett fel.");

                }
            }
            catch
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
            }

        }
        #endregion       
    }

}
