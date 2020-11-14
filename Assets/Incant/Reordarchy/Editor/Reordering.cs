using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Reordarchy
{

    public enum Direction { UP = -1, DOWN = 1 }

    public static class Reordering
    {
        const string hierarchyWindowName = "Hierarchy";

        public static bool hierarchyIsFocused => (EditorWindow.focusedWindow.titleContent.text == hierarchyWindowName);

        public static EditorWindow GetHierarchyWindow()
        {
            if (hierarchyIsFocused) { return EditorWindow.focusedWindow; }

            EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window.titleContent.text == hierarchyWindowName) { return window; }
            }

            return null;//TODO: handle null
        }

        public static void Unparent() => Unparent(GetTopSelected());
        public static void Unparent(Transform[] targets)
        {
            if (targets.Length == 0) { return; }

            for (int i = targets.Length - 1; i >= 0; i--)
            {
                var target = targets[i];
                if (target.parent != null)
                {
                    int parentSiblingIndex = target.parent.GetSiblingIndex();
                    Undo.SetTransformParent(target, target.parent.parent, "unparent");
                    target.SetSiblingIndex(parentSiblingIndex + 1);
                }
            }
        }

        public static void Parent() => Parent(GetTopSelected());
        public static void Parent(Transform[] targets)
        {
            if (targets.Length == 0) { return; }
            Transform parent = targets[0].parent;
            bool areSiblings = true;
            foreach (var t in targets)
            {
                if (t.parent != parent) { areSiblings = false; }
            }

            if (areSiblings)
            {
                targets = SortBySiblingIndex(targets);
                var firstTransform = targets[0];
                Transform newParent;
                if (firstTransform.parent == null)
                {
                    newParent = GetSiblingAboveRoot(firstTransform);
                }
                else
                {
                    newParent = GetSiblingAbove(firstTransform);
                }


                if (newParent != null)
                {
                    foreach (var target in targets)
                    {
                        Undo.SetTransformParent(target, newParent, "Parenting");
                        ExpandChildren(newParent);
                    }
                }
            }


        }

        public static void Reorder(Direction direction) => Reorder(GetTopSelected(), direction);
        public static void Reorder(Transform[] targets, Direction direction)
        {
            if (targets.Length == 0) { return; }
            foreach (var target in targets) { Debug.Assert(target != null); }

            targets = SortBySiblingIndex(targets);

            Transform firstParent = targets[0].parent;
            foreach (var target in targets)
            {
                if (target.parent != firstParent)
                {
                    return;
                }
            }

            //Todo: Fix this for groups
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i]; //reverse loop depending on reordering direction:
                if (direction == Direction.DOWN) { target = targets[targets.Length - 1 - i]; }

                if (target.parent?.childCount == 1) { break; }//skip if only child.

                int siblingIndex = target.GetSiblingIndex();

                if (direction == Direction.UP && siblingIndex == 0) { break; }
                if (direction == Direction.DOWN)
                {
                    if (siblingIndex == target.parent?.childCount - 1) { break; }
                    if (target.parent == null && IsLastSibling(siblingIndex)) { break; }
                }

                //Register undo that works for both root objects and children:
                Undo.SetTransformParent(target, target.parent, $"Reorder {direction}");

                foreach (var t in targets)
                {
                    if (t.GetSiblingIndex() == siblingIndex + (int)direction) { break; }
                }
                target.SetSiblingIndex(siblingIndex + (int)direction);
            }
        }

        private static void ExpandChildren(Transform target)
        {
            SetExpandedRecursive(target.gameObject, true);
            for (int i = 0; i < target.childCount; i++)
            {
                SetExpandedRecursive(target.GetChild(i).gameObject, false);
            }
            //Todo: Expand non-recursively.
        }

        public static void SetExpandedRecursive(GameObject go, bool expand) //Source: http://answers.unity.com/comments/858132/view.html
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var method = type.GetMethod("SetExpandedRecursive");
            method.Invoke(GetHierarchyWindow(), new object[] { go.GetInstanceID(), expand });
        }

        private static bool IsLastSibling(int siblingIndex)
        {
            //Todo: find a better way?
            //How does this interact with multiple scenes loaded?
            Transform dummy = new GameObject("Dummy").transform;
            dummy.parent = null;
            dummy.SetAsLastSibling();
            bool isLast = (dummy.GetSiblingIndex() == siblingIndex + 1);
            MonoBehaviour.DestroyImmediate(dummy.gameObject);
            return isLast;
        }

        private static Transform GetSiblingAbove(Transform target)
        {
            Debug.Assert(target.parent != null);

            int siblingIndex = target.GetSiblingIndex();
            List<Transform> transforms = new List<Transform>();
            target.parent.GetComponentsInChildren<Transform>(true, transforms);

            List<Transform> sorted = new List<Transform>();

            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].parent == target.parent && transforms[i].GetSiblingIndex() == siblingIndex - 1)
                {
                    return transforms[i];
                }
            }
            return null;
        }
        private static Transform GetSiblingAboveRoot(Transform target)
        {
            Debug.Assert(target.parent == null);

            int siblingIndex = target.GetSiblingIndex();
            Transform[] transforms = (Transform[])MonoBehaviour.FindObjectsOfType(typeof(Transform));

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].parent == null && transforms[i].GetSiblingIndex() == siblingIndex - 1)
                {
                    return transforms[i];
                }
            }

            return null;
        }

        public static Transform[] GetTopSelected() => SortBySiblingIndex(Selection.GetFiltered<Transform>(SelectionMode.TopLevel));

        public static Transform[] SortBySiblingIndex(Transform[] transforms)
        {
            //Todo: Does this even work properly?
            var list = new List<Transform>(transforms);
            list.Sort((a, b) =>
            {
                return a.GetSiblingIndex() - b.GetSiblingIndex();
            });
            return list.ToArray();
        }

        static Transform[] CullChildren(Transform[] transforms)
        {
            List<Transform> culled = new List<Transform>(transforms);
            for (int i = 0; i < transforms.Length; i++)
            {
                bool hasRelation = false;
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (IsRecursiveChild(transforms[i], transforms[j]))
                    {
                        hasRelation = true;
                        Debug.Log($"culled {transforms[i].name}");
                    }
                }

                if (hasRelation) { culled.Remove(culled[i]); }
            }
            return culled.ToArray();
        }
        static bool IsRecursiveChild(Transform child, Transform target)
        {
            Transform parent = child.parent;
            for (int i = 0; i < 200; i++)
            {
                if (parent == null) { break; }

                if (parent == target)
                {
                    return true;
                }
                else
                {
                    parent = parent.parent;
                }
            }
            return false;
        }

        static Transform[] CullDeeper(Transform[] transforms) //Not sure if I need this anymore. 
        {
            Dictionary<Transform, int> depths = new Dictionary<Transform, int>();
            Dictionary<Transform, int> siblingIndices = new Dictionary<Transform, int>();

            int minDepth = -1;

            foreach (var t in transforms)
            {
                int depth = 0;
                Transform parent = t.parent;
                while (parent != null)
                {
                    parent = parent.parent;
                    depth += 1;
                }
                if (minDepth == -1 || depth < minDepth)
                {
                    minDepth = depth;
                }

                depths.Add(t, depth);
                siblingIndices.Add(t, t.GetSiblingIndex());
            }

            List<Transform> output = new List<Transform>();

            foreach (var t in transforms)
            {
                if (depths[t] == minDepth)
                {
                    output.Add(t);
                }
            }

            return output.ToArray();
        }
    }

}
