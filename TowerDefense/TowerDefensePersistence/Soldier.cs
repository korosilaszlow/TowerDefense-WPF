namespace TowerDefensePersistence
{
    public abstract class Soldier
    {
        protected int hitpoints;
        protected int damage;
        private PlayerType player;

        public Soldier(PlayerType inplayer)
        {
            player = inplayer;
        }

        public bool IsDead { get => hitpoints <= 0; }
        public int HitPoints { get => hitpoints; set => hitpoints = value; }
        public int Damage { get => damage; set => damage = value; }
        public PlayerType Player { get => player; set => player = value; }
    }
}
