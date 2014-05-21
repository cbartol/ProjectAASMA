using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AASMAHoshimi;
using PH.Common;

namespace AASMAHoshimi.Reactive
{
    [Characteristics(ContainerCapacity = 20, CollectTransfertSpeed = 5, Scan = 0, MaxDamage = 0, DefenseDistance = 0, Constitution = 20)]
    public class ReactiveCollector : AASMAContainer
    {
        private void flee(List<Point> enemies)
        {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies)
            {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            this.MoveTo(Utils.getMiddlePoint(possibleMoves.ToArray()));
        }

        public override void DoActions()
        {
            List<Point> visibleAZN = getAASMAFramework().visibleAznPoints(this);
            List<Point> visibleEmptyNeedle = getAASMAFramework().visibleEmptyNeedles(this);

            if (getAASMAFramework().visiblePierres(this).Count != 0)
            {
                List<Point> enemies = getAASMAFramework().visiblePierres(this);
                flee(enemies);
            }

            if (Stock < ContainerCapacity && this.getAASMAFramework().overAZN(this))
            {
                // Collect AZN if still has capacity and if it's over an AZN Point
                this.collectAZN();
            }
            else if (Stock > 0 && this.getAASMAFramework().overEmptyNeedle(this))
            {
                // Transfer AZN if still has AZN and if it's over a Needle
                this.transferAZN();
            }
            else if (Stock < ContainerCapacity && visibleAZN.Count > 0)
            {
                // Move to a visible AZN Point if container is not full
                Point target = visibleAZN[0];
                this.MoveTo(target);
            }
            else if (Stock > 0 && visibleEmptyNeedle.Count > 0)
            {
                // Move to a visible "empty" needle if still has AZN Points
                Point target = visibleEmptyNeedle[0];
                this.MoveTo(target);
            }
            else if (frontClear())
            {
                // Move front with 80% chance
                if (Utils.randomValue(100) < 80)
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
                // Change direction randomly because it can't move front
                this.RandomTurn();
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            getAASMAFramework().logData(this, "received message from " + msg.Sender + " : " + msg.Content);
        }
    }
}
