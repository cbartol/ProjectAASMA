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
        public override void DoActions()
        {
            List<Point> visibleAZN = getAASMAFramework().visibleAznPoints(this);
            List<Point> visibleEmptyNeedle = getAASMAFramework().visibleEmptyNeedles(this);

            if (Stock < ContainerCapacity && this.getAASMAFramework().overAZN(this))
            {
                this.collectAZN();
            }
            else if (Stock > 0 && this.getAASMAFramework().overEmptyNeedle(this))
            {
                this.transferAZN();
            }
            else if (Stock < ContainerCapacity && visibleAZN.Count > 0)
            {
                Point target = visibleAZN[0];
                this.MoveTo(target);
            }
            else if (Stock > 0 && visibleEmptyNeedle.Count > 0)
            {
                Point target = visibleEmptyNeedle[0];
                this.MoveTo(target);
            }
            else if (frontClear())
            {
                if(Utils.randomValue(100) < 80)
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
