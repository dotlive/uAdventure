﻿using UnityEngine;
using System.Collections.Generic;

using uAdventure.Core;
using System;
using UnityEditorInternal;
using UnityEditor;
using System.Linq;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Core.Geometry;

namespace uAdventure.Editor
{
    [EditorWindowExtension(10, typeof(Scene))]
    public class ScenesWindow : DataControlListEditorWindowExtension
    {
        private enum ScenesWindowType
        {
            ActiveAreas,
            Appearance,
            Documentation,
            ElementRefrence,
            Exits,
            Barriers,
            PlayerMovement
        }

        private static ScenesWindowType openedWindow = ScenesWindowType.Appearance;
        private static ScenesWindowActiveAreas scenesWindowActiveAreas;
        private static ScenesWindowAppearance scenesWindowAppearance;
        private static ScenesWindowDocumentation scenesWindowDocumentation;
        private static ScenesWindowElementReference scenesWindowElementReference;
        private static ScenesWindowExits scenesWindowExits;
        private static ScenesWindowBarriers scenesWindowBarriers;
        private static ScenesWindowPlayerMovement scenesWindowPlayerMovement;

        private static ChapterPreview chapterPreview;

        private static List<bool> toggleList;

        private static GUISkin selectedButtonSkin;
        private static GUISkin defaultSkin;

        private SceneEditor sceneEditor;

        private List<KeyValuePair<string, ScenesWindowType>> tabs;

        DataControlList sceneList;

        public ScenesWindow(Rect rect, GUIStyle style, params GUILayoutOption[] options)
            : base(rect, new GUIContent(TC.get("Element.Name1")), style, options)
        {

            var content = new GUIContent();

            new RectangleComponentEditor(Rect.zero, new GUIContent(""), style);

            // Button
            content.image = (Texture2D) Resources.Load("EAdventureData/img/icons/scenes", typeof(Texture2D));
            content.text = TC.get("Element.Name1");
            ButtonContent = content;

            sceneEditor = new SceneEditor();

            RequestRepaint repaint = () => Repaint();

            // Windows
            scenesWindowActiveAreas = new ScenesWindowActiveAreas(rect,
                new GUIContent(TC.get("ActiveAreasList.Title")), "Window", sceneEditor);
            scenesWindowActiveAreas.OnRequestRepaint = repaint;
            scenesWindowAppearance = new ScenesWindowAppearance(rect, new GUIContent(TC.get("Scene.LookPanelTitle")),
                "Window", sceneEditor);
            scenesWindowAppearance.OnRequestRepaint = repaint;
            scenesWindowDocumentation = new ScenesWindowDocumentation(rect,
                new GUIContent(TC.get("Scene.DocPanelTitle")), "Window", sceneEditor);
            scenesWindowDocumentation.OnRequestRepaint = repaint;
            scenesWindowElementReference = new ScenesWindowElementReference(rect,
                new GUIContent(TC.get("ItemReferencesList.Title")), "Window", sceneEditor);
            scenesWindowElementReference.OnRequestRepaint = repaint;
            scenesWindowExits = new ScenesWindowExits(rect, new GUIContent(TC.get("Element.Name3")), "Window", sceneEditor);
            scenesWindowExits.OnRequestRepaint = repaint;

            chapterPreview = new ChapterPreview(rect, new GUIContent(""), "Window");
            chapterPreview.OnRequestRepaint = repaint;
            chapterPreview.OnSelectElement += (scene) =>
            {
                var index = Controller.Instance.SelectedChapterDataControl.getScenesList().getScenes().FindIndex(s => s == scene as SceneDataControl);
                ShowItemWindowView(index);
            };

            scenesWindowBarriers = new ScenesWindowBarriers(rect, new GUIContent(TC.get("BarriersList.Title")), "Window", sceneEditor);
            scenesWindowPlayerMovement = new ScenesWindowPlayerMovement(rect, new GUIContent(TC.get("Trajectory.Title")), "Window", sceneEditor);

            tabs = new List<KeyValuePair<string, ScenesWindowType>>()
                {
                    new KeyValuePair<string, ScenesWindowType>(TC.get("Scene.LookPanelTitle"),      ScenesWindowType.Appearance),
                    new KeyValuePair<string, ScenesWindowType>(TC.get("Scene.DocPanelTitle"),       ScenesWindowType.Documentation),
                    new KeyValuePair<string, ScenesWindowType>(TC.get("ItemReferencesList.Title"),  ScenesWindowType.ElementRefrence),
                    new KeyValuePair<string, ScenesWindowType>(TC.get("ActiveAreasList.Title"),     ScenesWindowType.ActiveAreas),
                    new KeyValuePair<string, ScenesWindowType>(TC.get("Element.Name3"),             ScenesWindowType.Exits)
                };
            if (Controller.Instance.playerMode() == DescriptorData.MODE_PLAYER_3RDPERSON)
            {
                tabs.Add(new KeyValuePair<string, ScenesWindowType>(TC.get("BarriersList.Title"), ScenesWindowType.Barriers));
                tabs.Add(new KeyValuePair<string, ScenesWindowType>(TC.get("Trajectory.Title"), ScenesWindowType.PlayerMovement));
            }

            selectedButtonSkin = (GUISkin)Resources.Load("Editor/ButtonSelected", typeof(GUISkin));
        }


