using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AASMAHoshimi {
    class VisitObjective : Action {
        public delegate void VisitPoint(Point p);

        private VisitPoint callback;
        private Point pointToVisit;

        public VisitObjective(VisitPoint callback, Point pointToVisit) {
            this.callback = callback;
            this.pointToVisit = pointToVisit;
        }

        public void execute() {
            callback(pointToVisit);
        }

        public void cancel() {
        }
    }
}
