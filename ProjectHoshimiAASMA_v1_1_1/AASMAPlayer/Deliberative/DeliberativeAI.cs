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
		private List<Point> viewedHoshimies;
		private List<Point> createdNeedles;
		private List<Point> viewedEnemies;
		private List<Action> plan;
		private Intention intention;

		public DeliberativeAI(NanoAI nano)
		{
			this._nanoAI = nano;
			this.viewedHoshimies = new List<Point>();
			this.createdNeedles = new List<Point>();
		}

		public override void onAgentCreated (Type agentType)
		{
			if (Type.Equals (typeof(DeliberativeNeedle))) 
			{
				this.createdNeedles.Add (this._nanoAI.Location);
			}
		}

		public override void DoActions()
		{
			// get perceptions
			updatePerceptions ();

			if (plan == null) {
				// get desires
				Intention[] desires = Options ();

				// get intention based on the desires and the previous intention
				this.intention = Filter (desires, this.intention);

				this.plan = Plan (this.intention);
			}

			if (plan.Count > 0) {
				// Continue with the same plan

				Action action = this.plan [0];
				action.execute ();
				this.plan.RemoveAt (0);

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
			List<Point> visibleHoshimies = getAASMAFramework ().visibleHoshimies (this._nanoAI);
			foreach (Point p in visibleHoshimies) {
				if (!viewedHoshimies.Contains (p)) {
					viewedHoshimies.Add (p);
				}
			}

			// update list of viewed enemies
			viewedEnemies = getAASMAFramework ().visiblePierres (this);
		}

		public override void receiveMessage(AASMAMessage msg)
		{
			// Empty on purpose
		}

		private static Intention[] Options () {
			return (Intention[]) Enum.GetValues(typeof(Intention));
		}

		/**
		 * TODO Must go to old needles sometimes
		 */
		private static Intention Filter(Intention[] desires, Intention prevIntention) {
			if (getAASMAFramework ().visiblePierres (this._nanoAI).Count != 0) {
				return Intention.FLEE;
			}

			if (getAASMAFramework ().protectorsAlive () < 10 ||
			    getAASMAFramework ().containersAlive () < 10 ||
				getAASMAFramework ().explorersAlive () < 10 ||
				(getAASMAFramework ().overHoshimiPoint (this._nanoAI) && 
					(!getAASMAFramework ().overNeedle (this._nanoAI))))
			{
				return Intention.CREATE_AGENT;
			}
				

				return Action.FLEE;


			if (!getAASMAFramework ().overHoshimiPoint (this._nanoAI) || getAASMAFramework ().overNeedle (this._nanoAI)) {
				// TODO take into account the distance (choose the closest spot)
				foreach (Point p in this.viewedHoshimies) {
					if (!this.createdNeedles.Contains (p)) {
						targetAction = p;
						return Action.MOVE_EMPTY_HOLE;
					}
				}
			} 

			// TODO either move around or visit a previous created hole
			return Action.MOVE_AROUND;
		}

//		private void executeAction(Action action, Point targetAction) {
//			if (action == Action.MOVE_AROUND) {
//				this.MoveToClearPosition (90);
//			} else if (action == Action.MOVE_CREATED_HOSHIMIE || action == Action.FLEE || 
//				action == Action.MOVE_EMPTY_HOLE) {
//				this._nanoAI.MoveTo (targetAction);
//			} else if (action == Action.CREATE_AGENT_CONTAINER) {
//				this._nanoAI.Build (typeof(DeliberativeContainer), "C");
//			} else if (action == Action.CREATE_AGENT_EXPLORER) {
//				this._nanoAI.Build (typeof(DeliberativeExplorer), "E");
//			} else if (action == Action.CREATE_AGENT_NEEDLE) {
//				this.createdNeedles.Add (targetAction);
//				this._nanoAI.Build (typeof(DeliberativeNeedle), "N");
//			} else if (action == Action.CREATE_AGENT_PROTECTOR) {
//				this._nanoAI.Build (typeof(DeliberativeProtector), "P");
//			}
//
//			this.action = action;
//			this.targetAction = targetAction;
//		}

		private static List<Action> Plan(Intention intention) {
			List<Action> plan = new List<Action> ();

			switch (intention) {
			case Intention.CREATE_AGENT:
				break;
			case Intention.FLEE:
				List<Point> possibleMoves = new List<Point> ();
				foreach (Point enemy in getAASMAFramework().visiblePierres(this._nanoAI)) {
					possibleMoves.Add (Utils.oppositDirection (this._nanoAI.Location, enemy, getAASMAFramework ().Tissue));
				}
				Point target = Utils.getMiddlePoint (possibleMoves.ToArray ());
				plan.Add(new MoveAction(this, target));
				break;
			case Intention.MOVE_AROUND:
				break;
			case Intention.MOVE_CREATED_HOSHIMIE:
				break;
			}
		} 

		private static bool Reconsider() {
			// TODO
		}
	}

	enum Intention {
		MOVE_AROUND, FLEE, MOVE_CREATED_HOSHIMIE, CREATE_AGENT
	}
}