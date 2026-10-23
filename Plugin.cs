using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;



namespace BobbysMods
{
    [BepInPlugin(Id, "Bobbys Mods", "0.6.0")]
    
    
    public sealed partial class Plugin : BaseUnityPlugin
    {
        public const string Id = "bobby.gorillatag.mods";
        internal static Plugin Current;
        readonly Dictionary<string, string> settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, bool> previous = new Dictionary<string, bool>();
        readonly List<Text> rows = new List<Text>();
        readonly List<Image> rowImages = new List<Image>();
        GameObject menu;
        GameObject[] platforms = new GameObject[2];
        LineRenderer line;
        Material neon;
        Harmony harmony;
        bool gripPlatforms, triggerPlatforms, teleport, smartArms;
        readonly FreezeState freezeState = new FreezeState();
        readonly GhostVisual ghostVisual = new GhostVisual();
        readonly FrozenAvatar frozenAvatar = new FrozenAvatar();
        readonly FreezeKey freezeKey = new FreezeKey();
        bool frozen => freezeState.Enabled;
        bool freezeSupported;
        bool menuVisible = false;
        readonly MenuTouch touch = new MenuTouch();
        readonly GameObject[] dots = new GameObject[1];
        Vector3 pointerTouchPosition;
        Sprite pointerSprite;
        Texture2D pointerTexture;
        readonly RaycastHit[] aimHits = new RaycastHit[64];
        readonly Collider[] overlapHits = new Collider[64];
        Font menuFont;
        Camera cachedCamera;
        static readonly FieldInfo CameraField = AccessTools.Field(typeof(GorillaLocomotion.GTPlayer), "mainCamera");
        static readonly FieldInfo OfflineRigField = AccessTools.Field(typeof(GorillaTagger), "offlineVRRig");
        Color buttonColour, enabledColour, pointerColour;
        float nextTeleport;
        string message = "";
        string configPath;
        string profilePath, activeLobbyKey, activeLobbyName, activeLobbyMode;
        int savedProfileMask;
        readonly LobbyProfiles lobbyProfiles = new LobbyProfiles();
        Component frozenRig;
        bool AllLobbies => bool.TryParse(S("work_in_all_lobbies", "true"), out bool enabled) && enabled;
        public bool Allowed
        {
            get
            {
                try
                {
                    var network = NetworkSystem.Instance;
                    return network != null && FreezeState.RoomAllowed(network.InRoom, network.SessionIsPrivate, network.GameModeString, AllLobbies);
                }
                catch { return false; }
            }
        }
        void Awake()
        {
            Current = this;
            var pluginDirectory = Path.GetDirectoryName(Info.Location);
            // The DLL may live in plugins/ or a subfolder; configuration has one stable home.
            configPath = Path.Combine(Paths.PluginPath, "BobbysMods", "config.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            if (!File.Exists(configPath))
            {
                string adjacent = Path.Combine(pluginDirectory, "config.txt");
                if (File.Exists(adjacent)) File.Copy(adjacent, configPath);
            }
            Logger.LogInfo("Bobbys Mods config: " + configPath);
            Reload();
            profilePath = Path.Combine(Path.GetDirectoryName(configPath), "lobby-settings.txt");
            try { if(File.Exists(profilePath)) lobbyProfiles.Read(File.ReadAllText(profilePath)); } catch(Exception e) { Logger.LogWarning("Could not read lobby settings: "+e.Message); }
            neon = new Material(Shader.Find("GUI/Text Shader") ?? Shader.Find("Sprites/Default"));
            harmony = new Harmony(Id);
            MethodInfo serialize = AccessTools.DeclaredMethod(typeof(VRRig), "SerializeWriteShared", Type.EmptyTypes);
            freezeSupported = serialize != null && serialize.ReturnType == typeof(InputStruct);
            if (freezeSupported)
            {
                try
                {
                    harmony.Patch(serialize, postfix: new HarmonyMethod(typeof(Plugin), nameof(FreezeOutgoingPose)));
                    Logger.LogInfo("Avatar freeze attached to VRRig.SerializeWriteShared");
                }
                catch (Exception e) { freezeSupported = false; Logger.LogError(e); }
            }
            if (!freezeSupported) message = "Avatar freeze unavailable on this version";
            menuFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var handOffset = AccessTools.DeclaredMethod(typeof(GorillaLocomotion.GTPlayer), "PositionWithOffset", new[] { typeof(Transform), typeof(Vector3) });
            if (handOffset != null) harmony.Patch(handOffset, postfix: new HarmonyMethod(typeof(Plugin), nameof(ExtendHands)));
            BuildMenu();
        }
         public void Joined(string mode) { ResetFeatures(); }
         public void Left(string mode) { ResetFeatures(); }
        string S(string key, string fallback) => settings.TryGetValue(key, out string value) ? value : fallback;
        float N(string key, float fallback, float min, float max) => float.TryParse(S(key, ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && !float.IsNaN(value) && !float.IsInfinity(value) ? Mathf.Clamp(value, min, max) : fallback;
        Color C(string key, string fallback) => ColorUtility.TryParseHtmlString(S(key, fallback), out Color colour) ? colour : Color.white;
        void Reload()
        {
            try
            {
                var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string raw in File.ReadAllLines(configPath))
                {
                    string value = raw.Trim();
                    if (value.Length == 0 || value.StartsWith("#")) continue;
                    int split = value.IndexOf('=');
                    if (split > 0) parsed[value.Substring(0, split).Trim()] = value.Substring(split + 1).Trim();
                }
                settings.Clear(); foreach (var entry in parsed) settings[entry.Key] = entry.Value;
                previous.Clear(); message = "Config loaded";
                buttonColour = C("menu_button_colour", "#802EFF"); enabledColour = C("menu_enabled_colour", "#39FF88"); pointerColour = C("pointer_colour", "#00FFFF");
            }
            catch (Exception e) { message = "Config could not load; using current settings"; Logger.LogWarning(e.Message); }
        }
        bool Held(string binding)
        {
            if (binding == "none") return false;
            var poller = ControllerInputPoller.instance;
            if (poller)
            {
                switch (binding)
                {
                    case "left_x": return poller.leftControllerPrimaryButton;
                    case "left_y": return poller.leftControllerSecondaryButton;
                    case "right_a": return poller.rightControllerPrimaryButton;
                    case "right_b": return poller.rightControllerSecondaryButton;
                    case "left_grip": return ModRules.AnalogPressed(poller.leftControllerGripFloat);
                    case "right_grip": return ModRules.AnalogPressed(poller.rightControllerGripFloat);
                    case "left_trigger": return poller.leftControllerTriggerButton;
                    case "right_trigger": return poller.rightControllerTriggerButton;
                }
            }
            InputDevice device = InputDevices.GetDeviceAtXRNode(binding.StartsWith("left_") ? XRNode.LeftHand : XRNode.RightHand);
            InputFeatureUsage<bool> feature;
            switch (binding)
            {
                case "left_x": case "right_a": feature = CommonUsages.primaryButton; break;
                case "left_y": case "right_b": feature = CommonUsages.secondaryButton; break;
                case "left_grip": case "right_grip": feature = CommonUsages.gripButton; break;
                case "left_trigger": case "right_trigger": feature = CommonUsages.triggerButton; break;
                case "left_stick": case "right_stick": feature = CommonUsages.primary2DAxisClick; break;
                default: return false;
            }
            return device.TryGetFeatureValue(feature, out bool held) && held;
        }
        bool Pressed(string key, string fallback)
        {
            bool now = Held(S(key, fallback));
            bool before = previous.TryGetValue(key, out bool old) && old;
            previous[key] = now;
            return now && !before;
        }
        Camera ViewCamera()
        {
            if (cachedCamera && cachedCamera.isActiveAndEnabled) return cachedCamera;
            var player = GorillaLocomotion.GTPlayer.Instance;
            cachedCamera = player ? CameraField?.GetValue(player) as Camera : null;
            if (!cachedCamera) cachedCamera = Camera.main;
            return cachedCamera;
        }
        bool HandPose(XRNode hand, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero; rotation = Quaternion.identity;
            var player = GorillaLocomotion.GTPlayer.Instance;
            if (player)
            {
                var controller = player.GetControllerTransform(hand == XRNode.LeftHand);
                if (controller) { position = controller.position; rotation = controller.rotation; return true; }
            }
            Camera camera = ViewCamera();
            var device = InputDevices.GetDeviceAtXRNode(hand);
            if (!camera || !device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 localPosition) || !device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion localRotation)) return false;
            Transform origin = camera.transform.parent;
            position = origin ? origin.TransformPoint(localPosition) : localPosition;
            rotation = origin ? origin.rotation * localRotation : localRotation;
            return true;
        }
        int SelectionMask() => (freezeKey.Armed?1:0)|(gripPlatforms?2:0)|(triggerPlatforms?4:0)|(teleport?8:0)|(smartArms?16:0)|(lowGravity?32:0)|(checkpointArmed?64:0)|(fpsDisplay?128:0);
        bool SyncLobby()
        {
            if(!Allowed){activeLobbyKey=null;activeLobbyName=null;activeLobbyMode=null;return false;}
            var network=NetworkSystem.Instance;
            string name=network.RoomName, mode=network.GameModeString ?? "";
            if(string.IsNullOrEmpty(name)){activeLobbyKey=null;return false;}
            if(activeLobbyKey!=null && activeLobbyName==name && activeLobbyMode==mode)return false;
            ResetFeatures();
            activeLobbyName=name;activeLobbyMode=mode;activeLobbyKey=LobbyProfiles.Key(name,mode);
            int mask=lobbyProfiles.Get(activeLobbyKey);
            freezeKey.SetArmed((mask&1)!=0);
            gripPlatforms=(mask&2)!=0;triggerPlatforms=(mask&4)!=0;teleport=(mask&8)!=0;smartArms=(mask&16)!=0;lowGravity=(mask&32)!=0;checkpointArmed=(mask&64)!=0;fpsDisplay=(mask&128)!=0;
            savedProfileMask=mask;
            message="Lobby selections restored";
            return true;
        }
        void SaveLobby()
        {
            if(activeLobbyKey==null || !Allowed)return;
            int mask=SelectionMask();if(mask==savedProfileMask)return;
            lobbyProfiles.Set(activeLobbyKey,mask);
            try
            {
                string temporary=profilePath+".tmp";
                File.WriteAllText(temporary,lobbyProfiles.Write());
                if(File.Exists(profilePath))File.Replace(temporary,profilePath,null);else File.Move(temporary,profilePath);
                savedProfileMask=mask;
            }
            catch(Exception e){message="Lobby settings could not save";Logger.LogWarning(e.Message);savedProfileMask=mask;}
        }
        void Update()
        {
            bool lobbyChanged=SyncLobby();
            if (Pressed("menu_button", "left_x")) { menuVisible = !menuVisible; Logger.LogInfo("Menu " + (menuVisible ? "opened" : "closed")); if(!menuVisible)EndSearch(false); }
            bool freezePress = Pressed("freeze_button", "right_b");
            bool teleportPress = Pressed("teleport_button", "right_trigger");
            if (!Allowed) ResetFeatures();
            if (!lobbyChanged && Allowed && freezeKey.Respond(freezePress)) ToggleFreeze();
            UpdateMenu();
            SaveLobby();
            UpdateExtras(lobbyChanged);
            if (!Allowed) return;
            for (int i = 0; i < 2; i++)
            {
                string hand = i == 0 ? "left" : "right";
                bool grip = gripPlatforms && Held(S("platform_" + hand + "_grip", hand + "_grip"));
                string triggerBinding = S("platform_" + hand + "_trigger", hand + "_trigger");
                bool trigger = triggerPlatforms && Held(triggerBinding) && !(teleport && triggerBinding == S("teleport_button", "right_trigger"));
                UpdatePlatform(i, grip || trigger);
            }
            UpdateTeleport(teleportPress);
        }
        void LateUpdate()
        {
            if (!frozen) return;
            if (!frozenRig || !Allowed) { Unfreeze(); return; }
            ghostVisual.Maintain();
        }
        void UpdatePlatform(int index, bool active)
        {
            if (!active) { if (platforms[index] && platforms[index].activeSelf) platforms[index].SetActive(false); return; }
            if ((platforms[index] && platforms[index].activeSelf) || !HandPose(index == 0 ? XRNode.LeftHand : XRNode.RightHand, out Vector3 p, out Quaternion q)) return;
            var platform = platforms[index] ? platforms[index] : GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.SetActive(true); DontDestroyOnLoad(platform);
            platform.name = "Bobbys platform";
            var locomotion = GorillaLocomotion.GTPlayer.Instance;
            int mask = locomotion ? ModRules.MovementMask(locomotion.locomotionEnabledLayers.value, GorillaLocomotion.GTPlayer.LocomotionEnabledLayers.value) : Physics.DefaultRaycastLayers;
            int solidLayer = ModRules.SolidLayer(mask);
            if (solidLayer < 0) { platform.SetActive(false); message = "No movement collision layer available"; return; }
            platform.layer = solidLayer;
            var collision = platform.GetComponent<Collider>(); collision.enabled = true; collision.isTrigger = false;
                        var follower=locomotion ? locomotion.GetHandFollower(index==0) : null;
            Vector3 handPosition=follower ? follower.position : p;
            float thickness=N("platform_thickness",.04f,.02f,.2f);
            float handClearance=locomotion ? Mathf.Clamp(locomotion.minimumRaycastDistance*locomotion.scale,.04f,.12f) : .05f;
            platform.transform.position=handPosition-Vector3.up*(handClearance+thickness*.5f+.012f);
            platform.transform.localScale = new Vector3(N("platform_width", .3f, .1f, 1), N("platform_thickness", .04f, .02f, .2f), N("platform_depth", .3f, .1f, 1));
                        if(!platformMaterial)
            {
                Color tint=C("platform_colour","#BD38FF");tint.a=N("platform_opacity",.25f,.05f,.75f);
                platformMaterial=VisualMaterial.Transparent("Bobbys translucent platforms",tint);
            }
            var platformRenderer=platform.GetComponent<Renderer>();platformRenderer.sharedMaterial=platformMaterial;
            platformRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;platformRenderer.receiveShadows=false;
            
            platforms[index] = platform; Physics.SyncTransforms();
        }
        bool IsOwnCollider(Collider collider)
        {
            if (!collider) return true;
            var player = GorillaLocomotion.GTPlayer.Instance;
            if (player && collider.transform.IsChildOf(player.transform)) return true;
            if (collider.GetComponentInParent<VRRig>()) return true;
            foreach (var platform in platforms) if (platform && collider.gameObject == platform) return true;
            return false;
        }
        void UpdateTeleport(bool pressed)
        {
            if (!teleport || !HandPose(XRNode.RightHand, out Vector3 p, out Quaternion q)) { if (line) line.enabled = false; return; }
            if (!line)
            {
                line = new GameObject("Bobbys teleport aim").AddComponent<LineRenderer>();
                DontDestroyOnLoad(line.gameObject);
                line.sharedMaterial = neon; line.positionCount = 2; line.startWidth = .008f; line.endWidth = .004f;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var player = GorillaLocomotion.GTPlayer.Instance;
            int mask = player ? ModRules.MovementMask(player.locomotionEnabledLayers.value, GorillaLocomotion.GTPlayer.LocomotionEnabledLayers.value) : Physics.DefaultRaycastLayers;
            float range = N("teleport_range", 25, 1, 50);
            Vector3 direction = q * Vector3.forward;
            int count = Physics.RaycastNonAlloc(p + direction * .06f, direction, aimHits, range, mask, QueryTriggerInteraction.Ignore);
            bool hit = false; RaycastHit target = default; float nearest = float.MaxValue;
            if (count < aimHits.Length)
                for (int i=0;i<count;i++)
                    if (!IsOwnCollider(aimHits[i].collider) && aimHits[i].distance < nearest) { target=aimHits[i]; nearest=target.distance; hit=true; }
            bool valid = hit && target.normal.y >= .65f;
            line.enabled = true; line.SetPosition(0,p); line.SetPosition(1,hit ? target.point : p+direction*range);
            line.startColor=line.endColor=valid ? C("teleport_line_colour","#00F5FF") : Color.red;
            if (!pressed || Time.unscaledTime < nextTeleport) return;
            if (!valid) { message="Point at an open, flat surface"; return; }
            var camera = ViewCamera(); if (!player || !camera || !player.bodyCollider) return;
            float height = Mathf.Clamp(camera.transform.position.y-player.bodyCollider.bounds.min.y,.6f,2f);
            float radius = Mathf.Clamp(player.bodyCollider.radius*player.scale,.12f,.35f);
            Vector3 feet = target.point + Vector3.up * .06f;
            Vector3 headTarget = feet + Vector3.up * height;
            if(!TryWarp(headTarget,player.transform.rotation))return;
            nextTeleport=Time.unscaledTime+N("teleport_cooldown",.5f,.2f,5);
            message="Teleported";
        }
        Component Player() => GorillaLocomotion.GTPlayer.Instance;
        static void ExtendHands(GorillaLocomotion.GTPlayer __instance, Transform __0, ref Vector3 __result)
        {
            var plugin = Current;
            if (!plugin || !plugin.isActiveAndEnabled || !plugin.smartArms || !plugin.Allowed || !__instance || __instance != GorillaLocomotion.GTPlayer.Instance || !__0) return;
            bool left = __0 == __instance.GetControllerTransform(true);
            if (!left && __0 != __instance.GetControllerTransform(false)) return;
            if (!__instance.headCollider || !__instance.bodyCollider) return;
            var body = __instance.bodyCollider.transform;
            float scale = __instance.scale;
            Vector3 shoulder = __instance.headCollider.transform.position + body.right * (left ? -.18f : .18f) * scale - body.up * .22f * scale;
            __result += (__0.position - shoulder) * .5f;
        }
        void ToggleFreeze()
        {
            if(!freezeKey.Armed)return;
            if (frozen) { Unfreeze(); message = "Live avatar restored"; return; }
            if (!Allowed || !freezeSupported) { message = "Freeze unavailable here"; return; }
            var tagger = GorillaTagger.Instance;
            frozenRig = tagger ? OfflineRigField?.GetValue(tagger) as Component : null;
            if (!frozenRig) { message = "Avatar rig not ready"; return; }
            try { frozenAvatar.Show(frozenRig); } catch(Exception e) { Logger.LogWarning(e.Message); message="Snapshot unavailable; live ghost still enabled"; }
            freezeState.Enable();
            Color ghostColour = C("ghost_colour", "#39FF88"); ghostColour.a = N("ghost_opacity", .4f, .15f, .85f);
            ghostVisual.Show(frozenRig, ghostColour);
            message = "Avatar frozen - press " + S("freeze_button", "right_b") + " to restore";
        }
        static void FreezeOutgoingPose(VRRig __instance, ref InputStruct __result)
        {
            var plugin = Current;
            if (!plugin || !plugin.frozen) return;
            if (!plugin.Allowed) { plugin.Unfreeze(); return; }
            // Only the local avatar is changed; remote rigs and local locomotion keep updating.
            if (__instance != plugin.frozenRig) return;
            var pose = new AvatarPose
            {
                HeadRotation = __result.headRotation, BodyRotation = __result.bodyRotation,
                Rotation = __result.rotation, Position = __result.position,
                LeftHand = __result.leftHandLong, RightHand = __result.rightHandLong,
                HandPosition = __result.handPosition, UsingNewIK = __result.usingNewIK,
                LeftArm = __result.leftUpperArmRotation, RightArm = __result.rightUpperArmRotation
            };
            plugin.freezeState.Apply(ref pose);
            __result.headRotation = pose.HeadRotation; __result.bodyRotation = pose.BodyRotation;
            __result.rotation = pose.Rotation; __result.position = pose.Position;
            __result.leftHandLong = pose.LeftHand; __result.rightHandLong = pose.RightHand;
            __result.handPosition = pose.HandPosition; __result.usingNewIK = pose.UsingNewIK;
            __result.leftUpperArmRotation = pose.LeftArm; __result.rightUpperArmRotation = pose.RightArm;
            // Prevent remote extrapolation while preserving current timestamps and other game state.
            __result.velocity = Vector3.zero;
        }
        void Unfreeze() { freezeState.Disable(); ghostVisual.Dispose(); frozenAvatar.Clear(); frozenRig = null; }
        void ResetFeatures()
        {
            Unfreeze(); freezeKey.Reset(); gripPlatforms = triggerPlatforms = teleport = smartArms = lowGravity = checkpointArmed = fpsDisplay = false; hasCheckpoint=false;
            foreach (var platform in platforms) if (platform && platform.activeSelf) platform.SetActive(false);
            if (line) line.enabled = false;
        }
        void BuildMenu()
        {
            if (menu) Destroy(menu); rows.Clear(); rowImages.Clear();
            menu = new GameObject("Bobbys Mods neon menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            DontDestroyOnLoad(menu);
            menu.SetActive(false);
            var canvas = menu.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 100;
            var rect = menu.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(420, 620); menu.transform.localScale = Vector3.one * .001f;
            var backdrop = menu.AddComponent<Image>(); backdrop.raycastTarget = false; backdrop.color = C("menu_background", "#090B1C");
            var outline = menu.AddComponent<Outline>(); outline.effectColor = C("menu_border", "#00F5FF"); outline.effectDistance = new Vector2(4, -4);
            AddRow(S("menu_name", "Bobbys Mods"), 0);
            AddRow(AllLobbies ? "ALL LOBBIES ENABLED" : "PRIVATE MODDED ROOM REQUIRED", 1);
            AddRow("Freeze Keybind", 2);
            AddRow(S("label_grip", "Grip Platforms"), 3);
            AddRow(S("label_trigger", "Trigger Platforms"), 4);
            AddRow(S("label_teleport", "Teleport"), 5);
            AddRow(S("label_arms", "Smart Arms"), 6); AddRow("Next Page", 7); AddRow("Disable All", 8); AddRow("", 9);
        }
        void AddRow(string label, int index)
        {
            var row = new GameObject("Row " + index, typeof(RectTransform), typeof(Image)); row.transform.SetParent(menu.transform, false);
            var rect = row.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(390, 48); rect.anchoredPosition = new Vector2(0, 260 - index * 57);
            var img = row.GetComponent<Image>(); img.raycastTarget = false; img.color = index < 1 || index == 9 ? Color.clear : C("menu_button_colour", "#802EFF"); rowImages.Add(img);
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(row.transform, false);
            var tr = textObject.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>(); text.raycastTarget = false; text.font = menuFont; text.text = label; text.fontSize = index == 0 ? 30 : 19; text.alignment = TextAnchor.MiddleCenter; text.color = C("menu_text_colour", "#FFFFFF"); rows.Add(text);
        }
        void SetVisible(bool visible)
        {
            if (menu && menu.activeSelf != visible) menu.SetActive(visible);
            foreach (var dot in dots) if (dot && dot.activeSelf != visible) dot.SetActive(visible);
            if (!visible) {touch.Release();searchKeyboard?.Visible(false);keyboardTouch.Release();}
        }
        void UpdateDots(Vector3 position, Quaternion rotation)
        {
            pointerTouchPosition=position+rotation*new Vector3(0,0,.055f);
            if (!dots[0])
            {
                pointerTexture=new Texture2D(24,24,TextureFormat.RGBA32,false);
                for(int y=0;y<24;y++)for(int x=0;x<24;x++)
                    pointerTexture.SetPixel(x,y,(x-11.5f)*(x-11.5f)+(y-11.5f)*(y-11.5f)<=121 ? Color.white : Color.clear);
                pointerTexture.Apply();
                pointerSprite=Sprite.Create(pointerTexture,new Rect(0,0,24,24),new Vector2(.5f,.5f),24);
                var dot=new GameObject("Bobbys touch pointer",typeof(RectTransform),typeof(Canvas),typeof(Image));
                dot.layer=2;
                var canvas=dot.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.overrideSorting=true;canvas.sortingOrder=200;
                var rect=dot.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(16,16);dot.transform.localScale=Vector3.one*.001f;
                var graphic=dot.GetComponent<Image>();graphic.sprite=pointerSprite;graphic.color=pointerColour;graphic.raycastTarget=false;
                DontDestroyOnLoad(dot);dots[0]=dot;
            }
            if(!dots[0].activeSelf)dots[0].SetActive(true);
            // Its canvas sorts above the panel. Contact still uses the real pointer position.
            dots[0].transform.SetPositionAndRotation(pointerTouchPosition,menu.transform.rotation);
        }
        void UpdateMenu()
        {
            if (!menu) BuildMenu();
            if (!menuVisible || !HandPose(XRNode.LeftHand, out Vector3 left, out Quaternion rotation)) { SetVisible(false); return; }
            var camera = ViewCamera();
            if (!camera || !HandPose(XRNode.RightHand, out Vector3 right, out Quaternion rq)) { SetVisible(false); return; }
            menu.transform.position = searchOpen ? searchPanelPosition : left + Vector3.up * .25f;
            menu.transform.rotation = searchOpen ? searchPanelRotation : Quaternion.LookRotation(menu.transform.position - camera.transform.position);
            SetVisible(true);
            UpdateDots(right, rq);
            bool allowed = Allowed;
            string status = allowed ? (AllLobbies ? "ALL LOBBIES - READY" : "PRIVATE MODDED ROOM - READY") : (AllLobbies ? "JOIN A ROOM TO USE MODS" : "LOCKED: PRIVATE MODDED ROOM ONLY");
            string searchHeader=searchOpen ? "Search: "+(search.Query.Length==0?"type below...":search.Query) : "Search mods...";
            if(rows[1].text!=searchHeader)rows[1].text=searchHeader;
            string title=S("menu_name","Bobbys Mods")+(searchOpen?" Search":" "+(menuPage+1)+"/2");
            if(rows[0].text!=title)rows[0].text=title;
            for(int i=0;i<5;i++){int feature=RowFeature(i+2);string label=feature>=0?FeatureLabel(feature):(i==0?"No matching mods":"");if(rows[i+2].text!=label)rows[i+2].text=label;}
            string pageLabel=searchOpen?(searchCount>5?"More results":"All results shown"):(menuPage==0?"Next Page":"Previous Page");
            if(rows[7].text!=pageLabel)rows[7].text=pageLabel;
            string footer=!allowed?status:(fpsDisplay ? fpsText+" | "+message : message);
            if(rows[9].text!=footer)rows[9].text=footer;
            for (int i = 0; i < 5; i++)
            {
                int feature=RowFeature(i+2);bool on=feature>=0 && FeatureEnabled(feature);
                Color colour = !allowed ? new Color(.15f, .15f, .2f) : on ? enabledColour : buttonColour;
                if (rowImages[i + 2].color != colour) rowImages[i + 2].color = colour;
            }
            if(HandleKeyboard())return;
            int hovered = -1;
            foreach (var dot in dots)
            {
                var local = menu.transform.InverseTransformPoint(pointerTouchPosition);
                int row = MenuTouch.Hit(local.x, local.y, local.z);
                if (row >= 0) { hovered = row; break; }
            }
            int selected = touch.Enter(hovered, Time.unscaledTime);
            if (selected < 0) return;
            if(selected==1){if(!searchOpen)BeginSearch();return;}
            if(selected==7){if(searchOpen){int pages=(searchCount+4)/5;if(pages>1)searchPage=(searchPage+1)%pages;}else menuPage=1-menuPage;return;}
            if(selected==8){ResetFeatures();return;}
            int selectedFeature=RowFeature(selected);if(selectedFeature<0)return;
            ActivateFeature(selectedFeature);
        }
        void ActivateFeature(int feature)
        {
            if(feature==8){ResetFeatures();Reload();if(platformMaterial)Destroy(platformMaterial);platformMaterial=null;BuildMenu();return;}
            if(feature==9){message="0.6.0 beta: headset tests pending";return;}
            if(!Allowed)return;
            switch(feature)
            {
                case 0:freezeKey.Toggle();if(!freezeKey.Armed)Unfreeze();message=freezeKey.Armed?"Freeze armed: press "+S("freeze_button","right_b"):"Freeze keybind disabled";break;
                case 1:gripPlatforms=!gripPlatforms;break;
                case 2:triggerPlatforms=!triggerPlatforms;break;
                case 3:teleport=!teleport;break;
                case 4:smartArms=!smartArms;break;
                case 5:lowGravity=!lowGravity;break;
                case 6:checkpointArmed=!checkpointArmed;if(!checkpointArmed)hasCheckpoint=false;message="Checkpoint: "+S("checkpoint_save_button","right_a")+" save / "+S("checkpoint_return_button","left_y")+" return";break;
                case 7:fpsDisplay=!fpsDisplay;break;
            }
        }
        void OnDisable() { SaveLobby(); ResetFeatures(); SetVisible(false); }
        void OnDestroy()
        {
            EndSearch(false);searchKeyboard?.Destroy();ResetFeatures(); foreach (var platform in platforms) if (platform) Destroy(platform); foreach (var dot in dots) if (dot) Destroy(dot); harmony?.UnpatchSelf(); if (menu) Destroy(menu); if (line) Destroy(line.gameObject); if (neon) Destroy(neon); if(platformMaterial)Destroy(platformMaterial); if(pointerSprite)Destroy(pointerSprite); if(pointerTexture)Destroy(pointerTexture); Current = null;
        }
    }
}

















