namespace TowerDefensePersistence
{
    public class AttackSoldier : Soldier
    {
        public AttackSoldier(PlayerType inplayer)
        : base(inplayer)
        {
            damage = 10;
            hitpoints = 35;
        }

        public static int BuildCost()
        {
            return 30;
        }
    }
}
