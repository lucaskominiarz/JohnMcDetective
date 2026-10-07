using UnityEditor;
using UnityEngine;

public class LevelEditor : EditorWindow // un peu le bordel mais ca marche
{
    private MapData targetMap;
    private Vector2 scrollPosition;
    private Vector2 selectedCell = new Vector2(-1, -1);
    private  Vector2 cameraMoveValue =  new Vector2(4.5f, 10f);
    
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
        targetMap.startPosition = EditorGUILayout.Vector2IntField("Position de départ", targetMap.startPosition);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetMap, "Redimensionner la grille");
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
        if (GUILayout.Button(hasSelection ? $"Générer Séléction [{selectedCell.x}, {selectedCell.y}]" : "Générer la salle", GUILayout.Height(25)))
        {
            GenerateRoomInScene((int)selectedCell.x, (int)selectedCell.y);
        }
        if (GUILayout.Button(hasSelection ? $"Effacer Séléction [{selectedCell.x}, {selectedCell.y}]" : "Effacer la salle", GUILayout.Height(25)))
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

        if (room.backGround != null)
        {
            GameObject bg = (GameObject)PrefabUtility.InstantiatePrefab(room.backGround, roomGo.transform);
            bg.transform.position = roomBasePosition;
            Undo.RegisterCreatedObjectUndo(bg, "Instantiate Background");
        }

        if (room.roomObjects != null)
        {
            foreach (ObjectItem obj in room.roomObjects)
            {
                if (obj.prefab == null) continue;

                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.prefab, roomGo.transform);
                newObj.transform.position = new Vector2(
                    roomBasePosition.x + Mathf.Clamp(obj.position.x, -cameraMoveValue.x / 2f, cameraMoveValue.x / 2f),
                    roomBasePosition.y + Mathf.Clamp(obj.position.y, -cameraMoveValue.y / 2f, cameraMoveValue.y / 2f)
                );

                SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = obj.layer + 1;

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
        else
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);

        string label = room.IsEmpty ? $"[{x},{y}]\nVide" : $"[{x},{y}]\n{room.roomName}\n({room.type})";

        if (GUILayout.Button(label, GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
        {
            selectedCell = new Vector2(x, y);
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
        EditorGUILayout.Space(5);

        room.type = (RoomType)EditorGUILayout.EnumPopup("Type de pièce", room.type);

        if (!room.IsEmpty)
        {
            room.roomName = EditorGUILayout.TextField("Nom de la pièce", room.roomName);
            room.isLocked = EditorGUILayout.Toggle("Pièce verrouillée", room.isLocked);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Background (Prefab)", EditorStyles.boldLabel);
            room.backGround = (GameObject)EditorGUILayout.ObjectField("Background (Prefab)", room.backGround, typeof(GameObject), false);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Objets de la pièce (ObjectItem)", EditorStyles.boldLabel);

            if (room.roomObjects == null)
            {
                room.roomObjects = new ObjectItem[0];
            }

            for (int i = 0; i < room.roomObjects.Length; i++)
            {
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Objet {i + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    var list = new System.Collections.Generic.List<ObjectItem>(room.roomObjects);
                    list.RemoveAt(i);
                    room.roomObjects = list.ToArray();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                room.roomObjects[i].prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", room.roomObjects[i].prefab, typeof(GameObject), false);
                room.roomObjects[i].layer = EditorGUILayout.IntField("Layer", room.roomObjects[i].layer);
                room.roomObjects[i].layerSpacing = EditorGUILayout.FloatField("Multiplicateur Y", room.roomObjects[i].layerSpacing);
                room.roomObjects[i].position.y = room.roomObjects[i].layer * room.roomObjects[i].layerSpacing;
                room.roomObjects[i].position.x = EditorGUILayout.FloatField("Position X", room.roomObjects[i].position.x);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector2Field("Position Finale (X, Y)", room.roomObjects[i].position);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Ajouter un Objet"))
            {
                var list = new System.Collections.Generic.List<ObjectItem>(room.roomObjects);
                list.Add(new ObjectItem 
                { 
                    layer = 1,
                    layerSpacing = 2.5f,
                    position = new Vector2(0f, 2.5f)
                });
                
                room.roomObjects = list.ToArray();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetMap, "Modification de la salle");
            targetMap.SetRoom(posX, posY, room);
        }

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Vider cette case"))
        {
            Undo.RecordObject(targetMap, "Vider la salle");
            targetMap.SetRoom(posX, posY, new RoomData { type = RoomType.Empty });
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPreviewArea()
    {
        float screenWidth = 180f;
        float screenHeight = screenWidth * (10f / 4.5f);

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
        
        EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f, 1f));

        if (room.backGround != null)
        {
            DrawPrefabPreview(previewRect, room.backGround, true);
        }

        Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        Handles.DrawLine(new Vector3(previewRect.x + previewRect.width / 2f, previewRect.y), new Vector3(previewRect.x + previewRect.width / 2f, previewRect.yMax));
        Handles.DrawLine(new Vector3(previewRect.x, previewRect.y + previewRect.height / 2f), new Vector3(previewRect.xMax, previewRect.y + previewRect.height / 2f));

        if (!room.IsEmpty && room.roomObjects != null)
        {
            float ppu = previewRect.width / 4.5f;

            foreach (var item in room.roomObjects)
            {
                float normalizedX = (item.position.x + 2.25f) / 4.5f;
                float normalizedY = (10f - (item.position.y + 5f)) / 10f;

                float pixelX = previewRect.x + (normalizedX * previewRect.width);
                float pixelY = previewRect.y + (normalizedY * previewRect.height);

                float objWidth = 36f;
                float objHeight = 36f;

                if (item.prefab != null)
                {
                    SpriteRenderer objSr = item.prefab.GetComponentInChildren<SpriteRenderer>();
                    if (objSr != null && objSr.sprite != null)
                    {
                        Vector3 scale = item.prefab.transform.localScale;
                        objWidth = objSr.sprite.bounds.size.x * scale.x * ppu;
                        objHeight = objSr.sprite.bounds.size.y * scale.y * ppu;
                    }

                    if (objWidth < 4f) objWidth = 12f;
                    if (objHeight < 4f) objHeight = 12f;

                    Rect objRect = new Rect(pixelX - objWidth / 2f, pixelY - objHeight / 2f, objWidth, objHeight);
                    DrawPrefabPreview(objRect, item.prefab, false);
                }
                else
                {
                    DrawDefaultObjectMarker(pixelX, pixelY, "Null", 12f, 12f);
                }
            }
        }

        Handles.color = Color.gray;
        Handles.DrawWireCube(previewRect.center, previewRect.size);

        EditorGUILayout.EndVertical();
    }

    private void DrawPrefabPreview(Rect rect, GameObject prefab, bool isBackground)
    {
        if (prefab == null) return;

        Color savedColor = GUI.color;
        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            GUI.color = sr.color;

            if (sr.sprite != null && sr.sprite.texture != null)
            {
                Sprite sprite = sr.sprite;
                Rect tr = sprite.textureRect;
                Texture2D tex = sprite.texture;

                Rect uv = new Rect(
                    tr.x / tex.width,
                    tr.y / tex.height,
                    tr.width / tex.width,
                    tr.height / tex.height
                );

                GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
                GUI.color = savedColor;
                return;
            }
        }

        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
        if (preview != null)
        {
            GUI.DrawTexture(rect, preview, isBackground ? ScaleMode.StretchToFill : ScaleMode.ScaleToFit);
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