using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{
    /// <summary>
    /// replaces PR instruction by adequate PA
    /// </summary>
    public class Absolutizer : HpglProcessorBase
    {
        List<HpglItem> result;
        public List<HpglItem> Process(List<HpglItem> items)
        {
            result = new List<HpglItem>();
            Visit(items);
            return result;
        }

        protected override void MoveTo(HPoint pt)
        {
            if (isPenDown)
            {
                result.Add(new PenDown() { Points = { pt } });
            }
            else
            {
                result.Add(new PenUp() { Points = { pt } });
            }
            base.MoveTo(pt);
        }

        protected override void VisitTerminator(Terminator item)
        {
            
        }

        protected override void VisitInitialization(Initialization item)
        {
            
        }

        protected override void VisitLabel(Label item)
        {
            result.Add(item);
        }
    }
}
