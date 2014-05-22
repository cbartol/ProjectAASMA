using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Communicative {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class CommunicativeExplorer : AASMAExplorer {
        private List<Point> visitedPositions = new List<Point>();
        private List<Point> pointsToVisit = new List<Point>();

        private Point exploringPoint = Point.Empty;
        public CommunicativeExplorer() {

        }

        public override void DoActions() {
            if (exploringPoint == this.Location) {
                visitedPositions.Add(exploringPoint);
                broadcastVisitedPoint(exploringPoint);
            }

            // get perceptions
            List<Point> visibleObjectives = getAASMAFramework().visibleNavigationPoints(this);
            List<Point> nearPierres = getAASMAFramework().visiblePierres(this);
            List<Point> visibleHoshimies = getAASMAFramework().visibleHoshimies(this);
            List<Point> visibleAZN = getAASMAFramework().visibleAznPoints(this);

            // reactive action ||| avoid enemies
/*            if (nearPierres.Count > 0) {
                this.StopMoving();
                this.MoveTo(flee(nearPierres));
                return;
            }*/
            List<Point> filteredObjectives = filterObjectives(visibleObjectives);
            sendVisiblePointsToOthers(nearPierres, filteredObjectives, visibleHoshimies, visibleAZN);
            addObjectivesToVisit(filteredObjectives);
            // get Desires
            Desire desire = getDesire();
            // get Intentions
            Point intention = getIntention(desire);
            // do a plan
            // the plan is move the position returned from the intention

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
            if (visitedPositions.Contains(exploringPoint)) {
                // someone else explored this point or already been there
                getAASMAFramework().logData(this, "Aborting action");
                this.StopMoving();
            }
            switch (desire) {
                case Desire.EXPLORE:
                    exploringPoint = Utils.randomPoint(pointsToVisit);
                    pointsToVisit.Remove(exploringPoint);
                    return exploringPoint;
                case Desire.MOVE_AROUND:
                default:
                    return exploringPoint = Utils.randomValidPoint(this.getAASMAFramework().Tissue);
            }
        }

        private void addObjectivesToVisit(List<Point> visibleObjectives) {
            foreach (Point p in visibleObjectives) {
                if (!pointsToVisit.Contains(p) && !visitedPositions.Contains(p)) {
                    pointsToVisit.Add(p);
                }
            }
        }

        private List<Point> filterObjectives(List<Point> visibleObjectives) {
            List<Point> pointsFiltered = new List<Point>();
            foreach (Point p in visibleObjectives) {
                if(!visitedPositions.Contains(p)){
                    pointsFiltered.Add(p);
                }
            }
            return pointsFiltered;
        }

        private Point flee(List<Point> enemies) {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies) {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            return Utils.getMiddlePoint(possibleMoves.ToArray());
        }

        private void sendVisiblePointsToOthers(List<Point> nearPierres, List<Point> visibleObjectives, List<Point> visibleHoshimies, List<Point> visibleAZN) {
            if (nearPierres.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "Pierre;" + Utils.serializePoints(nearPierres));
                getAASMAFramework().broadCastMessage(message); // or iterate each protector
            }

            if (visibleObjectives.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "New objective;" + Utils.serializePoints(visibleObjectives));
                getAASMAFramework().broadCastMessage(message); // or iterate each explorer
            }
            if (visibleHoshimies.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "Hoshimi;" + Utils.serializePoints(visibleHoshimies));
                getAASMAFramework().sendMessage(message, "AI");
            }
            if (visibleAZN.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "AZN;" + Utils.serializePoints(visibleAZN));
                getAASMAFramework().broadCastMessage(message); // or iterate each Container
            }
        }

        public override void receiveMessage(AASMAMessage msg) {
            string[] content = msg.Content.Split(';');
            if (content.Length == 2 && content[0].Equals("New objective")) {
                addObjectivesToVisit(Utils.deserializePoints(content[1]));
            }
            else if (content.Length == 3 && content[0].Equals("Visited objective")) {
                getAASMAFramework().logData(this, "Recived visited position");
                Point p = new Point(int.Parse(content[1]), int.Parse(content[2]));
                pointsToVisit.Remove(p);
                visitedPositions.Add(p);
            }
            // ignore other messages
        }

        private void broadcastVisitedPoint(Point p) {
            AASMAMessage message = new AASMAMessage(this.InternalName, "Visited objective;" + p.X + ";" + p.Y);
            getAASMAFramework().broadCastMessage(message); // or iterate each explorer
        }
    }

    enum Desire {
        MOVE_AROUND,
        EXPLORE
    }
}
