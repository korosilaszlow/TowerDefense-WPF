using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TowerDefensePersistence;

namespace TowerDefenseModel
{
    public class EditorModel
    {

        #region private

        private GameData gameData;
        private IPersistence dataAccess;
        private GamePhase gamePhase;

        #endregion

        #region property

        public Cell this[int row, int col]
        {
            get { return gameData.Cells[row, col]; }
            private set { gameData.Cells[row, col] = value; }
        }

        public int Rows
        {
            get { return gameData.Rows; }
        }

        public int Cols
        {
            get { return gameData.Cols; }
        }

        public int StartMoney
        {
            get { return gameData.Player1Money; }
            set
            {
                gameData.Player1Money = value;
                gameData.Player2Money = value;
            }
        }

        #endregion
        #region events
        public event EventHandler<GridUpdatedEventArgs> GridUpdated;
        #endregion

        #region constructor

        public EditorModel()
        {
            this.gameData = new GameData();
            this.dataAccess = new FilePersistence();
        }

        public EditorModel(IPersistence persistence)
        {
            this.gameData = new GameData();
            this.dataAccess = persistence;
        }

        public EditorModel(IPersistence persistence, GameData gamedata)
        {
            this.gameData = gamedata;
            this.dataAccess = persistence;
        }

        #endregion

        #region async methods

        public async Task SaveGame(string path)
        {
            await dataAccess.SaveAsync(path, gameData);
        }

        public async Task LoadGame(string path)
        {
            GameData temp = gameData;
            GamePhase tempPhase = this.gamePhase;

            gameData = await dataAccess.LoadAsync(path);
            gamePhase = gameData.CurrentPlayer == PlayerType.Player1 ? GamePhase.Player1 : GamePhase.Player2;

            var m = PathFinding.GetPathFindMatrix(gameData);
            if (   gameData.Player1Soldiers.Count != 0
                || gameData.Player2Soldiers.Count != 0
                || gameData.Player1Towers.Count != 0
                || gameData.Player2Towers.Count != 0
                || PathFinding.PathFind(m, gameData) == null)
            {
                gameData = temp;
                gamePhase = tempPhase;
                throw new ModelLoadException();
            }          

        }

        #endregion

        #region public method

        public GridExceptionEnum CanMapCastleOne(int x, int y)
        {
            var m = PathFinding.GetPathFindMatrix(gameData);
            m[x, y] = 1;

            if (Math.Abs(gameData.Player2Castle.Row - x) < (gameData.Rows / 3 + 1) && Math.Abs(gameData.Player2Castle.Col - y) < (gameData.Cols / 3 + 1))
            {
                return GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE;
            }
            if (PathFinding.PathFind(m, gameData, x, y, gameData.Player2Castle.Row, gameData.Player2Castle.Col) == null)
            {
                return GridExceptionEnum.SOLDIER_PATH_CUT;
            }
            return GridExceptionEnum.NO_PROBLEM;
        }

        public void MapCastleOne(int x, int y)
        {
            if (GridExceptionEnum.NO_PROBLEM != CanMapCastleOne(x, y)) { return; }
            int changedx = gameData.Player1Castle.Row;
            int changedy = gameData.Player1Castle.Col;
            gameData.Cells[changedx, changedy] = new Plain(changedx, changedy);
            gameData.Player1Castle = new Castle(PlayerType.Player1, x, y);
            gameData.Cells[x, y] = gameData.Player1Castle;


            GridUpdated?.Invoke(this, new GridUpdatedEventArgs(GamePhase.Player1) { Updates = new List<CellUpdate> { new CellUpdate() { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed }, new CellUpdate() { Cell = gameData.Cells[changedx, changedy], CellUpdateType = CellUpdateType.Changed } } });
        }

