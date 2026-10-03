using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullSignal.Editor
{
    /// <summary>Final materials and small reusable feedback, applied after the existing room builder.</summary>
    internal static class NullSignalFinalVisuals
    {
        internal static void ApplyScene(Astra7VisualKit kit, Transform root)
        {
            foreach (HologramPerformance hologram in root.GetComponentsInChildren<HologramPerformance>(true)) ApplyHologram(kit, hologram);
            foreach (PlayerController player in root.GetComponentsInChildren<PlayerController>(true)) ApplyPlayer(kit, player.gameObject);
            foreach (DoorController door in root.GetComponentsInChildren<DoorController>(true))
            {
                if (door.GetComponent<DoorAccessFeedback>() != null) continue;
                var strips = new System.Collections.Generic.List<Renderer>();
                foreach (Renderer surface in door.GetComponentsInChildren<Renderer>(true))
                    if (surface.name == "Amber latch guide") strips.Add(surface);
                door.gameObject.AddComponent<DoorAccessFeedback>().Configure(door, strips.ToArray());
            }
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.name == "Scientific fill") light.color = new Color(.84f, .83f, .74f);
                if (light.type == LightType.Directional) light.color = new Color(.84f, .89f, .94f);
            }
            RenderSettings.ambientSkyColor = new Color(.30f, .32f, .33f);
            RenderSettings.ambientEquatorColor = new Color(.18f, .20f, .21f);
            RenderSettings.ambientGroundColor = new Color(.10f, .095f, .09f);
        }

        internal static void ApplyHologram(Astra7VisualKit kit, HologramPerformance hologram)
        {
            Transform body = hologram.Body, head = hologram.Head;
            if (body == null || head == null) return;
            foreach (MeshRenderer surface in body.GetComponentsInChildren<MeshRenderer>(true))
            {
                string part = surface.name.ToLowerInvariant();
                surface.sharedMaterial = part.Contains("hair") || part.Contains("leg") || part.Contains("eye") || part.Contains("brow") || part.Contains("mouth") || part.Contains("spectacle") || part.Contains("undershirt") ? kit.ResearcherEcho
                    : part == "head" || part.Contains("hand") || part.Contains("nose") || part.Contains("clip") || part.Contains("stitch") ? kit.ResearcherFace : kit.ResearcherProjection;
                surface.shadowCastingMode = ShadowCastingMode.Off;
                surface.receiveShadows = false;
            }
            if (body.Find("Projection detail") != null) return;
            Transform detail = Group(body, "Projection detail");
            kit.Box(detail, "Dark undershirt", new Vector3(0, 1.42f, -.187f), new Vector3(.25f, .44f, .025f), kit.ResearcherEcho);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform lapel = kit.Box(detail, "Ivory coat lapel", new Vector3(side * .16f, 1.43f, -.22f), new Vector3(.075f, .44f, .035f), kit.ResearcherProjection);
                lapel.localRotation = Quaternion.Euler(0, 0, side * 15f);
                kit.Box(detail, "Pocket stitch", new Vector3(side * .19f, 1.0f, -.191f), new Vector3(.16f, .018f, .019f), kit.ResearcherFace);
            }
            kit.Box(detail, "Recorded identity card", new Vector3(.19f, 1.32f, -.215f), new Vector3(.09f, .13f, .022f), kit.ResearcherProjection);
            kit.Box(detail, "Copper card clip", new Vector3(.19f, 1.40f, -.218f), new Vector3(.055f, .025f, .023f), kit.ResearcherFace);
            if (hologram.Arm != null) kit.Box(hologram.Arm, "Projected hand", new Vector3(0, -.69f, 0), new Vector3(.14f, .16f, .13f), kit.ResearcherFace);
            kit.Box(detail, "Resting hand", new Vector3(.42f, .80f, 0), new Vector3(.14f, .17f, .13f), kit.ResearcherFace);
            kit.Box(head, "Projected nose", new Vector3(0, -.015f, -.167f), new Vector3(.07f, .10f, .065f), kit.ResearcherFace);
            kit.Box(head, "Brow line", new Vector3(0, .07f, -.163f), new Vector3(.29f, .024f, .026f), kit.ResearcherEcho);
            kit.Box(head, "Mouth silhouette", new Vector3(0, -.105f, -.16f), new Vector3(.095f, .014f, .016f), kit.ResearcherEcho);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(head, "Projected eye", new Vector3(side * .077f, .025f, -.162f), new Vector3(.039f, .022f, .021f), kit.ResearcherEcho);
                if (hologram.Speaker == "LUBNA")
                {
                    kit.Box(head, "Spectacle rim", new Vector3(side * .079f, .049f, -.185f), new Vector3(.12f, .014f, .016f), kit.ResearcherEcho);
                    kit.Box(head, "Spectacle arm", new Vector3(side * .147f, .02f, -.13f), new Vector3(.014f, .055f, .10f), kit.ResearcherEcho);
                }
            }
            kit.Box(head, "Tied hair silhouette", new Vector3(0, .12f, .17f), new Vector3(.19f, .19f, .15f), kit.ResearcherEcho);
        }

        internal static void ApplyPlayer(Astra7VisualKit kit, GameObject player)
        {
            PlayerDodge dodge = player.GetComponent<PlayerDodge>();
            if (dodge == null || player.GetComponent<PlayerDodgeFeedback>() != null) return;
            var trails = new TrailRenderer[2];
            for (int i = 0; i < trails.Length; i++)
            {
                Transform anchor = Group(player.transform, "Dodge suit trail " + i);
                anchor.localPosition = new Vector3(i == 0 ? -.31f : .31f, .9f, -.1f);
                TrailRenderer trail = anchor.gameObject.AddComponent<TrailRenderer>();
                trail.sharedMaterial = kit.Line; trail.time = .16f; trail.minVertexDistance = .08f;
                trail.widthMultiplier = .075f; trail.widthCurve = AnimationCurve.Linear(0, 1, 1, 0);
                Color tint = i == 0 ? new Color(.94f, .70f, .39f) : new Color(.50f, .92f, .96f);
                trail.colorGradient = new Gradient
                {
                    colorKeys = new[] { new GradientColorKey(tint, 0), new GradientColorKey(tint, 1) },
                    alphaKeys = new[] { new GradientAlphaKey(.65f, 0), new GradientAlphaKey(0, 1) }
                };
                trail.shadowCastingMode = ShadowCastingMode.Off; trail.receiveShadows = false;
                trail.emitting = false; trail.numCapVertices = 2; trails[i] = trail;
            }
            Transform silhouette = player.transform.Find("Humanoid silhouette");
            if (silhouette == null) silhouette = player.transform.Find("Anirudh / articulated silhouette");
            player.AddComponent<PlayerDodgeFeedback>().Configure(dodge, trails, silhouette);
        }

        private static Transform Group(Transform parent, string name)
        { var child = new GameObject(name).transform; child.SetParent(parent, false); return child; }
    }
}
