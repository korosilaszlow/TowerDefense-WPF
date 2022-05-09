using System.Collections.Generic;

namespace TowerDefensePersistence
{
    public abstract class SoldierContainer : Cell
    {
        private List<Soldier> player1Soldiers;
        private List<Soldier> player2Soldiers;

        protected List<Soldier> player1DeadSoldiers;
        protected List<Soldier> player2DeadSoldiers;


        public List<Soldier> Player1Soldiers { get => player1Soldiers; set => player1Soldiers = value; }
        public List<Soldier> Player2Soldiers { get => player2Soldiers; set => player2Soldiers = value; }
        public List<Soldier> Player1DeadSoldiers { get => player1DeadSoldiers; set => player1DeadSoldiers = value; }
        public List<Soldier> Player2DeadSoldiers { get => player2DeadSoldiers; set => player2DeadSoldiers = value; }


        public SoldierContainer(int inrow, int incol)
        : base(inrow, incol)
        {
            Player1Soldiers = new List<Soldier>();
            player2Soldiers = new List<Soldier>();
            player1DeadSoldiers = new List<Soldier>();
            player2DeadSoldiers = new List<Soldier>();
        }
    }
}
