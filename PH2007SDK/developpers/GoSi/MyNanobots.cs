/* Template done by
 * 
 * Richard Clark
 * Project Hoshimi Lead Manager
 * Contact at ori@c2i.fr
 *  
 * Changes done by
 * 
 * Bruno Silva & Paulo Gomes
 * 
 * 
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Map;

namespace GoSi
{

    [Characteristics(ContainerCapacity = 20, CollectTransfertSpeed = 5, Scan = 5, MaxDamage = 0, DefenseDistance = 0, Constitution = 20)]
    public class Collector : PH.Common.NanoCollector, IActionable
    {
        public const int SquadNumber = 5;

        public enum WhatToDoNextAction
        {
            MoveToAZN = 0,
            CollectAZN = 1,
            MoveToHoshimi = 2,
            TransfertToNeedle = 3,
        }

        private WhatToDoNextAction m_WhatToDoNext = WhatToDoNextAction.CollectAZN;

        public WhatToDoNextAction WhatToDoNext
        {
            get { return m_WhatToDoNext; }
            set { m_WhatToDoNext = value; }
        }  

        #region IAction Members
        public void DoActions()
        {
            switch (this.WhatToDoNext)
            {
                // Move primeiro porque pode n�o come�ar num AZN
                case WhatToDoNextAction.MoveToAZN:
                    this.MoveTo(Utils.getNearestPoint(this.Location, ((myPlayer)this.PlayerOwner).AznEntities));
                    this.WhatToDoNext = WhatToDoNextAction.CollectAZN;
                    break;

                case WhatToDoNextAction.CollectAZN:
                    //TODO: 4 should be replaced by a number dependent on the current number of squad members
                    this.CollectFrom(this.Location, 4);
                    this.WhatToDoNext = WhatToDoNextAction.MoveToHoshimi;
                    break;

                case WhatToDoNextAction.MoveToHoshimi:
                    this.MoveTo(Utils.getNearestPoint(this.Location, ((myPlayer)this.PlayerOwner).HoshimiEntities));
                    this.WhatToDoNext = WhatToDoNextAction.TransfertToNeedle;
                    break;

                case WhatToDoNextAction.TransfertToNeedle:
                    //TODO:PG: we need to recheck if this is a empty needle
                    //TODO:PG: 4 should be replaced by a number dependent on the current number of squad members
                    this.TransferTo(this.Location, 4);
                    this.WhatToDoNext = WhatToDoNextAction.MoveToAZN;
                    break;
            }
        }
        #endregion
    }

    [Characteristics(ContainerCapacity = 100, CollectTransfertSpeed = 0, Scan = 10, MaxDamage = 5, DefenseDistance = 10, Constitution = 25)]
    public class Needle : PH.Common.NanoNeedle, IActionable
    {
        public const int SquadNumber = 0;
        #region IAction Members
        public void DoActions()
        { }
        #endregion
    }

    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 30, MaxDamage = 0, DefenseDistance = 0, Constitution = 10)]
    public class Explorer : PH.Common.NanoExplorer, IActionable
    {
        public const int SquadNumber = 1;

        public enum WhatToDoNextAction
        {
            WaitingForPoints = 0,
            MoveToPoint = 1,
        }

        private WhatToDoNextAction m_WhatToDoNext = WhatToDoNextAction.WaitingForPoints;
        public WhatToDoNextAction WhatToDoNext
        {
            get { return m_WhatToDoNext; }
            set { m_WhatToDoNext = value; }
        }

        private Queue<Point> m_PointsToVisit = new Queue<Point>();
        public Queue<Point> PointsToVisit { get { return m_PointsToVisit; } }

        #region IAction Members
        public void DoActions()
        {
            switch (this.WhatToDoNext)
            {
                case WhatToDoNextAction.WaitingForPoints:
                    /*TODO: ir buscar um ponto sem ninguem*/
                    /*PG: Atribuir pontos a explorers
                     *    P: E se eles morrem?
                     *    R: T�m de ir renovando a atribui��o.
                     *    P: Mas ent�o os pontos n�o v�o voltar a ser atribu�dos a cada turno?
                     *    R: N�o se obrigarmos o explorer a renovar atribui��o para dois turnos.
                     * 
                     *  Rascunho: vector(navigation) tem 0, 1 ou 2
                     *            1 e 2, o ponto est� atribu�do
                     *            0 o ponto n�o est� atribu�do
                     *            em cada update devemos decrementar um valor a todas as posi��es
                     *            que n�o t�m zero antes de dar aos explorers possibilidade de escolher.
                     * 
                     * */
                    break;
                case WhatToDoNextAction.MoveToPoint:
                    if (this.PointsToVisit.Count > 0)
                        this.MoveTo(PointsToVisit.Dequeue());
                    else
                        this.ForceAutoDestruction();
                    break;
            }
        }
        #endregion
    }

    [Characteristics(ContainerCapacity = 0, CollectTransfertSpeed = 0, Scan = 5, MaxDamage = 5, DefenseDistance = 12, Constitution = 25)]
    public class Protector : PH.Common.NanoCollector, IActionable
    {
        public const int SquadNumber = 0;
        #region IAction Members
        public void DoActions()
        { }
        #endregion
    }
}
