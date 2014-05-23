using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AASMAHoshimi
{
    class TransferAction : Action
    {
        private AASMAContainer ownerContainer;
        public delegate void removeNeedleFromList(Point position);
        private removeNeedleFromList callback;

        public TransferAction(AASMAContainer container, removeNeedleFromList callback)
        {
            this.ownerContainer = container;
            this.callback = callback;
        }

        public void execute()
        {
            if (!this.ownerContainer.transferAZN())
            {
                callback(ownerContainer.Location);
            }
        }

        public void cancel() {}
    }
}