        public override void Draw(int aID)
        {
            dataControlList.SetData(Controller.Instance.SelectedChapterDataControl.getScenesList(),
                sceneList => (sceneList as ScenesListDataControl).getScenes().Cast<DataControl>().ToList());

            // Show information of concrete item
            if (GameRources.GetInstance().selectedSceneIndex != -1)
            {
                var scene = Controller.Instance.SelectedChapterDataControl.getScenesList().getScenes()[GameRources.GetInstance().selectedSceneIndex];
               

                sceneEditor.Components = EditorWindowBase.Components;
                var allElements = new List<DataControl>();
                allElements.AddRange(scene.getReferencesList().getAllReferencesDataControl().FindAll(elem => elem.getErdc() != null).ConvertAll(elem => elem.getErdc() as DataControl));
                allElements.AddRange(scene.getActiveAreasList().getActiveAreas().Cast<DataControl>());
                allElements.AddRange(scene.getExitsList().getExits().Cast<DataControl>());

                if (Controller.Instance.playerMode() == DescriptorData.MODE_PLAYER_3RDPERSON)
                {
                    allElements.AddRange(scene.getBarriersList().getBarriers().Cast<DataControl>());
                    var hasTrajectory = scene.getTrajectory().hasTrajectory();
                    if (hasTrajectory)
                    {
                        allElements.AddRange(scene.getTrajectory().getNodes().Cast<DataControl>());
                        allElements.Add(scene.getTrajectory());
                    }
                    else
                        allElements.Add(Controller.Instance.SelectedChapterDataControl.getPlayer());

                }
                sceneEditor.elements = allElements;

                /**
                 UPPER MENU
                */
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                openedWindow = tabs[GUILayout.Toolbar(tabs.FindIndex(t => t.Value == openedWindow), tabs.ConvertAll(t => t.Key).ToArray(), GUILayout.ExpandWidth(false))].Value;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                switch (openedWindow)
                {
                    case ScenesWindowType.ActiveAreas:
                        scenesWindowActiveAreas.Rect = this.Rect;
                        scenesWindowActiveAreas.Draw(aID);
                        break;
                    case ScenesWindowType.Appearance:
                        scenesWindowAppearance.Rect = this.Rect;
                        scenesWindowAppearance.Draw(aID);
                        break;
                    case ScenesWindowType.Documentation:
                        scenesWindowDocumentation.Rect = this.Rect;
                        scenesWindowDocumentation.Draw(aID);
                        break;
                    case ScenesWindowType.ElementRefrence:
                        scenesWindowElementReference.Rect = this.Rect;
                        scenesWindowElementReference.Draw(aID);
                        break;
                    case ScenesWindowType.Exits:
                        scenesWindowExits.Rect = this.Rect;
                        scenesWindowExits.Draw(aID);
                        break;
                    case ScenesWindowType.Barriers:
                        scenesWindowBarriers.Rect = this.Rect;
                        scenesWindowBarriers.Draw(aID);
                        break;
                    case ScenesWindowType.PlayerMovement:
                        scenesWindowPlayerMovement.Rect = this.Rect;
                        scenesWindowPlayerMovement.Draw(aID);
                        break;
                }
            }
            // Show information of whole scenes (global-scene view)
            else
            {
                chapterPreview.Rect = this.Rect;
                chapterPreview.Draw(aID);
            }
        }

