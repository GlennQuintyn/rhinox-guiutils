using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public class HorizontalGroupDrawable : BaseHorizontalGroupDrawable<HorizontalGroupAttribute>
    {
        public HorizontalGroupDrawable(GroupedDrawable parent, string groupID, int order)
            : base(parent, groupID, order)
        {
        }
        
        protected override void ParseAttribute(IOrderedDrawable child, HorizontalGroupAttribute attr)
        {
            var info = new SizeInfo
            {
                PreferredSize = attr.Width,
                MaxSize = attr.MaxWidth,
                MinSize = attr.MinWidth
            };

            EnsureSizeFits(info);

            _sizeInfoByDrawable.Add(child, info);
        }
        
        protected override void ParseAttribute(HorizontalGroupAttribute attr)
        {
            SetOrder(attr.Order);

            _size.MinSize = _parsedAttributes.Sum(x => x.Width > 0 ? x.Width : x.MinWidth);
            if (_parsedAttributes.All(x => x.Width > 0))
                _size.PreferredSize = _parsedAttributes.Sum(x => x.Width);
            else
                _size.PreferredSize = 0;
            if (_parsedAttributes.All(x => x.Width > 0 || x.MaxWidth > 0))
                _size.MaxSize = _parsedAttributes.Sum(x => x.Width > 0 ? x.Width : x.MaxWidth);
            else
                _size.MaxSize = 0;

            _parent?.EnsureSizeFits(_size);
        }
    }
}