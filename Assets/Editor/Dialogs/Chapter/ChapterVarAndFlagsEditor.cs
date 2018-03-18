﻿using UnityEngine;
using UnityEditor;
using System.Collections;

using uAdventure.Core;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;
using uAdventure.Runner;

namespace uAdventure.Editor
{
	public class ChapterVarAndFlagsEditor : EditorWindow, DialogReceiverInterface
    {
        private enum WindowType
        {
            FLAGS,
            VARS
        }

        private Texture2D flagsTex = null;
        private Texture2D varTex = null;

        private WindowType openedWindow;

        public static ChapterVarAndFlagsEditor s_DrawerParametersMenu;
        private static long s_LastClosedTime;

        private ColumnList variablesAndFlagsList;
        private string filter;

        internal static bool ShowAtPosition(Rect buttonRect)
        {
            long num = DateTime.Now.Ticks / 10000L;
            if (num >= ChapterVarAndFlagsEditor.s_LastClosedTime + 50L)
            {
                if (Event.current != null)
                {
                    Event.current.Use();
                }
                if (ChapterVarAndFlagsEditor.s_DrawerParametersMenu == null)
                {
                    ChapterVarAndFlagsEditor.s_DrawerParametersMenu = ScriptableObject.CreateInstance<ChapterVarAndFlagsEditor>();
                }
                ChapterVarAndFlagsEditor.s_DrawerParametersMenu.Init(buttonRect);

                return true;
            }
            return false;
        }

        private void Init(Rect buttonRect)
        {
            buttonRect.position = GUIUtility.GUIToScreenPoint(buttonRect.position);
            float y = 305f;
            Vector2 windowSize = new Vector2(300f, y);
            base.ShowAsDropDown(buttonRect, windowSize);
        }


        [MenuItem("eAdventure/Flags and variables")]
		public static void Init()
		{
			var window = GetWindow<ChapterVarAndFlagsEditor> ();
			window.Show();
		}

        bool inited = false;

        public void OnEnable()
        {
            inited = false;
			if(!flagsTex)
				flagsTex = (Texture2D)Resources.Load("EAdventureData/img/icons/flag16", typeof(Texture2D));
			if(!varTex)
            	varTex = (Texture2D)Resources.Load("EAdventureData/img/icons/vars", typeof(Texture2D));

            variablesAndFlagsList = new ColumnList(new List<int>(), typeof(int))
            {
                Columns = new List<ColumnList.Column>()
                {
                    new ColumnList.Column(), new ColumnList.Column() { SizeOptions = new GUILayoutOption[] { GUILayout.Width(80) } }
                },
                drawCell = (rect, row, column, isActive, isFocused) =>
                {
                    // The list is only storing indexes
                    var index = (int)variablesAndFlagsList.list[row];
                    var elem = "";
                    switch (openedWindow)
                    {
                        case WindowType.FLAGS: elem = Controller.Instance.VarFlagSummary.getFlag(index); break;
                        case WindowType.VARS: elem = Controller.Instance.VarFlagSummary.getVar(index); break;
                    }

                    switch (column)
                    {
                        case 0:
                            EditorGUI.LabelField(rect, elem);
                            break;
                        case 1:
                            object value = 0;
                            if (Application.isPlaying)
                            {
                                switch (openedWindow)
                                {
                                    case WindowType.FLAGS: value = Game.Instance.GameState.checkFlag(elem) == 1 ? "inactive" : "active"; break;
                                    case WindowType.VARS: value = Game.Instance.GameState.getVariable(elem); break;
                                }
                            }
                            else
                            {
                                switch (openedWindow)
                                {
                                    case WindowType.FLAGS: value = Controller.Instance.VarFlagSummary.getFlagReferences(index); break;
                                    case WindowType.VARS: value = Controller.Instance.VarFlagSummary.getVarReferences(index); break;
                                }
                            }
                            EditorGUI.LabelField(rect, value.ToString());
                            break;
                    }
                },
                onRemoveCallback = OnDeleteClicked,
                onAddCallback = OnAddCliked,
                draggable = false
            };
        }

