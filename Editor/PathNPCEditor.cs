using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using PathNPCTool;
using Button = UnityEngine.UIElements.Button;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;
using Toggle = UnityEngine.UIElements.Toggle;

namespace PathNPCTool.Editor
{
    [CustomEditor(typeof(PathNPC))]
    public class PathNPCEditor : UnityEditor.Editor
    {
        private SerializedProperty _pathsProp;
        private SerializedProperty _npcSpeedProp;
        private SerializedProperty _agentProp;
        private SerializedProperty _canWalkProp;

        private static GameObject _persistedWaypointPrefab;
        private GameObject waypointPrefab;

        private bool _isPlacing = false;
        private int _placePathIndex = -1;
        private bool _ShowingPath = false;
        private bool _ShowingLabel = false;
        private bool _Looping = false;
        private int _LoopingPathIndex = -1;
        private int _showLabelPathIndex = -1;

        private Rect _hudRect = new Rect(10, 10, 300, 70);

        private VisualElement _root;
        private Label _pathsLabel;
        private Label _waypointsLabel;
        private Label _missingWaypointsLabel;
        private VisualElement _pathsContainer;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[32];
        private readonly Collider[] _npcCollidersCache = new Collider[32];

        private Dictionary<int, bool> _pathExpandedStates = new Dictionary<int, bool>();

        private LayerMask placemntLayerMask;

        private void OnEnable()
        {
            _pathsProp = serializedObject.FindProperty("paths");
            _npcSpeedProp = serializedObject.FindProperty("NPCspeed");
            _agentProp = serializedObject.FindProperty("agent");
            _canWalkProp = serializedObject.FindProperty("CanWalk");

            waypointPrefab = _persistedWaypointPrefab;

            placemntLayerMask = ~LayerMask.GetMask("Ignore Raycast");

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();

            _root = new VisualElement();
            _root.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f);
            _root.style.flexGrow = 1;
            _root.style.minHeight = 100;
            _root.style.marginLeft = -15;
            _root.style.marginRight = -15;
            _root.style.marginBottom = -4;
            _root.style.paddingLeft = 15;
            _root.style.paddingRight = 15;
            _root.style.paddingTop = 0;
            _root.style.paddingBottom = 0;

            DrawHeaderCard();
            DrawNPCSettings();
            DrawPathsPropertyField();
            DrawWaypointPrefabField();
            DrawNewPathButton();

            RefreshStats();

            _root.TrackPropertyValue(_pathsProp, _ =>
            {
                serializedObject.Update();
                RefreshStats();
                RefreshPathsUI();
            });

            return _root;
        }

        private void DrawHeaderCard()
        {
            VisualElement card = new VisualElement();
            card.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            card.style.borderTopLeftRadius = 0;
            card.style.borderTopRightRadius = 0;
            card.style.borderBottomLeftRadius = 0;
            card.style.borderBottomRightRadius = 0;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.marginLeft = -15;
            card.style.marginRight = -15;

            Label title = new Label("Path NPC Tool");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = Color.white;
            title.style.marginBottom = 8;

            VisualElement statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.flexWrap = Wrap.Wrap;

            VisualElement pathsBox = CreateStatBox("0 Paths",
                new Color(0.129f, 0.635f, 0.871f), out _pathsLabel);
            VisualElement waypointsBox = CreateStatBox("0 WayPoints",
                new Color(0.886f, 0.769f, 0.235f), out _waypointsLabel);
            VisualElement missingBox = CreateStatBox("0 Missing WayPoints",
                new Color(0.886f, 0.235f, 0.353f), out _missingWaypointsLabel);

            statsRow.Add(pathsBox);
            statsRow.Add(waypointsBox);
            statsRow.Add(missingBox);

            card.Add(title);
            card.Add(statsRow);
            _root.Add(card);
        }

        private VisualElement CreateStatBox(string text, Color color, out Label label)
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            container.style.borderTopLeftRadius = 8;
            container.style.borderTopRightRadius = 8;
            container.style.borderBottomLeftRadius = 8;
            container.style.borderBottomRightRadius = 8;
            container.style.borderBottomWidth = 1;
            container.style.borderTopWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderTopColor = new Color(0.85f, 0.85f, 0.85f);
            container.style.borderBottomColor = new Color(0.85f, 0.85f, 0.85f);
            container.style.borderLeftColor = new Color(0.85f, 0.85f, 0.85f);
            container.style.borderRightColor = new Color(0.85f, 0.85f, 0.85f);
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.paddingTop = 5;
            container.style.paddingBottom = 5;
            container.style.marginRight = 6;
            container.style.marginBottom = 6;

