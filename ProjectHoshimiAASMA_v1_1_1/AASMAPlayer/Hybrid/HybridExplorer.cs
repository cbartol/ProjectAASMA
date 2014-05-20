using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Hybrid {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class HybridExplorer : AASMAExplorer {
        private List<Point> visitedPositions = new List<Point>();
        private List<Point> pointsToVisit = new List<Point>();

        private Point exploringPoint = Point.Empty;
        public HybridExplorer() {

        }

        public override void DoActions() {
            if (exploringPoint == this.Location) {
                visitedPositions.Add(exploringPoint);
            }

            // get perceptions
            List<Point> visibleObjectives = getAASMAFramework().visibleNavigationPoints(this);
            List<Point> nearPierres = getAASMAFramework().visiblePierres(this);

            // reactive action ||| avoid enemies
            if (nearPierres.Count > 0) {
                this.StopMoving();
                this.MoveTo(flee(nearPierres));
                return;
            }

            addObjectivesToVisit(visibleObjectives);
            // get Desires
            Desire desire = getDesire();
            // get Intentions
            Point intention = getIntention(desire);
            // do a plan
            // the plan is move the position returned from getting the intention

            // execute the plan
            this.MoveTo(intention);
        }

        private Desire getDesire() {
            if (pointsToVisit.Count > 0) {
                return Desire.EXPLORE;
            }
            return Desire.MOVE_AROUND;
        }

        private Point getIntention(Desire desire) {
            switch (desire) {
                case Desire.EXPLORE:
                    exploringPoint = Utils.randomPoint(pointsToVisit);
                    pointsToVisit.Remove(exploringPoint);
                    return exploringPoint;
                case Desire.MOVE_AROUND:
                default:
                    return Utils.randomValidPoint(this.getAASMAFramework().Tissue);
            }
        }

        private void addObjectivesToVisit(List<Point> visibleObjectives) {
            foreach (Point p in visibleObjectives) {
                if (!pointsToVisit.Contains(p) && !visitedPositions.Contains(p)) {
                    pointsToVisit.Add(p);
                }
            }
        }

        public override void receiveMessage(AASMAMessage msg) {
            getAASMAFramework().logData(this, "received message from " + msg.Sender + " : " + msg.Content);
        }

        private Point flee(List<Point> enemies) {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies) {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            return Utils.getMiddlePoint(possibleMoves.ToArray());
        }
    }

    enum Desire {
        MOVE_AROUND,
        EXPLORE
    }
}
