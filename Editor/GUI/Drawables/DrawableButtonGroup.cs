﻿using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableButtonGroup : SimpleDrawable
    {
        public string ID { get; set; }
        
        private List<DrawableButton> _buttons;
        
        public DrawableButtonGroup(SerializedObject obj, string groupId, int order = 0) : base(obj, order)
        {
            _buttons = new List<DrawableButton>();
            ID = groupId;
        }

        public void AddButton(DrawableButton button)
        {
            _buttons.AddUnique(button);
        }

        protected override void Draw(Object target)
        {
            GUILayout.BeginHorizontal();
            foreach (var button in _buttons)
            {
                if (button == null)
                    continue;
                button.Draw();
            }
            GUILayout.EndHorizontal();
        }
    }
}