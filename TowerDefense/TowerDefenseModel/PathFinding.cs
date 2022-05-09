using System.Collections.Generic;
using TowerDefensePersistence;

namespace TowerDefenseModel
{
    /// <summary>
    /// Útkeresés osztály statikus metódusokkal
    /// </summary>
    public static class PathFinding
    {
        /// <summary>
        /// Jelenlegi játék állásából megalkotja az útkereső mátrixot
        /// </summary>
        /// <param name="gameData">Jelenlegi játék adatai</param>
        /// <returns>Útkereső mátrix</returns>
        public static int[,] GetPathFindMatrix(GameData gameData)
        {
            int[,] pathFindMatrix = new int[gameData.Rows, gameData.Cols];
            for (int i = 0; i < gameData.Rows; i++)
            {
                for (int j = 0; j < gameData.Cols; j++)
                {
                    pathFindMatrix[i, j] = gameData.Cells[i, j] is SoldierContainer ? 1 : 0;
                }
            }
            return pathFindMatrix;
        }

        /// <summary>
        /// Megadja egy koordináta szomszédjait
        /// </summary>
        /// <param name="x">X koordináta</param>
        /// <param name="y">Y koordináta</param>
        /// <param name="gameData">Jelenlegi játék adatai</param>
        /// <returns>A koordináta szomszédos koordinátái</returns>
        private static List<(int x, int y)> GetNeighbours(int x, int y, GameData gameData)
        {
            var l = new List<(int, int)>();
            if (x < gameData.Rows - 1) { l.Add((x + 1, y)); }
            if (y < gameData.Cols - 1) { l.Add((x, y + 1)); }
            if (x > 0) { l.Add((x - 1, y)); }
            if (y > 0) { l.Add((x, y - 1)); }
            return l;
        }

        /// <summary>
        /// Megkeresi az útkereső mátrix és a jelenlegi játék segítségével a legrövidebb utat a két kastély között
        /// </summary>
        /// <param name="g">Útkereső mátrix</param>
        /// <param name="gameData">Jelenlegi játék adatai</param>
        /// <returns>Legrövidebb út koordinátái a második játékos várától az első játékos váráig</returns>
             
        public static List<(int, int)> PathFind(int[,] g, GameData gameData)
        {
            //g: 1, ha rá lehet lépni, 0, ha nem  
            return PathFind(g, gameData, gameData.Player1Castle.Row, gameData.Player1Castle.Col, gameData.Player2Castle.Row, gameData.Player2Castle.Col);
        }

        /// <summary>
        /// Megkeresi az útkereső mátrix és a jelenlegi játék segítségével a legrövidebb utat a két kastély között egyedileg megadott kastély kezdőhellyel
        /// </summary>
        /// <param name="g">Útkereső mátrix</param>
        /// <param name="gameData">Jelenlegi játék adatai</param>
        /// <param name="c1x">Első játékos várának x koordinátája</param>
        /// <param name="c1y">Első játékos várának y koordinátája</param>
        /// <param name="c2x">Második játékos várának x koordinátája</param>
        /// <param name="c2y">Második játékos várának y koordinátája</param>
        /// <returns>Legrövidebb út koordinátái a második játékos várától az első játékos váráig</returns>
        public static List<(int, int)> PathFind(int[,] g, GameData gameData, int c1x, int c1y, int c2x, int c2y)
        {
            int startx = c1x;
            int starty = c1y;
            int endx = c2x;
            int endy = c2y;
            if (g[startx, starty] == 0 || g[endx, endy] == 0) { return null; }
            (int d, int px, int py)[,] data = new (int d, int px, int py)[gameData.Rows, gameData.Cols];
            var q = new Queue<(int x, int y)>();
            for (int i = 0; i < gameData.Rows; i++)
            {
                for (int j = 0; j < gameData.Cols; j++)
                {
                    data[i, j] = (-1, -1, -1);
                }
            }
            data[startx, starty] = (0, -2, -2);
            var neighbours = GetNeighbours(startx, starty, gameData);
            foreach (var e in neighbours)
            {
                if (g[e.x, e.y] != 0)
                {
                    q.Enqueue(e);
                }
            }
            while (q.Count != 0)
            {
                var v = q.Dequeue();
                neighbours = GetNeighbours(v.x, v.y, gameData);
                foreach (var e in neighbours)
                {
                    if (g[e.x, e.y] == 0) { continue; }
                    if (data[e.x, e.y].d == -1)
                    {
                        q.Enqueue(e);
                    }
                    else if (data[v.x, v.y].d == -1 || data[v.x, v.y].d > data[e.x, e.y].d + 1)
                    {
                        data[v.x, v.y].d = data[e.x, e.y].d + 1;
                        data[v.x, v.y].px = e.x;
                        data[v.x, v.y].py = e.y;
                    }
                }
            }
            if (data[endx, endy].d == -1)
            {
                return null;
            }
            else
            {
                var l = new List<(int, int)>();
                (int x, int y) = (endx, endy);
                while ((x, y) != (-2, -2))
                {
                    l.Add((x, y));
                    (x, y) = (data[x, y].px, data[x, y].py);
                }
                return l;
            }
        }

    }
}
