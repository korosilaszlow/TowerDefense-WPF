namespace TowerDefensePersistence
{
    public abstract class Cell
    {
        private int row;
        private int col;

        public int Row { get => row; set => row = value; }
        public int Col { get => col; set => col = value; }

        public int getNumber(int num)
        {
            return num * row + col;
        }

        public Cell(int inrow, int incol)
        {
            row = inrow;
            col = incol;
        }

    }
}
