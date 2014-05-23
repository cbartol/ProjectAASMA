using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AASMAHoshimi
{
    class DefendAction : Action
    {
        private AASMAProtector ownerProtector;
        private Point enemy;
        private int nTurn;

        public DefendAction(AASMAProtector ownerProtector, Point enemy, int nTurn)
        {
            this.ownerProtector = ownerProtector;
            this.enemy = enemy;
            this.nTurn = nTurn;
        }

        public void execute()
        {
            this.ownerProtector.DefendTo(enemy, nTurn);
        }

        public void cancel()
        {
            this.ownerProtector.StopMoving();
        }
    }
}
