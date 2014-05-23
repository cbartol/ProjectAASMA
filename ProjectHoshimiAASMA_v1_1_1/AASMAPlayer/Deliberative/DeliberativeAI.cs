using System;
using System.Collections.Generic;
using System.Text;
using AASMAHoshimi;
using PH.Common;
using System.Drawing;

namespace AASMAHoshimi.Deliberative
{
	public class DeliberativeAI : AASMAAI
	{
		private List<Point> emptyNeedles;
		private List<Point> createdNeedles;
		private List<Point> viewedEnemies;
		private List<Action> plan;
		private Intention intention;

		public DeliberativeAI(NanoAI nano)
		{
			this._nanoAI = nano;
			this.emptyNeedles = new List<Point>();
			this.createdNeedles = new List<Point>();
			this.plan = new List<Action> ();
		}

		public override void DoActions()
		{
			// get perceptions
			updatePerceptions ();

			if (plan.Count == 0) {
				// get desires
				Intention[] desires = Options ();

				// get intention based on the desires and the previous intention
				this.intention = Filter (desires, this.intention);

				this.plan = Plan (this.intention);
			}

			if (plan.Count > 0) {
				// Continue with the same plan

				Action action = this.plan [0];
				// Only remove action when it has finished
				if (this._nanoAI.State == NanoBotState.WaitingOrders) {
					action.execute ();
					this.plan.RemoveAt (0);
				}

				updatePerceptions ();
				if (Reconsider ()) {
					action.cancel ();

					// get desires
					Intention[] desires = Options ();

					// get intention based on the desires and the previous intention
					this.intention = Filter (desires, this.intention);

					this.plan = Plan (this.intention);
				}
			}
		}

		private void updatePerceptions() {
			// update list of viewed hoshimies
			List<Point> emptyNeedles = getAASMAFramework ().visibleEmptyNeedles (this._nanoAI);
			foreach (Point p in emptyNeedles) {
				if (!emptyNeedles.Contains (p)) {
					emptyNeedles.Add (p);
				}
			}

			// update list of viewed enemies
			viewedEnemies = getAASMAFramework ().visiblePierres (this._nanoAI);
		}

		public override void receiveMessage(AASMAMessage msg)
		{
			// Empty on purpose
		}

		private static Intention[] Options () {
			return (Intention[]) Enum.GetValues(typeof(Intention));
		}
			
		private Intention Filter(Intention[] desires, Intention prevIntention) {
			if (this.viewedEnemies.Count != 0) {
				return Intention.FLEE;
			}

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
			if (this.emptyNeedles.Count > 0) {
				return Intention.MOVE_EMPTY_NEEDLE;
			}

			// Visit an old hoshimie point with a certain probability
			if (this.createdNeedles.Count > 0) {
				bool visitCreatedNeedle = Utils.randomValue (100) < 20;
				if (visitCreatedNeedle) {
					return Intention.MOVE_CREATED_HOSHIMIE;
				}
			}

			return Intention.MOVE_RANDOM;
		}

		private List<Action> Plan(Intention intention) {
			List<Action> plan = new List<Action> ();
			Point target = Point.Empty;

			switch (intention) {
			case Intention.CREATE_CONTAINER:
				plan.Add (new CreateAgentAction (this, typeof(DeliberativeContainer), this._containerNumber));
				this._containerNumber++;
				break;

			case Intention.CREATE_EXPLORER:
				plan.Add(new CreateAgentAction(this, typeof(DeliberativeExplorer), this._explorerNumber));
				this._explorerNumber++;
				break;

			case Intention.CREATE_PROTECTOR:
				if (getAASMAFramework ().protectorsAlive () < 8) {
					plan.Add (new CreateAgentAction (this, typeof(DeliberativeProtectorAI), this._protectorNumber));
				} else {
					plan.Add (new CreateAgentAction (this, typeof(DeliberativeProtector), this._protectorNumber));
				}
				this._protectorNumber++;
				break;

			case Intention.CREATE_NEEDLE:
				plan.Add (new CreateAgentAction (this, typeof(DeliberativeNeedle), 
					new CreateAgentAction.AgentCreatedDelegate (this.onAgentCreated), this._needleNumber));
				this._needleNumber++;
				break;

			case Intention.MOVE_EMPTY_NEEDLE:
				// choose the nearest hole
				int distance = int.MaxValue;
				foreach (Point p in this.emptyNeedles) {
					if (!this.createdNeedles.Contains (p)) {
						if (Utils.SquareDistance (this._nanoAI.Location, p) < distance) {
							distance = Utils.SquareDistance (this._nanoAI.Location, p);
							target = p;
						}
					}
				}
				plan.Add (new MoveAction (this._nanoAI, target));
				plan.Add (new CreateAgentAction (this, typeof(DeliberativeNeedle), 
					new CreateAgentAction.AgentCreatedDelegate (this.onAgentCreated), this._needleNumber));
				this._needleNumber++;
				break;

			case Intention.FLEE:
				List<Point> possibleMoves = new List<Point> ();
				foreach (Point enemy in this.viewedEnemies) {
					possibleMoves.Add (Utils.oppositDirection (this._nanoAI.Location, enemy, getAASMAFramework ().Tissue));
				}
				target = Utils.getMiddlePoint (possibleMoves.ToArray ());
				plan.Add(new MoveAction(this._nanoAI, target));
				break;

			case Intention.MOVE_CREATED_HOSHIMIE:
				// choose random hoshimie to visit
				int randomIdx = Utils.randomValue (this.createdNeedles.Count);
				target = this.createdNeedles [randomIdx];
				plan.Add (new MoveAction (this._nanoAI, target));
				break;

			case Intention.MOVE_RANDOM:
				target = ClearPoint (80);
				plan.Add (new MoveAction (this._nanoAI, target));
				break;
			}
			
			return plan;
		} 

		private void onAgentCreated (Type agentType)
		{
			if (agentType.Equals (typeof(DeliberativeNeedle))) 
			{
				this.createdNeedles.Add (this._nanoAI.Location);
				this.emptyNeedles.Remove (this._nanoAI.Location);
			}
		}

		/**
		 * Reconsiders when:
		 * - Enemies are in the view range;
		 * - A hole is in the range and it's unoccupied
		 */ 
		private bool Reconsider() {
			bool enemieSpotted = getAASMAFramework ().visiblePierres (this._nanoAI).Count > 0;
			bool emptyHoleInRange = false;

			List<Point> emptyNeedles = getAASMAFramework().visibleEmptyNeedles(this._nanoAI);
			foreach (Point p in emptyNeedles) {
				if (!emptyNeedles.Contains (p)) {
					emptyHoleInRange = true;
					break;
				}
			}

			return enemieSpotted || emptyHoleInRange;
		}

		private enum Intention {
			MOVE_RANDOM, FLEE, MOVE_CREATED_HOSHIMIE, MOVE_EMPTY_NEEDLE,
			CREATE_PROTECTOR, CREATE_CONTAINER, CREATE_EXPLORER, CREATE_NEEDLE
		}
	}
}