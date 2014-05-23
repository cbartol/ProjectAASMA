using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Communicative {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class CommunicativeExplorer : AASMAExplorer {
        // perceptions
        private List<Point> nearPierres = new List<Point>();
        private List<Point> visibleHoshimies = new List<Point>();
        private List<Point> visibleAZN = new List<Point>();
        private List<Point> pointsToSend = new List<Point>();
        // end perceptions
        private Dictionary<Point, Boolean> sentPoints = new Dictionary<Point, bool>();
        
        private List<Action> plan = new List<Action>();
        private Intention intention;
        private bool firstTime = true;

        private Dictionary<Point, Boolean> visitedPositions = new Dictionary<Point, Boolean>();
        private Dictionary<Point, Boolean> pointsToVisit = new Dictionary<Point, Boolean>();

        private Point exploringPoint = Point.Empty;

        public override void DoActions() {
            try { 
            if (firstTime) {
                firstTime = false;
                AASMAMessage message = new AASMAMessage(this.InternalName, "Give me knowledge");
                this.getAASMAFramework().sendMessage(message, "AI");
                return;
            }
            // get perceptions
            updatePerceptions();

            if (nearPierres.Count > 0) {
                plan = new List<Action>();
                this.StopMoving();
                this.MoveTo(flee(nearPierres));
                return;
            }
            sendVisiblePointsToOthers();

            if (plan.Count == 0) {
                // get desires
                Intention[] desires = Options();

                // get intention based on the desires and the previous intention
                this.intention = Filter(desires, this.intention);

                this.plan = Plan(this.intention);
            }

            if (plan.Count > 0) {
                // Continue with the same plan
                Action action = this.plan[0];
                if (this.State == NanoBotState.WaitingOrders) {
                    action.execute();
                    this.plan.RemoveAt(0);
                }

                updatePerceptions();
                sendVisiblePointsToOthers();
                if (Reconsider()) {
                    action.cancel();

                    // get desires
                    Intention[] desires = Options();

                    // get intention based on the desires and the previous intention
                    this.intention = Filter(desires, this.intention);

                    this.plan = Plan(this.intention);
                }
            }
            } catch (Exception e) {
                getAASMAFramework().logData(this, e.Message);
                getAASMAFramework().logData(this, e.StackTrace);
            }
        }

        private List<Point> addObjectivesToVisit(List<Point> visibleObjectives) {
            List<Point> newPoints = new List<Point>();
            foreach (Point p in visibleObjectives) {
                if (!pointsToVisit.ContainsKey(p) && !visitedPositions.ContainsKey(p)) {
                    pointsToVisit[p] = true;
                    newPoints.Add(p);
                }
            }
            return newPoints;
        }

        public override void receiveMessage(AASMAMessage msg) {
            try {
                string[] content = msg.Content.Split(';');
                if (content.Length == 2 && content[0].Equals("New objective")) {
                    addObjectivesToVisit(Utils.deserializePoints(content[1]));
                } else if (content.Length == 3 && content[0].Equals("Visited objective")) {
                    getAASMAFramework().logData(this, "Received visited position: " + msg.Content);
                    Point p = new Point(int.Parse(content[1]), int.Parse(content[2]));
                    if (pointsToVisit.ContainsKey(p)) {
                        pointsToVisit.Remove(p);
                    }
                    visitedPositions[p] = true;
                } else if (content.Length == 2 && content[0].Equals("Objectives knowledge")) {
                    foreach (Point p in Utils.deserializePoints(content[1])) {
                        visitPoint(p);
                    }
                }
            } catch (Exception e) {
                getAASMAFramework().logData(this, e.Message);
                getAASMAFramework().logData(this, e.StackTrace);
            }
        }

        private Point flee(List<Point> enemies) {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies) {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            return Utils.getMiddlePoint(possibleMoves.ToArray());
        }


        // from here all new
        private void updatePerceptions() {
            pointsToSend.AddRange(addObjectivesToVisit(getAASMAFramework().visibleNavigationPoints(this)));
            nearPierres = getAASMAFramework().visiblePierres(this);
            visibleHoshimies = getAASMAFramework().visibleHoshimies(this);
            visibleAZN = getAASMAFramework().visibleAznPoints(this);
        }
        private static Intention[] Options() {
            return (Intention[])Enum.GetValues(typeof(Intention));
        }

        private Intention Filter(Intention[] desires, Intention prevIntention) {
            if (pointsToVisit.Count > 0) {
                return Intention.EXPLORE;
            }
            return Intention.MOVE_AROUND;
        }

        private List<Action> Plan(Intention intention) {
            List<Action> plan = new List<Action>();
            Point target;
            switch (intention) {
                case Intention.EXPLORE:
                    List<Point> possibilities = new List<Point>();
                    foreach (KeyValuePair<Point, Boolean> point in pointsToVisit) {
                        possibilities.Add(point.Key);
                    }
                    target = exploringPoint = Utils.randomPoint(possibilities, getAASMAFramework().Tissue);
                    plan.Add(new MoveAction(this, target));
                    plan.Add(new VisitObjective(visitPoint, target));
                    break;
                case Intention.MOVE_AROUND:
                    plan.Add(new MoveAction(this, Utils.randomValidPoint(this.getAASMAFramework().Tissue)));
                    break;
            }
            return plan;
        }

        private void visitPoint(Point p) {
            if (pointsToVisit.ContainsKey(p)) {
                pointsToVisit.Remove(p);
            }
            visitedPositions[p] = true;
            broadcastVisitedPoint(p);
        }

        private bool Reconsider() {
            if (intention == Intention.EXPLORE && visitedPositions.ContainsKey(exploringPoint)) {
                return true;
            }
            if (intention == Intention.MOVE_AROUND && pointsToVisit.Count > 0) {
                return true;
            }
            return false;
        }

        private void sendVisiblePointsToOthers() {

            List<Point> points = filterSentPoints(pointsToSend);
            if (points.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "New objective;" + Utils.serializePoints(points));
                getAASMAFramework().broadCastMessage(message); // or iterate each explorer
                pointsToSend.Clear();
            }

            points = filterSentPoints(visibleHoshimies);
            if (points.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "Hoshimi;" + Utils.serializePoints(points));
                getAASMAFramework().sendMessage(message, "AI");
            }

            points = filterSentPoints(visibleAZN);
            if (points.Count > 0) {
                AASMAMessage message = new AASMAMessage(this.InternalName, "AZN;" + Utils.serializePoints(points));
                getAASMAFramework().broadCastMessage(message); // or iterate each Container
            }
        }

        private void broadcastVisitedPoint(Point p) {
            AASMAMessage message = new AASMAMessage(this.InternalName, "Visited objective;" + p.X + ";" + p.Y);
            getAASMAFramework().broadCastMessage(message); // or iterate each explorer
        }

        private List<Point> filterSentPoints(List<Point> points) {
            List<Point> filtered = new List<Point>();
            foreach(Point p in points){
                if (!sentPoints.ContainsKey(p)) {
                    filtered.Add(p);
                    sentPoints[p] = true;
                }
            }
            return filtered;
        }

        private enum Intention {
            MOVE_AROUND,
            EXPLORE
        }
    }
}
