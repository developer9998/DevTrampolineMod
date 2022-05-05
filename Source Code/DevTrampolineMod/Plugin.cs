using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;
using Utilla;

namespace DevTrampolineMod
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        /*Mod under the MIT license, if you reproduce please credit*/

        /*Assetloading*/
        public static readonly string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static GameObject Trampoline; // the main trampoline model used
        static GameObject TrampolineFolder; // the folder containing all of the trampolines

        /*Dynamic mod enabling/disabling*/
        static bool isActive; // is the mod enabled
        static bool inRoom = false; // is the player in a modded lobby

        /*Keybind*/
        static bool inEditMode = false; // is the mod in edit mode
        static bool editModeToggle = true; // can edit mode be toggled
        bool isPrimaryDown; // is the primary button down
        bool isSecondaryDown; // is the secondary button down
        bool isTriggerDown; // is the trigger button down

        /*Cooldowns*/
        static int TrampolineCooldown = 0; // cooldown for placing trampolines
        static bool TrampolineDocumented = true; // cooldown toggle variable for placing trampolines
        static int TrampolineCooldown2 = 0; // cooldown for deleting the last trampoline
        static bool TrampolineDocumented2 = true; // cooldown toggle variable for deleting the last trampoline

        /*Tracking*/
        public static string findThisTrampoline; // the string used for finding a trampoline
        public static int currentTrampoline = -1; // current # of trampolines -1    

        /*General config*/
        public static ConfigEntry<float> multiplier;

        void OnEnable()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
            TrampolineFolder.SetActive(inRoom);
            if (inRoom)
            {
                isActive = true;
            }
        }
        void OnDisable()
        {
            Utilla.Events.GameInitialized -= OnGameInitialized;
            isActive = false;
            TrampolineFolder.SetActive(false);
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            TrampolineFolder = new GameObject();
            TrampolineFolder.name = "DevTrampolineMod";
            
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("DevTrampolineMod.Assets.trampoline");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject trampoline = bundle.LoadAsset<GameObject>("Trampoline");
            Trampoline = Instantiate(trampoline);
            Trampoline.transform.SetParent(TrampolineFolder.transform, false);
            Trampoline.transform.position = new Vector3(-65.74f, 21.38f, -83.16f);
            Trampoline.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            Trampoline.transform.GetChild(1).transform.GetChild(0).gameObject.layer = 9;
            Trampoline.transform.GetChild(1).transform.GetChild(0).gameObject.AddComponent<TrampolineBounce>(); // makes it bouncy for body
            Trampoline.transform.GetChild(1).transform.GetChild(1).gameObject.layer = 18;
            Trampoline.transform.GetChild(1).transform.GetChild(1).gameObject.AddComponent<TrampolineBounce>(); // makes it bouncy for hands
            Trampoline.SetActive(false);
        }

        void Update()
        {

            if (!isActive)
            {
                inEditMode = false;
                TrampolineFolder.SetActive(false);
            }
            else
            {
                TrampolineFolder.SetActive(true);
            }

            if (TrampolineCooldown == 0)
            {
                if (!TrampolineDocumented)
                {
                    TrampolineDocumented = true;

                }
            }
            else
            {
                TrampolineCooldown--;
            }

            if (TrampolineCooldown2 == 0)
            {
                if (!TrampolineDocumented2)
                {
                    TrampolineDocumented2 = true;

                }
            }
            else
            {
                TrampolineCooldown2--;
            }

            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out isPrimaryDown);
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out isSecondaryDown);

            if (isSecondaryDown && isActive && !inEditMode && TrampolineCooldown2 == 0)
            {
                if (GameObject.Find("PlantedTrampoline" + currentTrampoline.ToString())) { GameObject.Destroy(GameObject.Find("PlantedTrampoline" + currentTrampoline.ToString())); }
                TrampolineCooldown2 = 12;
                TrampolineDocumented2 = true;
                if (currentTrampoline > -2)
                {
                    currentTrampoline--;
                }
            }


            if (isPrimaryDown && editModeToggle && isActive)
            {
                editModeToggle = false;
                inEditMode = !inEditMode;
            }
            if (!isPrimaryDown && !editModeToggle && isActive)
            {
                editModeToggle = true;
            }

            if (!inEditMode)
            {
                Trampoline.transform.GetChild(1).gameObject.SetActive(true);
                Trampoline.transform.GetChild(2).gameObject.SetActive(true);
                Trampoline.transform.GetChild(3).gameObject.SetActive(true);
                Trampoline.transform.GetChild(4).gameObject.SetActive(false);
                Trampoline.SetActive(false);
            }

            if (inEditMode)
            {
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerDown);

                #pragma warning disable IDE0018 // Inline variable declaration
                RaycastHit hitInfo;
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position - GorillaLocomotion.Player.Instance.rightHandTransform.up, -GorillaLocomotion.Player.Instance.rightHandTransform.up, out hitInfo);
                Trampoline.transform.GetChild(1).gameObject.SetActive(false);
                Trampoline.transform.GetChild(2).gameObject.SetActive(false);
                Trampoline.transform.GetChild(3).gameObject.SetActive(false);
                Trampoline.transform.GetChild(4).gameObject.SetActive(true);
                Trampoline.SetActive(true);

                Trampoline.transform.localPosition = hitInfo.point + new Vector3(0f, 0.237f, 0f);


                if (isTriggerDown)
                {
                    if (TrampolineCooldown == 0)
                    {
                        TrampolineCooldown = 60;
                        TrampolineDocumented = true;
                        if (currentTrampoline < 9)
                        {
                            GameObject ClonedTrampoline = GameObject.Instantiate(Trampoline);
                            currentTrampoline++;
                            ClonedTrampoline.transform.SetParent(TrampolineFolder.transform, true);
                            ClonedTrampoline.name = "PlantedTrampoline" + currentTrampoline.ToString();
                            ClonedTrampoline.transform.GetChild(0).gameObject.SetActive(true);
                            ClonedTrampoline.transform.GetChild(1).gameObject.SetActive(true);
                            ClonedTrampoline.transform.GetChild(2).gameObject.SetActive(true);
                            ClonedTrampoline.transform.GetChild(3).gameObject.SetActive(true);
                            ClonedTrampoline.transform.GetChild(4).gameObject.SetActive(false);
                            ClonedTrampoline.SetActive(true);
                        }
                    }
                }
            }
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
            isActive = this.enabled;
            TrampolineFolder.SetActive(this.enabled);
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
            isActive = false;
            TrampolineFolder.SetActive(false);
        }

        public static void ActivateSound(int Sound)
        {
            GameObject soundTramp = GameObject.Find(findThisTrampoline);
            soundTramp.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
            soundTramp.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
            soundTramp.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);

            soundTramp.transform.GetChild(0).transform.GetChild(Sound).gameObject.SetActive(true);
        }

        public static void Bounce(float Height)
        {
            Rigidbody PlayerRigidbody = GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>();
            PlayerRigidbody.AddForce(new Vector3(0f, Height, 0f), ForceMode.VelocityChange);

            System.Random random = new System.Random();

            ActivateSound(random.Next(3));
        }
    }
}
