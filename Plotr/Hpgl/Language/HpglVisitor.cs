using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hpgl.Language
{
    /// <summary>
    /// visitor with virtual methods, so implement only what you need
    /// - do not call base
    /// - all non-overriden methods will call <see cref="Unprocessed"/> method
    /// </summary>
    public class HpglVisitor : HpglAllVisitor
    {
        protected virtual void Unprocessed(HpglItem item)
        {
            //
        }
        protected override void VisitTerminator(Terminator item) { Unprocessed(item); }
        protected override void VisitPenUp(PenUp item) { Unprocessed(item); }
        protected override void VisitPenDown(PenDown item) { Unprocessed(item); }
        protected override void VisitPenAbsolute(PenAbsolute item) { Unprocessed(item); }
        protected override void VisitPenRelative(PenRelative item) { Unprocessed(item); }
        protected override void VisitInitialization(Initialization item) { Unprocessed(item); }
        protected override void VisitSelectPen(SelectPen item) { Unprocessed(item); }
        protected override void VisitLabel(Label item) { Unprocessed(item); }
        protected override void VisitUnknown(UnknownCommand item) { Unprocessed(item); }
    }
}