        public override void OnDrawMoreWindows()
        {
            if (GameRources.GetInstance().selectedSceneIndex != -1)
            {
                switch (openedWindow)
                {
                    case ScenesWindowType.ActiveAreas:
                        scenesWindowActiveAreas.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.Appearance:
                        scenesWindowAppearance.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.Documentation:
                        scenesWindowDocumentation.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.ElementRefrence:
                        scenesWindowElementReference.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.Exits:
                        scenesWindowExits.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.Barriers:
                        scenesWindowBarriers.OnDrawMoreWindows();
                        break;
                    case ScenesWindowType.PlayerMovement:
                        scenesWindowPlayerMovement.OnDrawMoreWindows();
                        break;
                }
            }
            else
            {
                chapterPreview.OnDrawMoreWindows();
            }

        }

        void OnWindowTypeChanged(ScenesWindowType type_)
        {
            openedWindow = type_;
        }


        // Two methods responsible for showing right window content 
        // - concrete item info or base window view
        public void ShowBaseWindowView()
        {
            GameRources.GetInstance().selectedSceneIndex = -1;
        }

        public void ShowItemWindowView(int s)
        {
            GameRources.GetInstance().selectedSceneIndex = s;
        }

        // ---------------------------------------------
        //         Reorderable List Handlers
        // ---------------------------------------------

        protected override void OnSelect(ReorderableList r)
        {
            ShowItemWindowView(r.index);
        }

        protected override void OnButton()
        {
            ShowBaseWindowView();

            dataControlList.SetData(Controller.Instance.SelectedChapterDataControl.getScenesList(),
                sceneList => (sceneList as ScenesListDataControl).getScenes().Cast<DataControl>().ToList());
        }

        private class ChapterPreview : PreviewLayoutWindow, ProjectConfigDataConsumer
        {
            private const float SceneScaling = 0.2f;
            private const float SpaceWidth = 800f;
            private const float SpaceHeight = 600f;
            private Rect space;

            private Dictionary<string, Color> sceneColors;

            private Dictionary<string, Vector2> positions;
            private Dictionary<string, Texture2D> images;
            private Dictionary<string, Vector2> sizes;

            public delegate void OnSelectElementDelegate(DataControl selected);
            public event OnSelectElementDelegate OnSelectElement;

            private DataControlList sceneList;

            public ChapterPreview(Rect rect, GUIContent content, GUIStyle style, params GUILayoutOption[] options) : base(rect, content, style, options)
            {
                ProjectConfigData.addConsumer(this);

                sceneColors = new Dictionary<string, Color>();
                positions = new Dictionary<string, Vector2>();
                images = new Dictionary<string, Texture2D>();
                sizes = new Dictionary<string, Vector2>();

                // SceneList
                sceneList = new DataControlList()
                {
                    footerHeight = 10,
                    elementHeight = 20,
                    Columns = new List<ColumnList.Column>()
                    {
                        new ColumnList.Column()
                        {
                            Text =  TC.get("Element.Name1"),
                            SizeOptions = new GUILayoutOption[]{ GUILayout.ExpandWidth(true) }
                        },
                        new ColumnList.Column()
                        {
                            Text = "Open",
                            SizeOptions = new GUILayoutOption[]{ GUILayout.Width(250) }
                        }
                    },
                    drawCell = (cellRect, row, column, isActive, isFocused) =>
                    {
                        var scene = ((ScenesListDataControl)sceneList.DataControl).getScenes()[row];
                        switch (column)
                        {
                            case 0: GUI.Label(cellRect, scene.getId()); break;
                            case 1:
                                if (GUI.Button(cellRect, TC.get("GeneralText.Edit")))
                                    GameRources.GetInstance().selectedSceneIndex = row;
                                break;
                        }
                    }
                };

            }

            protected override void DrawInspector()
            {
                var scenesListDataControl = Controller.Instance.ChapterList.getSelectedChapterDataControl().getScenesList();
                sceneList.SetData(scenesListDataControl, data => (data as ScenesListDataControl).getScenes().Cast<DataControl>().ToList());

                sceneList.DoList(160);

            }
            private Rect prevSpace;
            private bool inited = false;

            protected override void DrawPreviewHeader()
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal("preToolbar");
                GUILayout.Label(TC.get("ImageAssets.Preview"), "preToolbar", GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Layout", "preButton"))
                {
                    inited = false;
                }
                GUILayout.EndHorizontal();
            }
            public override void DrawPreview(Rect rect)
            {
                space = rect.AdjustToRatio(SpaceWidth, SpaceHeight);
                if (!inited && Event.current.type != EventType.Layout)
                {
                    Layout();
                    inited = true;
                }
                    
                if (space != prevSpace)
                {
                    prevSpace = space;
                }
                foreach (var scene in Controller.Instance.ChapterList.getSelectedChapterDataControl().getScenesList().getScenes())
                {
                    DrawScene(scene);
                    //DoSceneControl(scene);
                }

                foreach (var scene in Controller.Instance.ChapterList.getSelectedChapterDataControl().getScenesList().getScenes())
                {
                    foreach (var exit in scene.getExitsList().getExits())
                        DrawExit(scene, exit);
                }
            }

