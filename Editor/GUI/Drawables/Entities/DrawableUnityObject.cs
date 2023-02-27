﻿using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityObject : BaseEntityDrawable
    {
        public DrawableUnityObject(object obj) 
            : base(obj)
        {
        }

        protected override void Draw(object target)
        {
            EditorGUILayout.ObjectField(Label, target as UnityEngine.Object, target.GetType(), true);
        }

        protected override void Draw(Rect rect, object target)
        {
            EditorGUI.ObjectField(rect, Label, target as UnityEngine.Object, target.GetType(), true);
        }
    }
}