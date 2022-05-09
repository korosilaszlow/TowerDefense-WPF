using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using TowerDefensePersistence;

namespace TowerDefenseModel
{
    /// <summary>
    /// A játék közben kezeli a játékos interakcióit. Amennyiben valami nem lehetséges vagy invalid művelet, abban az esetben hibát dob vagy hamissal/exception enummal tér vissza.
    /// </summary>
    public class GameModel
    {
        #region private

        private GameData gameData;
        private IPersistence dataAccess;
        private Random random;
        private Timer timer;
        private GamePhase gamePhase;
        private List<(int, int)> currentPath;
        private int step;

        #endregion
        #region property

        public int Player1Money
        {
            get { return gameData.Player1Money; }
            set { gameData.Player1Money = value; }
        }

        public int Player2Money
        {
            get { return gameData.Player2Money; }
            set { gameData.Player2Money = value; }
        }

        public GamePhase GamePhase
        {
            get { return gamePhase; }
            set { gamePhase = value; }
        }

        public int Rows
        {
            get { return gameData.Rows; }
        }

        public int Cols
        {
            get { return gameData.Cols; }
        }

        public int Player1CastleHP
        {
            get { return gameData.Player1Castle.Hitpoints; }
        }

        public int Player2CastleHP
        {
            get { return gameData.Player2Castle.Hitpoints; }
        }

        public bool GameIsOver
        {
            get { return gameData.Player1Castle.Hitpoints <= 0 | gameData.Player2Castle.Hitpoints <= 0; }
        }

        public Cell this[int row, int col]
        {
            get { return gameData.Cells[row, col]; }
            private set { gameData.Cells[row, col] = value; }
        }

        public (int, int) Player1Soldiers
        {
            get
            {
                int attack = 0, tank = 0;
                foreach (Soldier soldier in gameData.Player1Soldiers)
                {
                    if (soldier is AttackSoldier)
                    {
                        ++attack;
                    }
                    else
                    {
                        ++tank;
                    }
                }
                return (attack, tank);

            }
        }
        public (int, int) Player2Soldiers
        {
            get
            {
                int attack = 0, tank = 0;
                foreach (Soldier soldier in gameData.Player2Soldiers)
                {
                    if (soldier is AttackSoldier)
                    {
                        ++attack;
                    }
                    else
                    {
                        ++tank;
                    }
                }
                return (attack, tank);
            }
        }

        public bool IsInPausedState { get => GamePhase == GamePhase.Simulation && !timer.Enabled; }

        public int Step { get => step; set => step = value; }

        #endregion
        #region events

        public event EventHandler<GameOverEventArgs> GameOver;
        public event EventHandler<GridUpdatedEventArgs> GridUpdated;
        public event EventHandler<EventArgs> EndOfSimulation;

        #endregion
        #region async methods
        /// <summary>
        /// Megkéri a perzisztenciát, hogy mentse el a jelenlegi játék állását a path nevű útvonalra
        /// </summary>
        /// <param name="path">A mentendő fájl útvonala</param>
        public async Task SaveGame(string path)
        {
            await dataAccess.SaveAsync(path, gameData);
        }

        /// <summary>
        /// Megkéri a perzisztenciát, hogy töltse be a jelenlegi játék helyére a fájlból az eltárolt játékot
        /// </summary>
        /// <param name="path">Az eltárolt játék útvonala</param>
        /// <exception cref="ModelLoadException">Amennyiben nem megfelelő játék töltődik be</exception>
        public async Task LoadGame(string path)
        {

            GameData loadedData = await dataAccess.LoadAsync(path);
            var loadedPhase = loadedData.CurrentPlayer == PlayerType.Player1 ? GamePhase.Player1 : GamePhase.Player2;

            var m = PathFinding.GetPathFindMatrix(loadedData);
            if (PathFinding.PathFind(m, loadedData) == null)
            {
                throw new ModelLoadException();
            }
            timer.Stop();
            gameData = loadedData;
            gamePhase = loadedPhase;

        }

        #endregion
        #region constructor

        public GameModel()
        {
            this.gameData = new GameData();
            this.dataAccess = new FilePersistence();
            this.random = new Random();
            this.timer = new Timer();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Simulation_Tick;
        }

