using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using TowerDefenseModel;
using TowerDefensePersistence;

namespace TowerDefenseTests
{
    [TestClass]
    public class Tests
    {
        private GameModel _model;
        private GameData _mockedGameData;
        private Mock<IPersistence> _mock;
        private EditorModel _editorModel;

        #region Initialization

        [TestInitialize]
        public void Initialize()
        {
            _mockedGameData = new GameData(5, 5);

            _mockedGameData.CurrentPlayer = PlayerType.Player1;

            _mockedGameData.Player1Money = 500;
            _mockedGameData.Player2Money = 500;

            _mockedGameData.Player1KillsThisRound = 0;
            _mockedGameData.Player2KillsThisRound = 0;

            _mockedGameData.Player1TotalSoldiersCount = 2;
            _mockedGameData.Player2TotalSoldiersCount = 2;

            for (int i = 0; i < _mockedGameData.Rows; i++)
            {
                for (int j = 0; j < _mockedGameData.Cols; j++)
                {
                    _mockedGameData.Cells[i, j] = new Plain(i, j);
                }
            }
            _mockedGameData.Player1Castle = new TowerDefensePersistence.Castle(PlayerType.Player1, 0, 0);
            _mockedGameData.Player1Castle.Hitpoints = 100;
            _mockedGameData.Cells[0, 0] = _mockedGameData.Player1Castle;

            _mockedGameData.Player2Castle = new TowerDefensePersistence.Castle(PlayerType.Player2, 4, 4);
            _mockedGameData.Player2Castle.Hitpoints = 100;
            _mockedGameData.Cells[4, 4] = _mockedGameData.Player2Castle;

            _mockedGameData.Cells[4, 0] = new Water(4, 0);
            _mockedGameData.Cells[0, 4] = new Mountain(0, 4);

            TankSoldier ts1 = new TankSoldier(PlayerType.Player1);
            AttackSoldier as1 = new AttackSoldier(PlayerType.Player1);

            TankSoldier ts2 = new TankSoldier(PlayerType.Player2);
            AttackSoldier as2 = new AttackSoldier(PlayerType.Player2);

            _mock = new Mock<IPersistence>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>())).Returns(() => Task.FromResult(_mockedGameData));
            _model = new GameModel(_mock.Object, _mockedGameData);
            _editorModel = new EditorModel(_mock.Object, _mockedGameData);
        }

        #endregion

        #region Menu
        [TestMethod]
        public void NewGameTest()
        {
            GameData gameData = new GameData(5, 5);
            FilePersistence filePersistence = new FilePersistence();
            GameModel model = new GameModel(filePersistence, gameData);
            model.NewGame();
            var gameDataField = model.GetType().GetField("gameData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            gameData = gameDataField.GetValue(model) as GameData;

            Assert.AreEqual(model.GamePhase, GamePhase.Player1);
            Assert.AreEqual(gameData.CurrentPlayer, PlayerType.Player1);
            Assert.AreEqual(gameData.Player1Money, 500);
            Assert.AreEqual(gameData.Player2Money, 500);
            Assert.AreEqual(gameData.Player1Money, gameData.Player2Money);
            Assert.AreEqual(gameData.GameOver, false);
            Assert.AreEqual(gameData.Round, 1);
            Assert.AreEqual(gameData.Player1KillsThisRound, 0);
            Assert.AreEqual(gameData.Player2KillsThisRound, 0);
            Assert.AreEqual(gameData.Player1TotalSoldiersCount, 0);
            Assert.AreEqual(gameData.Player2TotalSoldiersCount, 0);

            int castleNum = 0;
            int obstacleNum = 0;
            for (int i = 0; i < gameData.Cols; i++)
            {
                for (int j = 0; j < gameData.Rows; j++)
                {
                    if (gameData.Cells[i, j] is Obstacle)
                    {
                        obstacleNum++;
                    }
                    if (gameData.Cells[i, j] is TowerDefensePersistence.Castle)
                    {
                        castleNum++;
                    }
                }
            }

            Assert.AreEqual(castleNum, 2);
            Assert.AreNotEqual(obstacleNum, 0);

        }

        [TestMethod]
        public void SaveTest()
        {
            _model.SaveGame(String.Empty);
            _mock.Verify(dataAccess => dataAccess.SaveAsync(String.Empty, _mockedGameData), Times.Once());
        }

        [TestMethod]
        public async Task LoadTest()
        {
            await _model.LoadGame(String.Empty);
            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }
        #endregion

        #region RangedTowerBuild
        [TestMethod]
        public void BuildRangedTowerSuccessfulTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildRangedTower(1, 0));
            _model.PlaceRangedTower(1, 0);
            Assert.AreEqual(true, _mockedGameData.Cells[1, 0] is RangedTower);

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildRangedTower(3, 4));
            _model.PlaceRangedTower(3, 4);
            Assert.AreEqual(true, _mockedGameData.Cells[3, 4] is RangedTower);

        }
        [TestMethod]
        public void BuildRangedTowerBadPositionTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildRangedTower(4, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildRangedTower(0, 0));

        }
        [TestMethod]
        public void BuildRangedTowerBadPhaseTest()
        {
            _model.GamePhase = GamePhase.Simulation;
            Assert.AreEqual(GamePhase.Simulation, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_GAME_STATE, _model.CanBuildRangedTower(1, 4));

        }
        [TestMethod]
        public void BuildRangedTowerNoMoneyTest()
        {
            _mockedGameData.Player1Money = 0;
            _mockedGameData.Player2Money = 0;

            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildRangedTower(1, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildRangedTower(3, 2));

        }
        #endregion

        #region SupportTowerBuild
        [TestMethod]
        public void BuildSupportTowerSuccessfulTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildSupportTower(0, 1));
            _model.PlaceSupportTower(0, 1);
            Assert.AreEqual(true, _mockedGameData.Cells[0, 1] is SupportTower);

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildSupportTower(4, 3));
            _model.PlaceSupportTower(4, 3);
            Assert.AreEqual(true, _mockedGameData.Cells[4, 3] is SupportTower);
        }
        [TestMethod]
        public void BuildSupportTowerBadPositionTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildSupportTower(4, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildSupportTower(0, 0));
        }
        [TestMethod]
        public void BuildSupportTowerBadPhaseTest()
        {
            _model.GamePhase = GamePhase.Simulation;
            Assert.AreEqual(GamePhase.Simulation, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_GAME_STATE, _model.CanBuildSupportTower(1, 4));

        }
        [TestMethod]
        public void BuildSupportTowerNoMoneyTest()
        {
            _mockedGameData.Player1Money = 0;
            _mockedGameData.Player2Money = 0;

            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildSupportTower(1, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildSupportTower(3, 2));
        }
        #endregion

        #region DamageTowerBuild
        [TestMethod]
        public void BuildDamageTowerSuccessfulTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildDamageTower(3, 0));
            _model.PlaceDamageTower(3, 0);
            Assert.AreEqual(true, _mockedGameData.Cells[3, 0] is DamageTower);

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuildDamageTower(2, 4));
            _model.PlaceDamageTower(2, 4);
            Assert.AreEqual(true, _mockedGameData.Cells[2, 4] is DamageTower);
        }
        [TestMethod]
        public void BuildDamageTowerBadPositionTest()
        {
            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildDamageTower(4, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanBuildDamageTower(0, 0));
        }
        [TestMethod]
        public void BuildDamageTowerBadPhaseTest()
        {
            _model.GamePhase = GamePhase.Simulation;
            Assert.AreEqual(GamePhase.Simulation, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.INVALID_GAME_STATE, _model.CanBuildDamageTower(1, 4));

        }
        [TestMethod]
        public void BuildDamageTowerNoMoneyTest()
        {
            _mockedGameData.Player1Money = 0;
            _mockedGameData.Player2Money = 0;

            //Player_1
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildDamageTower(1, 4));

            //Player_2
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuildDamageTower(3, 2));
        }
        #endregion

        #region SoldierBuy
        [TestMethod]
        public void BuyPlayer1SoldiersSuccessfulTest()
        {
            _model.GamePhase = GamePhase.Player1;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuyAttackSoldier());
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuyTankSoldier());
            int s1Num = _mockedGameData.Player1Soldiers.Count;
            _model.BuyAttackSoldier();
            _model.BuyTankSoldier();
            Assert.AreEqual(s1Num + 2, _mockedGameData.Player1Soldiers.Count);
        }
        [TestMethod]
        public void BuyPlayer1SoldiersFailTest()
        {
            _model.GamePhase = GamePhase.Player1;
            _model.Player1Money = 0;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuyAttackSoldier());
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuyTankSoldier());
            int s1Num = _mockedGameData.Player1Soldiers.Count;
            Assert.AreEqual(s1Num, _mockedGameData.Player1Soldiers.Count);
        }
        [TestMethod]
        public void BuyPlayer2SoldiersSuccessfulTest()
        {
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuyAttackSoldier());
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanBuyTankSoldier());
            int s2Num = _mockedGameData.Player2Soldiers.Count;
            _model.BuyAttackSoldier();
            _model.BuyTankSoldier();
            Assert.AreEqual(s2Num + 2, _mockedGameData.Player2Soldiers.Count);
        }
        [TestMethod]
        public void BuyPlayer2SoldiersFailTest()
        {
            _model.GamePhase = GamePhase.Player2;
            _model.Player2Money = 0;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuyAttackSoldier());
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanBuyTankSoldier());
            int s2Num = _mockedGameData.Player2Soldiers.Count;
            Assert.AreEqual(s2Num, _mockedGameData.Player2Soldiers.Count);
        }
        #endregion

        #region UpgradeTowers
        [TestMethod]
        public void UpgradeTowersSuccessfulTest()
        {
            //Player_1
            _mockedGameData.Player1Money = 1000;
            _model.GamePhase = GamePhase.Player1;

            //SupportTower
            _model.PlaceSupportTower(0, 1);
            SupportTower st1 = _mockedGameData.Cells[0, 1] as SupportTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(0, 1));
            int st1Lvl = st1.Level;
            _model.UpgradeTower(0, 1);
            st1Lvl++;
            Assert.AreEqual(st1Lvl, st1.Level);

            //DamageTower
            _model.PlaceDamageTower(0, 2);
            DamageTower dt1 = _mockedGameData.Cells[0, 2] as DamageTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(0, 2));
            int dt1Lvl = dt1.Level;
            _model.UpgradeTower(0, 2);
            dt1Lvl++;
            Assert.AreEqual(dt1Lvl, dt1.Level);

            //RangedTower
            _model.PlaceRangedTower(0, 3);
            RangedTower rt1 = _mockedGameData.Cells[0, 3] as RangedTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(0, 3));
            int rt1Lvl = rt1.Level;
            _model.UpgradeTower(0, 3);
            rt1Lvl++;
            Assert.AreEqual(rt1Lvl, rt1.Level);

            //Player_2
            _mockedGameData.Player2Money = 1000;
            _model.GamePhase = GamePhase.Player2;

            //SupportTower
            _model.PlaceSupportTower(4, 3);
            SupportTower st2 = _mockedGameData.Cells[4, 3] as SupportTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(4, 3));
            int st2Lvl = st2.Level;
            _model.UpgradeTower(4, 3);
            st2Lvl++;
            Assert.AreEqual(st2Lvl, st2.Level);

            //DamageTower
            _model.PlaceDamageTower(4, 2);
            DamageTower dt2 = _mockedGameData.Cells[4, 2] as DamageTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(4, 2));
            int dt2Lvl = dt2.Level;
            _model.UpgradeTower(4, 2);
            dt2Lvl++;
            Assert.AreEqual(dt2Lvl, dt2.Level);

            //RangedTower
            _model.PlaceRangedTower(4, 1);
            RangedTower rt2 = _mockedGameData.Cells[4, 1] as RangedTower;
            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanUpgradeTower(4, 1));
            int rt2Lvl = rt2.Level;
            _model.UpgradeTower(4, 1);
            rt2Lvl++;
            Assert.AreEqual(rt2Lvl, rt2.Level);
        }
        [TestMethod]
        public void UpgradeTowerFailTest()
        {
            //Player_1
            _model.GamePhase = GamePhase.Player1;

            //SupportTower
            _mockedGameData.Player1Money = 500;
            _model.PlaceSupportTower(0, 1);
            _mockedGameData.Player1Money = 0;
            SupportTower st1 = _mockedGameData.Cells[0, 1] as SupportTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(0, 1));
            int st1Lvl = st1.Level;
            Assert.AreEqual(st1Lvl, st1.Level);

            //DamageTower
            _mockedGameData.Player1Money = 500;
            _model.PlaceDamageTower(0, 2);
            _mockedGameData.Player1Money = 0;
            DamageTower dt1 = _mockedGameData.Cells[0, 2] as DamageTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(0, 2));
            int dt1Lvl = dt1.Level;
            Assert.AreEqual(dt1Lvl, dt1.Level);

            //RangedTower
            _mockedGameData.Player1Money = 500;
            _model.PlaceRangedTower(0, 3);
            _mockedGameData.Player1Money = 0;
            RangedTower rt1 = _mockedGameData.Cells[0, 3] as RangedTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(0, 3));
            int rt1Lvl = rt1.Level;
            Assert.AreEqual(rt1Lvl, rt1.Level);

            //Player_2
            _model.GamePhase = GamePhase.Player2;

            //SupportTower
            _mockedGameData.Player2Money = 500;
            _model.PlaceSupportTower(4, 3);
            _mockedGameData.Player2Money = 0;
            SupportTower st2 = _mockedGameData.Cells[4, 3] as SupportTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(4, 3));
            int st2Lvl = st2.Level;
            Assert.AreEqual(st2Lvl, st2.Level);

            //DamageTower
            _mockedGameData.Player2Money = 500;
            _model.PlaceDamageTower(4, 2);
            _mockedGameData.Player2Money = 0;
            DamageTower dt2 = _mockedGameData.Cells[4, 2] as DamageTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(4, 2));
            int dt2Lvl = dt2.Level;
            Assert.AreEqual(dt2Lvl, dt2.Level);

            //RangedTower
            _mockedGameData.Player2Money = 500;
            _model.PlaceRangedTower(4, 1);
            _mockedGameData.Player2Money = 0;
            RangedTower rt2 = _mockedGameData.Cells[4, 1] as RangedTower;
            Assert.AreEqual(GridExceptionEnum.MONEY, _model.CanUpgradeTower(4, 1));
            int rt2Lvl = rt2.Level;
            Assert.AreEqual(rt2Lvl, rt2.Level);
        }
        #endregion

        #region RemoveTowers
        [TestMethod]
        public void RemoveTowersSuccessfulTest()
        {
            //Player_1
            _mockedGameData.Player1Money = 1000;
            _model.GamePhase = GamePhase.Player1;

            _model.PlaceSupportTower(0, 1);
            _model.PlaceDamageTower(3, 0);
            _model.PlaceRangedTower(0, 2);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(0, 2));
            _model.RemoveTower(0, 2);
            Assert.AreEqual("Plain", _mockedGameData.Cells[0, 2].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(3, 0));
            _model.RemoveTower(3, 0);
            Assert.AreEqual("Plain", _mockedGameData.Cells[3, 0].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(0, 1));
            _model.RemoveTower(0, 1);
            Assert.AreEqual("Plain", _mockedGameData.Cells[0, 1].GetType().Name);

            //Player_2
            _mockedGameData.Player2Money = 1000;
            _model.GamePhase = GamePhase.Player2;

            _model.PlaceSupportTower(3, 4);
            _model.PlaceDamageTower(2, 4);
            _model.PlaceRangedTower(4, 2);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(3, 4));
            _model.RemoveTower(3, 4);
            Assert.AreEqual("Plain", _mockedGameData.Cells[3, 4].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(2, 4));
            _model.RemoveTower(2, 4);
            Assert.AreEqual("Plain", _mockedGameData.Cells[2, 4].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.NO_PROBLEM, _model.CanRemoveTower(4, 2));
            _model.RemoveTower(4, 2);
            Assert.AreEqual("Plain", _mockedGameData.Cells[4, 2].GetType().Name);

        }
        public void RemoveTowersFailTest()
        {
            //Player_1
            _mockedGameData.Player1Money = 1000;
            _model.GamePhase = GamePhase.Player1;

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(0, 1));
            Assert.AreEqual("Plain", _mockedGameData.Cells[0, 1].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(3, 1));
            Assert.AreEqual("Plain", _mockedGameData.Cells[3, 1].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(1, 1));
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 1].GetType().Name);

            //Player_2
            _mockedGameData.Player2Money = 1000;
            _model.GamePhase = GamePhase.Player2;

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(4, 3));
            Assert.AreEqual("Plain", _mockedGameData.Cells[4, 3].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(1, 3));
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 3].GetType().Name);

            Assert.AreEqual(GridExceptionEnum.INVALID_CELL, _model.CanRemoveTower(3, 3));
            Assert.AreEqual("Plain", _mockedGameData.Cells[3, 3].GetType().Name);
        }
        #endregion

        #region PathFinding
        [TestMethod]
        public void PathFindingForcedNullTest()
        {
            _mockedGameData.Cells[0, 1] = new DamageTower(PlayerType.Player1, 0, 1);
            _mockedGameData.Cells[1, 1] = new DamageTower(PlayerType.Player1, 1, 1);
            _mockedGameData.Cells[1, 0] = new DamageTower(PlayerType.Player1, 1, 0);

            int[,] tmp = PathFinding.GetPathFindMatrix(_mockedGameData);
            Assert.IsNull(PathFinding.PathFind(tmp, _mockedGameData));
        }
        [TestMethod]
        public void PathFindingSuccessFulTest()
        {
            //Csak v?rak ?s akad?lyok
            int[,] tmp = PathFinding.GetPathFindMatrix(_mockedGameData);
            Assert.IsNotNull(PathFinding.PathFind(tmp, _mockedGameData));

            //V?rak, akad?lyok ?s tornyok
            _mockedGameData.Cells[0, 1] = new DamageTower(PlayerType.Player1, 0, 1);
            _mockedGameData.Cells[1, 1] = new DamageTower(PlayerType.Player1, 1, 1);
            _mockedGameData.Cells[3, 3] = new DamageTower(PlayerType.Player2, 3, 3);
            int[,] tmp2 = PathFinding.GetPathFindMatrix(_mockedGameData);
            Assert.IsNotNull(PathFinding.PathFind(tmp2, _mockedGameData));
        }
        #endregion

        #region Simulation
        [TestMethod]
        public void SimulationDoneCheckSoldiers()
        {
            _model.Player1Money = 10000;
            _model.Player2Money = 10000;
            _model.GamePhase = GamePhase.Player1;
            _model.BuyAttackSoldier();
            _model.GamePhase = GamePhase.Player2;
            _model.BuyAttackSoldier();
            _model.GamePhase = GamePhase.Simulation;

            var method = _model.GetType().GetMethod("Simulation_Tick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var method2 = _model.GetType().GetMethod("StorePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method2.Invoke(_model, new object[] { });

            while (_mockedGameData.Player1Soldiers.Count != 0 && _mockedGameData.Player2Soldiers.Count != 0)
            {
                method.Invoke(_model, new object[] { this, null });

            }
            Assert.AreEqual(0, _mockedGameData.Player1Soldiers.Count);
            Assert.AreEqual(0, _mockedGameData.Player2Soldiers.Count);
        }
        [TestMethod]
        public void SimulationDoneCheckCastle()
        {
            _model.Player1Money = 10000;
            _model.Player2Money = 10000;
            _model.GamePhase = GamePhase.Player1;
            _model.BuyAttackSoldier();
            _model.GamePhase = GamePhase.Player2;
            _model.BuyAttackSoldier();
            _model.GamePhase = GamePhase.Simulation;

            var method = _model.GetType().GetMethod("Simulation_Tick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var method2 = _model.GetType().GetMethod("StorePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method2.Invoke(_model, new object[] { });

            while (_mockedGameData.Player1Soldiers.Count != 0 && _mockedGameData.Player2Soldiers.Count != 0)
            {
                method.Invoke(_model, new object[] { this, null });
            }

            Assert.IsTrue(100 > _model.Player1CastleHP);
            Assert.IsTrue(100 > _model.Player2CastleHP);
        }

        [TestMethod]
        public void SimulationDoneCheckTowers()
        {
            _model.Player1Money = 10000;
            _model.Player2Money = 10000;
            _model.GamePhase = GamePhase.Player1;
            _model.PlaceDamageTower(0, 1);
            _model.PlaceDamageTower(0, 2);
            _model.GamePhase = GamePhase.Player2;
            _model.BuyAttackSoldier();
            _model.GamePhase = GamePhase.Simulation;

            var method2 = _model.GetType().GetMethod("StorePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method2.Invoke(_model, new object[] { });

            bool isOk = false;
            var method = _model.GetType().GetMethod("Simulation_Tick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            while (_mockedGameData.Player2Soldiers.Count != 0)
            {
                method.Invoke(_model, new object[] { this, null });
                if (_mockedGameData.Player2Soldiers.Count == 0 && _model.Player1CastleHP == 100)
                {
                    isOk = true;
                }
            }

            Assert.IsTrue(isOk);

        }
        #endregion

        #region MapEditor
        [TestMethod]
        public void SaveTestEditor()
        {
            _editorModel.SaveGame(String.Empty);
            _mock.Verify(dataAccess => dataAccess.SaveAsync(String.Empty, _mockedGameData), Times.Once());
        }

        [TestMethod]
        public async Task LoadTestEditor()
        {
            await _editorModel.LoadGame(String.Empty);
            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }
        [TestMethod]
        public void CastleOnePlaceSuccessfulTests()
        {
            _editorModel.MapCastleOne(1, 1);
            Assert.AreEqual("Castle", _mockedGameData.Cells[1, 1].GetType().Name);
            Assert.AreEqual(PlayerType.Player1, (_mockedGameData.Cells[1, 1] as TowerDefensePersistence.Castle).Player);
        }
        [TestMethod]
        public void CastleTwoPlaceSuccessfulTests()
        {
            _editorModel.MapCastleTwo(4, 4);
            Assert.AreEqual("Castle", _mockedGameData.Cells[4, 4].GetType().Name);
            Assert.AreEqual(PlayerType.Player2, (_mockedGameData.Cells[4, 4] as TowerDefensePersistence.Castle).Player);
        }
        [TestMethod]
        public void ObstaclePlaceSuccessfulTests()
        {
            _editorModel.CellPlace(2, 2, ObstaclePhase.Mountain);
            Assert.AreEqual("Mountain", _mockedGameData.Cells[2, 2].GetType().Name);
            _editorModel.CellPlace(2, 4, ObstaclePhase.Water);
            Assert.AreEqual("Water", _mockedGameData.Cells[2, 4].GetType().Name);
            _editorModel.CellPlace(1, 3, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 3].GetType().Name);
        }
        [TestMethod]
        public void CastleOneAndTwoNotFarEnoughTest()
        {
            _editorModel.MapCastleOne(1, 1);
            Assert.AreEqual("Castle", _mockedGameData.Cells[1, 1].GetType().Name);
            Assert.AreEqual(PlayerType.Player1, (_mockedGameData.Cells[1, 1] as TowerDefensePersistence.Castle).Player);
            _editorModel.MapCastleTwo(1, 1);
            Assert.AreEqual("Castle", _mockedGameData.Cells[1, 1].GetType().Name);
            Assert.AreEqual(PlayerType.Player1, (_mockedGameData.Cells[1, 1] as TowerDefensePersistence.Castle).Player);
        }
        [TestMethod]
        public void OverWriteTests()
        {
            _editorModel.CellPlace(2, 2, ObstaclePhase.Mountain);
            Assert.AreEqual("Mountain", _mockedGameData.Cells[2, 2].GetType().Name);
            _editorModel.CellPlace(2, 4, ObstaclePhase.Water);
            Assert.AreEqual("Water", _mockedGameData.Cells[2, 4].GetType().Name);
            _editorModel.CellPlace(1, 3, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 3].GetType().Name);

            _editorModel.CellPlace(2, 2, ObstaclePhase.Water);
            Assert.AreEqual("Water", _mockedGameData.Cells[2, 2].GetType().Name);
            _editorModel.CellPlace(2, 4, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[2, 4].GetType().Name);
            _editorModel.CellPlace(1, 3, ObstaclePhase.Mountain);
            Assert.AreEqual("Mountain", _mockedGameData.Cells[1, 3].GetType().Name);

            _editorModel.MapCastleOne(2, 2);
            Assert.AreEqual("Castle", _mockedGameData.Cells[2, 2].GetType().Name);
            _editorModel.MapCastleTwo(1, 3);
            Assert.AreEqual("Castle", _mockedGameData.Cells[1, 3].GetType().Name);
        }

        [TestMethod]
        public void SetEmptyTests()
        {
            _editorModel.CellPlace(2, 2, ObstaclePhase.Mountain);
            Assert.AreEqual("Mountain", _mockedGameData.Cells[2, 2].GetType().Name);
            _editorModel.CellPlace(2, 2, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[2, 2].GetType().Name);

            _editorModel.CellPlace(2, 4, ObstaclePhase.Water);
            Assert.AreEqual("Water", _mockedGameData.Cells[2, 4].GetType().Name);
            _editorModel.CellPlace(2, 4, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[2, 4].GetType().Name);

            _editorModel.CellPlace(1, 3, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 3].GetType().Name);
            _editorModel.CellPlace(1, 3, ObstaclePhase.Empty);
            Assert.AreEqual("Plain", _mockedGameData.Cells[1, 3].GetType().Name);
        }
        [TestMethod]
        public void ResizeTest()
        {
            Assert.AreEqual(5, _editorModel.Rows);
            Assert.AreEqual(5, _editorModel.Cols);
            _editorModel.ResizeMap(10, 10);
            Assert.AreEqual(10, _editorModel.Rows);
            Assert.AreEqual(10, _editorModel.Cols);
        }

        public void SetMoneyTest()
        {
            _editorModel.StartMoney = 1000;
            Assert.AreEqual(_mockedGameData.Player1Money, _mockedGameData.Player2Money);
        }
        #endregion

        #region GameStages
        [TestMethod]
        public void PlayerOneStage()
        {
            _model.GamePhase = GamePhase.Player1;
            Assert.AreEqual(GamePhase.Player1, _model.GamePhase);
        }
        [TestMethod]
        public void PlayerTwoStage()
        {
            _model.GamePhase = GamePhase.Player2;
            Assert.AreEqual(GamePhase.Player2, _model.GamePhase);
        }
        [TestMethod]
        public void SimulationStage()
        {
            _model.GamePhase = GamePhase.Simulation;
            Assert.AreEqual(GamePhase.Simulation, _model.GamePhase);
        }
        #endregion
    }
}