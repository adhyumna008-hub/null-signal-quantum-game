using System;
using UnityEngine;

namespace NullSignal.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maximum = 100;
        private float invulnerableUntil;
        public int Maximum => maximum;
        public int Current { get; private set; }
        public event Action Changed;
        public event Action Died;
        private void Awake() => Restore();
        public void Restore() { Current = maximum; invulnerableUntil = Time.time + 1f; Changed?.Invoke(); }
        public bool Damage(int amount)
        {
            if (amount <= 0 || Current <= 0 || Time.time < invulnerableUntil) return false;
            Current = Mathf.Max(0, Current - amount); invulnerableUntil = Time.time + 0.9f;
            Changed?.Invoke(); if (Current == 0) Died?.Invoke(); return true;
        }
    }
}
