﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class GenericPicker
    {
        public static SimplePicker<T> Show<T>(
            Rect rect,
            ICollection<T> options,
            Func<T, string> textSelector = null,
            Func<T, string> subtextSelector = null)
        {
            var picker = new SimplePicker<T>(options, textSelector, subtextSelector);
            picker.Show(rect);
            return picker;
        }
    }
}