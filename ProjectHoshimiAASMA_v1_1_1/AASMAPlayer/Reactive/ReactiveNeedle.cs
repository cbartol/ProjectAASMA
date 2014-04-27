using System;
using System.Collections.Generic;
using System.Text;
using PH.Common;
using PH.Map;

namespace AASMAHoshimi.Reactive
{
     [Characteristics(ContainerCapacity = 100, CollectTransfertSpeed = 0, Scan = 10, MaxDamage = 5, DefenseDistance = 10, Constitution = 25)]
    class ReactiveNeedle : AASMANeedle
    {
        public override void receiveMessage(AASMAMessage msg) {}

        public override void DoActions() {
            List<System.Drawing.Point> visiblePierresList = getAASMAFramework().visiblePierres(this);
            if (visiblePierresList.Count != 0)
            {
                this.DefendTo(visiblePierresList[0], 2);
            }
        }
    }
}
