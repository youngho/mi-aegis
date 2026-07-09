#if UNITY_EDITOR
#nullable enable
using System.IO;
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>
    /// cut_timeline Stage1 스펙에 맞춘 로비 내부 구조물 —
    /// 2층 발코니(1-3), 소파(1-2), 엘리베이터 3문(1-5), 주차장(1-6), 보스 광장 유리벽(1-7).
    /// </summary>
    public static class Stage1LobbyArchitectureSetup
    {
        const string Stage1ScenePath = "Assets/Scenes/Stages/Stage1_Lobby.unity";
        const string ArchitectureRootName = "Architecture_Stage1_Lobby";
        const string MatFolder = "Assets/Materials";

        [MenuItem("Aegis/Build Stage1 Lobby Architecture")]
        public static void BuildFromMenu()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError($"[Stage1LobbyArchitectureSetup] Missing {Stage1ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            var stage1 = GameObject.Find("Stage1_Lobby");
            if (stage1 == null)
            {
                Debug.LogError("[Stage1LobbyArchitectureSetup] Stage1_Lobby root not found.");
                return;
            }

            BuildArchitecture(stage1.transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Stage1LobbyArchitectureSetup] Mezzanine, elevators, parking, and cut props built.");
        }

        public static void BuildArchitecture(Transform stage1)
        {
            var floor = LoadMat("M_Lobby_Floor");
            var wall = LoadMat("M_Lobby_Wall");
            var metal = LoadMat("M_Lobby_Desk");
            var glass = LoadMat("M_Lobby_Glass");
            var column = LoadMat("M_Lobby_Column");
            var chair = LoadMat("M_Lobby_Chair");

            var root = GetOrCreateRoot(stage1, ArchitectureRootName);
            ClearChildren(root.transform);

            OpenBackWallForParking(stage1);
            BuildMezzanine(root.transform, floor, metal, glass, column);
            BuildWaitingSeating(root.transform, chair, metal);
            BuildElevatorBank(root.transform, wall, metal, glass);
            BuildParkingExtension(root.transform, floor, metal);
            BuildBossPlazaGlass(root.transform, glass, metal);
        }

        static void OpenBackWallForParking(Transform stage1)
        {
            var env = stage1.Find("Environment_Stage1_Lobby");
            if (env == null)
                return;

            Transform? backWall = null;
            foreach (Transform child in env)
            {
                if (child.name.Contains("Wall_Back"))
                {
                    backWall = child;
                    break;
                }
            }

            if (backWall == null)
                return;

            backWall.gameObject.SetActive(false);
        }

        static void BuildMezzanine(Transform parent, Material floor, Material metal, Material glass, Material column)
        {
            var mezz = new GameObject("Mezzanine_2F");
            mezz.transform.SetParent(parent, false);

            var deckY = Stage1LobbyDimensions.MezzanineFloorY;
            var deckHalf = Stage1LobbyDimensions.MezzanineDeckThickness * 0.5f;

            // 좌·우 2층 발코니 덱 (중앙 아트리움 공간은 비움)
            BuildDeck(mezz.transform, "Deck_L", new Vector3(-10f, deckY, 0f), new Vector3(8f, Stage1LobbyDimensions.MezzanineDeckThickness, 20f), floor);
            BuildDeck(mezz.transform, "Deck_R", new Vector3(10f, deckY, 0f), new Vector3(8f, Stage1LobbyDimensions.MezzanineDeckThickness, 20f), floor);

            // 후면 연결 브릿지 (엘리베이터 구역 위)
            BuildDeck(mezz.transform, "Deck_BackBridge", new Vector3(0f, deckY, 12f), new Vector3(18f, Stage1LobbyDimensions.MezzanineDeckThickness, 4f), floor);

            BuildRailingRun(mezz.transform, "Railing_L_Inner", new Vector3(-6.2f, deckY + deckHalf, 0f), 20f, true, metal, glass);
            BuildRailingRun(mezz.transform, "Railing_R_Inner", new Vector3(6.2f, deckY + deckHalf, 0f), 20f, false, metal, glass);

            // 발코니 스나이퍼 위치 마커 (1-3)
            BuildRailingPost(mezz.transform, "BalconyPost_L", new Vector3(-8f, deckY, 0f), metal);
            BuildRailingPost(mezz.transform, "BalconyPost_R", new Vector3(8f, deckY, 0f), metal);

            // 메자닌 지지대 / 스텔
            foreach (var x in new[] { -10f, -6f, 10f, 6f })
            {
                var brace = CreatePbCube("MezzanineBrace", mezz.transform,
                    new Vector3(x, deckY * 0.5f, -4f),
                    new Vector3(0.35f, deckY, 0.35f));
                SetMat(brace, column);
            }

            // 좌·우 계단 (시각적)
            BuildStair(mezz.transform, "Stair_L", new Vector3(-12.5f, 0f, 8f), floor, metal);
            BuildStair(mezz.transform, "Stair_R", new Vector3(12.5f, 0f, 8f), floor, metal);
        }

        static void BuildRailingRun(Transform parent, string name, Vector3 center, float lengthZ, bool faceRight, Material metal, Material glass)
        {
            var railRoot = new GameObject(name);
            railRoot.transform.SetParent(parent, false);
            railRoot.transform.position = center;

            var postCount = 9;
            for (var i = 0; i < postCount; i++)
            {
                var t = postCount <= 1 ? 0.5f : i / (float)(postCount - 1);
                var z = Mathf.Lerp(-lengthZ * 0.5f, lengthZ * 0.5f, t);
                var post = CreatePbCube("Post", railRoot.transform, new Vector3(0f, 0.55f, z), new Vector3(0.08f, 1.1f, 0.08f));
                SetMat(post, metal);
            }

            var panel = CreatePbCube("GlassPanel", railRoot.transform, new Vector3(faceRight ? 0.04f : -0.04f, 0.65f, 0f), new Vector3(0.04f, 0.95f, lengthZ - 0.2f));
            SetMat(panel, glass);

            var topRail = CreatePbCube("TopRail", railRoot.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.06f, 0.06f, lengthZ));
            SetMat(topRail, metal);
        }

        static void BuildRailingPost(Transform parent, string name, Vector3 worldPos, Material metal)
        {
            var post = CreatePbCube(name, parent, worldPos + Vector3.up * 0.55f, new Vector3(0.12f, 1.1f, 0.12f));
            SetMat(post, metal);
        }

        static void BuildStair(Transform parent, string name, Vector3 basePos, Material floor, Material metal)
        {
            var stair = new GameObject(name);
            stair.transform.SetParent(parent, false);

            const int steps = 10;
            var rise = Stage1LobbyDimensions.MezzanineFloorY / steps;
            for (var i = 0; i < steps; i++)
            {
                var step = CreatePbCube("Step", stair.transform,
                    basePos + new Vector3(0f, rise * (i + 0.5f), -i * 0.45f),
                    new Vector3(1.8f, rise, 0.42f));
                SetMat(step, floor);
            }

            var handrail = CreatePbCube("Handrail", stair.transform,
                basePos + new Vector3(0.9f, Stage1LobbyDimensions.MezzanineFloorY * 0.5f, -steps * 0.22f),
                new Vector3(0.05f, Stage1LobbyDimensions.MezzanineFloorY, steps * 0.45f));
            SetMat(handrail, metal);
        }

        static void BuildWaitingSeating(Transform parent, Material chair, Material metal)
        {
            var seating = new GameObject("Waiting_Seating");
            seating.transform.SetParent(parent, false);

            // 1-2: 데스크 좌측 소파 뒤 스폰
            BuildSofa(seating.transform, "Sofa_L", new Vector3(-3.5f, 0.35f, -7f), chair, metal);
            BuildSofa(seating.transform, "Sofa_R", new Vector3(3.5f, 0.35f, -7f), chair, metal);

            var coffeeTable = new GameObject("CoffeeTable");
            coffeeTable.transform.SetParent(seating.transform, false);
            coffeeTable.transform.position = new Vector3(0f, 0.3f, -7.5f);

            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Noguchi_Table/Noguchi_Table/scene.gltf");
            if (tablePrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
                inst.name = "Table_Visual";
                inst.transform.SetParent(coffeeTable.transform, false);
                inst.transform.localPosition = Vector3.zero;
                // Correct glTF axis rotation offset
                inst.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                inst.transform.localScale = Vector3.one * 1.35f;
            }
            else
            {
                var fallback = CreatePbCube("Table_Visual", coffeeTable.transform, Vector3.zero, new Vector3(1.6f, 0.06f, 0.8f));
                SetMat(fallback, metal);
            }
        }

        static void BuildSofa(Transform parent, string name, Vector3 pos, Material chair, Material metal)
        {
            var sofa = new GameObject(name);
            sofa.transform.SetParent(parent, false);
            sofa.transform.position = pos;

            var sofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Lobby_Sofa/scene.gltf");
            if (sofaPrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(sofaPrefab);
                inst.name = "Sofa_Visual";
                inst.transform.SetParent(sofa.transform, false);
                inst.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                inst.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                inst.transform.localScale = Vector3.one * 0.013f;
            }
            else
            {
                var seat = CreatePbCube("Seat", sofa.transform, Vector3.zero, new Vector3(2.2f, 0.45f, 0.85f));
                SetMat(seat, chair);
                var back = CreatePbCube("Back", sofa.transform, new Vector3(0f, 0.55f, -0.38f), new Vector3(2.2f, 0.55f, 0.12f));
                SetMat(back, chair);
                var leg = CreatePbCube("Leg", sofa.transform, new Vector3(0f, 0.08f, 0.2f), new Vector3(2f, 0.08f, 0.6f));
                SetMat(leg, metal);
            }
        }

        static void BuildElevatorBank(Transform parent, Material wall, Material metal, Material glass)
        {
            var bank = new GameObject("Elevator_Bank");
            bank.transform.SetParent(parent, false);

            var backZ = Stage1LobbyDimensions.BackWallZ - 0.35f;
            var doorHeight = Stage1LobbyDimensions.ElevatorDoorHeight;

            // 후벽 분할 (주차장 개구부 포함)
            BuildWallSegment(bank.transform, "BackWall_Left", new Vector3(-11f, Stage1LobbyDimensions.WallCenterY, Stage1LobbyDimensions.BackWallZ), new Vector3(6f, Stage1LobbyDimensions.WallHeight, 0.35f), wall);
            BuildWallSegment(bank.transform, "BackWall_Right", new Vector3(11f, Stage1LobbyDimensions.WallCenterY, Stage1LobbyDimensions.BackWallZ), new Vector3(6f, Stage1LobbyDimensions.WallHeight, 0.35f), wall);
            BuildWallSegment(bank.transform, "BackWall_Header", new Vector3(0f, Stage1LobbyDimensions.CeilingHeight - 0.75f, Stage1LobbyDimensions.BackWallZ), new Vector3(12f, 1.5f, 0.35f), wall);

            foreach (var (label, x) in new[] { ("L", -4.5f), ("C", 0f), ("R", 4.5f) })
            {
                var doorRoot = new GameObject($"ElevatorDoor_{label}");
                doorRoot.transform.SetParent(bank.transform, false);
                doorRoot.transform.position = new Vector3(x, 0f, backZ);

                var frameL = CreatePbCube("Frame_L", doorRoot.transform, new Vector3(-1.05f, doorHeight * 0.5f, 0f), new Vector3(0.12f, doorHeight, 0.18f));
                var frameR = CreatePbCube("Frame_R", doorRoot.transform, new Vector3(1.05f, doorHeight * 0.5f, 0f), new Vector3(0.12f, doorHeight, 0.18f));
                var frameT = CreatePbCube("Frame_T", doorRoot.transform, new Vector3(0f, doorHeight + 0.08f, 0f), new Vector3(2.2f, 0.16f, 0.18f));
                SetMat(frameL, metal);
                SetMat(frameR, metal);
                SetMat(frameT, metal);

                var door = CreatePbCube("DoorPanel", doorRoot.transform, new Vector3(0f, doorHeight * 0.5f, 0.05f), new Vector3(1.85f, doorHeight - 0.1f, 0.06f));
                SetMat(door, metal);

                var indicator = CreatePbCube("FloorIndicator", doorRoot.transform, new Vector3(1.15f, doorHeight + 0.35f, 0.06f), new Vector3(0.25f, 0.18f, 0.06f));
                SetMat(indicator, glass);
            }
        }

        static void BuildParkingExtension(Transform parent, Material floor, Material metal)
        {
            var parking = new GameObject("Parking_Extension");
            parking.transform.SetParent(parent, false);

            var slab = CreatePbCube("ParkingSlab", parking.transform,
                new Vector3(0f, -0.15f, 19f),
                new Vector3(14f, 0.12f, 10f));
            SetMat(slab, floor);

            var ramp = CreatePbCube("ParkingRamp", parking.transform,
                new Vector3(0f, 0.05f, Stage1LobbyDimensions.ParkingStartZ),
                new Vector3(10f, 0.08f, 2.5f));
            ramp.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            SetMat(ramp, floor);

            // 1-6: 차량 뒤 엄폐 L·C·R
            BuildCar(parking.transform, "Car_L", new Vector3(-5f, 0.55f, 16.5f), metal);
            BuildCar(parking.transform, "Car_C", new Vector3(0f, 0.55f, 17.5f), metal);
            BuildCar(parking.transform, "Car_R", new Vector3(5f, 0.55f, 16f), metal);

            var ceiling = CreatePbCube("ParkingCanopy", parking.transform, new Vector3(0f, 4.2f, 19f), new Vector3(14f, 0.15f, 10f));
            SetMat(ceiling, metal);
        }

        static void BuildCar(Transform parent, string name, Vector3 pos, Material metal)
        {
            var car = new GameObject(name);
            car.transform.SetParent(parent, false);
            car.transform.position = pos;

            var carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Lobby_Car/scene.gltf");
            if (carPrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
                inst.name = "Car_Visual";
                inst.transform.SetParent(car.transform, false);
                inst.transform.localPosition = new Vector3(0f, -0.55f, 0f);
                inst.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                inst.transform.localScale = Vector3.one * 0.013f;
            }
            else
            {
                var body = CreatePbCube("Body", car.transform, Vector3.zero, new Vector3(1.9f, 0.75f, 4.2f));
                SetMat(body, metal);
                var cabin = CreatePbCube("Cabin", car.transform, new Vector3(0f, 0.55f, -0.3f), new Vector3(1.7f, 0.55f, 2f));
                SetMat(cabin, metal);
            }
        }

        static void BuildBossPlazaGlass(Transform parent, Material glass, Material metal)
        {
            var plaza = new GameObject("BossPlaza_GlassWall");
            plaza.transform.SetParent(parent, false);

            // 1-7: APC가 돌진해 들어오는 유리벽 (파손 연출 — 좌·우 잔해)
            var frameL = CreatePbCube("GlassFrame_L", plaza.transform, new Vector3(-5f, 2.5f, 11.5f), new Vector3(0.12f, 4.5f, 0.12f));
            var frameR = CreatePbCube("GlassFrame_R", plaza.transform, new Vector3(5f, 2.5f, 11.5f), new Vector3(0.12f, 4.5f, 0.12f));
            SetMat(frameL, metal);
            SetMat(frameR, metal);

            foreach (var (x, tilt) in new[] { (-3.5f, -10f), (0f, 4f), (3.5f, 12f) })
            {
                var shard = CreatePbCube("GlassShard", plaza.transform, new Vector3(x, 1.2f, 11.45f), new Vector3(1.1f, 2.2f, 0.05f));
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
                SetMat(shard, glass);
            }

            var debris = CreatePbCube("GlassDebris", plaza.transform, new Vector3(0.8f, 0.08f, 10.8f), new Vector3(2.5f, 0.06f, 1.2f));
            SetMat(debris, glass);
        }

        static void BuildDeck(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var deck = CreatePbCube(name, parent, pos, scale);
            SetMat(deck, mat);
        }

        static void BuildWallSegment(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var wall = CreatePbCube(name, parent, pos, scale);
            SetMat(wall, mat);
        }

        static GameObject CreatePbCube(string name, Transform parent, Vector3 worldPos, Vector3 scale)
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
            var go = pb.gameObject;
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            return go;
        }

        static GameObject GetOrCreateRoot(Transform stage1, string name)
        {
            var existing = stage1.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(stage1, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        static Material? LoadMat(string name)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MatFolder}/{name}.mat");
            if (mat == null)
                mat = AssetDatabase.LoadAssetAtPath<Material>($"{MatFolder}/M_Lobby_Wall.mat");
            return mat;
        }

        static void SetMat(GameObject go, Material? mat)
        {
            if (mat == null)
                return;

            var r = go.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = mat;
        }
    }
}
#endif
