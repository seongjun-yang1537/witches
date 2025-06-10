using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Corelib.SUI
{
    public class SUIElement
    {
        public Rect? rect;
        protected readonly UnityAction onRender;

        protected SUIElement()
        {

        }
        protected SUIElement(UnityAction onRender)
        {
            this.onRender = onRender;
        }
        protected SUIElement(Rect rect, UnityAction onRender) : this(onRender)
        {
            this.rect = rect;
        }

        public virtual void Render() => onRender?.Invoke();

        public virtual float GetElementHeight() => EditorGUIUtility.singleLineHeight;

        public static SUIElement operator |(SUIElement element, UnityAction<SUIElement> render)
        {
            render?.Invoke(element);
            return element;
        }

        public static SUIElement operator +(SUIElement a, SUIElement b)
            => new SUIElement(() =>
            {
                a.Render();
                b.Render();
            });
    }
}