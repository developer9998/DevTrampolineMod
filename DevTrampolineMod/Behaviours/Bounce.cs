using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevTrampolineMod.Behaviours
{
    public class Bounce : MonoBehaviour
    {
        public static event Action<AudioSource, float> PlayBounceSound;
        public GameObject trampoline;

        private Player player;
        private AudioSource source;

        public void Start()
        {
            player = Player.Instance;
            source = gameObject.transform.GetComponentInParent<AudioSource>();
        }

        public void Impact(bool lighterForce = true)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.velocity *= lighterForce ? 0.85f : 1.1f;
            rb.AddForce(trampoline.transform.up * (lighterForce ? 5f : 10f) * Player.Instance.scale, ForceMode.VelocityChange);

            PlayBounceSound?.Invoke(source, 1f / (lighterForce ? 1.5f : 1f));
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject != null && collision.gameObject.GetComponentInParent<Player>() != null) Impact(false);
            else if (collision.rigidbody != null && collision.rigidbody.transform.GetComponentInParent<Player>() != null) Impact(false);
        }
    }
}
