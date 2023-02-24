﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IOrderedDrawable
    {
        public string Name { get; }
        public float Order { get; set; }
        //public PropertyGroupAttribute Grouping { get; private set; }
        
        public string Title { get; private set; }
        public GUIStyle TitleStyle { get; private set; }
        
        
        public virtual float ElementHeight
        {
            get
            {
                if (Children != null)
                {
                    if (_groupHorizontally)
                        return Children.Max(x => x.ElementHeight);
                    else
                    {
                        float height = 0.0f;
                        foreach (var child in Children)
                            height += child.ElementHeight;
                        return height;
                    }
                }

                return EditorGUIUtility.singleLineHeight;
            }
        }

        private readonly List<IOrderedDrawable> _drawableMemberChildren;
        private List<Attribute> _attributes;
        private bool _groupHorizontally;

        public IReadOnlyCollection<IOrderedDrawable> Children => _drawableMemberChildren;

        public IOrderedDrawable FirstOrDefault(Func<IOrderedDrawable, bool> func = null)
        {
            if (func == null)
                return _drawableMemberChildren.FirstOrDefault();

            foreach (var child in _drawableMemberChildren)
            {
                if (child is CompositeDrawableMember compositeChild)
                {
                    return compositeChild.FirstOrDefault(func);
                }

                if (func.Invoke(child))
                    return child;
            }

            return null;
        }

        public static CompositeDrawableMember CreateFrom(PropertyGroupAttribute groupingAttr)
        {
            var drawableMember = new CompositeDrawableMember(groupingAttr.GroupID, groupingAttr.Order);
            if (groupingAttr is HorizontalGroupAttribute || groupingAttr is ButtonGroupAttribute)
                drawableMember.GroupHorizontal();
            else
                drawableMember.GroupVertical();

            if (groupingAttr is TitleGroupAttribute titleAttr)
            {
                drawableMember.Title = titleAttr.GroupName;
                drawableMember.TitleStyle = titleAttr.BoldTitle ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;
            }

            return drawableMember;
        }



        public CompositeDrawableMember(string name = null, float order = 0)
        {
            Name = name;
            Order = order;
            _drawableMemberChildren = new List<IOrderedDrawable>();
        }

        public void AddAttribute(Attribute attribute)
        {
            if (_attributes == null)
                _attributes = new List<Attribute>();
            _attributes.AddUnique(attribute);
        }

        public void Add(IOrderedDrawable child)
        {
            if (child != null)
                _drawableMemberChildren.AddUnique(child);
        }

        public void AddRange(ICollection<IOrderedDrawable> children)
        {
            if (children != null)
                _drawableMemberChildren.AddRange(children);
        }

        public ICollection<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute
        {
            if (_attributes == null)
                return Array.Empty<TAttribute>();
            return _attributes.OfType<TAttribute>().ToList();
        }

        public void Draw()
        {
            if (_drawableMemberChildren == null)
                return;

            StartGrouping();
            if (!string.IsNullOrWhiteSpace(Title))
                EditorGUILayout.LabelField(GUIContentHelper.TempContent(Title), TitleStyle);
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw();
            }
            EndGrouping();
        }

        public void Draw(Rect rect)
        {
            if (_drawableMemberChildren == null)
                return;
            
            if (!string.IsNullOrWhiteSpace(Title))
                EditorGUI.LabelField(rect, GUIContentHelper.TempContent(Title), TitleStyle);

            HandleRectGrouping(ref rect);
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw(rect);

                if (_groupHorizontally)
                    rect.x += rect.width;
                else
                    rect.y += rect.height;
            }
        }

        private void HandleRectGrouping(ref Rect rect)
        {
            // if (Grouping is HorizontalGroupAttribute horizontalAttr)
            // {
            //     if (horizontalAttr.Width > 0.0f)
            //     {
            //         rect.width = horizontalAttr.Width > 1.0f ? horizontalAttr.Width : horizontalAttr.Width * rect.width;
            //     }
            //     else
            //         rect.width = rect.width / _drawableMemberChildren.Count;
            // }
            // else
            if (!_groupHorizontally)
            {
                rect.height /= _drawableMemberChildren.Count;
            }
            else
            {
                rect.width /= _drawableMemberChildren.Count;
            }
        }

        // public void GroupBy(PropertyGroupAttribute grouping)
        // {
        //     Grouping = grouping;
        // }

        private void StartGrouping()
        {
            // if (Grouping == null)
            //     return;
            //
            // if (Grouping is HorizontalGroupAttribute horizontalAttr)
            // {
            //     if (horizontalAttr.Width > 0.0f)
            //         GUILayout.BeginHorizontal(GUILayout.Width(horizontalAttr.Width));
            //     else
            //         GUILayout.BeginHorizontal();
            // }
            // else if (Grouping is VerticalGroupAttribute || Grouping is TitleGroupAttribute)
            if (!_groupHorizontally)
            {
                GUILayout.BeginVertical();
            }
            else
            {
                GUILayout.BeginHorizontal();
            }
        }

        private void EndGrouping()
        {
            // if (Grouping == null)
            //     return;
            //
            // if (Grouping is HorizontalGroupAttribute horizontalAttr)
            // {
            //     GUILayout.EndHorizontal();
            // }
            // else if (Grouping is VerticalGroupAttribute || Grouping is TitleGroupAttribute)
            if (!_groupHorizontally)
            {
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
        }

        public void Sort()
        {
            foreach (var drawable in Children)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }

            _drawableMemberChildren.SortBy(x => x.Order);
        }
        
        private void GroupVertical()
        {
            _groupHorizontally = false;
        }

        private void GroupHorizontal()
        {
            _groupHorizontally = true;
        }
    }
}