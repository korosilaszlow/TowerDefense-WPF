using System;

namespace TowerDefensePersistence
{
    public class Castle : SoldierContainer
    {
        private int hitpoints;
        private PlayerType player;

        public PlayerType Player { get => player; set => player = value; }
        public int Hitpoints { get => hitpoints; set => hitpoints = Math.Max(Math.Min(value, 100), 0); }

        public Castle(PlayerType inplayer, int inrow, int incol)
        : base(inrow, incol)
        {
            Hitpoints = 100;
            player = inplayer;
        }

        public void SupportTower_KilledUnit(object obj, EventArgs e)
        {
            Hitpoints += (obj as SupportTower).HealAmount;
        }

        public int TakeDamage(Soldier s)
        {
            Hitpoints -= s.Damage;
            return s.Damage;
        }
    }
}
