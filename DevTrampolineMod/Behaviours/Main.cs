using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GorillaLocomotion;

namespace DevTrampolineMod.Behaviours
{
    public class Main : MonoBehaviour, IInitializable
    {
        private bool _initialized, _inModdedRoom;

        private AssetLoader _assetLoader;

        private GameObject _container, _trampoline, _preview;
        private List<GameObject> _trampolineList = new List<GameObject>();

        private AudioClip _uiClack, _envClack;
        private AudioClip[] _trampolineArray;

        private bool _trigger, _grip, _edit;
        private float _deletionTime;

        [Inject]
        public void Construct(AssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
        }

        public async void Initialize()
        {
            if (_initialized) return;

            Plugin.Disabled += Disabled;
            Plugin.Joined += Joined;
            Plugin.Left += Left;

            _container = new GameObject("DevTrampolineMod | Container");
            _trampoline = await _assetLoader.LoadAsset<GameObject>("Trampoline");
            _trampoline.transform.localScale = Vector3.one * 0.75f;

            _uiClack = await _assetLoader.LoadAsset<AudioClip>("UI");
            _envClack = await _assetLoader.LoadAsset<AudioClip>("Interaction");

            _trampolineArray = new AudioClip[4]
            {
                await _assetLoader.LoadAsset<AudioClip>("bounce1"),
                await _assetLoader.LoadAsset<AudioClip>("bounce2"),
                await _assetLoader.LoadAsset<AudioClip>("bounce3"),
                await _assetLoader.LoadAsset<AudioClip>("bounce4")
            };

            Bounce.PlayBounceSound += delegate (AudioSource source, float volume)
            {
                source.clip = _trampolineArray[Random.Range(0, _trampolineArray.Length)];
                source.volume = volume;
                source.Play();
            };

            _initialized = true;
        }

        public void Update()
        {
            if (!Plugin.IsEnabled || !_inModdedRoom || !_inModdedRoom) return;

            bool grip = ControllerInputPoller.instance.rightControllerGripFloat > 0.5f;
            if (_grip != grip && grip)
            {
                _edit ^= true;
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(_uiClack, 0.6f);
                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength * 0.2f, GorillaTagger.Instance.tapHapticDuration);
            }
            _grip = grip;

            if (_edit)
            {
                _deletionTime -= Time.unscaledDeltaTime;
                if (_preview == null)
                {
                    _preview = Instantiate(_trampoline);
                    _preview.transform.SetParent(_container.transform);
                    Destroy(_preview.transform.GetChild(0).gameObject);
                }

                if (Physics.Raycast(Player.Instance.rightHandFollower.position, -Player.Instance.rightControllerTransform.up, out RaycastHit hit, 250 * Player.Instance.scale, Player.Instance.locomotionEnabledLayers))
                {
                    _preview.transform.position = hit.point + (hit.normal * 0.15f * Player.Instance.scale);
                    _preview.transform.localScale = Vector3.one * 0.5f * Player.Instance.scale;
                    _preview.transform.up = hit.normal;
                }

                bool trigger = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
                if (_trigger != trigger && trigger)
                {
                    var trampoline = Instantiate(_trampoline);
                    Destroy(trampoline.transform.GetChild(1).gameObject);

                    trampoline.transform.position = _preview.transform.position;
                    trampoline.transform.localScale = _preview.transform.localScale;
                    trampoline.transform.eulerAngles = _preview.transform.eulerAngles;
                    trampoline.transform.SetParent(_container.transform, true);

                    _trampolineList.Add(trampoline);
                    foreach (var collider in trampoline.GetComponentsInChildren<Collider>())
                    {
                        collider.gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 144;
                        switch (collider.gameObject.name)
                        {
                            case "Cylinder.001":
                                float h = Random.Range(0, 361), v = Random.Range(85, 101);
                                var trampolineRenderer = collider.transform.GetComponent<Renderer>();
                                var trampolineMaterial = new Material(trampolineRenderer.material) {color = Color.HSVToRGB(h / 360, 0.75f, v / 100)};
                                trampolineRenderer.material = trampolineMaterial;
                                break;
                            case "MainPart":
                                collider.gameObject.AddComponent<Bounce>().trampoline = trampoline;
                                break;
                        }
                    }

                    if (_trampolineList.Count > Constants.TrampolineLimit) 
                    {
                        trampoline = _trampolineList[0];
                        Destroy(trampoline);

                        _trampolineList.RemoveAt(0);
                    }

                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(_envClack, 0.6f);
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
                }

                bool thumb = ControllerInputPoller.instance.rightControllerPrimaryButton;
                if (thumb && _deletionTime <= 0 && _trampolineList.Count != 0)
                {
                    var trampoline = _trampolineList[_trampolineList.Count - 1];
                    Destroy(trampoline);

                    _deletionTime = 0.2f;
                    _trampolineList.RemoveAt(_trampolineList.Count - 1);

                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(_envClack, 0.6f);
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength * 0.2f, GorillaTagger.Instance.tapHapticDuration);
                }

                _trigger = trigger;
            }
            else if (_preview != null)
            {
                Destroy(_preview);
            }
        }

        public void Disabled()
        {
            if (_initialized && _preview != null)
            {
                Destroy(_preview);
            }
        }

        public void Joined()
        {
            _inModdedRoom = true;
            _container.SetActive(Plugin.IsEnabled);
        }

        public void Left()
        {
            _inModdedRoom = false;
            _container?.SetActive(false);

            _edit = false;
            if (_initialized && _preview != null)
            {
                Destroy(_preview);
            }
        }
    }
}
