using System.Collections;
using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Story
{
    public sealed class CheckpointSystem : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private MainGameDirector director;
        private Vector3 position;
        private bool respawning;
        public string Name { get; private set; }
        public void Configure(PlayerController movement, PlayerHealth vitals, MainGameDirector owner)
        { player = movement; health = vitals; director = owner; }
        private void OnEnable() { if (health != null) health.Died += OnDeath; }
        private void OnDisable() { if (health != null) health.Died -= OnDeath; }
        public void Set(string name, Vector3 spawn) { Name = name; position = spawn; }
        private void OnDeath() { if (!respawning) StartCoroutine(Respawn()); }
        public void Restart() { if (!respawning) StartCoroutine(Respawn()); }
        private IEnumerator Respawn()
        {
            respawning = true; player.enabled = false; director.BeginRespawn();
            yield return new WaitForSeconds(0.8f);
            CharacterController motor = player.GetComponent<CharacterController>();
            motor.enabled = false; player.transform.position = position; motor.enabled = true;
            health.Restore(); director.RestoreCheckpoint(); player.enabled = true; respawning = false;
        }
    }
}
