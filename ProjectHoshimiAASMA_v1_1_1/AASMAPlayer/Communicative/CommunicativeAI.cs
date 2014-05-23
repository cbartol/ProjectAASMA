using System;
using System.Collections.Generic;
using System.Text;
using AASMAHoshimi;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi.Communicative
{
	public class CommunicativeAI : AASMAAI
	{
        private List<Point> visitedObjectives = new List<Point>(); // send this for new Explorers

		private List<Point> viewedHoshimies;
		private List<Point> createdNeedles;
		private List<Point> viewedEnemies;
		private List<Action> plan;
		private Intention intention;
        private Point currentTarget;
		private bool canReconsider;
		private Action currentAction;

        public CommunicativeAI(NanoAI nano)
		{
			this._nanoAI = nano;
			this.viewedHoshimies = new List<Point>();
			this.createdNeedles = new List<Point>();
			this.plan = new List<Action> ();
		}

		public override void DoActions()
		{
            try {
                // get perceptions
                updatePerceptions();

				if (this.viewedEnemies.Count > 0) {
					// cancel current action and forget the plan
					if (this.plan.Count > 0) {
						this.currentAction.cancel ();
						this.plan.Clear ();
					}

					// Run away in the opposite direction
					List<Point> possibleMoves = new List<Point> ();
					foreach (Point enemy in this.viewedEnemies) {
						possibleMoves.Add (Utils.oppositDirection (this._nanoAI.Location, enemy, getAASMAFramework ().Tissue));
					}
					Point target = Utils.getMiddlePoint (possibleMoves.ToArray ());
					new MoveAction (this._nanoAI, target).execute ();
					this.currentTarget = target;
					return;
				}

				if (plan.Count == 0) {
					// get desires
					Intention[] desires = Options ();

					// get intention based on the desires and the previous intention
					this.intention = Filter (desires, this.intention);

					this.plan = Plan (this.intention);
				}

				if (plan.Count > 0) {
					// Continue with the same plan

					this.currentAction = this.plan [0];
					// Only remove action when it has finished
					if (this._nanoAI.State == NanoBotState.WaitingOrders) {
						this.currentAction.execute ();
						this.plan.RemoveAt (0);
					}

					updatePerceptions ();
					if (this.canReconsider && Reconsider (this.intention)) {
						this.currentAction.cancel ();

						// get desires
						Intention[] desires = Options ();

						// get intention based on the desires and the previous intention
						this.intention = Filter (desires, this.intention);

						this.plan = Plan (this.intention);
					}
				}
            } catch (Exception e) {
                getAASMAFramework().logData(this._nanoAI, e.Message);
                getAASMAFramework().logData(this._nanoAI, e.StackTrace);
            }
		}

		private void updatePerceptions() {
			// update list of viewed hoshimies
			List<Point> visibleEmptyHoshimies = getAASMAFramework ().visibleHoshimies (this._nanoAI);
			foreach (Point emptyNeedle in getAASMAFramework().visibleEmptyNeedles(this._nanoAI))
			{
				visibleEmptyHoshimies.Remove(emptyNeedle);
			}

			foreach (Point fullNeedle in getAASMAFramework().visibleFullNeedles(this._nanoAI))
			{
				visibleEmptyHoshimies.Remove(fullNeedle);
			}

			foreach (Point p in visibleEmptyHoshimies) {
				if (!this.viewedHoshimies.Contains(p)) {
					this.viewedHoshimies.Add (p);
				}

				// Needle was destroyed
				if (this.createdNeedles.Contains(p))
				{
					this.createdNeedles.Remove(p);
				}
			}

			// update list of viewed enemies
			viewedEnemies = getAASMAFramework ().visiblePierres (this._nanoAI);
		}

		public override void receiveMessage(AASMAMessage msg)
		{
            try {
                string[] content = msg.Content.Split(';');
                if (content.Length == 2 && content[0].Equals("AZN")) {
                    foreach (Point p in Utils.deserializePoints(content[1])) {
                        if (!viewedHoshimies.Contains(p) && !createdNeedles.Contains(p))
                            viewedHoshimies.Add(p);
                    }
                }
                if (content.Length == 3 && content[0].Equals("Visited objective")) {
                    //getAASMAFramework().logData(this._nanoAI, "Received visited position: " + msg.Content);
                    Point p = new Point(int.Parse(content[1]), int.Parse(content[2]));
                    if (!visitedObjectives.Contains(p)) {
                        visitedObjectives.Add(p);
                    }
                }
                if (content[0].Equals("Give me knowledge")) {
                    if(visitedObjectives.Count > 0){
                        AASMAMessage message = new AASMAMessage(this._nanoAI.InternalName, "Objectives knowledge;" + Utils.serializePoints(visitedObjectives));
                        getAASMAFramework().sendMessage(message, msg.Sender);
                    }
                }
            } catch (Exception e) {
                getAASMAFramework().logData(this._nanoAI, e.Message);
                getAASMAFramework().logData(this._nanoAI, e.StackTrace);
            }
		}

		private static Intention[] Options () {
			return (Intention[]) Enum.GetValues(typeof(Intention));
		}
			
		private Intention Filter(Intention[] desires, Intention prevIntention) {
			if (getAASMAFramework ().protectorsAlive () < 10) {
				return Intention.CREATE_PROTECTOR;
			} 

			if (getAASMAFramework ().containersAlive () < 10) {
				return Intention.CREATE_CONTAINER;
			}

			if (getAASMAFramework ().explorersAlive () < 10) {
				return Intention.CREATE_EXPLORER;
			}

			if (getAASMAFramework ().overHoshimiPoint (this._nanoAI) && 
				(!getAASMAFramework ().overNeedle (this._nanoAI))) {
				return Intention.CREATE_NEEDLE;
			}

			// If there's still an empty hole, go to there
			if (this.viewedHoshimies.Count > 0) {
				return Intention.MOVE_HOSHIMIE;
			}

			return Intention.MOVE_RANDOM;
		}

		private List<Action> Plan(Intention intention) {
			List<Action> plan = new List<Action> ();
			Point target = Point.Empty;

			this.canReconsider = false;

			switch (intention) {
			case Intention.CREATE_CONTAINER:
				plan.Add (new CreateAgentAction (this, typeof(CommunicativeContainer), "C" + this._containerNumber));
				this._containerNumber++;
				break;

			case Intention.CREATE_EXPLORER:
				plan.Add(new CreateAgentAction(this, typeof(CommunicativeExplorer), "E" + this._explorerNumber));
				this._explorerNumber++;
				break;

			case Intention.CREATE_PROTECTOR:
				if (getAASMAFramework ().protectorsAlive () < 6) {
					plan.Add (new CreateAgentAction (this, typeof(CommunicativeProtectorAI), "PAI" + this._protectorNumber));
				} else {
					plan.Add (new CreateAgentAction (this, typeof(CommunicativeProtector), "P" + this._protectorNumber));
				}
				this._protectorNumber++;
				break;

			case Intention.CREATE_NEEDLE:
				plan.Add (new CreateAgentAction (this, typeof(CommunicativeNeedle), 
					new CreateAgentAction.AgentCreatedDelegate (this.onAgentCreated), "N" + this._needleNumber));
				this._needleNumber++;
				break;

			case Intention.MOVE_HOSHIMIE:
				// choose the nearest hole
				int distance = int.MaxValue;
				foreach (Point p in this.viewedHoshimies) {
					if (!this.createdNeedles.Contains (p)) {
						if (Utils.SquareDistance (this._nanoAI.Location, p) < distance) {
							distance = Utils.SquareDistance (this._nanoAI.Location, p);
							target = p;
						}
					}
				}
				plan.Add (new MoveAction (this._nanoAI, target));
				plan.Add (new CreateAgentAction (this, typeof(CommunicativeNeedle), 
					new CreateAgentAction.AgentCreatedDelegate (this.onAgentCreated), "N" + this._needleNumber));
				this._needleNumber++;
				this.currentTarget = target;
				this.canReconsider = true;
				break;

			case Intention.MOVE_RANDOM:
				plan.Add (new MoveAction (this._nanoAI, Utils.randomValidPoint(getAASMAFramework().Tissue)));
				this.currentTarget = target;
				this.canReconsider = true;
				break;
			}

			getAASMAFramework().logData(this._nanoAI, "Current intention: " + Enum.GetName(typeof(Intention), intention));
			return plan;
		} 

		private void onAgentCreated (Type agentType)
		{
            if (agentType.Equals(typeof(CommunicativeNeedle))) 
			{
				this.createdNeedles.Add (this._nanoAI.Location);
				this.viewedHoshimies.Remove (this._nanoAI.Location);
			}
		}

		/**
		 * Reconsiders when:
		 * - Enemies are in the view range;
		 * - A hole is in the range and it's unoccupied
		 */ 
		private bool Reconsider(Intention prevIntention) {
			List<Point> hoshimiePoints = getAASMAFramework().visibleHoshimies(this._nanoAI);
			if (!hoshimiePoints.Contains(this.currentTarget))
			{
				foreach (Point p in hoshimiePoints)
				{
					if (!this.createdNeedles.Contains(p))
					{
						return true;
					}
				}
			}

			return false;
		}

		private enum Intention {
			MOVE_RANDOM, FLEE, MOVE_HOSHIMIE,
			CREATE_PROTECTOR, CREATE_CONTAINER, CREATE_EXPLORER, CREATE_NEEDLE
		}
	}
}