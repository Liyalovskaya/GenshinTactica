using System;
using UnityEngine;

namespace GT.Core
{
    public class Actor
    {
        public string Name;
        private BattleGrid _battleGrid;

        public BattleGrid BattleGrid
        {
            get => _battleGrid;
            set
            {
                if (value == null)
                {
                    _battleGrid.Actor = null;
                    _battleGrid = null;
                }
                else
                {
                    if (_battleGrid != null)
                    {
                        _battleGrid.Actor = null;
                    }
                    _battleGrid = value;
                    _battleGrid.Actor = this;
                }
            }
        }
        public int MoveRange = 5;
        public ActorType Type;

        public Actor(string name, BattleGrid grid)
        {
            Name = name;
            BattleGrid = grid;
            BattleGrid.Actor = this;
        }

        // public void MoveTo(BattleGrid grid)
        // {
        //     BattleGrid.Actor = null;
        //     BattleGrid = grid;
        //     BattleGrid.Actor = this;
        // }
    }

    public enum ActorType
    {
        Player,
        Ally,
        Enemy,
    }
}
