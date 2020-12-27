using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Incant
{
    public class Scam
    {
        static bool shouldTween = true;
        static int turnIncrement = 45;
        static SceneView view => SceneView.lastActiveSceneView;
        const string sub = "Incant/Scene View/";

        #region Menu Items
        [MenuItem(sub + "Top view")] static void ViewTop() => Rotate(Views.Top);
        [MenuItem(sub + "Bottom view")] static void ViewBottom() => Rotate(Views.Bottom);
        [MenuItem(sub + "Left view")] static void ViewLeft() => Rotate(Views.Left);
        [MenuItem(sub + "Right view")] static void ViewRight() => Rotate(Views.Right);
        [MenuItem(sub + "Front view")] static void ViewFront() => Rotate(Views.Front);
        [MenuItem(sub + "Back view")] static void ViewBack() => Rotate(Views.Back);
        [MenuItem(sub + "Orthographic")] static void ToggleOrtho() => view.orthographic = !view.orthographic;
        [MenuItem(sub + "Turn left")] static void RotateLeft() => RotateEuler(0, turnIncrement, 0);
        [MenuItem(sub + "Turn right")] static void RotateRight() => RotateEuler(0, -turnIncrement, 0);
        [MenuItem(sub + "Tilt down")] static void RotateDown() => Tilt(-90, turnIncrement);
        [MenuItem(sub + "Tilt up")] static void RotateUp() => Tilt(90, turnIncrement);
        [MenuItem(sub + "Zoom out")] static void ZoomOut() => Zoom(2);
        [MenuItem(sub + "Zoom In")] static void ZoomIn() => Zoom(0.5f);
        #endregion

        public static class Views
        {
            public static readonly Quaternion Top = Quaternion.Euler(90, 0, 0);
            public static readonly Quaternion Bottom = Quaternion.Euler(-90, 0, 0);
            public static readonly Quaternion Left = Quaternion.Euler(0, 90, 0);
            public static readonly Quaternion Right = Quaternion.Euler(0, -90, 0);
            public static readonly Quaternion Front = Quaternion.Euler(0, 0, 0);
            public static readonly Quaternion Back = Quaternion.Euler(0, 180, 0);
        }

        public static void Zoom(float value)
        {
            SceneView.RepaintAll();//Why repaint first?
            view.size *= value;
        }

        public static void Tilt(float target, float increment)
        {
            var vector = new Vector3(target, view.rotation.eulerAngles.y, view.rotation.eulerAngles.z);
            Rotate(Quaternion.RotateTowards(view.rotation, Quaternion.Euler(vector), increment));
        }
        static void RotateEuler(float x, float y, float z) => RotateEuler(new Vector3(x, y, z));
        static void RotateEuler(Vector3 euler) => Rotate(Quaternion.Euler(view.rotation.eulerAngles + euler));

        public static void Rotate(Quaternion newRotation)
        {
            if (shouldTween)
            {
                view.LookAt(view.pivot, newRotation);
            }
            else
            {
                view.LookAtDirect(view.pivot, newRotation);
            }
            view.Repaint();
        }
    }
}