        public GridExceptionEnum CanMapCastleTwo(int x, int y)
        {
            var m = PathFinding.GetPathFindMatrix(gameData);
            m[x, y] = 1;

            if (Math.Abs(gameData.Player1Castle.Row - x) < gameData.Rows / 3 && Math.Abs(gameData.Player1Castle.Col - y) < gameData.Cols / 3)
            {
                return GridExceptionEnum.ENEMY_CASTLE_TOO_CLOSE;
            }
            if (PathFinding.PathFind(m, gameData, gameData.Player1Castle.Row, gameData.Player1Castle.Col, x, y) == null)
            {
                return GridExceptionEnum.SOLDIER_PATH_CUT;
            }
            return GridExceptionEnum.NO_PROBLEM;
        }

        public void MapCastleTwo(int x, int y)
        {
            if (GridExceptionEnum.NO_PROBLEM != CanMapCastleTwo(x, y)) { return; }
            int changedx = gameData.Player2Castle.Row;
            int changedy = gameData.Player2Castle.Col;
            gameData.Cells[changedx, changedy] = new Plain(changedx, changedy);
            gameData.Player2Castle = new Castle(PlayerType.Player2, x, y);
            gameData.Cells[x, y] = gameData.Player2Castle;

            GridUpdated?.Invoke(this, new GridUpdatedEventArgs(GamePhase.Player1) { Updates = new List<CellUpdate> { new CellUpdate() { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed }, new CellUpdate() { Cell = gameData.Cells[changedx, changedy], CellUpdateType = CellUpdateType.Changed } } });
        }

        public bool CanPlace(int x, int y, ObstaclePhase phase)
        {
            if (gameData.Cells[x, y] is Castle) return false;

            if (phase != ObstaclePhase.Empty)
            {
                var n = PathFinding.GetPathFindMatrix(gameData);
                n[x, y] = 0;
                if (PathFinding.PathFind(n, gameData) == null) return false;
            }
            return true;

        }

        public void CellPlace(int x, int y, ObstaclePhase phase)
        {
            switch (phase)
            {
                case ObstaclePhase.Mountain:
                    gameData.Cells[x, y] = new Mountain(x, y);
                    break;
                case ObstaclePhase.Water:
                    gameData.Cells[x, y] = new Water(x, y);
                    break;
                case ObstaclePhase.Empty:
                    gameData.Cells[x, y] = new Plain(x, y);
                    break;
            }
            GridUpdated?.Invoke(this, new GridUpdatedEventArgs(GamePhase.Player1) { Updates = new List<CellUpdate> { new CellUpdate() { Cell = gameData.Cells[x, y], CellUpdateType = CellUpdateType.Changed } } });
        }

        public void ResizeMap(int x, int y)
        {
            gameData.Cells = new Cell[x, y];
            gameData.Rows = x;
            gameData.Cols = y;
            gameData.Player1Soldiers = new List<Soldier>();
            gameData.Player2Soldiers = new List<Soldier>();
            gameData.Player1Towers = new List<Tower>();
            gameData.Player2Towers = new List<Tower>();

            var updates = new List<CellUpdate>();

            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Cols; ++j)
                {
                    gameData.Cells[i, j] = new Plain(i, j);
                    if ((i, j) != (0, 0) && (i, j) != (Rows - 1, Cols - 1))
                    {
                        updates.Add(new CellUpdate() { Cell = gameData.Cells[i, j], CellUpdateType = CellUpdateType.Changed });
                    }
                }
            }

            gameData.Cells[0, 0] = new Castle(PlayerType.Player1, 0, 0);
            gameData.Player1Castle = gameData.Cells[0, 0] as Castle;
            updates.Add(new CellUpdate() { Cell = gameData.Cells[0, 0], CellUpdateType = CellUpdateType.Changed });
            gameData.Cells[Rows - 1, Cols - 1] = new Castle(PlayerType.Player2, Rows - 1, Cols - 1);
            gameData.Player2Castle = gameData.Cells[Rows - 1, Cols - 1] as Castle;
            updates.Add(new CellUpdate() { Cell = gameData.Cells[Rows - 1, Cols - 1], CellUpdateType = CellUpdateType.Changed });
        }

        #endregion


    }
}
