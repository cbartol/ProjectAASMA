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

        public override void DoActions()
        {
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
        }

        public override void receiveMessage(AASMAMessage msg)
        {
        }
    }
}
