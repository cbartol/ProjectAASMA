﻿using System;
using System.Collections.Generic;
using System.Text;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi.Deliberative
{
    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 28)]
    class DeliberativeProtectorAI : AASMAProtector
    {
        private enum Intention
        {
            DEFEND, MOVE, FOLLOW
        }

        private Point lastAIPosition = Point.Empty;
        private List<Point> aznPoints = new List<Point>();
        private List<Point> needles = new List<Point>();
        private List<Action> plan = new List<Action>();
        private Action currentAction;
        private Intention lastIntention;

        //Deliberates and return the choosen intention
        private Intention Deliberate()
        {
            List<Point> enemies = getAASMAFramework().visiblePierres(this);
            if (enemies.Count > 0)
                if (Utils.SquareDistance(this.Location, Utils.getNearestPoint(this.Location, enemies)) <=
                    this.DefenseDistance * this.DefenseDistance)
                    return Intention.DEFEND;
            if (isAInear()) {
                return Intention.FOLLOW;
            }
            return Intention.MOVE;
        }

        //Plan a set of actions
        private void Plan(Intention intention)
        {
            switch (intention)
            {
                case Intention.DEFEND:
                    plan.Add(new DefendAction(this, Utils.getNearestPoint(this.Location, getAASMAFramework().visiblePierres(this)), 10));
                    break;
                case Intention.FOLLOW:
                    plan.Add(new MoveAction(this, lastAIPosition));
                    break;
                case Intention.MOVE:
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
                    break;
            }
            lastIntention = intention;
        }

        //Reconsider the current plan
        public bool Reconsider()
        {
            List<Point> enemies = getAASMAFramework().visiblePierres(this);
            if (enemies.Count > 0)
                if (Utils.SquareDistance(this.Location, Utils.getNearestPoint(this.Location, enemies)) <=
                    this.DefenseDistance * this.DefenseDistance)
                {
                    currentAction.cancel();
                    plan.Clear();
                    Plan(Intention.DEFEND);
                    return true;
                }
            if (lastIntention != Intention.FOLLOW && isAInear()) {
                currentAction.cancel();
                plan.Clear();
                Plan(Intention.FOLLOW);
                return true;
            }
            return false;
        }

        public override void DoActions()
        {
            //Update Beliefs with Perceptions
            foreach (Point p in getAASMAFramework().visibleAznPoints(this))
            {
                if (!aznPoints.Contains(p))
                    aznPoints.Add(p);
            }
            foreach (Point p in getAASMAFramework().visibleEmptyNeedles(this))
            {
                if (!needles.Contains(p))
                    needles.Add(p);
            }
            foreach (Point p in getAASMAFramework().visibleFullNeedles(this))
            {
                if (!needles.Contains(p))
                    needles.Add(p);
            }

            //When there isn't a plan, plan one
            if (plan.Count == 0)
            {
                Intention intention = Deliberate();
                Plan(intention);
            }

            if (this.State == NanoBotState.WaitingOrders)
            {
                currentAction = plan[0];
                currentAction.execute();
                plan.Remove(currentAction);
            }

            if (Reconsider())
            {
                currentAction = plan[0];
                currentAction.execute();
                plan.Remove(currentAction);
            }
        }

        private bool isAInear() {
            Point aiPosition = getAASMAFramework().AI.Location;
            if (Utils.SquareDistance(this.Location, aiPosition) <= this.Scan) {
                this.lastAIPosition = aiPosition;
                return true;
            }
            return false;
        }

        public override void receiveMessage(AASMAMessage msg)
        {
            getAASMAFramework().logData(this, "received message from " + msg.Sender + " : " + msg.Content);
        }
    }
}
