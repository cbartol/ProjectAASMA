using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Reactive {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class ReactiveExplorer : AASMAExplorer {


        public override void DoActions() {
            List<Point> objectivePoints = getAASMAFramework().visibleNavigationPoints(this);
            if(objectivePoints.Count > 0){
                this.MoveTo(Utils.randomPoint(objectivePoints));
            } else if (frontClear()) {
                int rand = Utils.randomValue(100);
                if (rand < 95) {
                    this.MoveForward();
                } else {
                    this.RandomTurn();
                }
            } else {
                this.RandomTurn();
            }
        }

        public override void receiveMessage(AASMAMessage msg) {
            getAASMAFramework().logData(this, "received message from " + msg.Sender + " : " + msg.Content);
        }
    }
}
