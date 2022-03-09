﻿using System;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    [DrawerPriority(0, 200, 0)]
    public class PageSliderAttributeDrawer : OdinAttributeDrawer<PageSliderAttribute>
    {
        private static GUIStyle titleStyle;
        private static SlidePageNavigationHelper<InspectorProperty> currentSlider;
        private static InspectorProperty currentDrawingPageProperty;

        private SlidePageNavigationHelper<InspectorProperty> slider;
        private SlidePageNavigationHelper<InspectorProperty>.Page page;
        private GUIContent pageLabel;

        /*protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return !(property.ChildResolver is ICollectionResolver);
        }*/

        protected override void Initialize()
        {
            titleStyle = titleStyle ?? new GUIStyle("ShurikenModuleTitle");
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Property.ValueEntry.WeakSmartValue == null)
            {
                this.CallNextDrawer(label);
                return;
            }

            this.UpdateBreadcrumbLabel(label);

            if (currentSlider == null)
            {
                this.DrawPageSlider(label);
            }
            else if (currentDrawingPageProperty == this.Property)
            {
                this.CallNextDrawer(null);
            }
            else
            {
                GUIHelper.PushGUIEnabled(true);
                if (GUILayout.Button(new GUIContent(this.GetLabelText(label)), titleStyle))
                {
                    GUIHelper.PopGUIEnabled();

                    currentSlider.PushPage(this.Property, Guid.NewGuid().ToString());
                    this.page = currentSlider.EnumeratePages.Last();
                    this.page.Name = this.GetLabelText(label);
                    this.pageLabel = label;
                }
                else GUIHelper.PopGUIEnabled();
            }
        }

        private void UpdateBreadcrumbLabel(GUIContent label)
        {
            if (Event.current.type != EventType.Layout) return;
            if (this.page == null) return;
            if (this.pageLabel != null && this.pageLabel != this.Property.Label) return;

            var newLabel = this.GetLabelText(label ?? this.pageLabel);

            if (newLabel != this.page.Name)
            {
                this.page.Name = newLabel;
                var widthField = this.page.GetType().GetField("TitleWidth", Flags.AllMembers);
                widthField.SetValue(this.page, null);
            }
        }

        private void DrawPageSlider(GUIContent label)
        {
            try
            {
                if (this.slider == null)
                {
                    this.slider = new SlidePageNavigationHelper<InspectorProperty>();
                    this.slider.PushPage(this.Property, Guid.NewGuid().ToString());
                    this.page = this.slider.EnumeratePages.Last();
                    this.page.Name = this.GetLabelText(label);
                }

                currentSlider = this.slider;

                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginToolbarBoxHeader();
                {
                    var rect = GUILayoutUtility.GetRect(0, 20);
                    rect.x -= 5;
                    this.slider.DrawPageNavigation(rect);
                }
                SirenixEditorGUI.EndToolbarBoxHeader();
                {
                    this.slider.BeginGroup();
                    int pagesVisible = 0;
                    foreach (var p in this.slider.EnumeratePages)
                    {
                        if (p.BeginPage())
                        {
                            ++pagesVisible;
                            if (p.Value == this.Property)
                            {
                                this.CallNextDrawer(null);
                            }
                            else
                            {
                                currentDrawingPageProperty = p.Value;
                                if (p.Value.Tree != this.Property.Tree)
                                {
                                    InspectorUtilities.BeginDrawPropertyTree(p.Value.Tree, true);
                                }

                                p.Value.Draw(null);

                                if (p.Value.Tree != this.Property.Tree)
                                {
                                    InspectorUtilities.EndDrawPropertyTree(p.Value.Tree);
                                }

                                currentDrawingPageProperty = null;
                            }
                        }

                        p.EndPage();

                        // Repaint to keep a good flow (otherwise transition may be stuck halfway until mouse is moved)
                        if (pagesVisible > 1)
                            GUIHelper.CurrentWindow?.Repaint();
                    }

                    this.slider.EndGroup();
                }
                SirenixEditorGUI.EndBox();
            }
            finally
            {
                currentSlider = null;
            }
        }

        private string GetLabelText(GUIContent label)
        {
            if (label != null)
            {
                return label.text;
            }

            var val = this.Property.ValueEntry.WeakSmartValue;
            if (val == null)
            {
                return "Null";
            }

            var uObj = val as UnityEngine.Object;
            if (uObj)
            {
                if (string.IsNullOrEmpty(uObj.name))
                {
                    return uObj.ToString();
                }
                else
                {
                    return uObj.name;
                }
            }

            return val.ToString();
        }
    }
}