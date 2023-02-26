using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;
using HarmonyLib;
using Utilla;
using System.Collections.Generic;
using GorillaLocomotion;
using DevTrampolineMod.Scripts;

namespace DevTrampolineMod
{
    [ModdedGamemode]
    [Description("HauntedModMenu")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // Instance
        public static Plugin Instance { get; private set; }
        public bool hasInit = false;

        // Patches
        public static Harmony Harmony { get; private set; }
        public bool hasPatch = false;

        // Room
        public bool InRoom
        {
            get
            {
                return inRoom;
            }
            set
            {
                inRoom = value;
                folder?.SetActive(value);
            }
        }
        private bool inRoom = false;

        // Objects
        public GameObject folder;
        public GameObject baseObject;
        public GameObject previewTrmp;
        public List<GameObject> trmpObjects = new List<GameObject>();
        public List<AudioClip> trmpAudio = new List<AudioClip>();

        // Inputs
        public bool TriggerPressed;
        public bool GripPressed;
        public bool EditMode;
        public float DeleteTime;

        public void Awake()
        {
            Instance = this;

            if (!hasPatch)
            {
                hasPatch = true;
                Harmony = new Harmony(PluginInfo.GUID);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        public void OnEnable()
        {
            if (InRoom)
            {

            }
        }

        public void OnDisable()
        {
            if (previewTrmp != null) Destroy(previewTrmp);
        }

        public void InitMethod()
        {
            if (hasInit) return;
            hasInit = true;

            folder = new GameObject();
            folder.name = "DTMFolder";

            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("DevTrampolineMod.Assets.newtrampoline");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);

            baseObject = bundle.LoadAsset("Trampoline") as GameObject;
            baseObject.transform.localScale = Vector3.one * 0.75f;

            trmpAudio.Add(bundle.LoadAsset("bounce1") as AudioClip);
            trmpAudio.Add(bundle.LoadAsset("bounce2") as AudioClip);
            trmpAudio.Add(bundle.LoadAsset("bounce3") as AudioClip);
            trmpAudio.Add(bundle.LoadAsset("bounce4") as AudioClip);
        }

        public Vector3 SetVector3Multiplier(Vector3 v3, float multiplier)
        {
            var x = (float)(Math.Round(v3.x / multiplier) * multiplier);
            var y = (float)(Math.Round(v3.y / multiplier) * multiplier);
            var z = (float)(Math.Round(v3.z / multiplier) * multiplier);
            return new Vector3(x, y, z);
        }

        // It's not too much code so why not just do it in the Plugin class
        public void Update()
        {
            if (hasInit)
            {
                if (enabled && InRoom)
                {
                    InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.grip, out float gripF);
                    bool grip = gripF > 0.65f;

                    if (GripPressed != grip && grip)
                    {
                        EditMode = !EditMode;
                        GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength * 0.2f, GorillaTagger.Instance.tapHapticDuration);
                    }

                    GripPressed = grip;

                    if (EditMode)
                    {
                        DeleteTime -= Time.deltaTime;
                        if (previewTrmp == null)
                        {
                            previewTrmp = Instantiate(baseObject);
                            Destroy(previewTrmp.transform.GetChild(0).gameObject); // Delete the main object, not sure why I didn't make it two seperate prefabs but I didn't
                            previewTrmp.transform.SetParent(folder.transform, true);
                        }

                        if (Physics.Raycast(Player.Instance.rightHandTransform.position, -Player.Instance.rightHandTransform.up, out RaycastHit hit, 15 * Player.Instance.scale, Player.Instance.locomotionEnabledLayers))
                        {
                            previewTrmp.transform.position = hit.point + (hit.normal * 0.15f * Player.Instance.scale);
                            previewTrmp.transform.localScale = Vector3.one * 0.5f * Player.Instance.scale;
                            previewTrmp.transform.up = hit.normal;
                        }

                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.trigger, out float triggerF);
                        bool trigger = triggerF > 0.65f;

                        if (TriggerPressed != trigger && trigger)
                        {
                            var newTrmp = Instantiate(baseObject);
                            Destroy(newTrmp.transform.GetChild(1).gameObject); // Delete the selection object
                            newTrmp.transform.position = previewTrmp.transform.position;
                            newTrmp.transform.localScale = previewTrmp.transform.localScale;
                            newTrmp.transform.eulerAngles = previewTrmp.transform.eulerAngles;
                            newTrmp.transform.SetParent(folder.transform, true);
                            foreach (var collider in newTrmp.GetComponentsInChildren<Collider>())
                            {
                                collider.gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 144; // Table aka. skyjunglewood2
                                if (collider.gameObject.name == "Cylinder.001") // The rim of the trampoline
                                {
                                    float h = UnityEngine.Random.Range(0, 361);
                                    float s = 75f;
                                    float v = UnityEngine.Random.Range(85, 101);
                                    var r = collider.transform.GetComponent<Renderer>();
                                    var m = new Material(r.material)
                                    {
                                        color = Color.HSVToRGB(h / 360, s / 100, v / 100)
                                    };
                                    r.material = m; // Set it to a random HSV colour to make the trampoline look more polished
                                }
                                else if (collider.gameObject.name == "MainPart") // The main bounce part
                                {
                                    collider.gameObject.AddComponent<Bounce>().baseTrmp = newTrmp; // Add the bounce component to the trampoline 
                                }
                            }
                            trmpObjects.Add(newTrmp);
                            if (trmpObjects.Count == 21) // Limit the count of the trampolines to 20 at once
                            {
                                GameObject trmp_ = trmpObjects[0];
                                trmpObjects.RemoveAt(0);
                                Destroy(trmp_);
                            }
                            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
                        }

                        InputFeatureUsage<bool> buttonUsage = InputDevices.GetDeviceAtXRNode(XRNode.Head).name == "Oculus Rift" ? CommonUsages.secondaryButton : CommonUsages.primaryButton;
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(buttonUsage, out bool btn);

                        if (btn && DeleteTime <= 0 && trmpObjects.Count != 0)
                        {
                            DeleteTime = (float)0.2;

                            // Remove the first object in the trampoline list
                            GameObject trmp_ = trmpObjects[trmpObjects.Count - 1];
                            trmpObjects.RemoveAt(trmpObjects.Count - 1);
                            Destroy(trmp_);

                            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength * 0.2f, GorillaTagger.Instance.tapHapticDuration);
                        }

                        TriggerPressed = trigger;
                    }
                    else
                    {
                        if (previewTrmp != null) Destroy(previewTrmp);
                    }
                }
            }
        }

        [ModdedGamemodeJoin] public void OnJoin() => InRoom = true;
        [ModdedGamemodeLeave] public void OnLeave() => InRoom = false;
    }
}
