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
            BuildDeck(mezz.transform, "Deck_L", new Vector3(-22f, deckY, -5f), new Vector3(16f, Stage1LobbyDimensions.MezzanineDeckThickness, 50f), floor);
            BuildDeck(mezz.transform, "Deck_R", new Vector3(22f, deckY, -5f), new Vector3(16f, Stage1LobbyDimensions.MezzanineDeckThickness, 50f), floor);

            // 후면 연결 브릿지 (엘리베이터 구역 위)
            BuildDeck(mezz.transform, "Deck_BackBridge", new Vector3(0f, deckY, 24f), new Vector3(60f, Stage1LobbyDimensions.MezzanineDeckThickness, 12f), floor);

            BuildRailingRun(mezz.transform, "Railing_L_Inner", new Vector3(-14.2f, deckY + deckHalf, -5f), 50f, true, metal, glass);
            BuildRailingRun(mezz.transform, "Railing_R_Inner", new Vector3(14.2f, deckY + deckHalf, -5f), 50f, false, metal, glass);

            // 발코니 스나이퍼 위치 마커 (1-3)
            BuildRailingPost(mezz.transform, "BalconyPost_L", new Vector3(-16f, deckY, -2.5f), metal);
            BuildRailingPost(mezz.transform, "BalconyPost_R", new Vector3(16f, deckY, 2.0f), metal);

            // 메자닌 지지대 / 스텔
            foreach (var x in new[] { -22f, -14f, 14f, 22f })
            {
                foreach (var z in new[] { -15f, 0f, 15f })
                {
                    var brace = CreatePbCube("MezzanineBrace", mezz.transform,
                        new Vector3(x, deckY * 0.5f, z),
                        new Vector3(0.5f, deckY, 0.5f));
                    SetMat(brace, column);
                }
            }

            // 좌·우 계단 (시각적)
            BuildStair(mezz.transform, "Stair_L", new Vector3(-27f, 0f, 12f), floor, metal);
            BuildStair(mezz.transform, "Stair_R", new Vector3(27f, 0f, 12f), floor, metal);

            // 2층 벽 및 전투용 커버 상세
            Build2FDetails(mezz.transform, column, metal, glass);
        }

        static void Build2FDetails(Transform parent, Material wallMat, Material metalMat, Material glassMat)
        {
            var details = new GameObject("Balcony_2F_Details");
            details.transform.SetParent(parent, false);

            var deckY = Stage1LobbyDimensions.MezzanineFloorY;

            // 2층 사무실 파사드 / 보안 구역 게이트 벽 (좌/우)
            // 좌측 2층 벽 분할 구조
            BuildWallSegment(details.transform, "Wall_2F_L_Office1", new Vector3(-28f, deckY + 2f, -20f), new Vector3(0.3f, 4f, 8f), wallMat);
            BuildWallSegment(details.transform, "Wall_2F_L_DoorFrame", new Vector3(-28f, deckY + 2.2f, -12f), new Vector3(0.3f, 4.4f, 3f), metalMat);
            BuildWallSegment(details.transform, "Wall_2F_L_Office2", new Vector3(-28f, deckY + 2f, 0f), new Vector3(0.3f, 4f, 20f), wallMat);

            // 우측 2층 벽 분할 구조
            BuildWallSegment(details.transform, "Wall_2F_R_Office1", new Vector3(28f, deckY + 2f, -20f), new Vector3(0.3f, 4f, 8f), wallMat);
            BuildWallSegment(details.transform, "Wall_2F_R_DoorFrame", new Vector3(28f, deckY + 2.2f, -12f), new Vector3(0.3f, 4.4f, 3f), metalMat);
            BuildWallSegment(details.transform, "Wall_2F_R_Office2", new Vector3(28f, deckY + 2f, 0f), new Vector3(0.3f, 4f, 20f), wallMat);

            // 저격수 엄폐물 (바리케이드 및 보안 방어 쉴드)
            var covL1 = CreatePbCube("Cover_2F_L1", details.transform, new Vector3(-18f, deckY + 0.6f, -10f), new Vector3(1.2f, 1.2f, 2.5f));
            var covL2 = CreatePbCube("Cover_2F_L2", details.transform, new Vector3(-18f, deckY + 0.6f, 5f), new Vector3(1.2f, 1.2f, 2.5f));
            SetMat(covL1, metalMat);
            SetMat(covL2, metalMat);

            var covR1 = CreatePbCube("Cover_2F_R1", details.transform, new Vector3(18f, deckY + 0.6f, -8f), new Vector3(1.2f, 1.2f, 2.5f));
            var covR2 = CreatePbCube("Cover_2F_R2", details.transform, new Vector3(18f, deckY + 0.6f, 8f), new Vector3(1.2f, 1.2f, 2.5f));
            SetMat(covR1, metalMat);
            SetMat(covR2, metalMat);

            // 2층 복도 전등 기둥 및 디테일링 프레임
            var archL = CreatePbCube("ArchFrame_2F_L", details.transform, new Vector3(-22f, deckY + 3.8f, 0f), new Vector3(8f, 0.3f, 0.4f));
            var archR = CreatePbCube("ArchFrame_2F_R", details.transform, new Vector3(22f, deckY + 3.8f, 0f), new Vector3(8f, 0.3f, 0.4f));
            SetMat(archL, metalMat);
            SetMat(archR, metalMat);
        }

        static void BuildRailingRun(Transform parent, string name, Vector3 center, float lengthZ, bool faceRight, Material metal, Material glass)
        {
            var railRoot = new GameObject(name);
            railRoot.transform.SetParent(parent, false);
            railRoot.transform.position = center;

            var postCount = 13;
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

            const int steps = 14;
            var rise = Stage1LobbyDimensions.MezzanineFloorY / steps;
            for (var i = 0; i < steps; i++)
            {
                var step = CreatePbCube("Step", stair.transform,
                    basePos + new Vector3(0f, rise * (i + 0.5f), -i * 0.45f),
                    new Vector3(2.2f, rise, 0.42f));
                SetMat(step, floor);
            }

            var handrail = CreatePbCube("Handrail", stair.transform,
                basePos + new Vector3(1.1f, Stage1LobbyDimensions.MezzanineFloorY * 0.5f, -steps * 0.22f),
                new Vector3(0.05f, Stage1LobbyDimensions.MezzanineFloorY, steps * 0.45f));
            SetMat(handrail, metal);
        }

        static void BuildWaitingSeating(Transform parent, Material chair, Material metal)
        {
            var seating = new GameObject("Waiting_Seating");
            seating.transform.SetParent(parent, false);

            // 1-2: 데스크 좌측 소파 뒤 스폰 (스케일된 로비에 맞추어 X 오프셋 확대)
            BuildSofa(seating.transform, "Sofa_L", new Vector3(-8f, 0.35f, -14f), chair, metal);
            BuildSofa(seating.transform, "Sofa_R", new Vector3(8f, 0.35f, -14f), chair, metal);

            var coffeeTable = new GameObject("CoffeeTable");
            coffeeTable.transform.SetParent(seating.transform, false);
            coffeeTable.transform.position = new Vector3(0f, 0.3f, -14.5f);

            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modern_Table/Modern_Table/scene.gltf");
            if (tablePrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
                inst.name = "Table_Visual";
                inst.transform.SetParent(coffeeTable.transform, false);
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                inst.transform.localScale = Vector3.one * 1.5f; // 테이블 스케일 확대
            }
            else
            {
                var fallback = CreatePbCube("Table_Visual", coffeeTable.transform, Vector3.zero, new Vector3(2.4f, 0.06f, 1.2f));
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
                inst.transform.localScale = Vector3.one * 0.02f; // 소파 스케일 확대
            }
            else
            {
                var seat = CreatePbCube("Seat", sofa.transform, Vector3.zero, new Vector3(3.2f, 0.45f, 1.2f));
                SetMat(seat, chair);
                var back = CreatePbCube("Back", sofa.transform, new Vector3(0f, 0.55f, -0.5f), new Vector3(3.2f, 0.55f, 0.15f));
                SetMat(back, chair);
                var leg = CreatePbCube("Leg", sofa.transform, new Vector3(0f, 0.08f, 0.3f), new Vector3(2.8f, 0.08f, 0.8f));
                SetMat(leg, metal);
            }
        }

        static void BuildElevatorBank(Transform parent, Material wall, Material metal, Material glass)
        {
            var bank = new GameObject("Elevator_Bank");
            bank.transform.SetParent(parent, false);

            var backZ = Stage1LobbyDimensions.BackWallZ - 0.35f;
            var doorHeight = Stage1LobbyDimensions.ElevatorDoorHeight;

            // 후벽 분할 (주차장 개구부 포함, 60m 너비에 맞추어 X 크기 조절)
            BuildWallSegment(bank.transform, "BackWall_Left", new Vector3(-19f, Stage1LobbyDimensions.WallCenterY, Stage1LobbyDimensions.BackWallZ), new Vector3(22f, Stage1LobbyDimensions.WallHeight, 0.35f), wall);
            BuildWallSegment(bank.transform, "BackWall_Right", new Vector3(19f, Stage1LobbyDimensions.WallCenterY, Stage1LobbyDimensions.BackWallZ), new Vector3(22f, Stage1LobbyDimensions.WallHeight, 0.35f), wall);
            BuildWallSegment(bank.transform, "BackWall_Header", new Vector3(0f, Stage1LobbyDimensions.CeilingHeight - 2.5f, Stage1LobbyDimensions.BackWallZ), new Vector3(16f, 5f, 0.35f), wall);

            foreach (var (label, x) in new[] { ("L", -6f), ("C", 0f), ("R", 6f) })
            {
                var doorRoot = new GameObject($"ElevatorDoor_{label}");
                doorRoot.transform.SetParent(bank.transform, false);
                doorRoot.transform.position = new Vector3(x, 0f, backZ);

                var frameL = CreatePbCube("Frame_L", doorRoot.transform, new Vector3(-1.45f, doorHeight * 0.5f, 0f), new Vector3(0.18f, doorHeight, 0.25f));
                var frameR = CreatePbCube("Frame_R", doorRoot.transform, new Vector3(1.45f, doorHeight * 0.5f, 0f), new Vector3(0.18f, doorHeight, 0.25f));
                var frameT = CreatePbCube("Frame_T", doorRoot.transform, new Vector3(0f, doorHeight + 0.12f, 0f), new Vector3(3.1f, 0.24f, 0.25f));
                SetMat(frameL, metal);
                SetMat(frameR, metal);
                SetMat(frameT, metal);

                var door = CreatePbCube("DoorPanel", doorRoot.transform, new Vector3(0f, doorHeight * 0.5f, 0.05f), new Vector3(2.7f, doorHeight - 0.15f, 0.08f));
                SetMat(door, metal);

                var indicator = CreatePbCube("FloorIndicator", doorRoot.transform, new Vector3(1.5f, doorHeight + 0.5f, 0.08f), new Vector3(0.35f, 0.25f, 0.08f));
                SetMat(indicator, glass);
            }
        }

        static void BuildParkingExtension(Transform parent, Material floor, Material metal)
        {
            var parking = new GameObject("Parking_Extension");
            parking.transform.SetParent(parent, false);

            var slab = CreatePbCube("ParkingSlab", parking.transform,
                new Vector3(0f, -0.15f, Stage1LobbyDimensions.BackWallZ + 6f),
                new Vector3(26f, 0.12f, 14f));
            SetMat(slab, floor);

            var ramp = CreatePbCube("ParkingRamp", parking.transform,
                new Vector3(0f, 0.05f, Stage1LobbyDimensions.ParkingStartZ),
                new Vector3(16f, 0.08f, 3.5f));
            ramp.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            SetMat(ramp, floor);

            // 1-6: 차량 뒤 엄폐 L·C·R
            BuildCar(parking.transform, "Car_L", new Vector3(-8f, 0.55f, Stage1LobbyDimensions.BackWallZ + 3.5f), metal);
            BuildCar(parking.transform, "Car_C", new Vector3(0f, 0.55f, Stage1LobbyDimensions.BackWallZ + 4.5f), metal);
            BuildCar(parking.transform, "Car_R", new Vector3(8f, 0.55f, Stage1LobbyDimensions.BackWallZ + 3f), metal);

            var ceiling = CreatePbCube("ParkingCanopy", parking.transform, new Vector3(0f, 6.2f, Stage1LobbyDimensions.BackWallZ + 6f), new Vector3(26f, 0.15f, 14f));
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
                inst.transform.localScale = Vector3.one * 0.02f; // 스케일 확대
            }
            else
            {
                var body = CreatePbCube("Body", car.transform, Vector3.zero, new Vector3(2.6f, 0.95f, 5.2f));
                SetMat(body, metal);
                var cabin = CreatePbCube("Cabin", car.transform, new Vector3(0f, 0.7f, -0.4f), new Vector3(2.2f, 0.75f, 2.6f));
                SetMat(cabin, metal);
            }
        }

        static void BuildBossPlazaGlass(Transform parent, Material glass, Material metal)
        {
            var plaza = new GameObject("BossPlaza_GlassWall");
            plaza.transform.SetParent(parent, false);

            // 1-7: APC가 돌진해 들어오는 유리벽
            var frameL = CreatePbCube("GlassFrame_L", plaza.transform, new Vector3(-8f, 3.5f, 18.5f), new Vector3(0.18f, 6.5f, 0.18f));
            var frameR = CreatePbCube("GlassFrame_R", plaza.transform, new Vector3(8f, 3.5f, 18.5f), new Vector3(0.18f, 6.5f, 0.18f));
            SetMat(frameL, metal);
            SetMat(frameR, metal);

            foreach (var (x, tilt) in new[] { (-5.5f, -10f), (0f, 4f), (5.5f, 12f) })
            {
                var shard = CreatePbCube("GlassShard", plaza.transform, new Vector3(x, 1.8f, 18.45f), new Vector3(1.8f, 3.5f, 0.08f));
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
                SetMat(shard, glass);
            }

            var debris = CreatePbCube("GlassDebris", plaza.transform, new Vector3(1.2f, 0.08f, 17.8f), new Vector3(4.5f, 0.06f, 1.8f));
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
