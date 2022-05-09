using System;
using System.IO;
using System.Threading.Tasks;

namespace TowerDefensePersistence
{
    public class FilePersistence : IPersistence
    {
        private String fileName;

        public FilePersistence()
        {

        }

        public string FileName { get => fileName; set => fileName = value; }

        private int PlayerTypeToInt(PlayerType p)
        {
            return PlayerType.Player1 == p ? 1 : 2;
        }


        public async Task<GameData> LoadAsync(String path)
        {

            try
            {
                GameData gamedata = new GameData();
                using (StreamReader reader = new StreamReader(path))
                {


                    String line = await reader.ReadLineAsync();
                    String[] nums = line.Split(' ');
                    int player1Money = int.Parse(nums[0]);
                    int player2Money = int.Parse(nums[1]);

                    line = await reader.ReadLineAsync();
                    nums = line.Split(' ');
                    int player1TotalSoldierCount = int.Parse(nums[0]);
                    int player1KillsThisRound = int.Parse(nums[1]);

                    line = await reader.ReadLineAsync();
                    nums = line.Split(' ');
                    int player2TotalSoldierCount = int.Parse(nums[0]);
                    int player2KillsThisRound = int.Parse(nums[1]);

                    line = await reader.ReadLineAsync();
                    nums = line.Split(' ');
                    int round = int.Parse(nums[0]);
                    int currPlayer = int.Parse(nums[1]); // 1 - Player1 -- 2 -- PLayer2
                    if (currPlayer == 1)
                    {
                        gamedata.CurrentPlayer = PlayerType.Player1;
                    }
                    else
                    {
                        gamedata.CurrentPlayer = PlayerType.Player2;
                    }

                    line = await reader.ReadLineAsync();
                    nums = line.Split(' ');
                    int rows = int.Parse(nums[0]);
                    int cols = int.Parse(nums[1]);
                    gamedata.Cells = new Cell[rows, cols];
                    gamedata.Rows = rows;
                    gamedata.Cols = cols;
                    gamedata.Round = round;

                    gamedata.Player1Money = player1Money;
                    gamedata.Player2Money = player2Money;

                    gamedata.Player1KillsThisRound = player1KillsThisRound;
                    gamedata.Player2KillsThisRound = player2KillsThisRound;

                    gamedata.Player1TotalSoldiersCount = player1TotalSoldierCount;
                    gamedata.Player2TotalSoldiersCount = player2TotalSoldierCount;

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            line = await reader.ReadLineAsync();
                            nums = line.Split(' ');

                            //Plain
                            if (nums[0] == "Plain")
                            {
                                Plain p = new Plain(i, j);
                                gamedata.Cells[i, j] = p;
                            }

                            //Castle
                            if (nums[0] == "Castle")
                            {
                                if (int.Parse(nums[1]) == 1)
                                {
                                    Castle c = new Castle(PlayerType.Player1, i, j);
                                    c.Hitpoints = int.Parse(nums[2]);
                                    for (int l = 0; l < int.Parse(nums[3]); l++)
                                    {
                                        AttackSoldier aS = new AttackSoldier(PlayerType.Player1);
                                        c.Player1Soldiers.Add(aS);
                                        gamedata.Player1Soldiers.Add(aS);
                                    }
                                    for (int l = 0; l < int.Parse(nums[4]); l++)
                                    {
                                        TankSoldier tS = new TankSoldier(PlayerType.Player1);
                                        c.Player1Soldiers.Add(tS);
                                        gamedata.Player1Soldiers.Add(tS);
                                    }
                                    gamedata.Cells[i, j] = c;
                                    gamedata.Player1Castle = c;
                                }
                                else
                                {
                                    Castle c = new Castle(PlayerType.Player2, i, j);
                                    c.Hitpoints = int.Parse(nums[2]);
                                    for (int l = 0; l < int.Parse(nums[3]); l++)
                                    {
                                        AttackSoldier aS = new AttackSoldier(PlayerType.Player2);
                                        c.Player2Soldiers.Add(aS);
                                        gamedata.Player2Soldiers.Add(aS);
                                    }
                                    for (int l = 0; l < int.Parse(nums[4]); l++)
                                    {
                                        TankSoldier tS = new TankSoldier(PlayerType.Player2);
                                        c.Player2Soldiers.Add(tS);
                                        gamedata.Player2Soldiers.Add(tS);
                                    }
                                    gamedata.Cells[i, j] = c;
                                    gamedata.Player2Castle = c;
                                }
                            }

                            //RangedTower
                            if (nums[0] == "RangedTower")
                            {
                                if (int.Parse(nums[1]) == 1)
                                {
                                    RangedTower r = new RangedTower(PlayerType.Player1, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player1Towers.Add(r);
                                }
                                else
                                {
                                    RangedTower r = new RangedTower(PlayerType.Player2, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player2Towers.Add(r);
                                }

                            }
                            //DamageTower
                            if (nums[0] == "DamageTower")
                            {
                                if (int.Parse(nums[1]) == 1)
                                {
                                    DamageTower r = new DamageTower(PlayerType.Player1, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player1Towers.Add(r);
                                }
                                else
                                {
                                    DamageTower r = new DamageTower(PlayerType.Player2, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player2Towers.Add(r);
                                }

                            }
                            //SupportTower
                            if (nums[0] == "SupportTower")
                            {
                                if (int.Parse(nums[1]) == 1)
                                {
                                    SupportTower r = new SupportTower(PlayerType.Player1, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    r.HealAmount = int.Parse(nums[6]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player1Towers.Add(r);
                                    r.KilledUnit += gamedata.Player1Castle.SupportTower_KilledUnit;
                                }
                                else
                                {
                                    SupportTower r = new SupportTower(PlayerType.Player2, i, j);
                                    r.Level = int.Parse(nums[2]);
                                    r.Range = int.Parse(nums[3]);
                                    r.Damage = int.Parse(nums[4]);
                                    r.TargetCount = int.Parse(nums[5]);
                                    r.HealAmount = int.Parse(nums[6]);
                                    gamedata.Cells[i, j] = r;
                                    gamedata.Player2Towers.Add(r);
                                    r.KilledUnit += gamedata.Player2Castle.SupportTower_KilledUnit;
                                }

                            }

                            //Mountain
                            if (nums[0] == "Mountain")
                            {
                                Mountain m = new Mountain(i, j);
                                gamedata.Cells[i, j] = m;
                            }

                            //Water
                            if (nums[0] == "Water")
                            {
                                Water w = new Water(i, j);
                                gamedata.Cells[i, j] = w;
                            }
                        }
                    }


                }
                return gamedata;
            }
            catch (Exception)
            {

                throw new FilePersistenceException();
            }

        }
        public async Task SaveAsync(String path, GameData gamedata)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    int currPlayer;
                    if (gamedata.CurrentPlayer == PlayerType.Player1)
                    {
                        currPlayer = 1;
                    }
                    else
                    {
                        currPlayer = 2;
                    }
                    await writer.WriteAsync(gamedata.Player1Money + " " + gamedata.Player2Money + "\n");
                    await writer.WriteAsync(gamedata.Player1TotalSoldiersCount + " " + gamedata.Player1KillsThisRound + "\n");
                    await writer.WriteAsync(gamedata.Player2TotalSoldiersCount + " " + gamedata.Player2KillsThisRound + "\n");
                    await writer.WriteAsync(gamedata.Round + " " + currPlayer + "\n");
                    await writer.WriteAsync(gamedata.Rows + " " + gamedata.Cols + "\n");

                    for (int i = 0; i < gamedata.Rows; i++)
                    {
                        for (int j = 0; j < gamedata.Cols; j++)
                        {
                            if (gamedata.Cells[i, j] is Plain)
                            {
                                await writer.WriteAsync("Plain\n");
                            }
                            if (gamedata.Cells[i, j] is Castle)
                            {
                                int dbAs = 0;
                                int dbTs = 0;
                                Castle tmp = (Castle)gamedata.Cells[i, j];
                                await writer.WriteAsync("Castle " + PlayerTypeToInt(tmp.Player) + " " + tmp.Hitpoints);
                                if (tmp.Player == PlayerType.Player1)
                                {
                                    for (int k = 0; k < tmp.Player1Soldiers.Count; k++)
                                    {
                                        if (tmp.Player1Soldiers[k] is AttackSoldier)
                                        {
                                            dbAs++;
                                        }
                                        if (tmp.Player1Soldiers[k] is TankSoldier)
                                        {
                                            dbTs++;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int k = 0; k < tmp.Player2Soldiers.Count; k++)
                                    {
                                        if (tmp.Player2Soldiers[k] is AttackSoldier)
                                        {
                                            dbAs++;
                                        }
                                        if (tmp.Player2Soldiers[k] is TankSoldier)
                                        {
                                            dbTs++;
                                        }
                                    }
                                }
                                await writer.WriteAsync(" " + dbAs + " " + dbTs + "\n");
                            }
                            if (gamedata.Cells[i, j] is Mountain)
                            {
                                await writer.WriteAsync("Mountain\n");
                            }
                            if (gamedata.Cells[i, j] is Water)
                            {
                                await writer.WriteAsync("Water\n");
                            }
                            if (gamedata.Cells[i, j] is RangedTower)
                            {
                                RangedTower tmp = (RangedTower)gamedata.Cells[i, j];
                                await writer.WriteAsync("RangedTower " + PlayerTypeToInt(tmp.Player) + " " + tmp.Level + " " + tmp.Range + " " + tmp.Damage + " " + tmp.TargetCount + "\n");
                            }
                            if (gamedata.Cells[i, j] is DamageTower)
                            {
                                DamageTower tmp = (DamageTower)gamedata.Cells[i, j];
                                await writer.WriteAsync("DamageTower " + PlayerTypeToInt(tmp.Player) + " " + tmp.Level + " " + tmp.Range + " " + tmp.Damage + " " + tmp.TargetCount + "\n");
                            }
                            if (gamedata.Cells[i, j] is SupportTower)
                            {
                                SupportTower tmp = (SupportTower)gamedata.Cells[i, j];
                                await writer.WriteAsync("SupportTower " + PlayerTypeToInt(tmp.Player) + " " + tmp.Level + " " + tmp.Range + " " + tmp.Damage + " " + tmp.TargetCount + " " + tmp.HealAmount + "\n");
                            }
                        }
                    }
                }
            }
            catch
            {

                throw new FilePersistenceException();
            }
        }
    }
}
