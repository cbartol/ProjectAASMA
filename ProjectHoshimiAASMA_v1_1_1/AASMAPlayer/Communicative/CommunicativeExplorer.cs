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
            if (nearPierres.Count > 0) {
                this.StopMoving();
                this.MoveTo(flee(nearPierres));
                return;
            }

            sendVisiblePointsToOthers(nearPierres, visibleObjectives, visibleHoshimies, visibleAZN);
            addObjectivesToVisit(visibleObjectives);
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
            switch (desire) {
                case Desire.EXPLORE:
                    if (visitedPositions.Contains(exploringPoint)) {
                        // someone else explored this point
                        this.StopMoving();
                    }
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
                addObjectiveToVisit(p);
            }
        }

        private void addObjectiveToVisit(Point point) {
            if (!pointsToVisit.Contains(point) && !visitedPositions.Contains(point)) {
                pointsToVisit.Add(point);
            }
        }

        private Point flee(List<Point> enemies) {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies) {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            return Utils.getMiddlePoint(possibleMoves.ToArray());
        }

        private void sendVisiblePointsToOthers(List<Point> nearPierres, List<Point> visibleObjectives, List<Point> visibleHoshimies, List<Point> visibleAZN) {
            foreach(Point p in nearPierres){
                AASMAMessage message = new AASMAMessage(this.InternalName, "Pierre;" + p.X + ";" + p.Y);
                getAASMAFramework().broadCastMessage(message); // or iterate each protector
            }
            foreach (Point p in visibleObjectives) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "New objective;" + p.X + ";" + p.Y);
                getAASMAFramework().broadCastMessage(message); // or iterate each explorer
            }
            foreach (Point p in visibleHoshimies) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "Hoshimi;" + p.X + ";" + p.Y);
                getAASMAFramework().sendMessage(message, "AI");
            }
            foreach (Point p in visibleAZN) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "AZN;" + p.X + ";" + p.Y);
                getAASMAFramework().broadCastMessage(message); // or iterate each Container
            }
        }

        public override void receiveMessage(AASMAMessage msg) {
            string[] content = msg.Content.Split(';');
            if (content.Length == 3 && content[0].Equals("New objective")) {
                addObjectiveToVisit(new Point(int.Parse(content[1]), int.Parse(content[2])));
            }
            else if (content.Length == 3 && content[0].Equals("Visited objective")) {
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
