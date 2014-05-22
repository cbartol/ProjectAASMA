using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Mission;

namespace AASMAHoshimi.Deliberative {
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class DeliberativeExplorer : AASMAExplorer {
        // perceptions
        private List<Point> nearPierres = new List<Point>();
        // end perceptions

        private List<Action> plan = new List<Action>();
        private MyIntention intention;

        private Dictionary<Point, Boolean> visitedPositions = new Dictionary<Point,Boolean>();
        private Dictionary<Point, Boolean> pointsToVisit = new Dictionary<Point,Boolean>();

        private Point exploringPoint = Point.Empty;
        public DeliberativeExplorer() {

        }

        public override void DoActions() {
            // get perceptions
            updatePerceptions();

            if (plan.Count == 0) {
                // get desires
                MyIntention[] desires = Options();

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
                if (Reconsider()) {
                    action.cancel();

                    // get desires
                    MyIntention[] desires = Options();

                    // get intention based on the desires and the previous intention
                    this.intention = Filter(desires, this.intention);

                    this.plan = Plan(this.intention);
                }
            }
        }

        private void addObjectivesToVisit(List<Point> visibleObjectives){
            foreach(Point p in visibleObjectives){
                if (!pointsToVisit.ContainsKey(p) && !visitedPositions.ContainsKey(p)) {
                    pointsToVisit.Add(p,true);
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


        // from here all new
        private void updatePerceptions() {
            addObjectivesToVisit(getAASMAFramework().visibleNavigationPoints(this));
            nearPierres = getAASMAFramework().visiblePierres(this);
        }
        private static MyIntention[] Options() {
            return (MyIntention[])Enum.GetValues(typeof(MyIntention));
        }

        private MyIntention Filter(MyIntention[] desires, MyIntention prevIntention) {
            if (nearPierres.Count > 0) {
                return MyIntention.FLEE;
            }

            if (pointsToVisit.Count > 0) {
                return MyIntention.EXPLORE;
            }
            return MyIntention.MOVE_AROUND;
        }

        private List<Action> Plan(MyIntention intention) {
            List<Action> plan = new List<Action>();
            Point target;
            switch (intention) {
                case MyIntention.EXPLORE:
                    List<Point> possibilities = new List<Point>();
                    foreach(KeyValuePair<Point, Boolean> point in pointsToVisit){
                        possibilities.Add(point.Key);
                    }
                    target = Utils.randomPoint(possibilities);
                    plan.Add(new MoveAction(this, target));
                    plan.Add(new VisitObjective(visitPoint, target));
                    break;
                case MyIntention.FLEE:
                    target = flee(nearPierres);
                    plan.Add(new MoveAction(this, target));
                    break;
                case MyIntention.MOVE_AROUND:
                    plan.Add(new MoveAction(this, Utils.randomValidPoint(this.getAASMAFramework().Tissue)));
                    break;
            }
            return plan;
        }

        private void visitPoint(Point p) {
            pointsToVisit.Remove(p);
            visitedPositions.Add(p, true);
        }

        private bool Reconsider() {
            if(nearPierres.Count > 0){
                return true;
            }
            if (intention == MyIntention.MOVE_AROUND && pointsToVisit.Count > 0) {
                return true;
            }
            return false;
        }

        private enum MyIntention {
            MOVE_AROUND,
            EXPLORE,
            FLEE
        }
    }
}
