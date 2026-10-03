using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Gameplay
{
    [Flags]
    public enum AnveshAbility { None = 0, Scan = 1, Mark = 2, Amplify = 4, Lock = 8, All = 15 }
    [DisallowMultipleComponent]
    public sealed class AnveshController : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private AnveshAbility unlocked = AnveshAbility.All;
        private bool suppressed;
        private bool lockPermitted = true;
        public AnveshOperationController Operations { get; private set; }
        private void Awake() => Operations = GetComponent<AnveshOperationController>();
        public void RefreshAvailability() => AvailabilityChanged?.Invoke();
        public AnveshAbility Unlocked => unlocked;
        public bool ShowTeaching { get; set; } = true;
        public QuantumEncounter Encounter => encounter;
        public event Action ContextChanged;
        public event Action AvailabilityChanged;
        public event Action RestartRequested;
        public bool CanScan => Has(AnveshAbility.Scan) && encounter != null && encounter.CanScan;
        public bool CanMark => Has(AnveshAbility.Mark) && encounter != null && encounter.CanMark;
        public bool CanAmplify => Has(AnveshAbility.Amplify) && encounter != null && encounter.CanAmplify;
        public bool CanLock => Has(AnveshAbility.Lock) && lockPermitted && encounter != null && encounter.CanLock;
        private bool Has(AnveshAbility ability) => !suppressed && (Operations == null || !Operations.Busy) && (unlocked & ability) != 0;
        public void Configure(QuantumEncounter owner) { Operations?.Cancel(); encounter = owner; ContextChanged?.Invoke(); }
        public void SetUnlocked(AnveshAbility abilities) { unlocked = abilities; AvailabilityChanged?.Invoke(); }
        public void SetSuppressed(bool value) { suppressed = value; AvailabilityChanged?.Invoke(); }
        public void PermitLock(bool value) { lockPermitted = value; AvailabilityChanged?.Invoke(); }
        public bool Scan() => CanScan && Execute(AnveshAbility.Scan);
        public bool Mark() => CanMark && Execute(AnveshAbility.Mark);
        public bool Amplify() => CanAmplify && Execute(AnveshAbility.Amplify);
        public bool Lock() => CanLock && Execute(AnveshAbility.Lock);
        private bool Execute(AnveshAbility action) => Operations != null ? Operations.Begin(action) : AnveshOperationController.Apply(encounter, action);
        public void Restart()
        {
            Operations?.Cancel();
            if (RestartRequested != null) RestartRequested.Invoke();
            else if (encounter != null) encounter.RestartEncounter();
        }

        public event Action DebugToggleRequested;
#if ENABLE_INPUT_SYSTEM
        private enum Command { Scan, Mark, Amplify, Lock, Reset, Debug }
        private InputActionMap shortcuts;
        private readonly Queue<Command> pending = new Queue<Command>();

        private void OnEnable()
        {
            // Action callbacks capture presses in the Input System's own update, so they
            // cannot be missed by MonoBehaviour.Update when update timing differs.
            shortcuts?.Dispose();
            shortcuts = new InputActionMap("ANVESH shortcuts");
            Bind("Scan", "<Keyboard>/q", Command.Scan);
            // C# properties are digit1Key etc.; Input System control paths are /1, /2, /3.
            Bind("Mark", "<Keyboard>/1", Command.Mark, "<Keyboard>/numpad1");
            Bind("Amplify", "<Keyboard>/2", Command.Amplify, "<Keyboard>/numpad2");
            Bind("Lock", "<Keyboard>/3", Command.Lock, "<Keyboard>/numpad3");
            Bind("Reset", "<Keyboard>/r", Command.Reset);
            Bind("Debug", "<Keyboard>/f3", Command.Debug);
            shortcuts.Enable();
        }

        private void Bind(string name, string binding, Command command, string alternate = null)
        {
            InputAction action = shortcuts.AddAction(name, InputActionType.Button, binding);
            if (alternate != null) action.AddBinding(alternate);
            action.performed += _ => { if (command == Command.Debug || command == Command.Reset || Operations == null || !Operations.Busy) pending.Enqueue(command); };
        }

        private void OnDisable()
        {
            shortcuts?.Disable();
            shortcuts?.Dispose();
            shortcuts = null;
            pending.Clear();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) pending.Clear(); }
#endif

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (pending.Count == 0) return;
            Command command = pending.Dequeue();
            if (command == Command.Debug) { DebugToggleRequested?.Invoke(); return; }
            if (command == Command.Reset) { Restart(); return; }
            if (encounter == null || !encounter.IsInitialized || Time.timeScale <= 0f) return;
            switch (command)
            {
                case Command.Scan: Scan(); break;
                case Command.Mark: Mark(); break;
                case Command.Amplify: Amplify(); break;
                case Command.Lock: Lock(); break;
                case Command.Reset: Restart(); break;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F3)) DebugToggleRequested?.Invoke();
            if (encounter == null || !encounter.IsInitialized || Time.timeScale <= 0f) return;
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            else if (Input.GetKeyDown(KeyCode.Q)) Scan();
            else if (Input.GetKeyDown(KeyCode.Alpha1)) Mark();
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Amplify();
            else if (Input.GetKeyDown(KeyCode.Alpha3)) Lock();
#endif
        }
    }
}
