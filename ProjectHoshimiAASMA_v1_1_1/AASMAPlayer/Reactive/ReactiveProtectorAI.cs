using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;

namespace AASMAHoshimi.Reactive
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    class ReactiveProtectorAI : AASMAProtector
    {
        public override void DoActions()
        {
            List<Point> visibleEnemies = getAASMAFramework().visiblePierres(this);
            Point enemyPosition;
            Point aiPosition = getAASMAFramework().AI.Location;

            if (visibleEnemies.Count > 0)
            {
                enemyPosition = Utils.getNearestPoint(this.Location, visibleEnemies);
                if (Utils.SquareDistance(this.Location, enemyPosition) <= this.DefenseDistance * this.DefenseDistance)
                {
                    this.DefendTo(enemyPosition, 10);
                    return;
                }
            }
            if (Utils.SquareDistance(this.Location, aiPosition) <= this.Scan) {
                this.MoveTo(aiPosition);
            }
            if (frontClear())
            {
                if (Utils.randomValue(100) < 95)
                {
                    this.MoveForward();
                }
                else
                {
                    this.RandomTurn();
                }
            }
            else
            {
                this.RandomTurn();
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
