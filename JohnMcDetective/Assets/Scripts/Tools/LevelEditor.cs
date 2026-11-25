#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelEditor : EditorWindow
{
    private MapData targetMap;
    private Vector2 scrollPosition;
    private Vector2 objectsScrollPosition;
    private Vector2 selectedCell = new Vector2(-1, -1);
    private Vector2 cameraMoveValue = new Vector2(4.5f, 10f);

    private RoomData copiedRoom;
    private bool hasCopiedRoom = false;

    private const float CellSize = 75f;
    private const float CellPadding = 4f;
    private const string ParentName = "RoomsParent";

    [MenuItem("Tools/Level Editor")]
    public static void OpenWindow()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    private void OnGUI()
    {
        DrawHeader();

        if (targetMap == null)
        {
            EditorGUILayout.HelpBox("Sélectionne ou crée un fichier Data pour commencer", MessageType.Info);
            return;
        }

        DrawGenerationArea();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        DrawGridArea();
        DrawInspectorArea();
        DrawPreviewArea();
        EditorGUILayout.EndHorizontal();

        if (GUI.changed && targetMap != null)
        {
            EditorUtility.SetDirty(targetMap);
        }

        if (AssetPreview.IsLoadingAssetPreviews())
        {
            Repaint();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(5);
        targetMap = (MapData)EditorGUILayout.ObjectField("Carte active", targetMap, typeof(MapData), false);

        if (targetMap == null) return;

        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntSlider("Largeur (Salles)", targetMap.gridWidth, 1, 15);
        int newHeight = EditorGUILayout.IntSlider("Hauteur (Salles)", targetMap.gridHeight, 1, 15);

        targetMap.roomSpriteMaskPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab SpriteMask Salle", targetMap.roomSpriteMaskPrefab, typeof(GameObject), false);

        targetMap.startPosition = EditorGUILayout.Vector2IntField("Position de départ monde normal", targetMap.startPosition);
        targetMap.spiritStartPosition = EditorGUILayout.Vector2IntField("Position de départ monde des esprits", targetMap.spiritStartPosition);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Configuration Globale des Layers (Hauteurs Y)", EditorStyles.boldLabel);

        if (targetMap.globalLayers == null)
            targetMap.globalLayers = new List<GlobalLayerData>();
        int indexToRemove = -1;

        for (int i = 0; i < targetMap.globalLayers.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetMap.globalLayers[i].layer = EditorGUILayout.IntField("Layer ID", targetMap.globalLayers[i].layer, GUILayout.Width(300));
            targetMap.globalLayers[i].yPosition = EditorGUILayout.FloatField("Position Y", targetMap.globalLayers[i].yPosition);

            if (GUILayout.Button("Supprimer", GUILayout.Width(80)))
            {
                indexToRemove = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (indexToRemove != -1)
        {
            targetMap.globalLayers.RemoveAt(indexToRemove);
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("+ Ajouter un Layer Global"))
        {
            targetMap.globalLayers.Add(new GlobalLayerData
            {
                layer = targetMap.globalLayers.Count,
                yPosition = -targetMap.globalLayers.Count * 1.5f
            });
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetMap, "Redimensionner la grille ou modifier les layers");
            targetMap.ResizeGrid(newWidth, newHeight);
        }
    }

    #region Generation In Editor
    private void DrawGenerationArea()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Génération dans la Scène", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Générer toute la Map", GUILayout.Height(30)))
        {
            GenerateFullMap();
        }
        if (GUILayout.Button("Effacer toute la Map", GUILayout.Height(30)))
        {
            ClearFullMap();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        bool hasSelection = selectedCell.x >= 0 && selectedCell.y >= 0;
        GUI.enabled = hasSelection;
        if (GUILayout.Button(hasSelection ? $"Générer Sélection [{selectedCell.x}, {selectedCell.y}]" : "Générer la salle", GUILayout.Height(25)))
        {
            GenerateRoomInScene((int)selectedCell.x, (int)selectedCell.y);
        }
        if (GUILayout.Button(hasSelection ? $"Effacer Sélection [{selectedCell.x}, {selectedCell.y}]" : "Effacer la salle", GUILayout.Height(25)))
        {
            ClearRoomInScene((int)selectedCell.x, (int)selectedCell.y);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private Transform GetOrCreateRoomsParent()
    {
        GameObject parent = GameObject.Find(ParentName);
        if (parent == null)
        {
            parent = new GameObject(ParentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create Rooms Parent");
        }
        return parent.transform;
    }

    private void GenerateFullMap()
    {
        ClearFullMap();
        for (int x = 0; x < targetMap.gridWidth; x++)
        {
            for (int y = 0; y < targetMap.gridHeight; y++)
            {
                GenerateRoomInScene(x, y);
            }
        }
        Debug.Log("Map générée avec succès dans l'éditeur.");
    }

    private void ClearFullMap()
    {
        GameObject parent = GameObject.Find(ParentName);
        if (parent != null)
        {
            Undo.DestroyObjectImmediate(parent);
            Debug.Log("Map effacée.");
        }
    }

    private void GenerateRoomInScene(int x, int y)
    {
        RoomData room = targetMap.GetRoom(x, y);
        if (room.IsEmpty)
        {
            ClearRoomInScene(x, y);
            return;
        }

        Transform parent = GetOrCreateRoomsParent();
        string roomName = $"Room_{x}_{y}";

        ClearRoomInScene(x, y);

        GameObject roomGo = new GameObject(roomName);
        roomGo.transform.SetParent(parent);
        Undo.RegisterCreatedObjectUndo(roomGo, $"Generate Room {x}_{y}");

        Vector2 roomBasePosition = new Vector2(cameraMoveValue.x * x, cameraMoveValue.y * y);

        bool hasMask = (x + y) % 2 == 0;
        SpriteMaskInteraction interactionType = hasMask ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.VisibleOutsideMask;

        if (hasMask && targetMap.roomSpriteMaskPrefab != null)
        {
            GameObject maskGo = (GameObject)PrefabUtility.InstantiatePrefab(targetMap.roomSpriteMaskPrefab, roomGo.transform);
            maskGo.transform.position = roomBasePosition;
            Undo.RegisterCreatedObjectUndo(maskGo, "Instantiate Sprite Mask");
        }

        if (room.backGround != null)
        {
            GameObject bg = (GameObject)PrefabUtility.InstantiatePrefab(room.backGround, roomGo.transform);
            bg.transform.position = roomBasePosition;

            SpriteRenderer bgSr = bg.GetComponentInChildren<SpriteRenderer>();
            if (bgSr != null)
            {
                bgSr.sortingOrder = -10;
                bgSr.maskInteraction = interactionType;
            }

            Undo.RegisterCreatedObjectUndo(bg, "Instantiate Background");
        }

        GameObject cv = null;
        if (room.canvas != null)
        {
            cv = (GameObject)PrefabUtility.InstantiatePrefab(room.canvas, roomGo.transform);
            cv.transform.position = roomBasePosition;
            Undo.RegisterCreatedObjectUndo(cv, "Instantiate Canvas");
        }

        if (room.roomObjects != null)
        {
            foreach (ObjectItem obj in room.roomObjects)
            {
                if (obj.prefab == null) continue;

                float globalY = targetMap.GetLayerY(obj.layer);
                float finalY = globalY + obj.offsetY;

                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.prefab, roomGo.transform);
                newObj.transform.position = new Vector2(
                    roomBasePosition.x + Mathf.Clamp(obj.positionX, -cameraMoveValue.x / 2f, cameraMoveValue.x / 2f),
                    roomBasePosition.y + finalY
                );

                if (newObj.TryGetComponent<NPC>(out NPC component))
                {
                    if (cv != null)
                    {
                        component.SetSpeechBubble(cv.transform.GetChild(0));
                    }
                    component.SetNpcDatas(obj.npcData);
                }

                SpriteRenderer sr = newObj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = obj.layer;
                    sr.maskInteraction = interactionType;
                }

                Undo.RegisterCreatedObjectUndo(newObj, "Instantiate Object");
            }
        }
    }

    private void ClearRoomInScene(int x, int y)
    {
        GameObject parent = GameObject.Find(ParentName);
        if (parent != null)
        {
            Transform roomTransform = parent.transform.Find($"Room_{x}_{y}");
            if (roomTransform != null)
            {
                Undo.DestroyObjectImmediate(roomTransform.gameObject);
            }
        }
    }
    #endregion

    private void DrawGridArea()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(targetMap.gridWidth * (CellSize + CellPadding) + 20));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int y = targetMap.gridHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < targetMap.gridWidth; x++)
            {
                DrawCell(x, y);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawCell(int x, int y)
    {
        RoomData room = targetMap.GetRoom(x, y);
        bool isSelected = (selectedCell.x == x && selectedCell.y == y);

        Color originalColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = Color.yellow;
        else if (room.IsEmpty)
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        else if (room.inSpiritWorld)
            GUI.backgroundColor = new Color(0.8f, 0.6f, 1f);
        else
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);

        string label = room.IsEmpty ? $"[{x},{y}]\nVide" : $"[{x},{y}]\n{room.roomName}\n({room.type})";

        if (GUILayout.Button(label, GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
        {
            selectedCell = new Vector2(x, y);
            objectsScrollPosition = Vector2.zero;
            GUI.FocusControl(null);
        }

        GUI.backgroundColor = originalColor;
    }

    private void DrawInspectorArea()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Détails de la salle", EditorStyles.boldLabel);

        int posX = (int)selectedCell.x;
        int posY = (int)selectedCell.y;

        if (posX < 0 || posX >= targetMap.gridWidth || posY < 0 || posY >= targetMap.gridHeight)
        {
            EditorGUILayout.HelpBox("Clique sur une case de la grille pour modifier ses éléments.", MessageType.None);
            EditorGUILayout.EndVertical();
            return;
        }

        RoomData room = targetMap.GetRoom(posX, posY);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField($"Coordonnées : X = {posX}, Y = {posY}", EditorStyles.miniBoldLabel);

        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copier"))
        {
            copiedRoom = CloneRoom(room);
            hasCopiedRoom = true;
        }

        if (GUILayout.Button("Couper"))
        {
            Undo.RecordObject(targetMap, "Couper la salle");
            copiedRoom = CloneRoom(room);
            hasCopiedRoom = true;
            targetMap.SetRoom(posX, posY, new RoomData { type = RoomType.Empty });
            room = targetMap.GetRoom(posX, posY);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = hasCopiedRoom;
        if (GUILayout.Button("Coller"))
        {
            Undo.RecordObject(targetMap, "Coller la salle");
            targetMap.SetRoom(posX, posY, CloneRoom(copiedRoom));
            room = targetMap.GetRoom(posX, posY);
        }
        GUI.enabled = true;

        if (GUILayout.Button("Vider"))
        {
            Undo.RecordObject(targetMap, "Vider la salle");
            targetMap.SetRoom(posX, posY, new RoomData { type = RoomType.Empty });
            room = targetMap.GetRoom(posX, posY);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        room.type = (RoomType)EditorGUILayout.EnumPopup("Type de pièce", room.type);

        if (!room.IsEmpty)
        {
            room.roomName = EditorGUILayout.TextField("Nom de la pièce", room.roomName);
            room.inSpiritWorld = EditorGUILayout.Toggle("Pièce monde des esprits", room.inSpiritWorld);
            room.isLocked = EditorGUILayout.Toggle("Pièce verrouillée", room.isLocked);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Background (Prefab)", EditorStyles.boldLabel);
            room.backGround = (GameObject)EditorGUILayout.ObjectField("Background (Prefab)", room.backGround, typeof(GameObject), false);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Canvas (Prefab)", EditorStyles.boldLabel);
            room.canvas = (GameObject)EditorGUILayout.ObjectField("Canvas (Prefab)", room.canvas, typeof(GameObject), false);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Objets de la pièce (ObjectItem)", EditorStyles.boldLabel);

            if (room.roomObjects == null)
            {
                room.roomObjects = new ObjectItem[0];
            }

            objectsScrollPosition = EditorGUILayout.BeginScrollView(objectsScrollPosition, GUILayout.Height(250));

            int indexToRemove = -1;
            for (int i = 0; i < room.roomObjects.Length; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Objet {i + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    indexToRemove = i;
                }
                EditorGUILayout.EndHorizontal();

                room.roomObjects[i].prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", room.roomObjects[i].prefab, typeof(GameObject), false);

                if (room.roomObjects[i].prefab != null && room.roomObjects[i].prefab.GetComponentInChildren<NPC>() != null)
                {
                    room.roomObjects[i].npcData = (NPCScriptable)EditorGUILayout.ObjectField("Données NPC", room.roomObjects[i].npcData, typeof(NPCScriptable), false);
                }

                room.roomObjects[i].layer = EditorGUILayout.IntField("Layer", room.roomObjects[i].layer);
                room.roomObjects[i].positionX = EditorGUILayout.Slider("Position X", room.roomObjects[i].positionX, -cameraMoveValue.x / 2f, cameraMoveValue.x / 2f);
                room.roomObjects[i].offsetY = EditorGUILayout.Slider("Offset Y", room.roomObjects[i].offsetY, -cameraMoveValue.y / 2f, cameraMoveValue.y / 2f);

                float calculatedY = targetMap.GetLayerY(room.roomObjects[i].layer) + room.roomObjects[i].offsetY;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector2Field("Position Finale (X, Y)", new Vector2(room.roomObjects[i].positionX, calculatedY));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (indexToRemove != -1)
            {
                var list = new System.Collections.Generic.List<ObjectItem>(room.roomObjects);
                list.RemoveAt(indexToRemove);
                room.roomObjects = list.ToArray();
            }

            if (GUILayout.Button("+ Ajouter un Objet"))
            {
                var list = new System.Collections.Generic.List<ObjectItem>(room.roomObjects);
                list.Add(new ObjectItem
                {
                    layer = 1,
                    positionX = 0f,
                    offsetY = 0f
                });

                room.roomObjects = list.ToArray();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetMap, "Modification de la salle");
            targetMap.SetRoom(posX, posY, room);
        }

        EditorGUILayout.EndVertical();
    }

    private RoomData CloneRoom(RoomData source)
    {
        RoomData clone = new RoomData
        {
            type = source.type,
            roomName = source.roomName,
            inSpiritWorld = source.inSpiritWorld,
            isLocked = source.isLocked,
            backGround = source.backGround,
            canvas = source.canvas
        };

        if (source.roomObjects != null)
        {
            clone.roomObjects = new ObjectItem[source.roomObjects.Length];
            for (int i = 0; i < source.roomObjects.Length; i++)
            {
                clone.roomObjects[i] = new ObjectItem
                {
                    prefab = source.roomObjects[i].prefab,
                    layer = source.roomObjects[i].layer,
                    positionX = source.roomObjects[i].positionX,
                    offsetY = source.roomObjects[i].offsetY,
                    npcData = source.roomObjects[i].npcData
                };
            }
        }
        else
        {
            clone.roomObjects = new ObjectItem[0];
        }

        return clone;
    }

    private void DrawPreviewArea()
    {
        float screenWidth = 180f;
        float screenHeight = screenWidth * (cameraMoveValue.y / cameraMoveValue.x);

        EditorGUILayout.BeginVertical("box", GUILayout.Width(screenWidth + 15f));
        EditorGUILayout.LabelField("Aperçu Écran", EditorStyles.boldLabel);

        int posX = (int)selectedCell.x;
        int posY = (int)selectedCell.y;

        if (posX < 0 || posX >= targetMap.gridWidth || posY < 0 || posY >= targetMap.gridHeight)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        RoomData room = targetMap.GetRoom(posX, posY);
        Rect previewRect = GUILayoutUtility.GetRect(screenWidth, screenHeight);
        GUI.BeginGroup(previewRect);
        Rect localRect = new Rect(0, 0, previewRect.width, previewRect.height);

        EditorGUI.DrawRect(localRect, new Color(0.15f, 0.15f, 0.15f, 1f));

        if (room.backGround != null)
        {
            DrawPrefabComposite(localRect, room.backGround, true);
        }

        Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        Handles.DrawLine(new Vector3(localRect.width / 2f, 0), new Vector3(localRect.width / 2f, localRect.height));
        Handles.DrawLine(new Vector3(0, localRect.height / 2f), new Vector3(localRect.width, localRect.height / 2f));

        if (!room.IsEmpty && room.roomObjects != null)
        {
            var sortedObjects = new List<ObjectItem>(room.roomObjects);
            sortedObjects.Sort((a, b) => a.layer.CompareTo(b.layer));

            float halfWidth = cameraMoveValue.x / 2f;
            float halfHeight = cameraMoveValue.y / 2f;

            foreach (var item in sortedObjects)
            {
                float finalY = targetMap.GetLayerY(item.layer) + item.offsetY;
                float normalizedX = (item.positionX + halfWidth) / cameraMoveValue.x;
                float normalizedY = (halfHeight - finalY) / cameraMoveValue.y;

                float pixelX = normalizedX * localRect.width;
                float pixelY = normalizedY * localRect.height;

                if (item.prefab != null)
                {
                    Rect worldBounds = GetPrefabLocalBounds(item.prefab);
                    float worldWidth = Mathf.Max(worldBounds.width, 0.05f);
                    float worldHeight = Mathf.Max(worldBounds.height, 0.05f);

                    float objWidth = worldWidth * (localRect.width / cameraMoveValue.x);
                    float objHeight = worldHeight * (localRect.height / cameraMoveValue.y);

                    float centerOffsetX = worldBounds.center.x * (localRect.width / cameraMoveValue.x);
                    float centerOffsetY = -worldBounds.center.y * (localRect.height / cameraMoveValue.y);

                    Rect objRect = new Rect(
                        pixelX + centerOffsetX - objWidth / 2f,
                        pixelY + centerOffsetY - objHeight / 2f,
                        objWidth,
                        objHeight
                    );

                    DrawPrefabComposite(objRect, item.prefab, false);
                }
                else
                {
                    DrawDefaultObjectMarker(pixelX, pixelY, "Null", 12f, 12f);
                }
            }
        }

        Handles.color = Color.gray;
        Handles.DrawWireCube(localRect.center, localRect.size);

        GUI.EndGroup();
        EditorGUILayout.EndVertical();
    }

    private Rect GetPrefabLocalBounds(GameObject prefab)
    {
        SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        bool hasAny = false;
        float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

        foreach (var sr in renderers)
        {
            if (sr == null || sr.sprite == null) continue;

            GetRelativeTransform2D(prefab.transform, sr.transform, out Vector2 relPos, out Vector2 relScale);
            Bounds sb = sr.sprite.bounds;

            float sx = sb.min.x * relScale.x + relPos.x;
            float ex = sb.max.x * relScale.x + relPos.x;
            float sy = sb.min.y * relScale.y + relPos.y;
            float ey = sb.max.y * relScale.y + relPos.y;

            float rMinX = Mathf.Min(sx, ex);
            float rMaxX = Mathf.Max(sx, ex);
            float rMinY = Mathf.Min(sy, ey);
            float rMaxY = Mathf.Max(sy, ey);

            if (!hasAny)
            {
                minX = rMinX;
                maxX = rMaxX;
                minY = rMinY;
                maxY = rMaxY;
                hasAny = true;
            }
            else
            {
                minX = Mathf.Min(minX, rMinX);
                maxX = Mathf.Max(maxX, rMaxX);
                minY = Mathf.Min(minY, rMinY);
                maxY = Mathf.Max(maxY, rMaxY);
            }
        }

        if (!hasAny)
        {
            Vector3 s = prefab.transform.localScale;
            return new Rect(-0.5f * Mathf.Abs(s.x), -0.5f * Mathf.Abs(s.y), Mathf.Max(Mathf.Abs(s.x), 0.25f), Mathf.Max(Mathf.Abs(s.y), 0.25f));
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private void GetRelativeTransform2D(Transform root, Transform target, out Vector2 relPos, out Vector2 relScale)
    {
        List<Transform> chain = new List<Transform>();
        Transform curr = target;
        while (curr != null)
        {
            chain.Add(curr);
            if (curr == root) break;
            curr = curr.parent;
        }
        chain.Reverse();

        relPos = Vector2.zero;
        relScale = Vector2.one;

        for (int i = 0; i < chain.Count; i++)
        {
            Transform t = chain[i];
            if (i == 0 && t == root)
            {
                relScale = new Vector2(Mathf.Abs(t.localScale.x), Mathf.Abs(t.localScale.y));
                if (relScale.x < 0.0001f) relScale.x = 1f;
                if (relScale.y < 0.0001f) relScale.y = 1f;
                relPos = Vector2.zero;
            }
            else
            {
                relPos = Vector2.Scale(relPos, t.localScale) + (Vector2)t.localPosition;
                relScale = Vector2.Scale(relScale, new Vector2(Mathf.Abs(t.localScale.x), Mathf.Abs(t.localScale.y)));
            }
        }
    }

    private void DrawPrefabComposite(Rect rect, GameObject prefab, bool isBackground)
    {
        if (prefab == null) return;

        if (isBackground)
        {
            SpriteRenderer[] bgRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true)
                .Where(r => r != null && r.sprite != null && r.sprite.texture != null)
                .OrderBy(r => r.sortingOrder)
                .ToArray();

            if (bgRenderers.Length > 0)
            {
                Color saved = GUI.color;
                foreach (var sr in bgRenderers)
                {
                    GUI.color = sr.color;
                    Sprite sprite = sr.sprite;
                    Rect tr = sprite.textureRect;
                    Texture2D tex = sprite.texture;
                    Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
                    GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
                }
                GUI.color = saved;
                return;
            }
        }

        SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true)
            .Where(r => r != null && r.sprite != null && r.sprite.texture != null)
            .OrderBy(r => r.sortingOrder)
            .ToArray();

        if (renderers.Length == 0)
        {
            DrawDefaultObjectMarker(rect.center.x, rect.center.y, prefab.name, rect.width, rect.height);
            return;
        }

        Rect worldBounds = GetPrefabLocalBounds(prefab);
        float bw = Mathf.Max(worldBounds.width, 0.0001f);
        float bh = Mathf.Max(worldBounds.height, 0.0001f);

        Color savedColor = GUI.color;
        foreach (var sr in renderers)
        {
            GetRelativeTransform2D(prefab.transform, sr.transform, out Vector2 relPos, out Vector2 relScale);
            Bounds sb = sr.sprite.bounds;

            float sx = sb.min.x * relScale.x + relPos.x;
            float ex = sb.max.x * relScale.x + relPos.x;
            float sy = sb.min.y * relScale.y + relPos.y;
            float ey = sb.max.y * relScale.y + relPos.y;

            float rMinX = Mathf.Min(sx, ex);
            float rMaxX = Mathf.Max(sx, ex);
            float rMinY = Mathf.Min(sy, ey);
            float rMaxY = Mathf.Max(sy, ey);

            float u0 = (rMinX - worldBounds.xMin) / bw;
            float u1 = (rMaxX - worldBounds.xMin) / bw;
            float v0 = (rMinY - worldBounds.yMin) / bh;
            float v1 = (rMaxY - worldBounds.yMin) / bh;

            Rect partRect = Rect.MinMaxRect(
                rect.x + u0 * rect.width,
                rect.y + (1f - v1) * rect.height,
                rect.x + u1 * rect.width,
                rect.y + (1f - v0) * rect.height
            );

            GUI.color = sr.color;
            Sprite sprite = sr.sprite;
            Rect tr = sprite.textureRect;
            Texture2D tex = sprite.texture;

            Rect uv = new Rect(
                tr.x / tex.width,
                tr.y / tex.height,
                tr.width / tex.width,
                tr.height / tex.height
            );

            GUI.DrawTextureWithTexCoords(partRect, tex, uv, true);
        }
        GUI.color = savedColor;
    }

    private void DrawDefaultObjectMarker(float pixelX, float pixelY, string name, float width, float height)
    {
        Rect itemRect = new Rect(pixelX - width / 2f, pixelY - height / 2f, width, height);
        EditorGUI.DrawRect(itemRect, Color.cyan);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(pixelX - 40f, pixelY + height / 2f, 80f, 15f), name, labelStyle);
    }
}
#endif