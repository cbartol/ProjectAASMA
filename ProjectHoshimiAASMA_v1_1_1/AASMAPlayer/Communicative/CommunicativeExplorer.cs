using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Communicative {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class CommunicativeExplorer : AASMAExplorer {
        //We have to use a dictionary because HashSet is not available for .NET 2.0
        private Dictionary<Point, Boolean> visitedPositions = new Dictionary<Point, Boolean>();
        private Dictionary<Point, Boolean> pointsToVisit = new Dictionary<Point, Boolean>();

        private Point exploringPoint = new Point(-1,-1);
        private bool isMoving = false;

        public CommunicativeExplorer() {
            visitedPositions.Add(exploringPoint, true); // mark null Point as explored
        }

        public override void DoActions() {
            if (exploringPoint == this.Location) {
                visitedPositions.Add(exploringPoint,true);
//                broadcastVisitedPoint(exploringPoint);
                isMoving = false;
            }
            if (isMoving) {
                this.MoveTo(exploringPoint);
                return;
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
//            sendVisiblePointsToOthers(nearPierres, filteredObjectives, visibleHoshimies, visibleAZN);
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
            if (pointsToVisit.Keys.Count > 0) {
                return Desire.EXPLORE;
            }
            return Desire.MOVE_AROUND;
        }

        private Point getIntention(Desire desire) {
            if (visitedPositions.ContainsKey(exploringPoint)) {
                // someone else explored this point or already been there
                getAASMAFramework().logData(this, "Aborting action");
                this.StopMoving();
                isMoving = true;
                switch (desire) {
                    case Desire.EXPLORE:
                        List<Point> toExplore = new List<Point>();
                        foreach (KeyValuePair<Point, Boolean> p in pointsToVisit) {
                            toExplore.Add(p.Key);
                        }
                        // the points to explore should not be empty. If that happens it's concurrency problems.
                        exploringPoint = Utils.randomPoint(toExplore);
                        pointsToVisit.Remove(exploringPoint);
                        return exploringPoint;
                    case Desire.MOVE_AROUND:
                    default:
                        return exploringPoint = Utils.randomValidPoint(this.getAASMAFramework().Tissue);
                }
            }
            return exploringPoint;
        }

        private void addObjectivesToVisit(List<Point> visibleObjectives) {
            foreach (Point p in visibleObjectives) {
                if (!pointsToVisit.ContainsKey(p) && !visitedPositions.ContainsKey(p)) {
                    pointsToVisit.Add(p, true);
                }
            }
        }

        private List<Point> filterObjectives(List<Point> visibleObjectives) {
            List<Point> pointsFiltered = new List<Point>();
            foreach (Point p in visibleObjectives) {
                if(!visitedPositions.ContainsKey(p)){
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
                visitedPositions.Add(p,true);
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
