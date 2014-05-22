using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;

namespace AASMAHoshimi.Deliberative
{
    [Characteristics(ContainerCapacity = 20, CollectTransfertSpeed = 5, Scan = 0, MaxDamage = 0, DefenseDistance = 0, Constitution = 20)]
    class DeliberativeContainer : AASMAContainer
    {
        private List<Point> aznPoints = new List<Point>();
        private List<Point> discoveredNeedles = new List<Point>();
        private List<Point> fullNeedles = new List<Point>();
        private List<Point> hoshimiPoints = new List<Point>();
        private List<Intention> planList = new List<Intention>();

        enum Intention
        {
            COLLECT, TRANSFER, MOVE, FLEE
        }

        private void PlanIntentions()
        {
            if (Stock == 0)
            {
                for (int i = 0; i <= ContainerCapacity; i++)
                    planList.Add(Intention.COLLECT);
            }
        }


        public override void DoActions()
        {
            //Update Beliefs with Perceptions
            foreach(Point p in getAASMAFramework().visibleAznPoints(this))
            {
                if(!aznPoints.Contains(p))
                    aznPoints.Add(p);
            }
            foreach(Point p in getAASMAFramework().visibleEmptyNeedles(this))
            {
                if(!discoveredNeedles.Contains(p))
                    discoveredNeedles.Add(p);
            }
            foreach(Point p in getAASMAFramework().visibleFullNeedles(this))
            {
                if(!fullNeedles.Contains(p))
                    fullNeedles.Add(p);
            }
            foreach (Point p in getAASMAFramework().visibleHoshimies(this))
            {
                if (!hoshimiPoints.Contains(p))
                    hoshimiPoints.Add(p);
            }

            //When there isn't a plan, plan one
            if (planList.Count == 0)
            {
               
            }


        }
    }
}
