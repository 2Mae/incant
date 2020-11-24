using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;

namespace Reordarchy
{
    public class ReordarchyWindow : EditorWindow
    {
        private const KeyCode hotkey = KeyCode.BackQuote;
        bool close = false;
        static ReordarchyWindow instance;
        static EditorWindow hierarchy;

        [Shortcut("Incant/Reorder (sticky)", hotkey, ShortcutModifiers.Action)]
        public static void ShowSticky() { ShowWindow(); }

        [Shortcut("Incant/Reorder", hotkey)]
        public static void ShowWindow()
        {
            hierarchy = Reordering.GetHierarchyWindow();
            if (instance == null && Reordering.hierarchyIsFocused)
            {
                ReordarchyWindow window = ScriptableObject.CreateInstance<ReordarchyWindow>();
                const int leftIndent = 40;
                const int lineHeight = 20;
                window.position = new Rect(hierarchy.position.position + new Vector2(leftIndent, lineHeight), new Vector2(hierarchy.position.size.x - leftIndent, lineHeight - 2));
                window.ShowPopup();
                instance = window;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Reorder");

            var e = Event.current;

            bool releasedHotkeyWithoutModifier = (e.type == EventType.KeyUp && e.keyCode == hotkey && e.modifiers == EventModifiers.None);
            if (e.keyCode == KeyCode.Escape || releasedHotkeyWithoutModifier)
            {
                //Simulate lost focus to close window.
                OnLostFocus();
            }

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.UpArrow) { Reordering.Reorder(Direction.UP); }
                if (e.keyCode == KeyCode.DownArrow) { Reordering.Reorder(Direction.DOWN); }

                if (e.keyCode == KeyCode.LeftArrow) { Reordering.Unparent(); }
                if (e.keyCode == KeyCode.RightArrow) { Reordering.Parent(); }
                if (e.keyCode == KeyCode.Alpha0) { Reordering.ZeroOut(Reordering.GetTopSelected()); }
            }
        }

        private void OnLostFocus()
        {
            close = true;
        }

        private void Update()
        {
            //Had some issues when calling Close() outside Update.
            if (close)
            {
                Close();
            }
        }
    }
}