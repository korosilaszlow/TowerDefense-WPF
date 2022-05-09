using System;

namespace TowerDefensePersistence
{
    public class DamageTower : Tower
    {
        public DamageTower(PlayerType inplayer, int inrow, int incol)
        : base(inplayer, inrow, incol)
        {
            Range = 2;
            Damage = 10;
            TargetCount = 2;
        }

        public override void Upgrade()
        {
            base.Upgrade();
            switch (Level)
            {

                case 2:
                    Damage += 5;
                    break;
                case 3:
                    TargetCount++;
                    break;
                case 4:
                    Damage += 5;
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
                    return 80;
                case 2:
                    return 140;
                case 3:
                    return 160;
                case 4:
                    return 180;
                case 5:
                    return 200;
                default: throw new Exception();
            }
        }

        public override int RemovalGain()
        {
            return DamageTower.BuildCost() / 2 + Level * 20;
        }

        public static int BuildCost()
        {
            return 200;
        }
    }
}
