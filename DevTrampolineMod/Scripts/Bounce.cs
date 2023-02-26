using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevTrampolineMod.Scripts
{
    public class Bounce : MonoBehaviour
    {
        public GameObject baseTrmp;
        public Player player;
        public AudioSource source;

        public void Start ()
        {
            player = Player.Instance;
            source = gameObject.transform.GetComponentInParent<AudioSource>();
        }

        public void Impact(bool lighterForce = true)
        {
            if (lighterForce)
            {
                Rigidbody rb = player.GetComponent<Rigidbody>();
                rb.velocity *= 0.85f;
                rb.AddForce(baseTrmp.transform.up * 5, ForceMode.VelocityChange);

                source.clip = Plugin.Instance.trmpAudio[UnityEngine.Random.Range(0, Plugin.Instance.trmpAudio.Count)];
                source.volume = 0.5f;
                source.Play();
            }
            else
            {
                Rigidbody rb = player.GetComponent<Rigidbody>();
                rb.velocity *= 1.1f;
                rb.AddForce(baseTrmp.transform.up * 10, ForceMode.VelocityChange);

                source.clip = Plugin.Instance.trmpAudio[UnityEngine.Random.Range(0, Plugin.Instance.trmpAudio.Count)];
                source.volume = 1f;
                source.Play();
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject != null && collision.gameObject.GetComponentInParent<Player>() != null) Impact(false);
            else if (collision.rigidbody != null && collision.rigidbody.transform.GetComponentInParent<Player>() != null) Impact(false);
        }
    }
}
