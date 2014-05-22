using System;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi
{
	public class MoveAction : Action
	{
		private NanoMoveable owner;
		private Point target;

		public MoveAction (NanoMoveable owner, Point target)
		{
			this.owner = owner;
			this.target = target;
		}

		public void execute() 
		{
			this.owner.MoveTo (target);
		}

		public void cancel() 
		{
			this.owner.StopMoving ();
		}
	}
}