            VisualElement circle = new VisualElement();
            circle.style.width = 6;
            circle.style.height = 6;
            circle.style.backgroundColor = color;
            circle.style.borderTopLeftRadius = 3;
            circle.style.borderTopRightRadius = 3;
            circle.style.borderBottomLeftRadius = 3;
            circle.style.borderBottomRightRadius = 3;
            circle.style.marginRight = 6;

            label = new Label(text);
            label.style.color = Color.white;
            label.style.fontSize = 12;

            container.Add(circle);
            container.Add(label);
            return container;
        }

        private void DrawNPCSettings()
        {
            VisualElement settingsBox = new VisualElement();
            settingsBox.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            settingsBox.style.borderTopLeftRadius = 8;
            settingsBox.style.borderTopRightRadius = 8;
            settingsBox.style.borderBottomLeftRadius = 8;
            settingsBox.style.borderBottomRightRadius = 8;
            settingsBox.style.paddingLeft = 10;
            settingsBox.style.paddingRight = 10;
            settingsBox.style.paddingTop = 10;
            settingsBox.style.paddingBottom = 10;
            settingsBox.style.marginLeft = 10;
            settingsBox.style.marginRight = 10;
            settingsBox.style.marginTop = 10;
            settingsBox.style.marginBottom = 10;

            Label title = new Label("NPC Settings");
            title.style.color = Color.white;
            title.style.fontSize = 13;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;

            PropertyField npcSpeedField = new PropertyField(_npcSpeedProp, "NPC Speed");
            npcSpeedField.Bind(serializedObject);
            npcSpeedField.style.marginBottom = 8;

            PropertyField agentField = new PropertyField(_agentProp, "Agent");
            agentField.Bind(serializedObject);
            agentField.style.marginBottom = 8;

            PropertyField canWalkField = new PropertyField(_canWalkProp, "Can Walk");
            canWalkField.Bind(serializedObject);

            StyleField(npcSpeedField);
            StyleField(agentField);
            StyleField(canWalkField);

            settingsBox.Add(title);
            settingsBox.Add(npcSpeedField);
            settingsBox.Add(agentField);
            settingsBox.Add(canWalkField);

            _root.Add(settingsBox);
        }

        private void StyleField(VisualElement field)
        {
            field.style.color = Color.white;

            Label label = field.Q<Label>();
            if (label != null)
                label.style.color = Color.white;

            VisualElement input = field.Q(className: "unity-base-field__input");
            if (input != null)
            {
                input.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f);
                input.style.paddingLeft = 6;
                input.style.paddingRight = 6;
                input.style.paddingTop = 6;
                input.style.paddingBottom = 6;
                input.style.borderTopColor = new Color(0.25f, 0.25f, 0.25f);
                input.style.borderBottomColor = new Color(0.25f, 0.25f, 0.25f);
                input.style.borderLeftColor = new Color(0.25f, 0.25f, 0.25f);
                input.style.borderRightColor = new Color(0.25f, 0.25f, 0.25f);
                input.style.borderTopWidth = 1;
                input.style.borderBottomWidth = 1;
                input.style.borderLeftWidth = 1;
                input.style.borderRightWidth = 1;
            }
        }

        private void DrawPathsPropertyField()
        {
            VisualElement pathsBox = new VisualElement();
            pathsBox.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            pathsBox.style.borderTopLeftRadius = 8;
            pathsBox.style.borderTopRightRadius = 8;
            pathsBox.style.borderBottomLeftRadius = 8;
            pathsBox.style.borderBottomRightRadius = 8;
            pathsBox.style.paddingLeft = 10;
            pathsBox.style.paddingRight = 10;
            pathsBox.style.paddingTop = 10;
            pathsBox.style.paddingBottom = 10;
            pathsBox.style.marginLeft = 10;
            pathsBox.style.marginRight = 10;
            pathsBox.style.marginTop = 10;
            pathsBox.style.marginBottom = 10;

            _pathsContainer = pathsBox;
            _root.Add(pathsBox);
            RefreshPathsUI();
        }

        private void RefreshPathsUI()
        {
            if (_pathsContainer == null) return;

            _pathsContainer.Clear();
            serializedObject.Update();

            for (int i = 0; i < _pathsProp.arraySize; i++)
            {
                int capturedIndex = i;
                SerializedProperty pathProp = _pathsProp.GetArrayElementAtIndex(i);
                string pathName = pathProp.FindPropertyRelative("pathName").stringValue;

                Foldout foldout = new Foldout();
                foldout.text = "";
                foldout.style.flexGrow = 1;
                foldout.style.marginBottom = 0;
                foldout.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
                foldout.Q<Toggle>().style.display = DisplayStyle.None;

                foldout.Add(DrawPathEditButtons(capturedIndex));

                VisualElement header = new VisualElement();
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.paddingLeft = 8;
                header.style.paddingRight = 8;
                header.style.paddingTop = 6;
                header.style.paddingBottom = 6;
                header.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);

                VisualElement circle = new VisualElement();
                circle.style.width = 8;
                circle.style.height = 8;
                circle.style.backgroundColor = pathProp.FindPropertyRelative("color").colorValue;
                circle.style.borderTopLeftRadius = 4;
                circle.style.borderTopRightRadius = 4;
                circle.style.borderBottomLeftRadius = 4;
                circle.style.borderBottomRightRadius = 4;
                circle.style.marginRight = 8;
                circle.style.flexShrink = 0;

                Label nameLabel = new Label(
                    string.IsNullOrEmpty(pathName) ? $"Path {i}" : pathName);
                nameLabel.style.color = Color.white;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.fontSize = 13;
                nameLabel.style.flexGrow = 1;

                Label pathLabel = new Label($"{CountPathWayPoint(i)} pts");
                pathLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                pathLabel.style.fontSize = 11;
                pathLabel.style.marginRight = 6;
                pathLabel.style.unityTextAlign = TextAnchor.MiddleRight;

                bool isExpanded = _pathExpandedStates.ContainsKey(capturedIndex)
                    ? _pathExpandedStates[capturedIndex]
                    : false;

                Label arrow = new Label(isExpanded ? "▾" : "▸");
                arrow.style.color = Color.white;
                arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
                arrow.style.width = 16;
                arrow.style.flexShrink = 0;

                header.Add(circle);
                header.Add(nameLabel);
                header.Add(pathLabel);
                header.Add(arrow);

                nameLabel.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount != 2) return;
                    evt.StopPropagation();

                    TextField nameField = new TextField();
                    nameField.value = nameLabel.text;
                    nameField.style.flexGrow = 1;
                    nameField.style.marginRight = 6;

                    int nameIndex = header.IndexOf(nameLabel);
                    header.Remove(nameLabel);
                    header.Insert(nameIndex, nameField);
                    nameField.Focus();
                    nameField.SelectAll();

                    void SaveName()
                    {
                        serializedObject.Update();
                        _pathsProp.GetArrayElementAtIndex(capturedIndex)
                            .FindPropertyRelative("pathName").stringValue = nameField.value;
                        serializedObject.ApplyModifiedProperties();
                        RefreshPathsUI();
                    }

                    nameField.RegisterCallback<FocusOutEvent>(_ => SaveName());
                    nameField.RegisterCallback<KeyDownEvent>(keyEvt =>
                    {
                        if (keyEvt.keyCode == KeyCode.Return ||
                            keyEvt.keyCode == KeyCode.KeypadEnter)
                        {
                            SaveName();
                            keyEvt.StopPropagation();
                        }

                        if (keyEvt.keyCode == KeyCode.Escape)
                        {
                            RefreshPathsUI();
                            keyEvt.StopPropagation();
                        }
                    });
                });

                header.RegisterCallback<ClickEvent>(_ =>
                {
                    foldout.value = !foldout.value;
                    arrow.text = foldout.value ? "▾" : "▸";
                    _pathExpandedStates[capturedIndex] = foldout.value;
                });

                foldout.SetValueWithoutNotify(isExpanded);
                foldout.RegisterValueChangedCallback(evt =>
                {
                    _pathExpandedStates[capturedIndex] = evt.newValue;
                    arrow.text = evt.newValue ? "▾" : "▸";
                });

                SerializedProperty waypointsProp = pathProp.FindPropertyRelative("waypoints");
                foldout.Add(DrawWaypointsList(waypointsProp));

                SerializedProperty pathColorProp = pathProp.FindPropertyRelative("color");
                PropertyField pathColorField = new PropertyField(pathColorProp);
                pathColorField.Bind(serializedObject);
                pathColorField.style.paddingBottom = 6;
                foldout.Add(pathColorField);

                Button DeleteButton = new Button(() =>
                {
                    serializedObject.Update();
                    ClearPath(capturedIndex);
                    _pathsProp.DeleteArrayElementAtIndex(capturedIndex);
                    serializedObject.ApplyModifiedProperties();
                    RefreshStats();
                    SceneView.RepaintAll();
                });
                DeleteButton.style.flexDirection = FlexDirection.Row;
                DeleteButton.style.alignItems = Align.Center;
                DeleteButton.style.flexShrink = 0;
                DeleteButton.style.marginRight = 4;
                DeleteButton.style.marginLeft = 4;
                DeleteButton.style.marginTop = 4;
                DeleteButton.style.marginBottom = 8;
                DeleteButton.style.paddingLeft = 4;
                DeleteButton.style.paddingRight = 4;
                DeleteButton.style.backgroundColor = new Color(0.5f, 0.18f, 0.18f);
                DeleteButton.style.borderTopColor = new Color(0.75f, 0.27f, 0.27f);
                DeleteButton.style.borderBottomColor = new Color(0.75f, 0.27f, 0.27f);
                DeleteButton.style.borderLeftColor = new Color(0.75f, 0.27f, 0.27f);
                DeleteButton.style.borderRightColor = new Color(0.75f, 0.27f, 0.27f);
                DeleteButton.style.borderTopWidth = 1;
                DeleteButton.style.borderBottomWidth = 1;
                DeleteButton.style.borderLeftWidth = 1;
                DeleteButton.style.borderRightWidth = 1;
                DeleteButton.text = "Delete Path";

                foldout.Add(DeleteButton);

                VisualElement card = new VisualElement();
                card.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
                card.style.borderTopLeftRadius = 6;
                card.style.borderTopRightRadius = 6;
                card.style.borderBottomLeftRadius = 6;
                card.style.borderBottomRightRadius = 6;
                card.style.marginBottom = 6;
                card.style.overflow = Overflow.Hidden;
                card.style.borderBottomColor = new Color(0.25f, 0.25f, 0.25f);
                card.style.borderBottomWidth = 1;

                card.Add(header);
                card.Add(foldout);
                _pathsContainer.Add(card);
            }
        }

        private VisualElement DrawWaypointsList(SerializedProperty waypointsProp)
        {
            VisualElement container = new VisualElement();
            container.style.backgroundColor = new Color(0.11f, 0.11f, 0.11f);
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.paddingTop = 8;
            container.style.paddingBottom = 8;
            container.style.marginBottom = 6;

            for (int i = 0; i < waypointsProp.arraySize; i++)
            {
                SerializedProperty waypointProp = waypointsProp.GetArrayElementAtIndex(i);

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                Label indexLabel = new Label($"{i}");
                indexLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                indexLabel.style.width = 20;
                indexLabel.style.flexShrink = 0;
                indexLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                indexLabel.style.fontSize = 11;
                row.Add(indexLabel);

                ObjectField wpObjField = new ObjectField();
                wpObjField.objectType = typeof(WayPoint);
                wpObjField.allowSceneObjects = true;
                wpObjField.BindProperty(waypointProp);
                wpObjField.style.flexGrow = 1;
                wpObjField.style.marginLeft = 4;
                wpObjField.style.marginRight = 6;
                row.Add(wpObjField);

                VisualElement waitWrapper = new VisualElement();
                waitWrapper.style.flexDirection = FlexDirection.Row;
                waitWrapper.style.alignItems = Align.Center;
                waitWrapper.style.flexShrink = 0;
                waitWrapper.style.marginRight = 4;
                row.Add(waitWrapper);

                Button deleteButton = new Button();
                deleteButton.style.flexDirection = FlexDirection.Row;
                deleteButton.style.alignItems = Align.Center;
                deleteButton.style.flexShrink = 0;
                deleteButton.style.marginRight = 4;
                deleteButton.style.marginLeft = 4;
                deleteButton.style.paddingLeft = 4;
                deleteButton.style.paddingRight = 4;
                deleteButton.style.width = 20;
                deleteButton.style.height = 20;
                deleteButton.style.backgroundColor = new Color(0.5f, 0.18f, 0.18f);
                deleteButton.style.borderTopColor = new Color(0.75f, 0.27f, 0.27f);
                deleteButton.style.borderBottomColor = new Color(0.75f, 0.27f, 0.27f);
                deleteButton.style.borderLeftColor = new Color(0.75f, 0.27f, 0.27f);
                deleteButton.style.borderRightColor = new Color(0.75f, 0.27f, 0.27f);
                deleteButton.style.borderTopWidth = 1;
                deleteButton.style.borderBottomWidth = 1;
                deleteButton.style.borderLeftWidth = 1;
                deleteButton.style.borderRightWidth = 1;
                deleteButton.text = "X";

                row.Add(deleteButton);

                int capturedIndex = i;

                deleteButton.clicked += () =>
                {
                    DeleteWaypoint(waypointsProp, capturedIndex);
                    container.Clear();
                    container.Add(DrawWaypointsList(waypointsProp));
                };

                void RebuildWaitField(WayPoint wp)
                {
                    waitWrapper.Clear();
                    if (wp == null) return;

                    SerializedObject wpSO = new SerializedObject(wp);

                    SerializedProperty waitProp = wpSO.FindProperty("WaitTime")
                                                  ?? wpSO.FindProperty("waitTime");
                    if (waitProp == null) return;

                    Label waitLabel = new Label("Wait(s)");
                    waitLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                    waitLabel.style.fontSize = 11;
                    waitLabel.style.marginRight = 4;
                    waitWrapper.Add(waitLabel);

                    FloatField waitField = new FloatField();
                    waitField.value = waitProp.floatValue;
                    waitField.style.width = 55;
                    waitField.RegisterValueChangedCallback(evt =>
                    {
                        float clamped = Mathf.Max(0f, evt.newValue);
                        waitField.SetValueWithoutNotify(clamped);

                        wpSO.Update();
                        waitProp.floatValue = clamped;
                        wpSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(wp);
                    });
                    waitWrapper.Add(waitField);
                }

                RebuildWaitField(waypointProp.objectReferenceValue as WayPoint);

                wpObjField.RegisterValueChangedCallback(evt =>
                {
                    RebuildWaitField(evt.newValue as WayPoint);
                });

                container.Add(row);
            }

            return container;
        }

        private VisualElement DrawPathEditButtons(int capturedIndex)
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.flexWrap = Wrap.Wrap;

            container.Add(MakeButton(
                _isPlacing && _placePathIndex == capturedIndex ? "Stop" : "Place",
                () => TogglePlacing(capturedIndex), 80,
                _isPlacing && _placePathIndex == capturedIndex
                    ? (new Color(0.6f, 0.18f, 0.23f), new Color(0.847f, 0.275f, 0.341f))
                    : (new Color(0.18f, 0.42f, 0.6f), new Color(0.275f, 0.627f, 0.847f))));

            container.Add(MakeButton(
                _ShowingPath && _placePathIndex == capturedIndex ? "Hide path" : "Show path",
                () => ToggleShowPath(capturedIndex), 100,
                _ShowingPath && _placePathIndex == capturedIndex
                    ? (new Color(0.15f, 0.15f, 0.15f), new Color(0.3f, 0.3f, 0.3f))
                    : (new Color(0.18f, 0.5f, 0.18f), new Color(0.27f, 0.75f, 0.27f))));

            container.Add(MakeButton(
                _ShowingLabel && _showLabelPathIndex == capturedIndex
                    ? "Hide labels"
                    : "Show labels",
                () => ToggleShowLabel(capturedIndex), 100,
                _ShowingLabel && _showLabelPathIndex == capturedIndex
                    ? (new Color(0.15f, 0.15f, 0.15f), new Color(0.3f, 0.3f, 0.3f))
                    : (new Color(0.18f, 0.5f, 0.18f), new Color(0.27f, 0.75f, 0.27f))));

            container.Add(MakeButton(
                "Clear WayPoints",
                () => ClearPath(capturedIndex), 120,
                (new Color(0.5f, 0.18f, 0.18f), new Color(0.75f, 0.27f, 0.27f))));

            container.Add(MakeButton(
                _Looping && _LoopingPathIndex == capturedIndex ? "Loop" : "Stop Loop",
                () => ToggleLoop(capturedIndex), 80,
                _Looping && _LoopingPathIndex == capturedIndex
                    ? (new Color(0.15f, 0.15f, 0.15f), new Color(0.3f, 0.3f, 0.3f))
                    : (new Color(0.18f, 0.5f, 0.18f), new Color(0.27f, 0.75f, 0.27f))));

            return container;
        }

        private Button MakeButton(string text, System.Action onClick, float width,
            (Color bg, Color border) colors)
        {
            Button btn = new Button(onClick);
            btn.text = text;
            btn.style.backgroundColor = colors.bg;
            btn.style.borderTopColor = colors.border;
            btn.style.borderBottomColor = colors.border;
            btn.style.borderLeftColor = colors.border;
            btn.style.borderRightColor = colors.border;
            btn.style.borderTopWidth = 1;
            btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth = 1;
            btn.style.borderRightWidth = 1;
            btn.style.color = Color.white;
            btn.style.width = width;
            btn.style.height = 25;
            btn.style.marginLeft = 8;
            btn.style.marginBottom = 6;
            return btn;
        }

        private void DrawWaypointPrefabField()
        {
            serializedObject.Update();
            ObjectField prefabField = new ObjectField("Waypoint Prefab");
            prefabField.objectType = typeof(GameObject);
            prefabField.allowSceneObjects = false;
            prefabField.value = waypointPrefab;
            prefabField.style.marginBottom = 10;

            prefabField.RegisterValueChangedCallback(evt =>
            {
                waypointPrefab = evt.newValue as GameObject;
                _persistedWaypointPrefab = waypointPrefab;
            });

            serializedObject.ApplyModifiedProperties();

            _root.Add(prefabField);
        }

        private void DrawNewPathButton()
        {
            Button newPathButton = new Button(() =>
            {
                serializedObject.Update();
                _pathsProp.arraySize++;
                _pathsProp.GetArrayElementAtIndex(_pathsProp.arraySize - 1)
                    .FindPropertyRelative("waypoints").arraySize = 0;
                serializedObject.ApplyModifiedProperties();
                RefreshStats();
                SceneView.RepaintAll();
            });
            newPathButton.style.flexDirection = FlexDirection.Row;
            newPathButton.style.alignItems = Align.Center;
            newPathButton.style.flexShrink = 0;
            newPathButton.style.marginRight = 4;
            newPathButton.style.marginLeft = 4;
            newPathButton.style.marginTop = 4;
            newPathButton.style.marginBottom = 8;
            newPathButton.style.paddingLeft = 4;
            newPathButton.style.paddingRight = 4;
            newPathButton.style.backgroundColor = new Color(0.18f, 0.42f, 0.6f);
            newPathButton.style.borderTopColor = new Color(0.275f, 0.627f, 0.847f);
            newPathButton.style.borderBottomColor = new Color(0.275f, 0.627f, 0.847f);
            newPathButton.style.borderLeftColor = new Color(0.275f, 0.627f, 0.847f);
            newPathButton.style.borderRightColor = new Color(0.275f, 0.627f, 0.847f);
            newPathButton.style.borderTopWidth = 1;
            newPathButton.style.borderBottomWidth = 1;
            newPathButton.style.borderLeftWidth = 1;
            newPathButton.style.borderRightWidth = 1;
            newPathButton.text = "+ Add New Path";
            _root.Add(newPathButton);
        }

        private void TogglePlacing(int pathIndex)
        {
            serializedObject.Update();
            if (_isPlacing && _placePathIndex == pathIndex)
            {
                _isPlacing = false;
                _placePathIndex = -1;
            }
            else
            {
                _placePathIndex = pathIndex;
                _isPlacing = true;
            }

            RefreshPathsUI();
            SceneView.RepaintAll();
        }

        private void ToggleShowPath(int pathIndex)
        {
            serializedObject.Update();
            if (_ShowingPath && _placePathIndex == pathIndex)
            {
                _ShowingPath = false;
                _pathsProp.GetArrayElementAtIndex(pathIndex)
                    .FindPropertyRelative("ShowPath").boolValue = false;
                _placePathIndex = -1;
            }
            else
            {
                _ShowingPath = true;
                _pathsProp.GetArrayElementAtIndex(pathIndex)
                    .FindPropertyRelative("ShowPath").boolValue = true;
                _placePathIndex = pathIndex;
            }

            serializedObject.ApplyModifiedProperties();
            RefreshPathsUI();
            SceneView.RepaintAll();
        }

        private void ToggleShowLabel(int pathIndex)
        {
            if (_ShowingLabel && _showLabelPathIndex == pathIndex)
            {
                _ShowingLabel = false;
                _showLabelPathIndex = -1;
            }
            else
            {
                _ShowingLabel = true;
                _showLabelPathIndex = pathIndex;
            }

            RefreshPathsUI();
            SceneView.RepaintAll();
        }

        private void ToggleLoop(int pathIndex)
        {
            serializedObject.Update();
            if (_Looping && _LoopingPathIndex == pathIndex)
            {
                _Looping = false;
                _pathsProp.GetArrayElementAtIndex(pathIndex)
                    .FindPropertyRelative("loopPath").boolValue = false;
                _LoopingPathIndex = -1;
            }
            else
            {
                _Looping = true;
                _pathsProp.GetArrayElementAtIndex(pathIndex)
                    .FindPropertyRelative("loopPath").boolValue = true;
                _LoopingPathIndex = pathIndex;
            }

            serializedObject.ApplyModifiedProperties();
            RefreshPathsUI();
            SceneView.RepaintAll();
        }

        private void RefreshStats()
        {
            if (_pathsLabel == null) return;
            serializedObject.Update();
            _pathsLabel.text = $"{_pathsProp.arraySize} Paths";
            _waypointsLabel.text = $"{CountAllWayPoints()} WayPoints";
            _missingWaypointsLabel.text = $"{CountAllMissingWayPoints()} Missing WayPoints";
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            PathNPC npcTarget = (PathNPC)target;
            Collider[] npcColliders = npcTarget.GetComponentsInChildren<Collider>();

            if (_ShowingLabel && _showLabelPathIndex >= 0 &&
                _showLabelPathIndex < _pathsProp.arraySize)
            {
                serializedObject.Update();
                var waypointsProp = _pathsProp
                    .GetArrayElementAtIndex(_showLabelPathIndex)
                    .FindPropertyRelative("waypoints");

                for (int j = 0; j < waypointsProp.arraySize; j++)
                {
                    var wp = waypointsProp.GetArrayElementAtIndex(j).objectReferenceValue
                        as WayPoint;
                    if (wp == null) continue;
                    Handles.Label(wp.transform.position + Vector3.up * 1.5f,
                        $"Path {_showLabelPathIndex} - WP {j}");
                }

                sceneView.Repaint();
            }

            if (!_isPlacing) return;

            serializedObject.Update();

            if (_placePathIndex < 0 || _placePathIndex >= _pathsProp.arraySize)
            {
                _isPlacing = false;
                _placePathIndex = -1;
                RefreshPathsUI();
                return;
            }

            Event e = Event.current;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (PlacementRaycast(ray, out RaycastHit hitMouse))
            {
                Handles.color = Color.green;
                Handles.DrawSolidDisc(hitMouse.point, hitMouse.normal, 2f);
                DrawLineToTheNextWayPoint(_placePathIndex, hitMouse.point);
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (PlacementRaycast(ray, out RaycastHit hit))
                {
                    SpawnWayPoint(_placePathIndex, hit.point);
                    RefreshPathsUI();
                    e.Use();
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                _isPlacing = false;
                _placePathIndex = -1;
                RefreshPathsUI();
                e.Use();
            }

            Handles.BeginGUI();
            _hudRect = GUI.Window(0, _hudRect, DrawHUD, "");
            Handles.EndGUI();

            sceneView.Repaint();
        }

        private bool PlacementRaycast(Ray ray, out RaycastHit hit)
        {
            PathNPC npcTarget = (PathNPC)target;
            Collider[] npcColliders = npcTarget.GetComponentsInChildren<Collider>();

            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, placemntLayerMask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var h in hits)
            {
                bool isNPC = System.Array.Exists(npcColliders, c => c == h.collider);
                if (!isNPC)
                {
                    hit = h;
                    return true;
                }
            }

            hit = default;
            return false;
        }

        private void DrawHUD(int id)
        {
            serializedObject.Update();
            GUILayout.Label($"Placing -> Path {_placePathIndex} | Esc to stop");
            var waypointsProp = _pathsProp.GetArrayElementAtIndex(_placePathIndex)
                .FindPropertyRelative("waypoints");
            GUILayout.Label($"Current path has {waypointsProp.arraySize} waypoints");
            GUI.DragWindow();
        }

        private void SpawnWayPoint(int pathIndex, Vector3 position)
        {
            if (waypointPrefab == null)
            {
                Debug.LogWarning("Assign a Waypoint Prefab before placing waypoints.");
                return;
            }

            Vector3 newPosition = new Vector3(position.x, position.y + 5f, position.z);

            GameObject newWaypoint = PrefabUtility.InstantiatePrefab(waypointPrefab) as GameObject
                                     ?? UnityEngine.Object.Instantiate(waypointPrefab);

            newWaypoint.transform.position = newPosition;
            newWaypoint.transform.rotation = Quaternion.identity;

            WayPoint waypointComponent = newWaypoint.GetComponent<WayPoint>();
            if (waypointComponent == null)
            {
                Debug.LogError("Waypoint Prefab must have a WayPoint component.");
                Undo.DestroyObjectImmediate(newWaypoint);
                return;
            }

            Undo.RegisterCreatedObjectUndo(newWaypoint, "Create Waypoint");

            serializedObject.Update();
            var waypointsProp = _pathsProp.GetArrayElementAtIndex(pathIndex)
                .FindPropertyRelative("waypoints");

            int newIndex = waypointsProp.arraySize;
            waypointsProp.InsertArrayElementAtIndex(newIndex);
            waypointsProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = waypointComponent;

            serializedObject.ApplyModifiedProperties();
            RefreshStats();
        }

        private void ClearPath(int pathIndex)
        {
            serializedObject.Update();
            var waypointsProp = _pathsProp.GetArrayElementAtIndex(pathIndex)
                .FindPropertyRelative("waypoints");

            for (int i = 0; i < waypointsProp.arraySize; i++)
            {
                var wp = waypointsProp.GetArrayElementAtIndex(i).objectReferenceValue as WayPoint;
                if (wp != null)
                    Undo.DestroyObjectImmediate(wp.gameObject);
            }

            waypointsProp.arraySize = 0;
            serializedObject.ApplyModifiedProperties();
            RefreshStats();
        }

        private void DeleteWaypoint(SerializedProperty waypointsProp, int waypointIndex)
        {
            serializedObject.Update();

            if (waypointIndex < 0 || waypointIndex >= waypointsProp.arraySize)
                return;

            SerializedProperty element = waypointsProp.GetArrayElementAtIndex(waypointIndex);
            WayPoint wp = element.objectReferenceValue as WayPoint;

            if (wp != null)
                Undo.DestroyObjectImmediate(wp.gameObject);

            waypointsProp.DeleteArrayElementAtIndex(waypointIndex);
            serializedObject.ApplyModifiedProperties();
            RefreshStats();
        }

        private void DrawLineToTheNextWayPoint(int pathIndex, Vector3 nextPosition)
        {
            var waypointsProp = _pathsProp.GetArrayElementAtIndex(pathIndex)
                .FindPropertyRelative("waypoints");
            if (waypointsProp.arraySize == 0) return;

            var wp = waypointsProp.GetArrayElementAtIndex(waypointsProp.arraySize - 1)
                .objectReferenceValue as WayPoint;
            if (wp == null) return;

            Handles.color = Color.red;
            Handles.DrawLine(wp.transform.position, nextPosition);
        }

        private int CountAllWayPoints()
        {
            int total = 0;
            for (int i = 0; i < _pathsProp.arraySize; i++)
                total += _pathsProp.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("waypoints").arraySize;
            return total;
        }

        private int CountAllMissingWayPoints()
        {
            int missing = 0;
            for (int i = 0; i < _pathsProp.arraySize; i++)
            {
                var wps = _pathsProp.GetArrayElementAtIndex(i).FindPropertyRelative("waypoints");
                for (int j = 0; j < wps.arraySize; j++)
                    if (wps.GetArrayElementAtIndex(j).objectReferenceValue == null)
                        missing++;
            }

            return missing;
        }

        private int CountPathWayPoint(int pathIndex) =>
            _pathsProp.GetArrayElementAtIndex(pathIndex)
                .FindPropertyRelative("waypoints").arraySize;
    }
}
