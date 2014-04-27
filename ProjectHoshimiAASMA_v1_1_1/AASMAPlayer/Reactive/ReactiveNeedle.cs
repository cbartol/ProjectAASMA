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

        public override void DoActions() {}
    }
}
