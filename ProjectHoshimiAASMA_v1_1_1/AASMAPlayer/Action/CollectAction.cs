using System;
using System.Collections.Generic;
using System.Text;

namespace AASMAHoshimi
{
    class CollectAction : Action
    {
        private AASMAContainer ownerContainer;

        public CollectAction(AASMAContainer container)
        {
            this.ownerContainer = container;
        }

        public void execute()
        {
            this.ownerContainer.collectAZN();
        }

        public void cancel() {}
    }
}