        private void Update()
        {
        }

        void OnGUI()
        {
            var windowWidth = position.width;
            var windowHeight = position.height;

            // Initialization
            if (!inited)
            {
                if (Controller.Instance.Loaded)
                {
                    RefreshList();
                    inited = true;
                }
            }
            
            /*
            * Upper buttons
            */
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            openedWindow = (WindowType) GUILayout.Toolbar((int)openedWindow, new GUIContent[] { new GUIContent(TC.get("Flags.Title"), flagsTex), new GUIContent(TC.get("Vars.Title"), varTex) });
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            filter = EditorGUILayout.TextField("Filter", filter);
            if (EditorGUI.EndChangeCheck())
                RefreshList();

            var height = windowHeight - GUILayoutUtility.GetLastRect().max.y - 90f;
            /*
            * Content part
            */
            switch (openedWindow)
            {
                case WindowType.FLAGS:
                    variablesAndFlagsList.Columns[0].Text = TC.get("Flags.FlagName");
                    variablesAndFlagsList.Columns[1].Text = Application.isPlaying ? TC.get("Conditions.Flag.State") : TC.get("Flags.FlagReferences");
                    break;
                case WindowType.VARS:
                    variablesAndFlagsList.Columns[0].Text = TC.get("Vars.VarName");
                    variablesAndFlagsList.Columns[1].Text = Application.isPlaying ? TC.get("Conditions.Var.Value") : TC.get("Vars.VarReferences");
                    break;
            }
            var playing = Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
            variablesAndFlagsList.displayAddButton = !playing;
            variablesAndFlagsList.displayRemoveButton = !playing;

            variablesAndFlagsList.DoList(height);
        }

        void OnAddCliked(ReorderableList reorderableList)
        {
            switch (openedWindow)
            {
                case WindowType.FLAGS:
                    CreateInstance<ChapterFlagNameInputPopup>().Init(this, "IdFlag");
                    break;
                case WindowType.VARS:
                    CreateInstance<ChapterVarNameInputPopup>().Init(this, "IdVar");
                    break;
            }
        }

        void OnDeleteClicked(ReorderableList reorderableList)
        {
            if (reorderableList.index >= 0)
            {
                var selected = (int)reorderableList.list[reorderableList.index];
                var summary = Controller.Instance.VarFlagSummary;
                switch (openedWindow)
                {
                    case WindowType.FLAGS:
                        summary.deleteFlag(Controller.Instance.VarFlagSummary.getFlag(selected));
                        break;
                    case WindowType.VARS:
                        summary.deleteVar(Controller.Instance.VarFlagSummary.getVar(selected));
                        break;
                }
            }
            RefreshList();
        }

        public void OnDialogOk(string message, object workingObject = null, object workingObjectSecond = null)
        {
            var summary = Controller.Instance.VarFlagSummary;
            if (workingObject is ChapterFlagNameInputPopup)     summary.addFlag(message);
            else if (workingObject is ChapterVarNameInputPopup) summary.addVar(message);

            RefreshList();
        }

        private void RefreshList()
        {
            var summary = Controller.Instance.VarFlagSummary;
            Func<int, bool> filterFunc = (_) => true;
            IEnumerable<int> indexes = new List<int>();
            switch (openedWindow)
            {
                case WindowType.FLAGS:
                    indexes = Enumerable.Range(0, summary.getFlagCount());
                    filterFunc = (i) => summary.getFlag(i).ToLowerInvariant().Contains(filter.ToLowerInvariant());
                    break;
                case WindowType.VARS:
                    indexes = Enumerable.Range(0, summary.getVarCount());
                    filterFunc = (i) => summary.getVar(i).ToLowerInvariant().Contains(filter.ToLowerInvariant());
                    break;
            }
            variablesAndFlagsList.list = string.IsNullOrEmpty(filter) ? indexes.ToList() : indexes.Where(filterFunc).ToList();
            this.Repaint();
        }

        public void OnDialogCanceled(object workingObject = null)
        {
        }
    }
}