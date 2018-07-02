﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

using uAdventure.Core;
using System.Linq;

namespace uAdventure.Editor
{
    public class ChangeOrientationEffectEditor : EffectEditor
    {
        private readonly Type[] types = { typeof(NPC) };
        private readonly int[] orientationValues;
        private readonly string[] orientationTexts;

        public bool Collapsed { get; set; }
        private Rect window = new Rect(0, 0, 300, 0);
        public Rect Window
        {
            get
            {
                return Collapsed ? new Rect(window.x, window.y, 50, 30) : window;
            }
            set
            {
                window = Collapsed ? new Rect(value.x, value.y, window.width, window.height) : value;
            }
        }

        private ChangeOrientationEffect effect;

        public ChangeOrientationEffectEditor()
        {
            var ids = Controller.Instance.IdentifierSummary.combineIds(types);
            this.effect = new ChangeOrientationEffect(ids.FirstOrDefault(), Runner.Orientation.S);

            var orientations = Enum.GetValues(typeof(Runner.Orientation));
            orientationValues = orientations.Cast<int>().ToArray();
            orientationTexts = orientations.Cast<Runner.Orientation>().Select(s => "Orientation." + s.ToString()).ToArray();
        }

        public void draw()
        {
            var ids = Controller.Instance.VarFlagSummary.getFlags();
            effect.setTargetId(ids[EditorGUILayout.Popup(TC.get("Condition.FlagID"), Array.IndexOf(ids, effect.getTargetId()), ids)]);

            EditorGUI.BeginChangeCheck();
            var orientationLabel = TC.get("ElementReference.Orientation");
            var translatedTexts = orientationTexts.Select(TC.get).ToArray();
            var newOrientation = (Runner.Orientation)EditorGUILayout.IntPopup(orientationLabel, (int)effect.GetOrientation(), translatedTexts, orientationValues);
            if (EditorGUI.EndChangeCheck())
            {
                effect.SetOrientation(newOrientation);
            }
        }

        public IEffect Effect { get { return effect; } set { effect = value as ChangeOrientationEffect; } }
        public string EffectName { get { return TC.get("ActivateEffect.Title"); } }

        public bool Usable
        {
            get
            {
                return Controller.Instance.IdentifierSummary.combineIds(types).Any();
            }
        }

        public EffectEditor clone() { return new ChangeOrientationEffectEditor(); }

        public bool manages(IEffect c)
        {

            return c.GetType() == effect.GetType();
        }
    }
}