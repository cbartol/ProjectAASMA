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
		private Action action;
		private Point targetAction;

		public DeliberativeAI(NanoAI nano)
		{
			this._nanoAI = nano;
			this.viewedHoshimies = new List<Point>();
			this.createdNeedles = new List<Point>();
		}

		public override void DoActions()
		{
			// get perceptions
			// update list of viewed hoshimies
			List<Point> visibleHoshimies = getAASMAFramework ().visibleHoshimies (this._nanoAI);
			foreach (Point p in visibleHoshimies) {
				if (!viewedHoshimies.Contains (p)) {
					viewedHoshimies.Add (p);
				}
			}

			// get desires
			Action[] desires = options();

			// get intention based on the desires and the previous intention
			Point newTarget;
			Action newAction = filter (desires, this.action, out newTarget);

			// stop previous action if it's different
			if (newAction != this.action || newTarget != this.targetAction) {
				stopCurrentAction ();
			}

			// execute the plan (action)
			executeAction (newAction, newTarget);
		}

		public override void receiveMessage(AASMAMessage msg)
		{
			// Empty on purpose
		}

		private Action[] options () {
			return (Action[]) Enum.GetValues(typeof(Action));
		}

		/**
		 * Must go to old needles sometimes
		 */
		private Action filter(Action[] desires, Action prevIntention, out Point newTarget) {
			newTarget = new Point();

			if (getAASMAFramework ().visiblePierres (this._nanoAI).Count != 0) {
				List<Point> possibleMoves = new List<Point> ();
				foreach (Point enemy in getAASMAFramework().visiblePierres(this._nanoAI)) {
					possibleMoves.Add (Utils.oppositDirection (this._nanoAI.Location, enemy, getAASMAFramework ().Tissue));
				}
				newTarget = Utils.getMiddlePoint (possibleMoves.ToArray ());

				return Action.FLEE;
			} 

			if (getAASMAFramework ().protectorsAlive () < 5) {
				return Action.CREATE_AGENT_PROTECTOR; 
			} 

			if (getAASMAFramework ().containersAlive () < 5) {
				return Action.CREATE_AGENT_CONTAINER;
			} 

			if (getAASMAFramework ().explorersAlive () < 5) {
				return Action.CREATE_AGENT_EXPLORER;
			}

			if (getAASMAFramework ().overHoshimiPoint (this._nanoAI) && (!getAASMAFramework ().overNeedle (this._nanoAI))) {
				return Action.CREATE_AGENT_NEEDLE;
			}

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

		private void executeAction(Action action, Point targetAction) {
			if (action == Action.MOVE_AROUND) {
				this.MoveToClearPosition (90);
			} else if (action == Action.MOVE_CREATED_HOSHIMIE || action == Action.FLEE || 
				action == Action.MOVE_EMPTY_HOLE) {
				this._nanoAI.MoveTo (targetAction);
			} else if (action == Action.CREATE_AGENT_CONTAINER) {
				this._nanoAI.Build (typeof(DeliberativeContainer), "C");
			} else if (action == Action.CREATE_AGENT_EXPLORER) {
				this._nanoAI.Build (typeof(DeliberativeExplorer), "E");
			} else if (action == Action.CREATE_AGENT_NEEDLE) {
				this.createdNeedles.Add (targetAction);
				this._nanoAI.Build (typeof(DeliberativeNeedle), "N");
			} else if (action == Action.CREATE_AGENT_PROTECTOR) {
				this._nanoAI.Build (typeof(DeliberativeProtector), "P");
			}

			this.action = action;
			this.targetAction = targetAction;
		}

		private void stopCurrentAction() {
			if (this.action == Action.FLEE || this.action == Action.MOVE_AROUND ||
				this.action == Action.MOVE_EMPTY_HOLE || this.action == Action.MOVE_CREATED_HOSHIMIE) {
				this._nanoAI.StopMoving ();
			}
		}
	}

	enum Action {
		MOVE_AROUND, FLEE, MOVE_CREATED_HOSHIMIE, MOVE_EMPTY_HOLE,
		CREATE_AGENT_CONTAINER, CREATE_AGENT_EXPLORER, CREATE_AGENT_NEEDLE, CREATE_AGENT_PROTECTOR
	}
}
