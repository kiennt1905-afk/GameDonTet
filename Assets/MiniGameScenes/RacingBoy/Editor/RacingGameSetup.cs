#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class RacingGameSetup
{
    [MenuItem("Tools/Tạo UI & Logic Đua Xe (Racing)")]
    public static void SetupRacingGame()
    {
        // 1. Setup Camera & Light
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCam.transform.position = new Vector3(0, 5, -8);
        mainCam.transform.rotation = Quaternion.Euler(25, 0, 0);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.2f, 0.6f, 0.8f); // Màu bầu trời

        if (Object.FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        // 2. Setup Canvas & EventSystem
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 3. Xóa GameManager và UI cũ nếu chạy lại tool
        GameObject oldManager = GameObject.Find("RacingGameManager");
        if (oldManager != null) Object.DestroyImmediate(oldManager);

        GameObject oldUI = GameObject.Find("RacingUI");
        if (oldUI != null) Object.DestroyImmediate(oldUI);

        GameObject gameManagerObj = new GameObject("RacingGameManager");
        RacingGameManager gameManager = gameManagerObj.AddComponent<RacingGameManager>();

        // Tạo 1 GameObject cha để chứa toàn bộ UI của Racing Game (để dễ xóa khi chạy lại tool)
        GameObject racingUI = new GameObject("RacingUI");
        racingUI.transform.SetParent(canvas.transform, false);
        RectTransform rUIRect = racingUI.AddComponent<RectTransform>();
        rUIRect.anchorMin = Vector2.zero; rUIRect.anchorMax = Vector2.one; rUIRect.sizeDelta = Vector2.zero;

        // 4. Tạo UI Panels
        // Start Panel
        GameObject startPanel = new GameObject("StartPanel");
        startPanel.transform.SetParent(racingUI.transform, false);
        RectTransform spRect = startPanel.AddComponent<RectTransform>();
        spRect.anchorMin = Vector2.zero; spRect.anchorMax = Vector2.one; spRect.sizeDelta = Vector2.zero;
        Image spImg = startPanel.AddComponent<Image>();
        spImg.color = new Color(0, 0, 0, 0.8f);
        
        GameObject startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(startPanel.transform, false);
        startBtnObj.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
        startBtnObj.AddComponent<Image>().color = Color.green;
        Button startBtn = startBtnObj.AddComponent<Button>();
        
        GameObject startTextObj = new GameObject("Text");
        startTextObj.transform.SetParent(startBtnObj.transform, false);
        TextMeshProUGUI startText = startTextObj.AddComponent<TextMeshProUGUI>();
        startText.text = "BẮT ĐẦU ĐUA";
        startText.color = Color.white;
        startText.alignment = TextAlignmentOptions.Center;
        startText.fontSize = 32;
        startText.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
        
        // Gắn sự kiện nút Start
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(startBtn.onClick, new UnityAction(gameManager.StartGame));

        // Game Over Panel
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(racingUI.transform, false);
        RectTransform goRect = goPanel.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero; goRect.anchorMax = Vector2.one; goRect.sizeDelta = Vector2.zero;
        Image goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0f, 0f, 0f, 0.2f); // Nền đen 20%

        GameObject goTextObj = new GameObject("GameOverText");
        goTextObj.transform.SetParent(goPanel.transform, false);
        TextMeshProUGUI goText = goTextObj.AddComponent<TextMeshProUGUI>();
        goText.text = "GẶP TAI NẠN";
        goText.color = Color.white;
        goText.alignment = TextAlignmentOptions.Center;
        goText.fontSize = 60;
        goText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        goText.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 200);

        GameObject retryBtnObj = new GameObject("RetryButton");
        retryBtnObj.transform.SetParent(goPanel.transform, false);
        retryBtnObj.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
        retryBtnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        retryBtnObj.AddComponent<Image>().color = Color.yellow;
        Button retryBtn = retryBtnObj.AddComponent<Button>();
        
        GameObject retryTextObj = new GameObject("Text");
        retryTextObj.transform.SetParent(retryBtnObj.transform, false);
        TextMeshProUGUI retryText = retryTextObj.AddComponent<TextMeshProUGUI>();
        retryText.text = "LÀM LẠI CUỘC ĐỜI";
        retryText.color = Color.black;
        retryText.alignment = TextAlignmentOptions.Center;
        retryText.fontSize = 32;
        retryText.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
        
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(retryBtn.onClick, new UnityAction(gameManager.RestartGame));

        // Score Text
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(racingUI.transform, false);
        TextMeshProUGUI scoreTmp = scoreTextObj.AddComponent<TextMeshProUGUI>();
        scoreTmp.text = "15";
        scoreTmp.color = Color.yellow;
        scoreTmp.fontSize = 40;
        RectTransform scoreRect = scoreTmp.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1); scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(20, -20);
        scoreRect.sizeDelta = new Vector2(300, 60);

        // Đảm bảo các ảnh được import dưới dạng Sprite (2D and UI) thay vì Default Texture
        string[] spritePaths = {
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/40kmh.png",
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/42kmh.png",
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/44kmh.png",
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/53kmh.png",
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/58kmh.png",
            "Assets/MiniGameScenes/RacingBoy/StreetRoad/60kmh.png"
        };
        foreach (string p in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }

        // Load Sprites Tốc độ
        Sprite sp40 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[0]);
        Sprite sp42 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[1]);
        Sprite sp44 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[2]);
        Sprite sp53 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[3]);
        Sprite sp58 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[4]);
        Sprite sp60 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[5]);
        
        // Tạo Speedometer UI ở góc dưới bên phải
        GameObject speedObj = new GameObject("Speedometer");
        speedObj.transform.SetParent(racingUI.transform, false);
        Image speedImg = speedObj.AddComponent<Image>();
        speedImg.sprite = sp60; // Hình mặc định
        RectTransform speedRect = speedObj.GetComponent<RectTransform>();
        speedRect.anchorMin = new Vector2(1, 0); 
        speedRect.anchorMax = new Vector2(1, 0);
        speedRect.pivot = new Vector2(1, 0);
        speedRect.anchoredPosition = new Vector2(-20, 20); // Cách lề 20px
        speedRect.sizeDelta = new Vector2(150, 150);

        SpeedometerUI speedScript = speedObj.AddComponent<SpeedometerUI>();
        speedScript.speedometerImage = speedImg;
        speedScript.speedSprites = new Sprite[] { sp40, sp42, sp44, sp53, sp58, sp60 };

        gameManager.startPanel = startPanel;
        gameManager.gameOverPanel = goPanel;
        gameManager.gameOverText = goText;
        gameManager.scoreText = scoreTmp;
        
        AudioClip newsClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/MiniGameScenes/RacingBoy/Audio/news.mp3");
        AudioClip shutdownClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/MiniGameScenes/RacingBoy/Audio/shutingdown.mp3");
        GameObject wreckModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/RacingBoy/red_car_wreck.glb");

        AudioClip tuyenTruyenClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/MiniGameScenes/RacingBoy/Audio/tuyentruyen.mp3");

        // Setup âm thanh cho GameManager
        AudioSource managerAudio = gameManagerObj.AddComponent<AudioSource>();
        managerAudio.playOnAwake = false;
        gameManager.audioSource = managerAudio;
        gameManager.newsClip = newsClip;
        gameManager.shutdownClip = shutdownClip;
        gameManager.tuyenTruyenClip = tuyenTruyenClip;
        
        // Setup xác xe
        if (wreckModel != null)
        {
            GameObject wreckInstance = (GameObject)PrefabUtility.InstantiatePrefab(wreckModel);
            wreckInstance.name = "RedCarWreck";
            wreckInstance.transform.SetParent(mainCam.transform);
            // Thông số từ user
            wreckInstance.transform.localPosition = new Vector3(-106.61f, -917.56f, -380.48f); 
            wreckInstance.transform.localRotation = Quaternion.Euler(-90f, 0f, 90f); 
            wreckInstance.transform.localScale = new Vector3(213.0947f, 213.0947f, 213.0947f); 
            wreckInstance.SetActive(false); // Ẩn mặc định
            gameManager.wreckInstance = wreckInstance;
        }

        goPanel.SetActive(false);

        // 5. Load Models & Audio
        GameObject carModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/RacingBoy/racing_car.glb");
        GameObject truckModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/RacingBoy/npc_truck.glb");
        GameObject roadModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniGameScenes/RacingBoy/street_road.glb");
        
        AudioClip crashClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/MiniGameScenes/RacingBoy/Audio/car_crash.mp3");
        AudioClip driftClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/MiniGameScenes/RacingBoy/Audio/car_drift.mp3");

        // --- Xóa môi trường cũ ---
        GameObject oldEnv = GameObject.Find("Environment");
        if (oldEnv != null) Object.DestroyImmediate(oldEnv);
        GameObject env = new GameObject("Environment");

        // --- Setup Đường (Road) ---
        if (roadModel != null)
        {
            GameObject road1 = (GameObject)PrefabUtility.InstantiatePrefab(roadModel);
            road1.name = "StreetRoad_1";
            road1.transform.SetParent(env.transform);
            road1.transform.position = new Vector3(4.3f, 0, 0);
            RoadScroller scroller1 = road1.AddComponent<RoadScroller>();
            scroller1.roadLength = 100f; // Chỉnh sửa lại tùy theo độ dài thực tế của model đường
            
            GameObject road2 = (GameObject)PrefabUtility.InstantiatePrefab(roadModel);
            road2.name = "StreetRoad_2";
            road2.transform.SetParent(env.transform);
            road2.transform.position = new Vector3(4.3f, 0, 100f); // Chỉnh sửa lại tùy theo độ dài
            RoadScroller scroller2 = road2.AddComponent<RoadScroller>();
            scroller2.roadLength = 100f;

            // Lề đường (Wall) để không cho xe chạy ra ngoài
            GameObject leftWall = new GameObject("LeftWall");
            leftWall.transform.SetParent(env.transform);
            leftWall.transform.position = new Vector3(-9f, 0, 50f);
            BoxCollider lwCol = leftWall.AddComponent<BoxCollider>();
            lwCol.size = new Vector3(1, 10, 200);
            lwCol.isTrigger = true;
            leftWall.tag = "Wall";

            GameObject rightWall = new GameObject("RightWall");
            rightWall.transform.SetParent(env.transform);
            rightWall.transform.position = new Vector3(9f, 0, 50f);
            BoxCollider rwCol = rightWall.AddComponent<BoxCollider>();
            rwCol.size = new Vector3(1, 10, 200);
            rwCol.isTrigger = true;
            rightWall.tag = "Wall";
        }

        // --- Setup Xe Người Chơi ---
        if (carModel != null)
        {
            GameObject carHitbox = new GameObject("RacingCar");
            carHitbox.transform.SetParent(env.transform);
            carHitbox.transform.position = new Vector3(0, 0, 0); // Vị trí xuất phát

            GameObject carMesh = (GameObject)PrefabUtility.InstantiatePrefab(carModel);
            carMesh.name = "car_mesh";
            carMesh.transform.SetParent(carHitbox.transform);
            carMesh.transform.localPosition = Vector3.zero;
            carMesh.transform.localRotation = Quaternion.Euler(-90, 0, -90);
            carMesh.transform.localScale = new Vector3(5, 5, 5);

            // Xóa toàn bộ Animator để tránh lỗi model tự reset tọa độ (Root Motion)
            Animator[] animCars = carMesh.GetComponentsInChildren<Animator>();
            foreach (var a in animCars) Object.DestroyImmediate(a);

            // Tự động Căn giữa (Center) Mesh vào Hitbox phòng trường hợp model 3D bị lệch tâm
            Renderer[] carRenderers = carMesh.GetComponentsInChildren<Renderer>();
            if (carRenderers.Length > 0)
            {
                Bounds bounds = carRenderers[0].bounds;
                foreach (var r in carRenderers) bounds.Encapsulate(r.bounds);
                Vector3 diff = carHitbox.transform.position - bounds.center;
                carMesh.transform.position += new Vector3(diff.x, 0, diff.z); // Chỉ căn giữa X và Z
            }

            Rigidbody rb = carHitbox.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Không cần rớt xuống do trọng lực vì game giả lập

            BoxCollider col = carHitbox.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2f, 1.5f, 4f);
            col.center = new Vector3(0, 0.75f, 0);
            
            RacingCarController carController = carHitbox.AddComponent<RacingCarController>();
            carController.boundaryX = 4f; // Giới hạn đường đua

            if (crashClip != null)
            {
                AudioSource crashSrc = carHitbox.AddComponent<AudioSource>();
                crashSrc.clip = crashClip;
                crashSrc.playOnAwake = false;
                carController.crashSource = crashSrc;
            }

            if (driftClip != null)
            {
                AudioSource driftSrc = carHitbox.AddComponent<AudioSource>();
                driftSrc.clip = driftClip;
                driftSrc.playOnAwake = false;
                driftSrc.loop = true;
                driftSrc.volume = 0.33f;
                carController.driftSource = driftSrc;
            }
        }

        // --- Setup Spawner & Truck Prefab ---
        if (truckModel != null)
        {
            GameObject spawnerObj = new GameObject("ObstacleSpawner");
            spawnerObj.transform.SetParent(env.transform);
            spawnerObj.transform.position = new Vector3(0, 0, 80f); // Spawn ở tít xa
            
            ObstacleSpawner spawner = spawnerObj.AddComponent<ObstacleSpawner>();

            // Tạo Empty GameObject làm Hitbox (Parent)
            GameObject truckHitbox = new GameObject("NPC_Truck_Template");
            truckHitbox.transform.SetParent(env.transform);
            truckHitbox.transform.position = new Vector3(0, -100, 0); // Giấu đi
            truckHitbox.transform.localScale = new Vector3(2f, 2f, 2f);

            // Tạo model và gắn vào Hitbox làm Child
            GameObject truckMesh = (GameObject)PrefabUtility.InstantiatePrefab(truckModel);
            truckMesh.name = "car.fbx"; // Đặt tên model con
            truckMesh.transform.SetParent(truckHitbox.transform);
            truckMesh.transform.localPosition = Vector3.zero;
            truckMesh.transform.localRotation = Quaternion.Euler(-90, 0, -90);
            truckMesh.transform.localScale = new Vector3(2f, 2f, 2f);

            // Xóa toàn bộ Animator để tránh lỗi model tự reset tọa độ về 0,0,0
            Animator[] animTrucks = truckMesh.GetComponentsInChildren<Animator>();
            foreach (var a in animTrucks) Object.DestroyImmediate(a);

            // Tự động Căn giữa (Center) Mesh vào Hitbox phòng trường hợp model 3D bị lệch tâm
            Renderer[] truckRenderers = truckMesh.GetComponentsInChildren<Renderer>();
            if (truckRenderers.Length > 0)
            {
                Bounds bounds = truckRenderers[0].bounds;
                foreach (var r in truckRenderers) bounds.Encapsulate(r.bounds);
                Vector3 diff = truckHitbox.transform.position - bounds.center;
                truckMesh.transform.position += new Vector3(diff.x, 0, diff.z); // Chỉ căn giữa X và Z
            }
            
            BoxCollider truckCol = truckHitbox.AddComponent<BoxCollider>();
            truckCol.isTrigger = true;
            truckCol.size = new Vector3(2.5f, 3f, 6f);
            truckCol.center = new Vector3(0, 1.5f, 0);
            
            Rigidbody trb = truckHitbox.AddComponent<Rigidbody>();
            trb.isKinematic = true;

            ObstacleMover mover = truckHitbox.AddComponent<ObstacleMover>();
            mover.truckSpeed = 12f;
            
            truckHitbox.tag = "Obstacle";
            truckHitbox.SetActive(false);

            spawner.obstaclePrefab = truckHitbox;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy npc_truck.glb !");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>Đã setup hoàn tất logic MiniGame Đua Xe!</color>");
    }
}
#endif
