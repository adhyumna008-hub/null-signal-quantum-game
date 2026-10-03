# NULL SIGNAL — Phase 3 visual foundation

## Supplied references inspected
- [Astra7Environment.png](References/Astra7Environment.png): layered modular plates, copper jaali vents, framed doors, perimeter consoles, circular research apparatus.
- [QuantumVFX_UI.png](References/QuantumVFX_UI.png): floating geometric cores, fine orbital rings, directional phase flow, compact ANVESH controls and live instruments.
- [VisualBible1.png](References/VisualBible1.png): graphite station structure, warm hardware against cool scientific light, real 3D isometric composition.
- [AnirudhCharacter.png](References/AnirudhCharacter.png) and [CharacterCast.png](References/CharacterCast.png): dark technical suit, copper harness, compact backpack, exposed face, cyan wrist device. Procedural silhouette only in this phase.

## Visual rules
- Graphite #141E29 structural frames; blue graphite #273B4A floor; titanium #738691 bevels; copper #AB744C fasteners and trims; saffron #DF9D4B practical accents.
- Cyan #63DCE8 means scientific equipment and interaction; violet #9380ED means quantum structure. All candidates use the same palette. Phase is direction/sign, never a special target color.
- Low metallic sheen, broad bevels, quiet surfaces, narrow emissive strips. No religious symbols, ornate architecture, saturated neon walls, or reference sheets pasted into the game.
- Floors use a 2.4 m grid; walkable height is y=0. Modules use meters, +Y up and +Z forward, with origin at floor center. Rear walls provide height; camera-facing boundaries stay low.
- One radial apparatus sits behind four widely spaced nodes. Negative space around the nodes preserves the ability to compare their waveforms. Floor circuits connect the apparatus to the nodes.
- Fixed 45-degree yaw, 42-degree pitch orthographic camera, bounded follow, gentle encounter zoom. Cutaway front boundaries preserve sightlines.
- One directional shadow light plus three shadowless practical lights. Small scene-local bloom and vignette; no depth of field, volumetric rendering, extra render features, or new packages.
- Humanoid: readable boots, separate legs/arms, armored torso, head/hair, copper harness and cyan ANVESH cuff. Small procedural stride, not a production rig.
- UI: screen-space uGUI, crisp sans serif, near-black translucent panels, fine cyan borders, warm action accents. Compact identity/objective at top; commands and actual per-candidate amplitude/probability along bottom. No fabricated health, telemetry, or energy meters. F3 developer numbers separate and hidden initially.

## Quantum presentation contract
Read existing QuantumEncounter snapshots only. Absolute amplitude drives core size, waveform height, alpha and particle count. Sign reverses waveform/particle travel. Probability drives the ground arc and HUD percentage. Measurement collapse uses the simulation's measured index. No target identity is read by node renderers.

## Scope and review
All geometry is generated locally from reusable meshes; no external assets or packages. Builder saves shared materials, modular prefabs and a separate VisualPrototype scene when invoked. Phase 1/2 scenes and quantum math remain intact. This pass receives compile/static review; the human performs visual and keyboard review in normal Unity Play Mode.

## Phase 4 extension
- Connected 24 m laboratory rooms on a 28.8 m station grid, joined by 4.8 m service passages. Extended structural underdeck fills camera-edge gaps. Front boundaries are cut away; floor and structure continue beyond the view.
- Room-centered 45-degree isometric framing holds four/eight states in view, then follows the player through entrances and passages. No free camera.
- Full-resolution forward URP preset with 2x MSAA, no additional camera AA, no dynamic resolution, no gameplay depth of field or motion blur. Bloom 0.10, threshold 1.5. Remove normal-gameplay fog haze.
- 1920x1080 screen-space UI with pixel alignment and larger existing dynamic-font labels. Subtitles remain outside world post processing. No new font package/import dependency.
- Maintenance uses dim cyan architecture and warm failing practicals; ANVESH discovery has a powered-down room restored by real measurement. Research rooms use clean cyan; reactor adds copper/orange damage and violet interference. Holographic researchers use animated 3D geometry, not still images.
# Phase 5 combat extension

Reuse the graphite, titanium, copper and cyan Astra-7 kit. Security drones use flattened mechanical bodies, articulated vanes and circular cyan sensors; red is reserved for attack windup and failed-measurement hostility. Chhaya uses tall broken humanoid silhouettes, offset dark fragments and restrained violet motes. Its three-second formation remains real geometry in the gameplay camera, with the player free to move. Ground circles indicate a committed strike position before damage. Copper-white shield breaks indicate the actual measured vulnerability window. Keep the right analysis panel narrow and all equations tied to captured simulation values; no decorative fictional telemetry.

## Phase 7 final palette and motion
- Broaden station surfaces beyond blue: neutral graphite #20262B, brushed titanium #9DA5A5, warm ivory #D3CEC1, muted sage gray floor plates #455154, copper #BB8057 and saffron practical light #E5AD60. Reserve cyan for science; violet for anomaly; red for danger; mint green for confirmed success. White and copper architecture provide contrast without rainbow lighting.
- Researcher recordings have coherent ivory coats, warm projected faces, dark hair, glasses and small copper identity details. A shared transparent URP shader supplies aligned scanlines, a moving acquisition band, restrained edge light and occasional tiny displacement. Continuous breathing, head turns and gesture anticipation keep the recording readable.
- Dodge leaves two short copper/cyan streaks, then settles immediately. SCAN expands from the wrist; MARK reverses signed wave travel with a brief phase ripple; AMPLIFY gathers then redistributes; LOCK contracts before the sampled result appears. Green/red only confirms the actual measured outcome. No particles or animation create fictitious amplitude changes.
- The visual pass uses source/static review; final camera-scale legibility and shader appearance require the human smoke test in Unity/WebGL.
