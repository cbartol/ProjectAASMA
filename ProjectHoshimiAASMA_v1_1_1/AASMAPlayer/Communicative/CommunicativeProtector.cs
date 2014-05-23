using System;
using System.Collections.Generic;
using System.Text;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi.Communicative
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    class CommunicativeProtector : AASMAProtector
    {
        private enum Intention
        {
            MOVE, DISTANT_DEFENCE
        }

        private List<Point> enemies = new List<Point>();
        private List<Point> otherEnemies = new List<Point>();
        private List<Point> aznPoints = new List<Point>();
        private List<Point> needles = new List<Point>();
        private List<Action> plan = new List<Action>();
        private Action currentAction;
        private Intention currentIntention;

        //Deliberates and return the choosen intention
        private Intention Deliberate()
        {
            if(otherEnemies.Count > 0){
                return Intention.DISTANT_DEFENCE;
            }
            return Intention.MOVE;
        }

        //Plan a set of actions
        private void Plan(Intention intention)
        {
            if(intention == Intention.DISTANT_DEFENCE){
                Point enemy = Utils.getNearestPoint(this.Location, otherEnemies);
                otherEnemies.Remove(enemy);
                if (enemy != Point.Empty) {
                    plan.Add(new MoveAction(this, enemy));
                    return;
                }
                intention = Intention.MOVE;
            }
            // Intention.MOVE
            Point point;
            int random = Utils.randomValue(100);
            if (random < 20)
                point = getAASMAFramework().InjectionPoint;
            else if (random < 60)
                point = Utils.randomValidPoint(getAASMAFramework().Tissue);
            else if (random < 80)
                point = Utils.randomPoint(aznPoints, getAASMAFramework().Tissue);
            else
                point = Utils.randomPoint(needles, getAASMAFramework().Tissue);
            plan.Add(new MoveAction(this, point));
        }

        //Reconsider the current plan
        public bool Reconsider()
        {
            if (currentIntention != Intention.DISTANT_DEFENCE && otherEnemies.Count > 0) {
                currentAction.cancel();
                plan.Clear();
                Plan(currentIntention = Intention.DISTANT_DEFENCE);
                return true;
            }
            return false;
        }

        public override void DoActions()
        {
            try {
                //Update Beliefs with Perceptions
                foreach (Point p in getAASMAFramework().visibleAznPoints(this)) {
                    if (!aznPoints.Contains(p))
                        aznPoints.Add(p);
                }
                foreach (Point p in getAASMAFramework().visibleEmptyNeedles(this)) {
                    if (!needles.Contains(p))
                        needles.Add(p);
                }
                foreach (Point p in getAASMAFramework().visibleFullNeedles(this)) {
                    if (!needles.Contains(p))
                        needles.Add(p);
                }

                //Reactive to enimies - defend
                enemies = getAASMAFramework().visiblePierres(this);
                if (enemies.Count > 0) {
                    if (Utils.SquareDistance(this.Location, Utils.getNearestPoint(this.Location, enemies)) <=
                        this.DefenseDistance * this.DefenseDistance) {
                        plan = new List<Action>();
                        this.StopMoving();
                        this.DefendTo(Utils.getNearestPoint(this.Location, enemies), 10);
                        return;
                    }
                }

                //When there isn't a plan, plan one
                if (plan.Count == 0) {
                    Intention intention = currentIntention = Deliberate();
                    Plan(intention);
                }

                if (this.State == NanoBotState.WaitingOrders) {
                    currentAction = plan[0];
                    currentAction.execute();
                    plan.Remove(currentAction);
                }

                if (Reconsider()) {
                    currentAction = plan[0];
                    currentAction.execute();
                    plan.Remove(currentAction);
                }
            } catch (Exception e) {
                getAASMAFramework().logData(this, e.Message);
                getAASMAFramework().logData(this, e.StackTrace);
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            // they just wall around
        }
    }
}
