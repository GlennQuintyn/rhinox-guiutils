using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberHelper
    {
        protected string _errorMessage;
        protected Type _objectType;

        public bool HasError => !_errorMessage.IsNullOrEmpty();

        protected object _host;
        protected NewFrameHandler _newFrameHandler;
        
        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        protected virtual MemberTypes AllowedMembers => MemberTypes.Property | MemberTypes.Field | MemberTypes.Method;

        protected bool TryParseInput(ref string input, out bool parameter)
        {
            parameter = false;
            
            if (string.IsNullOrEmpty(input) || _objectType == null || input.Length <= 0)
                return false;
            
            if (input[0] == '@')
            {
                input = input.Substring(1);

                if (!TryParseExpression(input))
                    return false;
            }

            if (input[0] == '$')
            {
                input = input.Substring(1);
                parameter = true;
                
                if (!TryParseParameter(ref input))
                    return false;
            }
            
            return true;
        }

        protected virtual bool TryParseParameter(ref string input)
        {
            return true;
        }

        protected virtual bool TryParseExpression(string input)
        {
            this._errorMessage = "Expressions are only supported with Odin Enabled";
            return false;
        }
        
        public bool DrawError()
        {
            if (_errorMessage.IsNullOrEmpty())
                return false;
                
            EditorGUILayout.HelpBox(_errorMessage, MessageType.Error, true);
            return true;
        }
        
        public bool DrawError(Rect rect)
        {
            if (_errorMessage.IsNullOrEmpty())
                return false;
                
            EditorGUI.HelpBox(rect, _errorMessage, MessageType.Error);
            return true;
        }
        
        protected bool TryFindMemberInHost<T>(string input, bool isStatic, out Func<T> staticGetter, out Func<object, T> instanceGetter)
        {
            staticGetter = null;
            instanceGetter = null;
            
            if (!TryFindMember(input, out MemberInfo info, isStatic, !isStatic))
                return false;

            if (!info.GetReturnType().InheritsFrom<T>())
                return false;

            if (isStatic)
                staticGetter = () => (T) info.GetValue(null);
            else
                instanceGetter = (i) => (T) info.GetValue(i);
            
            return true;
        }
        
        protected bool TryFindMember(string filter, out MemberInfo info, bool isStatic = false, bool includeExtensionMethods = false)
        {
            var flags = isStatic ? BindingFlags.Static : (BindingFlags.Static | BindingFlags.Instance);
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return ReflectionUtility.TryGetMember(_objectType, AllowedMembers, filter, out info, flags, includeExtensionMethods);
        }
    }
}