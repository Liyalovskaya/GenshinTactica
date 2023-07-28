using UnityEngine;

namespace GT.Core
{
    public class Actor
    {
        public string Name;
        public BattleGrid BattleGrid;
        public int MoveRange = 5;

        public Actor(string name, BattleGrid grid)
        {
            Name = name;
            BattleGrid = grid;
        }

        public void MoveTo(BattleGrid grid)
        {
            BattleGrid = grid;
        }
    }
}
