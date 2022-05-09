using System;

namespace TowerDefensePersistence
{
    public class SupportTower : Tower
    {
        private int healAmount;

        public event EventHandler KilledUnit;

        public int HealAmount { get => healAmount; set => healAmount = value; }

        public SupportTower(PlayerType inplayer, int inrow, int incol)
        : base(inplayer, inrow, incol)
        {
            Range = 2;
            Damage = 5;
            TargetCount = 1;
            healAmount = 5;
        }

        public override void AttackSoldier(Soldier s)
        {
            base.AttackSoldier(s);
            if (s.IsDead)
            {
                KilledUnit?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void Upgrade()
        {
            base.Upgrade();
            switch (Level)
            {
                case 2:
                    HealAmount++;
                    break;
                case 3:
                    Damage += 5;
                    Range++;
                    break;
                case 4:
                    TargetCount++;
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
                    return 70;
                case 2:
                    return 120;
                case 3:
                    return 140;
                case 4:
                    return 160;
                case 5:
                    return 170;
                default: throw new Exception();
            }
        }
        public override int RemovalGain()
        {
            return SupportTower.BuildCost() / 2 + Level * 20;
        }
        public static int BuildCost()
        {
            return 150;
        }
    }
}
