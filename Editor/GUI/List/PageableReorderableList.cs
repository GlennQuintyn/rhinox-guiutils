﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class PageableReorderableList : BetterReorderableList
    {
        public int MaxItemsPerPage { get; set; } = DEFAULT_ITEMS_PER_PAGE;
        private const int DEFAULT_ITEMS_PER_PAGE = 100;
        
        private ICollection<Type> m_AddOptionTypes;
        private bool HasMultipleTypeOptions
        {
            get 
            {
                if (m_AddOptionTypes != null)
                {
                    if (m_AddOptionTypes.Count > 1)
                        return true;
                    if (m_AddOptionTypes.Count == 1)
                        return m_AddOptionTypes.First() != this.m_ElementType;
                }

                return false;
            }
        }

        public PageableReorderableList(IList elements, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
            : base(elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        public PageableReorderableList(SerializedObject serializedObject, SerializedProperty elements, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
            : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        protected override void InitList(SerializedObject serializedObject, SerializedProperty elements, IList elementList, bool draggable,
            bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        {
            base.InitList(serializedObject, elements, elementList, draggable, displayHeader, displayAddButton, displayRemoveButton);
            
            if (this.displayAdd && this.m_ElementType != null)
            {
                var options = new HashSet<Type>();
                if (!m_ElementType.IsAbstract)
                    options.Add(this.m_ElementType);
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => x.InheritsFrom(this.m_ElementType))
                    .Where(x => !x.IsAbstract)
                    .ToArray();
                options.AddRange(types);
                this.m_AddOptionTypes = options;
            }
            else
            {
                this.m_AddOptionTypes = Array.Empty<Type>();
            }
        }

        protected override void OnDrawHeader(Rect rect)
        {
            var nameRect = rect.AlignLeft(rect.width);
            var sizeRect = rect.AlignRight(rect.width / 4.0f);
            EditorGUI.LabelField(nameRect, this.serializedProperty.displayName);
            EditorGUI.LabelField(sizeRect, $"{count} Items");
        }

        protected override void DoListFooter(Rect rect)
        {
            if (!displayAdd && !displayRemove)
                return;

            if (MaxItemsPerPage > 0)
            {
                var list = serializedProperty.GetValue() as IList;
                if (list != null && list.Count > MaxItemsPerPage)
                    rect.y -= (list.Count - MaxItemsPerPage - 1) * elementHeight;
            }
            
            s_Defaults.DrawFooter(rect, this, this.displayAdd, false);
        }

        protected override void DrawElement(Rect contentRect, int elementIndex, bool selected = false, bool focused = false)
        {
            if (MaxItemsPerPage > 0 && elementIndex > MaxItemsPerPage)
                return;
            // const float margin = 16.0f;
            // contentRect.y += margin; // TODO: is this margin?
            // contentRect.height = elementHeight + margin;

            Rect removeButton = default;
            if (this.displayRemove)
            {
                removeButton = contentRect.AlignRight(18).AlignCenterVertical(18);
                contentRect = contentRect.PadRight(9);
            }

            base.DrawElement(contentRect, elementIndex, selected, focused);

            if (this.displayRemove)
            {
                if (GUI.Button(removeButton, GUIContentHelper.TempContent(UnityIcon.AssetIcon("Fa_Times").Pad(2), tooltip: "Remove entry.")))
                {
                    this.index = elementIndex;
                    s_Defaults.OnRemoveElement(this);
                    onChangedCallback?.Invoke(this);
                }
            }
        }
        
        protected override void OnDrawElementBackground(Rect rect, int index, bool selected, bool focused, bool draggable)
        {
            if (MaxItemsPerPage > 0 && index > MaxItemsPerPage)
                return;

            s_Defaults.DrawElementBackgroundAlternating(rect, index, selected, focused, draggable);
        }
        
        protected override int GetListDrawCount()
        {
            if (MaxItemsPerPage > 0)
                return Mathf.Min(MaxItemsPerPage, base.GetListDrawCount());
            return base.GetListDrawCount();
        }

        protected override void OnAddElement(Rect rect1)
        {
            if (!HasMultipleTypeOptions)
            {
                base.OnAddElement(rect1);
                return;
            }
            
            var genericMenu = new GenericMenu();
            foreach (var option in this.m_AddOptionTypes)
            {
                genericMenu.AddItem(new GUIContent(option.Name), false, () =>
                {
                    if (serializedProperty != null)
                    {
                        ++serializedProperty.arraySize;
                        var serializedPropElement = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
                        var hostInfo = serializedPropElement.GetHostInfo();
                        var instance = Activator.CreateInstance(option);
                        hostInfo.SetValue(instance);
                    }
                    else
                    {
                        
                        if (list == null)
                            list = (IList)Activator.CreateInstance(this.m_ListType);
                        index = list.Add(Activator.CreateInstance(option));
                    }

                    if (onChangedCallback != null)
                        onChangedCallback?.Invoke(this);
                });
            }
            genericMenu.DropDown(rect1);
        }

        protected override GUIContent GetAddIcon()
        {
            if (HasMultipleTypeOptions)
                return s_Defaults.iconToolbarPlusMore;
            return base.GetAddIcon();
        }
    }
}