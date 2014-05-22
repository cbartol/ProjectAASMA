using System;
using System.Collections.Generic;
using System.Text;

namespace AASMAHoshimi
{
    class TransferAction : Action
    {
        private AASMAContainer ownerContainer;

        public TransferAction(AASMAContainer container)
        {
            this.ownerContainer = container;
        }

        public void execute()
        {
            this.ownerContainer.transferAZN();
        }

        public void cancel() {}
    }
}