            public void updateData()
            {
                positions.Clear();
            }

            // AUX FUNCTIONS

            private void DrawScene(SceneDataControl scene)
            {
                var rect = AdaptToViewport(GetSceneRect(scene), space);

                switch (Event.current.type)
                {
                    case EventType.Repaint:
                        GUI.DrawTexture(rect, images[scene.getPreviewBackground()]);
                        if (sceneList.index != -1 && Controller.Instance.SelectedChapterDataControl.getScenesList().getScenes()[sceneList.index] == scene)
                        {
                            HandleUtil.DrawPolyLine(rect.ToPoints().ToArray(), true, Color.red);
                        }
                        break;
                }

                EditorGUI.DropShadowLabel(new Rect(rect.position - new Vector2(20,0), rect.size), scene.getId());

                var prevHot = GUIUtility.hotControl;
                EditorGUI.BeginChangeCheck();
                rect = HandleUtil.HandleRectMovement(scene.GetHashCode(), rect);
                if (EditorGUI.EndChangeCheck())
                {
                    rect = RevertFromViewport(rect, space);
                    positions[scene.getId()] = rect.position;
                }
                if (GUIUtility.hotControl != prevHot)
                {
                    sceneList.index = Controller.Instance.SelectedChapterDataControl.getScenesList().getScenes().IndexOf(scene);
                }

            }

            private void DrawExit(SceneDataControl scene, ExitDataControl exit)
            {
                var polygon = AdaptToViewport(GetExitArea(scene, exit), space);

                var c = sceneColors[scene.getId()];
                c = new Color(c.r, c.g, c.b, 0.8f);
                HandleUtil.DrawPolygon(polygon, c);
                var scenes = Controller.Instance.SelectedChapterDataControl.getScenesList();
                var index = scenes.getSceneIndexByID(exit.getNextSceneId());

                // If the exit points to a cutscene it normally is out of the array
                if (index < 0 || index >= scenes.getScenes().Count)
                    return;

                var nextScene = scenes.getScenes()[index];
                var sceneRect = AdaptToViewport(GetSceneRect(nextScene), space);
                
                Vector2 origin = Center(polygon), destination = Vector2.zero;
                if (exit.hasDestinyPosition())
                {
                    destination = new Vector2(exit.getDestinyPositionX(), exit.getDestinyPositionY());
                    destination.x = sceneRect.x + (destination.x / sizes[nextScene.getPreviewBackground()].x) * sceneRect.width;
                    destination.y = sceneRect.y + (destination.y / sizes[nextScene.getPreviewBackground()].y) * sceneRect.height;
                }
                else
                {
                    destination = Center(sceneRect.ToPoints().ToArray());
                }
                
                HandleUtil.DrawPolyLine(new Vector2[] { origin, destination }, false, sceneColors[scene.getId()], 4);

                DrawArrowCap(destination, (destination - origin), 15f);
            }

            private void DrawArrowCap(Vector2 point, Vector2 direction, float size)
            {
                var halfSide = size / Mathf.Tan(60 * Mathf.Deg2Rad);
                var rotatedVector = new Vector2(-direction.y, direction.x).normalized;
                var basePoint = point - (direction.normalized * size);

                Vector3[] capPoints = new Vector3[]
                {
                    point,
                    basePoint + rotatedVector * halfSide,
                    basePoint - rotatedVector * halfSide
                };
                Handles.BeginGUI();
                Handles.DrawAAConvexPolygon(capPoints);
                Handles.EndGUI();
            }

