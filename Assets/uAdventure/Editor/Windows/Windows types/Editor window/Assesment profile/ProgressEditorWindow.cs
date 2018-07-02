﻿using UnityEngine;
using UnityEditor;

using uAdventure.Core;
using System.Collections.Generic;
using System.Linq;

namespace uAdventure.Editor
{
    public class ProgressEditorWindow : EditorWindow
    {
        private static ProgressEditorWindow editor;
        private readonly string[] milestoneTypeTexts = { "Analytics.Milestone.Progress.Sum", "Analytics.Milestone.Progress.Max" };
        
        private DataControlList progressList;
        private ProgressDataControl progress;
        private ColumnList.Column progressColumn;

        private Completable.Progress.ProgressType currentMode = Completable.Progress.ProgressType.SUM;

        public void Init(ProgressDataControl progress)
        {
            this.progress = progress;

            progressList = new DataControlList()
            {
                RequestRepaint = Repaint,
                Columns = new List<ColumnList.Column>()
                {
                    new ColumnList.Column()
                    {
                        Text = "Analytics.Milestone.Time".Traslate()
                    }
                },
                drawCell = (rect, row, column, isActive, isFocused) =>
                {
                    var milestone = progressList.list[row] as MilestoneDataControl;
                    switch (column)
                    {
                        default:
                            if (GUI.Button(rect, milestone.getContent().ToString()))
                            {
                                MilestoneEditorWindow.ShowMilestoneEditor(rect, milestone);
                            }
                            break;
                        case 1:
                            milestone.setProgress(EditorGUI.Slider(rect, milestone.getProgress(), 0, 1));
                            break;
                    }
                }
            };

            progressColumn = new ColumnList.Column()
            {
                Text = "Analytics.Completable.Progress".Traslate()
            };

        }
        
        public void OnGUI()
        {
            GUILayout.Label("Analytics.Milestone.Progress".Traslate());

            progress.setType((Completable.Progress.ProgressType)EditorGUILayout.Popup((int)progress.getType(), milestoneTypeTexts.Traslate()));

            if(progress.getType() != currentMode)
            {
                currentMode = progress.getType();
                switch (currentMode)
                {
                    default: // ProgressType.SUM:
                        if (progressList.Columns.Contains(progressColumn))
                        {
                            progressList.Columns.Remove(progressColumn);
                        }
                        break;
                    case Completable.Progress.ProgressType.SPECIFIC:
                        if (!progressList.Columns.Contains(progressColumn))
                        {
                            progressList.Columns.Add(progressColumn);
                        }
                        break;
                }
            }

            progressList.SetData(progress, (p) => (p as ProgressDataControl).getMilestones().ConvertAll(m => m as DataControl));
            progressList.DoList(position.height - 55);
        }


        public static ProgressEditorWindow Create(ProgressDataControl progress)
        {
            editor = ScriptableObject.CreateInstance<ProgressEditorWindow>();
            editor.Init(progress);
            return editor;
        }


        public static void ShowProgressEditor(Rect rect, ProgressDataControl progress)
        {
            var window = ProgressEditorWindow.Create(progress);
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);
            window.ShowAsDropDown(rect, new Vector2(Mathf.Max(rect.width, 250), 300));
        }
    }
}