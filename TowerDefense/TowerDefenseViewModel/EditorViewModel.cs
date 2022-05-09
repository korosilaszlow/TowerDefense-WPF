using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using TowerDefenseModel;
using TowerDefensePersistence;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Nézetmodell az <see cref="EditorModel"/> és a nézet összekapcsolására
    /// </summary>
    public class EditorViewModel : ViewModelBase
    {
        #region Constants
        private const int DEFAULT_ROWS = 10;
        private const int DEFAULT_COLS = 10;
        private const int DEFAULT_STARTMONEY = 500;
        #endregion
        #region Fields
        private EditorModel model;
        private ObservableCollection<GameField> fields;
        private GameField[,] grid;
        private BuildActionMode currentBuildActionMode;
        private BuildActionMode rotatingBuildActionMode;
        private bool active;
        #endregion
        #region Events
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott az extra beállításokra</summary>
        public event EventHandler<EventArgs> ExtraSettings;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott mentésre</summary>
        public event EventHandler<EventArgs> SaveMap;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott betöltésre</summary>
        public event EventHandler<EventArgs> LoadMap;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott az új játékra</summary>
        public event EventHandler<CancelEventArgs> NewMap;
        /// <summary>Jelzi, ha a nézetben a felhasználó érvénytelen műveletet próbált végrehajtani</summary>
        public event EventHandler<UserInteractionEventArgs> InvalidAction;
        /// <summary>Jelzi, ha a valamilyen hiba történt</summary>
        public event EventHandler<UserInteractionEventArgs> Error;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott az alapjátékhoz való visszatérésre</summary>
        public event EventHandler<EventArgs> SwitchToGame;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott a súgóra</summary>
        public event EventHandler<EventArgs> DisplayHelp;
        /// <summary>Jelzi, ha a nézetben a felhasználó rányomott a kilépésre</summary>
        public event EventHandler<EventArgs> ExitProgram;
        #endregion
        #region Commands
        /// <summary>Command a lerakott cella típusának megváltoztatására</summary>
        public DelegateCommand ChangeBuildActionMode { get; set; }
        /// <summary>Command új üres pálya generálására</summary>
        public DelegateCommand NewMapCommand { get; set; }
        /// <summary>Command a pálya elmentésére</summary>
        public DelegateCommand SaveMapCommand { get; set; }
        /// <summary>Command egy pálya betöltésére</summary>
        public DelegateCommand LoadMapCommand { get; set; }
        /// <summary>Command a súgó megnyitására</summary>
        public DelegateCommand DisplayHelpCommand { get; set; }
        /// <summary>Command a szerkesztő extra beállításainak megnyitására</summary>
        public DelegateCommand ExtraSettingsCommand { get; set; }
        /// <summary>Command az alapjátékra visszaváltásra</summary>
        public DelegateCommand GameModeCommand { get; set; }
        /// <summary>Command a programból való kilépésre</summary>
        public DelegateCommand ExitCommand { get; set; }
        #endregion
        #region Properties
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

        /// <summary>A cellarács sorainak száma</summary>
        public int GridRows
        {
            get
            {
                return model.Rows;
            }

        }
        /// <summary>A cellarács oszlopainak száma</summary>
        public int GridCols
        {
            get
            {
                return model.Cols;
            }

        }
        /// <summary>A jelenleg lerakott cella típusa számként elkódolva</summary>
        public int CurrentBuildActionModeNum
        {
            get => (int)currentBuildActionMode; set
            {
                currentBuildActionMode = (BuildActionMode)value;
                OnPropertyChanged();
            }
        }
        /// <summary>Ha nem várat helyezünk át, megegyezik <see cref="CurrentBuildActionModeNum"/>-pal. Ha <see cref="CurrentBuildActionModeNum"/> valamelyik kastély áthelyezését jelzi, akkor ez megjegyzi az előzőleg lerakott cella típusát</summary>
        public int RotatingBuildActionModeNum
        {
            get => (int)rotatingBuildActionMode;
            set
            {
                rotatingBuildActionMode = (BuildActionMode)value;
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
        /// <summary>A játékosok kezdőtőkéjét kéri le és állítja be a modellben</summary>
        public int StartMoney
        {
            get
            {
                return model.StartMoney;
            }
            set
            {
                model.StartMoney = value;
                OnPropertyChanged();
            }
        }

      
        #endregion
        #region Constructor
        /// <summary>
        /// Konstruktor, ami összeköti a megadott modellt a nézetmodellel
        /// </summary>
        /// <param name="model">A kezelni kívánt szerkesztőmodell-objektum</param>
        public EditorViewModel(EditorModel model)
        {

            this.model = model;
            this.Active = false;
            currentBuildActionMode = BuildActionMode.PlacePlain;
            rotatingBuildActionMode = currentBuildActionMode;

            model.GridUpdated += Model_GridUpdated;
            model.ResizeMap(DEFAULT_ROWS, DEFAULT_COLS);
            model.StartMoney = DEFAULT_STARTMONEY;

            ChangeBuildActionMode = new DelegateCommand(
                    (param) =>
                    {
                        switch (param as string)
                        {
                            case "Cycle":
                                if (currentBuildActionMode != BuildActionMode.RepositionCastle1
                                 && currentBuildActionMode != BuildActionMode.RepositionCastle2)
                                {
                                    rotatingBuildActionMode = NextBuildAction(rotatingBuildActionMode);
                                }
                                currentBuildActionMode = rotatingBuildActionMode;
                                break;
                            case "Castle1":
                                if (currentBuildActionMode == BuildActionMode.RepositionCastle1)
                                {
                                    currentBuildActionMode = rotatingBuildActionMode;
                                }
                                else
                                {
                                    currentBuildActionMode = BuildActionMode.RepositionCastle1;
                                }
                                break;
                            case "Castle2":
                                if (currentBuildActionMode == BuildActionMode.RepositionCastle2)
                                {
                                    currentBuildActionMode = rotatingBuildActionMode;
                                }
                                else
                                {
                                    currentBuildActionMode = BuildActionMode.RepositionCastle2;
                                }
                                break;

                        }
                        OnPropertyChanged(nameof(CurrentBuildActionModeNum));
                        OnPropertyChanged(nameof(RotatingBuildActionModeNum));
                    }

                );

            NewMapCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    CancelEventArgs e = new CancelEventArgs();
                    NewMap?.Invoke(this, e);
                    if (!e.Cancel)
                    {
                        model.ResizeMap(GridRows, GridCols);
                        InitializeData();
                    }
                })
            );
            LoadMapCommand = new DelegateCommand(
                new Action<object>((_) => LoadMap?.Invoke(this, EventArgs.Empty))
           );
            SaveMapCommand = new DelegateCommand(
                new Action<object>((_) => SaveMap?.Invoke(this, EventArgs.Empty))
            );
            ExitCommand = new DelegateCommand(
                new Action<object>((_) => ExitProgram?.Invoke(this, EventArgs.Empty))
           );
            DisplayHelpCommand = new DelegateCommand(
                new Action<object>((_) => DisplayHelp?.Invoke(this, EventArgs.Empty))
            );
            GameModeCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    SwitchToGame?.Invoke(this, EventArgs.Empty);
                    Active = false;
                })
            );

            ExtraSettingsCommand = new DelegateCommand(
                new Action<object>((_) =>
                {
                    ExtraSettings?.Invoke(this, EventArgs.Empty);
                })
            );

            InitializeData();
        }
        #endregion
        #region Private methods
        private BuildActionMode NextBuildAction(BuildActionMode buildActionMode)
        {
            switch (buildActionMode)
            {
                case BuildActionMode.RepositionCastle1:
                    return BuildActionMode.RepositionCastle1;
                case BuildActionMode.RepositionCastle2:
                    return BuildActionMode.RepositionCastle2;
                case BuildActionMode.PlacePlain:
                    return BuildActionMode.PlaceWater;
                case BuildActionMode.PlaceWater:
                    return BuildActionMode.PlaceMountain;
                case BuildActionMode.PlaceMountain:
                    return BuildActionMode.PlacePlain;
                default:
                    throw new Exception(); //unreachable code
            }
        }

        private void InitializeData()
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
                    fields.Add(grid[i, j]);
                }
            }

            OnPropertyChanged(nameof(Fields));
            OnPropertyChanged(nameof(GridRows));
            OnPropertyChanged(nameof(GridCols));
            OnPropertyChanged(nameof(Active));
            foreach (var c in fields)
            {
                c.RaisePropertyChanged();
            }
        }

        private void GameField_Clicked(object sender, EventArgs e)
        {
            GameField field = sender as GameField;
            int row = field.Row, col = field.Col;
            try
            {
                switch (currentBuildActionMode)
                {
                    case BuildActionMode.RepositionCastle1:
                        TryRepositionCastle1(row, col);
                        break;
                    case BuildActionMode.RepositionCastle2:
                        TryRepositionCastle2(row, col);
                        break;
                    case BuildActionMode.PlacePlain:
                        TryPlaceObstacle(row, col, CellType.Plain);
                        break;
                    case BuildActionMode.PlaceWater:
                        TryPlaceObstacle(row, col, CellType.Water);
                        break;
                    case BuildActionMode.PlaceMountain:
                        TryPlaceObstacle(row, col, CellType.Mountain);
                        break;
                    default:
                        throw new Exception(); //unreachable code                        
                }
            }
            catch
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
            }

        }

        private void TryPlaceObstacle(int row, int col, CellType type)
        {
            try
            {
                ObstaclePhase obstacle = type switch
                {
                    CellType.Plain => ObstaclePhase.Empty,
                    CellType.Water => ObstaclePhase.Water,
                    CellType.Mountain => ObstaclePhase.Mountain,
                    _ => throw new Exception() //unreachable code
                };


                if (model.CanPlace(row, col, obstacle))
                {
                    model.CellPlace(row, col, obstacle);
                }
                else
                {
                    if (model[row, col] is Castle)
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("A várat nem írhatod felül más cellával."));
                    }
                    else
                    {
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("A cella elvágná a két vár között az utat."));
                    }
                }
            }
            catch (Exception)
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
            }
        }

        private void TryRepositionCastle1(int row, int col)
        {
            try
            {
                switch (model.CanMapCastleOne(row, col))
                {
                    case GridExceptionEnum.NO_PROBLEM:
                        model.MapCastleOne(row, col);
                        break;
                    case GridExceptionEnum.SOLDIER_PATH_CUT:
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ide nem tudod áthelyezni a várat (nem lenne út a két vár között)."));
                        break;
                    case GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE:
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ide nem tudod áthelyezni a várat (túl közel lenne a két vár)."));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
            }
        }
        private void TryRepositionCastle2(int row, int col)
        {
            try
            {
                switch (model.CanMapCastleTwo(row, col))
                {
                    case GridExceptionEnum.NO_PROBLEM:
                        model.MapCastleTwo(row, col);
                        break;
                    case GridExceptionEnum.SOLDIER_PATH_CUT:
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ide nem tudod áthelyezni a várat (nem lenne út a két vár között)."));
                        break;
                    case GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE:
                        InvalidAction?.Invoke(this, new UserInteractionEventArgs("Ide nem tudod áthelyezni a várat (túl közel lenne a két vár)."));
                        break;
                    default:
                        break;
                }

            }
            catch (Exception)
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
            }
        }
        #endregion
        #region Public methods
        /// <summary>
        /// Átméretezi a pályát a megadott méretűre
        /// </summary>
        /// <param name="rows">Kívánt sorok száma</param>
        /// <param name="cols">Kívánt oszlopok száma</param>
        public void SetMapSize(int rows, int cols)
        {
            try
            {
                model.ResizeMap(rows, cols);
            }
            catch
            {
                Error?.Invoke(this, new UserInteractionEventArgs("Nem sikerült átméretezni a pályát"));
            }
            InitializeData();
        }
        #endregion
        #region Handler methods
        private void Model_GridUpdated(object sender, GridUpdatedEventArgs e)
        {
            foreach (var update in e.Updates)
            {
                if (update.CellUpdateType == CellUpdateType.Changed)
                {
                    grid[update.Cell.Row, update.Cell.Col].CellType = update.Cell.GetCellType();
                }
                else
                {
                    Error?.Invoke(this, new UserInteractionEventArgs("Ismeretlen hiba lépett fel."));
                }
            }

            OnPropertyChanged(nameof(Fields));
        }
        #endregion
        #region Async methods
        /// <summary>
        /// A megadott fájlútvonalra aszinkron elmenti a pályát
        /// </summary>
        /// <param name="fname">A mentés helye fájlnévként</param>
        /// <returns>Az aszinkron híváshoz tartozó <see cref="Task"/></returns>
        public async Task Save(string fname)
        {
            await model.SaveGame(fname);
        }
        /// <summary>
        /// A megadott fájlútvonalról aszinkron betölti a pályát
        /// </summary>
        /// <param name="fname">A betöltés forrása fájlnévként</param>
        /// <returns>Az aszinkron híváshoz tartozó <see cref="Task"/></returns>
        public async Task Load(string fname)
        {
            await model.LoadGame(fname);
            InitializeData();
        }
        #endregion
    }

}
