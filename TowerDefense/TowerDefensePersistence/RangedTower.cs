using System;

namespace TowerDefensePersistence
{
    public class RangedTower : Tower
    {
        public RangedTower(PlayerType inplayer, int inrow, int incol)
        : base(inplayer, inrow, incol)
        {
            Range = 3;
            Damage = 5;
            TargetCount = 2;
        }

        public override void Upgrade()
        {
            base.Upgrade();
            switch (Level)
            {
                case 2:
                    Damage += 3;
                    Range++;
                    break;
                case 3:
                    TargetCount++;
                    break;
                case 4:
                    Damage += 3;
                    break;
                case 5:
                    Damage += 5;
                    Range++;
                    TargetCount++;
                    break;
                default: throw new Exception();
            }
        }

        public override int UpgradeCost()
        {
            switch (Level)
            {
                case 1:
                    return 50;
                case 2:
                    return 100;
                case 3:
                    return 120;
                case 4:
                    return 140;
                case 5:
                    return 160;
                default: throw new Exception();
            }
        }

        public override int RemovalGain()
        {
            return RangedTower.BuildCost() / 2 + Level * 20;
        }

        public static int BuildCost()
        {
            return 100;
        }
    }
}
