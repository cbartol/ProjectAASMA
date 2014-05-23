using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;

namespace AASMAHoshimi.Communicative
{
    [Characteristics(ContainerCapacity = 20, CollectTransfertSpeed = 5, Scan = 0, MaxDamage = 0, DefenseDistance = 0, Constitution = 20)]
    class CommunicativeContainer : AASMAContainer
    {
        private enum Intention
        {
            COLLECT, TRANSFER, MOVE
        }

        private List<Point> enemies = new List<Point>();
        private List<Point> aznPoints = new List<Point>();
        private List<Point> availableNeedles = new List<Point>();
        private List<Point> hoshimiPoints = new List<Point>();
        private List<Action> plan = new List<Action>();
        private Action currentAction;
        private Intention currentIntetion;

        private void removeNeedleFromList(Point position)
        {
            availableNeedles.Remove(position);
        }

        private Point flee(List<Point> enemies)
        {
            List<Point> possibleMoves = new List<Point>();
            foreach (Point enemy in enemies)
            {
                possibleMoves.Add(Utils.oppositDirection(this.Location, enemy, getAASMAFramework().Tissue));
            }
            return Utils.getMiddlePoint(possibleMoves.ToArray());
        }

        //Deliberates and return the choosen intention
        private Intention Deliberate()
        {
            if (aznPoints.Count == 0 || availableNeedles.Count == 0)
                return Intention.MOVE;
            if (Stock == 0)
                return Intention.COLLECT;
            return Intention.TRANSFER;
        }

        //Plan a set of actions
        private void Plan(Intention intention)
        {
            switch (intention) 
            {
                case Intention.MOVE:
                    Point point;
                    if (hoshimiPoints.Count > 0 && Utils.randomValue(100) > 80)
                        point = Utils.randomPoint(hoshimiPoints, getAASMAFramework().Tissue);
                    else
                        point = Utils.randomValidPoint(getAASMAFramework().Tissue);
                    plan.Add(new MoveAction(this, point));
                    break;

                case Intention.COLLECT:
                    plan.Add(new MoveAction(this, Utils.getNearestPoint(this.Location, aznPoints)));
                    for(int i=0; i<=(ContainerCapacity/CollectTransfertSpeed); i++)
                        plan.Add(new CollectAction(this));
                    break;

                case Intention.TRANSFER:
                    plan.Add(new MoveAction(this, Utils.getNearestPoint(this.Location, availableNeedles)));
                    for (int i = 0; i <= (ContainerCapacity/CollectTransfertSpeed); i++)
                        plan.Add(new TransferAction(this, removeNeedleFromList));
                    break;
            }
        }

        //Reconsider the current plan
        public bool Reconsider()
        {
            if (enemies.Count > 0)
            {
                currentAction.cancel();
                plan.Clear();
                return false;
            }
            if (getAASMAFramework().visibleAznPoints(this).Count > 0 && Stock == 0)
            {
                currentAction.cancel();
                plan.Clear();
                Plan(currentIntetion = Intention.COLLECT);
                return true;
            }
            if (getAASMAFramework().visibleEmptyNeedles(this).Count > 0 && Stock > 0)
            {
                currentAction.cancel();
                plan.Clear();
                Plan(currentIntetion = Intention.TRANSFER);
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
                    if (!availableNeedles.Contains(p))
                        availableNeedles.Add(p);
                }
                foreach (Point p in getAASMAFramework().visibleFullNeedles(this)) {
                    if (availableNeedles.Contains(p))
                        availableNeedles.Remove(p);
                }
                foreach (Point p in getAASMAFramework().visibleHoshimies(this)) {
                    if (!hoshimiPoints.Contains(p))
                        hoshimiPoints.Add(p);
                }

                //Reactive to enimies - flee
                enemies = getAASMAFramework().visiblePierres(this);
                if (enemies.Count > 0) {
                    plan = new List<Action>();
                    this.StopMoving();
                    this.MoveTo(flee(enemies));
                    return;
                }

                //When there isn't a plan, plan one
                if (plan.Count == 0) {
                    Intention intention = currentIntetion = Deliberate();
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
            try{
            string[] content = msg.Content.Split(';');
            if (content.Length == 2 && content[0].Equals("AZN")) {
                getAASMAFramework().logData(this, "Received " + msg.Content);
                foreach (Point p in Utils.deserializePoints(content[1])) {
                    if (!aznPoints.Contains(p))
                        aznPoints.Add(p);
                }
            }
            } catch (Exception e) {
                getAASMAFramework().logData(this, e.Message);
                getAASMAFramework().logData(this, e.StackTrace);
            }
        }
    }
}
