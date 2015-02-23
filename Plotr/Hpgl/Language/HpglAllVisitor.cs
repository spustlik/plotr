using Hpgl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hpgl.Language
{
    /// <summary>
    /// visitor with abstract methods, so you must implement all
    /// </summary>
    public abstract class HpglAllVisitor
    {
        private Dictionary<Type, Action<HpglItem>> funcmap;
        private void EnsureMap()
        {
            if (funcmap != null)
                return;
            funcmap = new Dictionary<Type, Action<HpglItem>>();
            var methods = typeof(HpglAllVisitor).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var m in methods)
            {
                if (!m.IsAbstract)
                    continue;
                if (m.GetParameters().Length != 1)
                    continue;
                var itemType = m.GetParameters()[0].ParameterType;
                var p = Expression.Parameter(typeof(HpglItem), "item");
                var lambda = Expression.Lambda<Action<HpglItem>>(
                    Expression.Call(
                        Expression.Constant(this),
                        m,
                        Expression.Convert(p, itemType)
                        ), p);
                funcmap[itemType] = lambda.Compile();
            }
        }

        public void Visit(List<HpglItem> items)
        {
            EnsureMap();
            foreach (var item in items)
            {
                Visit(item);
            }
        }

        protected virtual void Visit(HpglItem item)
        {
            Action<HpglItem> a;
            if (!funcmap.TryGetValue(item.GetType(), out a))
            {
                throw new NotImplementedException(String.Format("Missing method for {0}", item.GetType().Name));
            }
            a(item);
        }

        protected abstract void VisitTerminator(Terminator item);
        protected abstract void VisitPenUp(PenUp item);
        protected abstract void VisitPenDown(PenDown item);
        protected abstract void VisitPenAbsolute(PenAbsolute item);
        protected abstract void VisitPenRelative(PenRelative item);
        protected abstract void VisitInitialization(Initialization item);
        protected abstract void VisitSelectPen(SelectPen item);
        protected abstract void VisitLabel(Label item);
        protected abstract void VisitUnknown(UnknownCommand item);

    }

}
