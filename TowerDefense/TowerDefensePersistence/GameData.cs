using System.Collections.Generic;

namespace TowerDefensePersistence
{
    public class GameData
    {
        #region Private datas
        private Cell[,] cells;
        private List<Soldier> player1Soldiers;
        private List<Soldier> player2Soldiers;
        private List<Tower> player1Tower;
        private List<Tower> player2Tower;
        private Castle player1Castle;
        private Castle player2Castle;

        private int round;
        private PlayerType currentPlayer;
        private int player1Money;
        private int player2Money;
        private int rows;
        private int cols;
        private bool gameOver;
        private int player1KillsThisRound;
        private int player2KillsThisRound;
        private int player1TotalSoldiersCount;
        private int player2TotalSoldiersCount;

        private int basicIncome = 20;
        #endregion

        #region Properties

        public Cell[,] Cells { get => cells; set => cells = value; }
        public PlayerType CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }
        public int Player1Money { get => player1Money; set => player1Money = value; }
        public int Player2Money { get => player2Money; set => player2Money = value; }
        public int Rows { get => rows; set => rows = value; }
        public int Cols { get => cols; set => cols = value; }
        public int Round { get => round; set => round = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }
        public int Player1MoneyIncome { get => 5 * player1TotalSoldiersCount + 15 * player1KillsThisRound + basicIncome; }
        public int Player2MoneyIncome { get => 5 * player2TotalSoldiersCount + 15 * player2KillsThisRound + basicIncome; }
        public int Player1KillsThisRound { get => player1KillsThisRound; set => player1KillsThisRound = value; }
        public int Player2KillsThisRound { get => player2KillsThisRound; set => player2KillsThisRound = value; }
        public int Player1TotalSoldiersCount { get => player1TotalSoldiersCount; set => player1TotalSoldiersCount = value; }
        public int Player2TotalSoldiersCount { get => player2TotalSoldiersCount; set => player2TotalSoldiersCount = value; }
        public List<Tower> Player1Towers { get => player1Tower; set => player1Tower = value; }
        public List<Tower> Player2Towers { get => player2Tower; set => player2Tower = value; }
        public List<Soldier> Player2Soldiers { get => player2Soldiers; set => player2Soldiers = value; }
        public List<Soldier> Player1Soldiers { get => player1Soldiers; set => player1Soldiers = value; }
        public Castle Player1Castle { get => player1Castle; set => player1Castle = value; }
        public Castle Player2Castle { get => player2Castle; set => player2Castle = value; }
        #endregion

        #region Constructor
        public GameData()
        {
            Cells = new Cell[rows, cols];
            player1Soldiers = new List<Soldier>();
            player2Soldiers = new List<Soldier>();
            player1Tower = new List<Tower>();
            player2Tower = new List<Tower>();
        }

        public GameData(int row, int col)
        {
            cells = new Cell[row, col];
            Rows = row;
            Cols = col;
            player1Soldiers = new List<Soldier>();
            player2Soldiers = new List<Soldier>();
            player1Tower = new List<Tower>();
            player2Tower = new List<Tower>();
        }
        #endregion
    }
}
