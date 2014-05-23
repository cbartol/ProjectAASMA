using System;
using System.Collections.Generic;
using System.Text;
using PH.Common;
using PH.Map;

namespace AASMAHoshimi.Communicative
{
	[Characteristics(ContainerCapacity = 100, CollectTransfertSpeed = 0, Scan = 10, MaxDamage = 5, DefenseDistance = 10, Constitution = 25)]
    class CommunicativeNeedle : AASMANeedle
	{
        // this is used, only one time, to send a message saying that the needle is full
        private bool haveToInform = true;

		public override void DoActions() {
			List<System.Drawing.Point> visiblePierresList = getAASMAFramework().visiblePierres(this);
			if (visiblePierresList.Count != 0)
			{
				this.DefendTo(visiblePierresList[0], 2);
			}
            if (full() && haveToInform) {
                haveToInform = false;
                AASMAMessage message = new AASMAMessage(this.InternalName, "Needle Full;" + this.Location.X + ";" + this.Location.Y);
                getAASMAFramework().broadCastMessage(message);
            }
		}

        private bool full() {
            return this.Stock == this.ContainerCapacity;
        }

        public override void receiveMessage(AASMAMessage msg) {
            string[] content = msg.Content.Split();
            if(content.Length == 1 && content[0].Equals("Looking for needle") && !full()){
                // reply my position
                AASMAMessage newMessage = new AASMAMessage(this.InternalName, "Needle;"+ this.Location.X + ";" + this.Location.Y);
                getAASMAFramework().sendMessage(newMessage, msg.Sender);
            }
        }
	}
}