            private static Vector3 ToHSV(Color color)
            {
                System.Drawing.Color sColor = System.Drawing.Color.FromArgb((int) (color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
                float hue = sColor.GetHue();
                float saturation = sColor.GetSaturation();
                float lightness = sColor.GetBrightness();

                return new Vector3(hue, saturation, lightness);
            }

            private static Color FromHSV(Vector3 hsv)
            {
                float hue = hsv.x, saturation = hsv.y, lightness = hsv.z;

                int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
                double f = hue / 60 - Math.Floor(hue / 60);

                lightness = lightness * 255;
                int v = Convert.ToInt32(lightness);
                int p = Convert.ToInt32(lightness * (1 - saturation));
                int q = Convert.ToInt32(lightness * (1 - f * saturation));
                int t = Convert.ToInt32(lightness * (1 - (1 - f) * saturation));

                if (hi == 0)
                    return new Color(v / 255f, t / 255f, p / 255f);
                else if (hi == 1)
                    return new Color(q / 255f, v / 255f, p / 255f);
                else if (hi == 2)
                    return new Color(p / 255f, v / 255f, t / 255f);
                else if (hi == 3)
                    return new Color(p / 255f, q / 255f, v / 255f);
                else if (hi == 4)
                    return new Color(t / 255f, p / 255f, v / 255f);
                else
                    return new Color(v / 255f, p / 255f, q / 255f);
            }

            private Rect AdaptToViewport(Rect rect, Rect viewport)
            {
                // ??? PROFIT
                return rect.ToPoints().ToList().ConvertAll(p => AdaptToViewport(p, viewport)).ToArray().ToRect();
            }
            private Rect RevertFromViewport(Rect rect, Rect viewport)
            {
                // ??? PROFIT
                return rect.ToPoints().ToList().ConvertAll(p => RevertFromViewport(p, viewport)).ToArray().ToRect();
            }

            private Vector2[] AdaptToViewport(Vector2[] points, Rect viewport)
            {
                return points.ToList().ConvertAll(p => AdaptToViewport(p, viewport)).ToArray();
            }
            private Vector2[] RevertFromViewport(Vector2[] points, Rect viewport)
            {
                return points.ToList().ConvertAll(p => RevertFromViewport(p, viewport)).ToArray();
            }

            private Vector2 AdaptToViewport(Vector2 point, Rect viewport)
            {
                return viewport.position + new Vector2(point.x * viewport.width / SpaceWidth, point.y * viewport.height / SpaceHeight);
            }
            private Vector2 RevertFromViewport(Vector2 point, Rect viewport)
            {
                return new Vector2((point.x-viewport.position.x) * SpaceWidth / viewport.width, (point.y-viewport.position.y) * SpaceHeight / viewport.height);
            }

            private Rect GetSceneRect(SceneDataControl scene)
            {
                if (!this.positions.ContainsKey(scene.getId()))
                {
                    string id = GetScenePropertyId(Controller.Instance.SelectedChapterDataControl, scene);
                    string X = ProjectConfigData.getProperty(id + ".X");
                    string Y = ProjectConfigData.getProperty(id + ".Y");
                    positions.Add(scene.getId(), new Vector2(ExParsers.ParseDefault(X, 0), ExParsers.ParseDefault(Y, 0)));
                }

                var background = scene.getPreviewBackground();
                if (!sizes.ContainsKey(background))
                {
                    Texture2D scenePreview;
                    if (!images.ContainsKey(background))
                    {
                        scenePreview = AssetsController.getImageTexture(background);
                        images[background] = scenePreview;
                    }
                    else
                        scenePreview = images[background];

                    sizes.Add(background, new Vector2(scenePreview.width, scenePreview.height));
                    var pixel = scenePreview.GetPixel(scenePreview.width / 2, scenePreview.height / 2);
                    var color = ToHSV(new Color(1f - pixel.r, 1f - pixel.g, 1f - pixel.b));
                    color.y *= 2f;
                    color.z *= 1.5f;

                    sceneColors[scene.getId()] = FromHSV(color);
                }

                return new Rect(positions[scene.getId()], sizes[background] * SceneScaling);
            }

            private Vector2[] GetExitArea(SceneDataControl scene, ExitDataControl exit)
            {
                var holder = GetSceneRect(scene);
                var xRatio = holder.width / images[scene.getPreviewBackground()].width;
                var yRatio = holder.height / images[scene.getPreviewBackground()].height;

                Vector2[] polygon = null;
                var rectangle = exit.getRectangle();
                if (rectangle.isRectangular())
                {
                    polygon = new Rect(rectangle.getX(), rectangle.getY(), rectangle.getWidth(), rectangle.getHeight()).ToPoints();
                }
                else
                {
                    polygon = rectangle.getPoints().ToArray();
                }

                return polygon.ToList().ConvertAll(p => (new Vector2(p.x * xRatio, p.y * yRatio) + holder.position)).ToArray();
            }

            private Vector2 Center(Vector2[] polygon)
            {
                Vector2 sum = Vector2.zero;
                for (int i = 0; i < polygon.Length; i++)
                    sum += polygon[i];
                return sum / polygon.Length;
            }

            private string GetScenePropertyId(ChapterDataControl chapter, SceneDataControl scene)
            {
                var index = Controller.Instance.ChapterList.getChapters().IndexOf(chapter);
                return "Chapter" + index + "." + scene.getId();
            }

            private void Layout()
            {
                try
                {
                    var scenes = Controller.Instance.ChapterList.getSelectedChapterDataControl().getScenesList();
                    
                    // Layout algorithm
                    var settings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();
                    settings.MaxAspectRatioEccentricity = 1.6;
                    settings.NodeSeparation = 10;
                    settings.PackingMethod = PackingMethod.Compact;

                    // Graph
                    GeometryGraph graph = new GeometryGraph();
                    graph.BoundingBox = new Microsoft.Msagl.Core.Geometry.Rectangle(0, 0, SpaceWidth, SpaceHeight);
                    graph.UpdateBoundingBox();

                    Dictionary<SceneDataControl, Node> sceneToNode = new Dictionary<SceneDataControl, Node>();
                    Dictionary<Tuple<Node, Node>, bool> present = new Dictionary<Tuple<Node, Node>, bool>();

                    foreach (var scene in scenes.getScenes())
                    {
                        sizes.Remove(scene.getPreviewBackground());
                        var rect = GetSceneRect(scene);
                        var node = new Node(CurveFactory.CreateRectangle(rect.width, rect.height, new Point()), scene.getId());
                        graph.Nodes.Add(node);
                        sceneToNode.Add(scene, node);
                    }

                    foreach (var scene in scenes.getScenes())
                    { 
                        var node = sceneToNode[scene];
                        foreach (var exit in scene.getExitsList().getExits())
                        {
                            var index = scenes.getSceneIndexByID(exit.getNextSceneId());

                            // If the exit points to a cutscene it normally is out of the array
                            if (index < 0 || index >= scenes.getScenes().Count)
                                continue;

                            var nextScene = scenes.getScenes()[index];

                            var t = new Tuple<Node, Node>(node, sceneToNode[nextScene]);
                            if (!present.ContainsKey(t))
                            {
                                present.Add(t, true);
                                graph.Edges.Add(new Edge(node, sceneToNode[nextScene]) /*{ Length = 2 }*/);

                                var exitOrigin = GetExitArea(scene, exit).ToRect().center;
                                var originRect = GetSceneRect(scene);

                                var pos = exitOrigin - originRect.position;
                                pos.x = Mathf.Clamp01(pos.x / originRect.width);
                                pos.y = Mathf.Clamp01(pos.y / originRect.height);

                                // Positioning constraints
                                if (pos.x < 0.3) 
                                    settings.AddLeftRightConstraint(t.Item2, t.Item1);
                                /*if (pos.y < 0.3)
                                    settings.AddUpDownConstraint(t.Item2, t.Item1);*/
                                if (pos.x > 0.7)
                                    settings.AddLeftRightConstraint(t.Item1, t.Item2);
                                /*if (pos.y > 0.7)
                                    settings.AddUpDownConstraint(t.Item1, t.Item2);*/
                            }
                        }
                    }


                    // Do the layouting
                    LayoutHelpers.CalculateLayout(graph, settings, null );

                    // Extract the results
                    var graphRect = new Rect((float)graph.Left, (float)graph.Bottom, (float)graph.Width, (float)graph.Height);
                    var canvasRect = new Rect(0, 0, SpaceWidth, SpaceHeight);

                    foreach (var scene in scenes.getScenes())
                    {
                        var n = sceneToNode[scene];
                        //Debug.Log(n.Width);
                        //sizes[scene.getPreviewBackground()] = new Vector2((float)n.Width, (float)n.Height);
                        positions[scene.getId()] = TransformPoint(new Vector2((float)(n.Center.X - n.Width / 2f), (float)(n.Center.Y + n.Height / 2f)), graphRect, canvasRect, true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message + " : " + ex.StackTrace);
                }
            }

            Vector2 TransformPoint(Vector2 point, Rect from, Rect to, bool invertY)
            {
                float absoluteX = (point.x - from.x) / from.width,
                    absoluteY = (point.y - from.y) / from.height;

                if(invertY)
                    absoluteY = 1 - absoluteY;

                return new Vector2(absoluteX * to.width + to.x, absoluteY * to.height + to.y);
            }
        }
    }
}