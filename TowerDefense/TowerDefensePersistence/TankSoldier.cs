namespace TowerDefensePersistence
{
    public class TankSoldier : Soldier
    {
        public TankSoldier(PlayerType inplayer)
        : base(inplayer)
        {
            damage = 5;
            hitpoints = 50;
        }
        public static int BuildCost()
        {
            return 50;
        }
    }
}
