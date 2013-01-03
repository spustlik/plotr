using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hpgl.Transformations
{
    /// <summary>
    /// converts LB to PR using <see cref="Font6"/>
    /// </summary>
    public class Textificator : HpglVisitor
    {
        List<HpglItem> result;
        internal List<HpglItem> Process(List<HpglItem> hpgl)
        {
            result = new List<HpglItem>();
            Visit(hpgl);
            return result;
        }

        protected override void Unprocessed(HpglItem item)
        {
            result.Add(item);
        }

        protected override void VisitLabel(Label item)
        {
            Console.WriteLine(" [{0}]", item.Text);
            result.AddRange(Font6.DrawString(item.Text));
        }

    }
}