        public GameModel(IPersistence persistence)
        {
            this.gameData = new GameData();
            this.dataAccess = persistence;
            this.random = new Random();
            this.timer = new Timer();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Simulation_Tick;
        }

        public GameModel(IPersistence persistence, GameData gamedata)
        {
            this.gameData = gamedata;
            this.dataAccess = persistence;
            this.random = new Random();
            this.timer = new Timer();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Simulation_Tick;
        }

        #endregion
        #region public method
        /// <summary>
        /// Új játék létrehozása
        /// </summary>
        /// <param name="rows">Pálya sorainak száma</param>
        /// <param name="cols">Pálya oszlopainak száma</param>
        public void NewGame(int rows, int cols)
        {
            if (timer != null) timer.Stop();
            step = 0;
            gamePhase = GamePhase.Player1;
            gameData = new GameData(rows, cols);
            for (int i = 0; i < gameData.Rows; i++)
            {
                for (int j = 0; j < gameData.Cols; j++)
                {
                    this[i, j] = new Plain(i, j);
                }
            }
            gameData.CurrentPlayer = PlayerType.Player1;
            gameData.Player1Money = 500;
            gameData.Player2Money = 500;
            gameData.Round = 1;
            gameData.GameOver = false;
            gameData.Player1KillsThisRound = 0;
            gameData.Player2KillsThisRound = 0;
            gameData.Player1TotalSoldiersCount = 0;
            gameData.Player2TotalSoldiersCount = 0;
            int x, y;
            x = random.Next(rows / 4 + 1);
            y = random.Next(cols / 4 + 1);
            gameData.Player1Castle = new Castle(PlayerType.Player1, x, y);
            gameData.Player1Castle.Hitpoints = 100;
            this[x, y] = gameData.Player1Castle;
            x = random.Next(rows - rows / 4 - 1, rows);
            y = random.Next(cols - cols / 4 - 1, cols);
            gameData.Player2Castle = new Castle(PlayerType.Player2, x, y);
            gameData.Player2Castle.Hitpoints = 100;
            this[x, y] = gameData.Player2Castle;
            int counter = 0;
            int extraObstacle = random.Next(0, (rows * cols) / 14);
            while (counter < (rows * cols) / 9 + extraObstacle)
            {
                x = random.Next(rows);
                y = random.Next(cols);
                var m = PathFinding.GetPathFindMatrix(gameData);
                m[x, y] = 0;
                if (PathFinding.PathFind(m, gameData) == null) { continue; }
                if (!(Distance.Chebyshev(Distance.CellToCoords(gameData.Player1Castle), (x, y)) < 3
                    || Distance.Chebyshev(Distance.CellToCoords(gameData.Player2Castle), (x, y)) < 3))
                {
                    counter++;
                    if (random.Next(10) < 5)
                    {
                        this[x, y] = new Mountain(x, y);
                    }
                    else
                    {
                        this[x, y] = new Water(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Új játék létrehozása az előző játék méreteivel
        /// </summary>
        public void NewGame()
        {
            if (gameData == null) return;
            NewGame(gameData.Rows, gameData.Cols);
        }

        /// <summary>
        /// Váltás a következő játék fázisra
        /// </summary>
        public void NextGamePhase()
        {
            switch (gamePhase)
            {
                case GamePhase.Player1:
                    gamePhase = GamePhase.Player2;
                    gameData.CurrentPlayer = PlayerType.Player2;
                    break;
                case GamePhase.Player2:
                    gamePhase = GamePhase.Simulation;
                    step = 0;
                    StartSimulation();
                    break;
            }
        }

        /// <summary>
        /// Megadja, hogy az adott koordináta közelében van szövetséges torony vagy vár
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha az adott koordináta közelében van-e szövetséges torony vagy vár</returns>
        private bool HasNearbyAllyTowerOrCastle(int x, int y)
        {
            var towers = GamePhase == GamePhase.Player1 ? gameData.Player1Towers : gameData.Player2Towers;
            var castle = GamePhase == GamePhase.Player1 ? gameData.Player1Castle : gameData.Player2Castle;
            // a tornyoknál szándékosan van 1-es norma távolság (míg a kastélynál végtelen)
            foreach (var t in towers)
            {
                if (Distance.Manhattan((t.Row, t.Col), (x, y)) <= 3)
                {
                    return true;
                }
            }
            return Distance.Chebyshev((castle.Row, castle.Col), (x, y)) <= 3;
        }

        /// <summary>
        /// Megadja, hogy az adott koordináta közelében van ellenséges vár
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha az adott koordináta közelében van ellenséges vár</returns>
        private bool HasNearbyEnemyCastle(int x, int y)
        {
            var castle = GamePhase == GamePhase.Player2 ? gameData.Player1Castle : gameData.Player2Castle; //ellenfél kastélya
            return Distance.Chebyshev((castle.Row, castle.Col), (x, y)) <= 3;
        }

        /// <summary>
        /// Megadja, hogy építhető RangedTower az adott koordinátán
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha építhető RangedTower az adott koordinátán</returns>
        public GridExceptionEnum CanBuildRangedTower(int x, int y)
        {
            if (!(gameData.Cells[x, y] is Plain)) return GridExceptionEnum.INVALID_CELL;

            if (gamePhase == GamePhase.Player1 && Player1Money < RangedTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < RangedTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;

            if (!HasNearbyAllyTowerOrCastle(x, y))
            {
                return GridExceptionEnum.NO_NEARBY_ALLY_TOWER;
            }

            if (HasNearbyEnemyCastle(x, y))
            {
                return GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE;
            }

            var m = PathFinding.GetPathFindMatrix(gameData);
            m[x, y] = 0;
            if (PathFinding.PathFind(m, gameData) == null)
            {
                return GridExceptionEnum.SOLDIER_PATH_CUT;
            }

            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy építhető DamageTower az adott koordinátán
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha építhető DamageTower az adott koordinátán</returns>
        public GridExceptionEnum CanBuildDamageTower(int x, int y)
        {
            if (!(gameData.Cells[x, y] is Plain)) return GridExceptionEnum.INVALID_CELL;

            if (gamePhase == GamePhase.Player1 && Player1Money < DamageTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < DamageTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;

            if ((!HasNearbyAllyTowerOrCastle(x, y)))
            {
                return GridExceptionEnum.NO_NEARBY_ALLY_TOWER;
            }

            if (HasNearbyEnemyCastle(x, y))
            {
                return GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE;
            }

            var m = PathFinding.GetPathFindMatrix(gameData);
            m[x, y] = 0;
            if (PathFinding.PathFind(m, gameData) == null) { return GridExceptionEnum.INVALID_CELL; }

            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy építhető SupportTower az adott koordinátán
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha építhető SupportTower az adott koordinátán</returns>
        public GridExceptionEnum CanBuildSupportTower(int x, int y)
        {
            if (!(gameData.Cells[x, y] is Plain)) return GridExceptionEnum.INVALID_CELL;

            if (gamePhase == GamePhase.Player1 && Player1Money < SupportTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < SupportTower.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;

            if ((!HasNearbyAllyTowerOrCastle(x, y)))
            {
                return GridExceptionEnum.NO_NEARBY_ALLY_TOWER;
            }

            if (HasNearbyEnemyCastle(x, y))
            {
                return GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE;
            }

            var m = PathFinding.GetPathFindMatrix(gameData);
            m[x, y] = 0;
            if (PathFinding.PathFind(m, gameData) == null) { return GridExceptionEnum.INVALID_CELL; }

            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy fejleszthető a Tower az adott koordinátán
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha fejleszthető a Tower az adott koordinátán</returns>
        public GridExceptionEnum CanUpgradeTower(int x, int y)
        {
            if (!(gameData.Cells[x, y] is Tower)) return GridExceptionEnum.INVALID_CELL;
            Tower tmpcell = gameData.Cells[x, y] as Tower;

            if (tmpcell.IsMaxLevel()) return GridExceptionEnum.TOWER_MAX_LEVEL;
            if (gamePhase == GamePhase.Player1 && Player1Money < tmpcell.UpgradeCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < tmpcell.UpgradeCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;

            if (gamePhase == GamePhase.Player1)
            {
                if (tmpcell.Player != PlayerType.Player1) return GridExceptionEnum.INVALID_CELL;
            }
            else
            {
                if (tmpcell.Player != PlayerType.Player2) return GridExceptionEnum.INVALID_CELL;
            }
            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy rombolható a Tower az adott koordinátán
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <returns>Igazat ad, ha rombolható a Tower az adott koordinátán</returns>
        public GridExceptionEnum CanRemoveTower(int x, int y)
        {
            if (!(gameData.Cells[x, y] is Tower)) return GridExceptionEnum.INVALID_CELL;
            Tower tmpcell = gameData.Cells[x, y] as Tower;

            if (gamePhase == GamePhase.Player1)
            {
                if (tmpcell.Player != PlayerType.Player1) return GridExceptionEnum.INVALID_CELL;
            }
            else
            {
                if (tmpcell.Player != PlayerType.Player2) return GridExceptionEnum.INVALID_CELL;
            }
            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy vásárolható AttackSoldier a játékos által
        /// </summary>
        /// <returns>Igazat ad, ha vásárolható AttackSoldier a játékos által</returns>
        public GridExceptionEnum CanBuyAttackSoldier()
        {

            if (gamePhase == GamePhase.Player1 && Player1Money < AttackSoldier.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < AttackSoldier.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;
            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// Megadja, hogy vásárolható TankSoldier a játékos által
        /// </summary>
        /// <returns>Igazat ad, ha vásárolható TankSoldier a játékos által</returns>
        public GridExceptionEnum CanBuyTankSoldier()
        {
            if (gamePhase == GamePhase.Player1 && Player1Money < TankSoldier.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Player2 && Player2Money < TankSoldier.BuildCost()) return GridExceptionEnum.MONEY;
            if (gamePhase == GamePhase.Simulation) return GridExceptionEnum.INVALID_GAME_STATE;
            return GridExceptionEnum.NO_PROBLEM;
        }

        /// <summary>
        /// AttackSoldier-t vásárol a játékos számára
        /// </summary>
        /// <exception cref="GridException">Amennyiben nem tud valamiért vásárolni</exception>
        public void BuyAttackSoldier()
        {
            GridExceptionEnum exc = CanBuyAttackSoldier();
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= AttackSoldier.BuildCost();
                AttackSoldier asr = new AttackSoldier(PlayerType.Player1);
                gameData.Player1Soldiers.Add(asr);
                gameData.Player1TotalSoldiersCount++;
                gameData.Player1Castle.Player1Soldiers.Add(asr);
                gameData.Player1TotalSoldiersCount++;
                tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Player1Castle, CellUpdateType = CellUpdateType.SoldierEntered } };
            }
            else
            {
                Player2Money -= AttackSoldier.BuildCost();
                AttackSoldier asr = new AttackSoldier(PlayerType.Player2);
                gameData.Player2Soldiers.Add(asr);
                gameData.Player2TotalSoldiersCount++;
                gameData.Player2Castle.Player2Soldiers.Add(asr);
                gameData.Player2TotalSoldiersCount++;
                tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Player2Castle, CellUpdateType = CellUpdateType.SoldierEntered } };
            }

            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// TankSoldier-t vásárol a játékos számára
        /// </summary>
        /// <exception cref="GridException">Amennyiben nem tud valamiért vásárolni</exception>
        public void BuyTankSoldier()
        {
            GridExceptionEnum exc = CanBuyTankSoldier();
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }
            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= TankSoldier.BuildCost();
                TankSoldier asr = new TankSoldier(PlayerType.Player1);
                gameData.Player1Soldiers.Add(asr);
                gameData.Player1TotalSoldiersCount++;
                gameData.Player1Castle.Player1Soldiers.Add(asr);
                gameData.Player1TotalSoldiersCount++;
                tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Player1Castle, CellUpdateType = CellUpdateType.SoldierEntered } };
            }
            else
            {
                Player2Money -= TankSoldier.BuildCost();
                TankSoldier asr = new TankSoldier(PlayerType.Player2);
                gameData.Player2Soldiers.Add(asr);
                gameData.Player2TotalSoldiersCount++;
                gameData.Player2Castle.Player2Soldiers.Add(asr);
                gameData.Player2TotalSoldiersCount++;
                tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Player2Castle, CellUpdateType = CellUpdateType.SoldierEntered } };
            }

            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// RangedTower-t rak le a megadott koordinátákra
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <exception cref="GridException">Amennyiben valamiért nem lehetséges a lerakás</exception>
        public void PlaceRangedTower(int x, int y)
        {

            GridExceptionEnum exc = CanBuildRangedTower(x, y);
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }

            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= RangedTower.BuildCost();
                gameData.Cells[x, y] = new RangedTower(PlayerType.Player1, x, y);
                gameData.Player1Towers.Add(gameData.Cells[x, y] as RangedTower);
            }
            else
            {
                Player2Money -= RangedTower.BuildCost();
                gameData.Cells[x, y] = new RangedTower(PlayerType.Player2, x, y);
                gameData.Player2Towers.Add(gameData.Cells[x, y] as RangedTower);
            }

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed } };
            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// DamageTower-t rak le a megadott koordinátákra
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <exception cref="GridException">Amennyiben valamiért nem lehetséges a lerakás</exception>
        public void PlaceDamageTower(int x, int y)
        {
            GridExceptionEnum exc = CanBuildDamageTower(x, y);
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }

            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= DamageTower.BuildCost();
                gameData.Cells[x, y] = new DamageTower(PlayerType.Player1, x, y);
                gameData.Player1Towers.Add(gameData.Cells[x, y] as DamageTower);
            }
            else
            {
                Player2Money -= DamageTower.BuildCost();
                gameData.Cells[x, y] = new DamageTower(PlayerType.Player2, x, y);
                gameData.Player2Towers.Add(gameData.Cells[x, y] as DamageTower);
            }

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed } };
            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// SupportTower-t rak le a megadott koordinátákra
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <exception cref="GridException">Amennyiben valamiért nem lehetséges a lerakás</exception>
        public void PlaceSupportTower(int x, int y)
        {
            GridExceptionEnum exc = CanBuildSupportTower(x, y);
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }

            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= SupportTower.BuildCost();
                gameData.Cells[x, y] = new SupportTower(PlayerType.Player1, x, y);
                gameData.Player1Towers.Add(gameData.Cells[x, y] as SupportTower);
                (gameData.Cells[x, y] as SupportTower).KilledUnit += gameData.Player1Castle.SupportTower_KilledUnit;
            }
            else
            {
                Player2Money -= SupportTower.BuildCost();
                gameData.Cells[x, y] = new SupportTower(PlayerType.Player2, x, y);
                gameData.Player2Towers.Add(gameData.Cells[x, y] as SupportTower);
                (gameData.Cells[x, y] as SupportTower).KilledUnit += gameData.Player2Castle.SupportTower_KilledUnit;
            }

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed } };
            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// A koordinátán lévő torony fejlesztése
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <exception cref="GridException">Amennyiben nem lehetséges fejleszteni</exception>
        public void UpgradeTower(int x, int y)
        {
            GridExceptionEnum exc = CanUpgradeTower(x, y);
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }
            Tower tmpcell = gameData.Cells[x, y] as Tower;

            if (gamePhase == GamePhase.Player1)
            {
                Player1Money -= tmpcell.UpgradeCost();
            }
            else
            {
                Player2Money -= tmpcell.UpgradeCost();
            }
            tmpcell.Upgrade();

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Upgraded } };
            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// A koordinátán lévő torony rombolása
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <exception cref="GridException">Amennyiben nem lehetséges rombolni</exception>
        public void RemoveTower(int x, int y)
        {
            GridExceptionEnum exc = CanRemoveTower(x, y);
            if (exc != GridExceptionEnum.NO_PROBLEM)
            {
                throw new GridException(exc);
            }
            Tower tmpcell = gameData.Cells[x, y] as Tower;

            if (gamePhase == GamePhase.Player1)
            {
                if (tmpcell is SupportTower)
                {
                    (tmpcell as SupportTower).KilledUnit -= gameData.Player1Castle.SupportTower_KilledUnit;
                }
                gameData.Player1Towers.Remove(tmpcell);
                Player1Money += tmpcell.RemovalGain();
                gameData.Cells[x, y] = new Plain(x, y);
            }
            else
            {
                if (tmpcell is SupportTower)
                {
                    (tmpcell as SupportTower).KilledUnit -= gameData.Player2Castle.SupportTower_KilledUnit;
                }
                gameData.Player2Towers.Remove(tmpcell);
                Player2Money += tmpcell.RemovalGain();
                gameData.Cells[x, y] = new Plain(x, y);
            }

            GridUpdatedEventArgs tmp = new GridUpdatedEventArgs(gamePhase);
            tmp.Updates = new List<CellUpdate> { new CellUpdate { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed } };
            GridUpdated?.Invoke(this, tmp);
        }

        /// <summary>
        /// Szimuláció leállítása
        /// </summary>
        public void PauseSimulation()
        {
            timer?.Stop();
        }

        /// <summary>
        /// Szimuláció folytatása
        /// </summary>
        public void UnPauseSimulation()
        {
            if (gamePhase == GamePhase.Simulation)
                timer?.Start();
        }

        #endregion
        #region private method

        /// <summary>
        /// Szimuláció indítása
        /// </summary>
        private void StartSimulation()
        {
            StorePath();
            timer?.Start();
        }

        /// <summary>
        /// Útvonal eltárolása a szimulációhoz
        /// </summary>
        private void StorePath()
        {
            var m = PathFinding.GetPathFindMatrix(gameData);
            currentPath = PathFinding.PathFind(m, gameData);
        }

        /// <summary>
        /// Szimuláció folyamata, mely x másodpercenként történik
        /// </summary>
        /// <param name="sender">A metódus hívója</param>
        /// <param name="e">Timer eventje</param>
        private void Simulation_Tick(object sender, ElapsedEventArgs e)
        {
            foreach ((int x, int y) pathCell in currentPath)
            {
                (this[pathCell.x, pathCell.y] as SoldierContainer).Player1DeadSoldiers.Clear();
                (this[pathCell.x, pathCell.y] as SoldierContainer).Player2DeadSoldiers.Clear();
            }

            if (GameIsOver)
            {
                timer?.Stop();
                var over = new GameOverEventArgs();
                if (Player1CastleHP <= 0 && Player2CastleHP <= 0)
                {
                    over.Winner = PlayerType.None;
                }
                else if (Player1CastleHP <= 0)
                {
                    over.Winner = PlayerType.Player2;
                }
                else
                {
                    over.Winner = PlayerType.Player1;
                }
                GameOver?.Invoke(this, over);
            }
            if (!(gameData.Player1Soldiers.Count != 0 || gameData.Player2Soldiers.Count != 0))
            {
                timer?.Stop();
                gamePhase = GamePhase.Player1;
                gameData.CurrentPlayer = PlayerType.Player1;
                Player1Money += gameData.Player1MoneyIncome;
                Player2Money += gameData.Player2MoneyIncome;
                gameData.Player1KillsThisRound = 0;
                gameData.Player2KillsThisRound = 0;
                EndOfSimulation?.Invoke(this, EventArgs.Empty);
                return;
            }
            step++;
            var updated = new GridUpdatedEventArgs(GamePhase.Simulation);
            var path = currentPath;

            //lépés

            for (int i = 0; i < path.Count; i++)
            {
                (int x, int y) = path[i];
                var cell = (gameData.Cells[x, y] as SoldierContainer);
                updated.Updates.Add(new CellUpdate() { Cell = this[x, y], CellUpdateType = CellUpdateType.SoldierEntered });
                if (cell.Player1Soldiers.Count > 0)
                {
                    var soldier = cell.Player1Soldiers[0];
                    cell.Player1Soldiers.Remove(soldier);
                    (int px, int py) = path[i - 1];
                    (gameData.Cells[px, py] as SoldierContainer).Player1Soldiers.Add(soldier);
                }
            }
            for (int i = path.Count - 1; i > -1; i--)
            {
                (int x, int y) = path[i];
                var cell = (gameData.Cells[x, y] as SoldierContainer);
                if (cell.Player2Soldiers.Count > 0)
                {
                    var soldier = cell.Player2Soldiers[0];
                    cell.Player2Soldiers.Remove(soldier);
                    (int px, int py) = path[i + 1];
                    (gameData.Cells[px, py] as SoldierContainer).Player2Soldiers.Add(soldier);
                }
            }

            //várra lépés
            if (gameData.Player1Castle.Player2Soldiers.Count > 0)
            {
                var soldier = gameData.Player1Castle.Player2Soldiers[0];
                gameData.Player1Castle.TakeDamage(soldier);
                gameData.Player1Castle.Player2Soldiers.Clear();
                gameData.Player2Soldiers.Remove(soldier);
                updated.Updates.Add(new CellUpdate() { Cell = gameData.Player1Castle, CellUpdateType = CellUpdateType.SoldierDied });

            }

            if (gameData.Player2Castle.Player1Soldiers.Count > 0)
            {
                var soldier = gameData.Player2Castle.Player1Soldiers[0];
                gameData.Player2Castle.TakeDamage(soldier);
                gameData.Player2Castle.Player1Soldiers.Clear();
                gameData.Player1Soldiers.Remove(soldier);
                updated.Updates.Add(new CellUpdate() { Cell = gameData.Player2Castle, CellUpdateType = CellUpdateType.SoldierDied });

            }
            //lövés
            foreach (var tower in gameData.Player1Towers)
            {
                int targetCount = tower.TargetCount;

                foreach ((int x, int y) in Enumerable.Reverse(path))
                {
                    if ((this[x, y] as SoldierContainer).Player2Soldiers.Count == 0) continue;
                    if (this[x, y] is Castle) continue;
                    if (tower.IsInRange(x, y))
                    {
                        var cell = (gameData.Cells[x, y] as SoldierContainer);
                        if (cell.Player2Soldiers.Count > 0)
                        {
                            var soldier = cell.Player2Soldiers[0];
                            tower.AttackSoldier(soldier);
                            if (soldier.IsDead)
                            {
                                gameData.Player2Soldiers.Remove(soldier);
                                cell.Player2Soldiers.Remove(soldier);
                                gameData.Player1KillsThisRound++;
                                updated.Updates.Add(new CellUpdate() { Cell = cell, CellUpdateType = CellUpdateType.SoldierDied });
                                cell.Player2DeadSoldiers.Add(soldier);
                            }

                            updated.Updates.Add(new CellUpdate() { Cell = cell, CellUpdateType = CellUpdateType.Hit });
                            targetCount--;
                            if (targetCount == 0) break;
                        }
                    }
                }
                if (targetCount != tower.TargetCount)
                {
                    updated.Updates.Add(new CellUpdate() { Cell = tower, CellUpdateType = CellUpdateType.Fired });
                }
            }
            foreach (var tower in gameData.Player2Towers)
            {
                int targetCount = tower.TargetCount;
                foreach ((int x, int y) in path)
                {
                    if ((this[x, y] as SoldierContainer).Player1Soldiers.Count == 0) continue;
                    if (this[x, y] is Castle) continue;
                    if (tower.IsInRange(x, y))
                    {
                        var cell = (gameData.Cells[x, y] as SoldierContainer);
                        if (cell.Player1Soldiers.Count > 0)
                        {
                            var soldier = cell.Player1Soldiers[0];
                            tower.AttackSoldier(soldier);
                            if (soldier.IsDead)
                            {
                                gameData.Player1Soldiers.Remove(soldier);
                                cell.Player1Soldiers.Remove(soldier);
                                gameData.Player2KillsThisRound++;

                                updated.Updates.Add(new CellUpdate() { Cell = cell, CellUpdateType = CellUpdateType.SoldierDied });
                                cell.Player1DeadSoldiers.Add(soldier);
                            }

                            updated.Updates.Add(new CellUpdate() { Cell = cell, CellUpdateType = CellUpdateType.Hit });
                            targetCount--;
                            if (targetCount == 0) break;


                        }
                    }
                }
                if (targetCount != tower.TargetCount)
                {
                    updated.Updates.Add(new CellUpdate() { Cell = tower, CellUpdateType = CellUpdateType.Fired });
                }
            }

            GridUpdated?.Invoke(this, updated);
        }

        #endregion
    }
}
