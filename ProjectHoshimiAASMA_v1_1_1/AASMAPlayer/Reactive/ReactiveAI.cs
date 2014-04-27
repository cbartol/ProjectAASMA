using System;
using System.Collections.Generic;
using System.Text;
using AASMAHoshimi;
using PH.Common;

namespace AASMAHoshimi.Reactive
{
    public class ReactiveAI : AASMAAI
    {
        public ReactiveAI(NanoAI nano)
        {
            this._nanoAI = nano;
        }

        public void MoveToClearPosition()
        {
            /*
            int x, y;
            do 
            {
            int angle = Utils.randomValue(360);
            x = 5*(int)Math.Cos(Math.PI*angle/180);
            y = 5*(int)Math.Sin(Math.PI*angle/180);
            }
            while(!getAASMAFramework().isMovablePoint(new System.Drawing.Point(x, y)));
            this._nanoAI.MoveTo(new System.Drawing.Point(x, y));
            */
            if (frontClear())
            {
                if(Utils.randomValue(100) < 90)
                {
                    this.MoveForward();
                }
                else 
                {
                    this.RandomTurn();
                }
            }
            else
            {
                this.RandomTurn();
            }
        }


        public override void DoActions()
        {
            /*
            if (getAASMAFramework().visiblePierres(this._nanoAI).Count != 0)
            {
                System.Drawing.Point nearestPierre = Utils.getNearestPoint(this._nanoAI.Location, getAASMAFramework().visiblePierres(this._nanoAI));
                TurnToOppositeDirection(nearestPierre);
                MoveToClearPosition();
            }
            */
            if (getAASMAFramework().protectorsAlive() < 5)
            {
                this._nanoAI.Build(typeof(ReactiveProtector),"P"+this._protectorNumber++);
            }
            else if (getAASMAFramework().containersAlive() < 5)
            {
                this._nanoAI.Build(typeof(ReactiveCollector), "C" + this._containerNumber++);
            }
            else if(getAASMAFramework().explorersAlive() < 5)
            {
                this._nanoAI.Build(typeof(ReactiveExplorer),"E"+this._explorerNumber++);
            }
            else if(!getAASMAFramework().overHoshimiPoint(this._nanoAI) || getAASMAFramework().overNeedle(this._nanoAI)) 
            {
                List<System.Drawing.Point> visibleHoshimiesList = getAASMAFramework().visibleHoshimies(this._nanoAI);
                if(visibleHoshimiesList.Count != 0) {
                    System.Drawing.Point nearestHoshimi = Utils.getNearestPoint(this._nanoAI.Location, visibleHoshimiesList);
                    if (!getAASMAFramework().visibleEmptyNeedles(this._nanoAI).Contains(nearestHoshimi) &&
                        !getAASMAFramework().visibleFullNeedles(this._nanoAI).Contains(nearestHoshimi))
                    {
                        this._nanoAI.MoveTo(nearestHoshimi);
                        return;
                    }
                }
                /*if (frontClear()) {
                    this.MoveForward();
                } else {
                    this.RandomTurn();
                }*/
                MoveToClearPosition();
            }
            else if (getAASMAFramework().overHoshimiPoint(this._nanoAI) && (!getAASMAFramework().overNeedle(this._nanoAI)))
            {
                this._nanoAI.Build(typeof(ReactiveNeedle), "N" + this._needleNumber++);
            }
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
