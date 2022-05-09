using System;

namespace TowerDefensePersistence
{
    public abstract class Tower : Cell
    {
        const int maxLevel = 5;
        private int level;
        private int range;
        private int damage;
        private int targetCount;
        private PlayerType player;

        public PlayerType Player { get => player; set => player = value; }
        public int Level { get => level; set => level = value; }
        public int Range { get => range; set => range = value; }
        public int Damage { get => damage; set => damage = value; }
        public int TargetCount { get => targetCount; set => targetCount = value; }

        public Tower(PlayerType inplayer, int inrow, int incol)
            : base(inrow, incol)
        {
            Player = inplayer;
            Level = 1;
        }

        public bool IsInRange(int x, int y)
        {
            return Math.Abs(x - Row) + Math.Abs(y - Col) <= Range;
        }

        public virtual void AttackSoldier(Soldier s)
        {
            s.HitPoints -= Damage;
        }

        public abstract int UpgradeCost();
        public abstract int RemovalGain();

        public virtual void Upgrade()
        {
            Level++;
        }

        public bool IsMaxLevel()
        {
            return Level == maxLevel;
        }

    }
}
