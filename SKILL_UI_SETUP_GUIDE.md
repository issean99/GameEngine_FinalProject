# Skill UI System - Unity Setup Guide
# 스킬 UI 시스템 - Unity 설정 가이드

## Overview / 개요

This guide explains how to set up the skill UI system in Unity. The system displays skill icons with cooldowns at the bottom-right of the screen, appearing dynamically as the player acquires skills.

이 가이드는 Unity에서 스킬 UI 시스템을 설정하는 방법을 설명합니다. 시스템은 화면 오른쪽 하단에 쿨타임이 표시되는 스킬 아이콘을 표시하며, 플레이어가 스킬을 획득할 때 동적으로 나타납니다.

---

## Step 1: Create Canvas / 캔버스 생성

1. **Hierarchy → Right-click → UI → Canvas**
2. Canvas settings / 캔버스 설정:
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler Component:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080` (or your target resolution)
     - Match: `0.5` (balance between width and height)

---

## Step 2: Create Skill Container / 스킬 컨테이너 생성

This container will hold all skill slots.
이 컨테이너는 모든 스킬 슬롯을 담게 됩니다.

1. **Right-click Canvas → Create Empty**
2. Rename to: `SkillContainer`
3. **RectTransform settings / RectTransform 설정:**
   - Anchor Preset: **Bottom-Right** (Alt+Shift+Click on bottom-right preset)
   - Anchor Min: `(1, 0)` - X=1, Y=0
   - Anchor Max: `(1, 0)` - X=1, Y=0
   - Pivot: `(1, 0)` - X=1, Y=0 (bottom-right pivot)
   - Anchored Position: `(-20, 20)` (20 pixels from bottom-right corner)
   - Width: `400`
   - Height: `100`

4. **Add Component: Horizontal Layout Group**
   - Child Alignment: `Middle Right`
   - Child Controls Size: ✓ Width, ✓ Height (both checked)
   - Child Force Expand: ☐ Width, ☐ Height (both unchecked)
   - Spacing: `0` (we'll handle spacing in SkillUIManager)
   - **Note:** Actually, disable this component or remove it - we'll position manually using SkillUIManager.RepositionSlots()
   - **참고:** 실제로는 이 컴포넌트를 비활성화하거나 제거하세요 - SkillUIManager.RepositionSlots()를 사용하여 수동으로 배치합니다

---

## Step 3: Create Skill Slot Prefab / 스킬 슬롯 프리팹 생성

### A. Create Slot Container / 슬롯 컨테이너 생성

1. **Right-click SkillContainer → UI → Image**
2. Rename to: `SkillSlotUI`
3. **RectTransform settings:**
   - Width: `60`
   - Height: `60`
   - Anchors: Center
   - Pivot: `(0.5, 0.5)`

4. **Image Component:**
   - Color: `#2B2B2B` (dark background) or transparent
   - Leave Source Image empty or use a slot background sprite

### B. Add Skill Icon / 스킬 아이콘 추가

1. **Right-click SkillSlotUI → UI → Image**
2. Rename to: `SkillIcon`
3. **RectTransform settings:**
   - Anchor Preset: Stretch (Alt+Shift+Click center)
   - Left/Right/Top/Bottom: `5` (5 pixel padding on all sides)

4. **Image Component:**
   - Source Image: Leave empty (will be set by script)
   - Preserve Aspect: ✓ Checked

### C. Add Cooldown Overlay / 쿨다운 오버레이 추가

1. **Right-click SkillSlotUI → UI → Image**
2. Rename to: `CooldownOverlay`
3. **RectTransform settings:**
   - Anchor Preset: Stretch
   - Left/Right/Top/Bottom: `5` (same padding as icon)

4. **Image Component:**
   - Source Image: `UISprite` (Unity's default UI circle sprite, or any filled sprite)
   - Color: `#000000CC` (black with 80% alpha)
   - Image Type: `Filled`
   - Fill Method: `Radial 360`
   - Fill Origin: `Top`
   - Fill Amount: `0` (will be controlled by script)
   - Clockwise: ✓ Checked

### D. Add Cooldown Text / 쿨다운 텍스트 추가

1. **Right-click SkillSlotUI → UI → Text - TextMeshPro**
   - If prompted to import TMP Essentials, click "Import TMP Essentials"

2. Rename to: `CooldownText`
3. **RectTransform settings:**
   - Anchor Preset: Center
   - Width: `50`
   - Height: `50`

4. **TextMeshPro Component:**
   - Font Size: `24`
   - Alignment: Center (Horizontal + Vertical)
   - Color: `#FFFFFF` (white)
   - Font Style: Bold
   - Outline: Optional (Thickness: 0.2, Color: black)

### E. Add Key Binding Text / 키 바인딩 텍스트 추가

1. **Right-click SkillSlotUI → UI → Text - TextMeshPro**
2. Rename to: `KeyBindingText`
3. **RectTransform settings:**
   - Anchor Preset: Bottom-Right
   - Anchored Position: `(-2, 2)` (2 pixels from bottom-right)
   - Width: `30`
   - Height: `20`

4. **TextMeshPro Component:**
   - Font Size: `14`
   - Alignment: Bottom-Right
   - Color: `#FFFFFF` (white)
   - Outline: Optional (Thickness: 0.2, Color: black)

### F. Attach SkillSlotUI Script / SkillSlotUI 스크립트 연결

1. **Select SkillSlotUI root object**
2. **Add Component → SkillSlotUI** (search for the script)
3. **Assign References in Inspector:**
   - Skill Icon: Drag `SkillIcon` object
   - Cooldown Overlay: Drag `CooldownOverlay` object
   - Cooldown Text: Drag `CooldownText` object
   - Key Binding Text: Drag `KeyBindingText` object

4. **Visual Settings (optional customization):**
   - Ready Color: `#FFFFFF` (white)
   - Cooldown Color: `#4D4D4DCC` (dark gray)

### G. Create Prefab / 프리팹 생성

1. **Drag SkillSlotUI from Hierarchy → Project folder**
   - Save to: `Assets/GE_FinalProject/Prefabs/SkillSlotUI.prefab`

2. **Delete SkillSlotUI from Hierarchy** (we only need the prefab)

---

## Step 4: Setup SkillUIManager / SkillUIManager 설정

1. **Select Canvas** (or create an empty GameObject under Canvas)
2. **Create new Empty GameObject under Canvas**
3. Rename to: `SkillUIManager`
4. **Add Component → SkillUIManager** (search for the script)

5. **Assign References in Inspector:**
   - **Skill Slot Prefab:** Drag `SkillSlotUI` prefab from Project
   - **Skill Container:** Drag `SkillContainer` from Hierarchy
   - **Slot Spacing:** `70` (distance between slots in pixels)

6. **Assign Skill Icons (create/import these sprites first):**
   - **Fireball Icon:** Drag fireball sprite
   - **Explosion Icon:** Drag arcane burst sprite
   - **Defense Icon:** Drag slime shield sprite
   - **Dash Icon:** Drag wolf dash sprite

   **How to prepare skill icons / 스킬 아이콘 준비 방법:**
   - Import your skill icon images to: `Assets/GE_FinalProject/UI/SkillIcons/`
   - Set Texture Type to: `Sprite (2D and UI)`
   - Recommended size: 64x64 or 128x128 pixels

---

## Step 5: Testing / 테스트

### Test in Play Mode / 플레이 모드에서 테스트

1. **Start Play Mode**
2. **Collect skill items** (Wizard spellbook, Slime defense item, Werewolf dash item)
3. **Expected behavior / 예상 동작:**
   - First skill appears at bottom-right corner
   - Second skill appears to the left of first skill
   - Third skill appears to the left of second skill
   - Each skill shows its icon and key binding
   - When you use a skill, cooldown overlay fills and timer counts down

### Debug Tips / 디버그 팁

If skills don't appear / 스킬이 나타나지 않는 경우:
- Check Console for errors
- Verify SkillUIManager.Instance is not null
- Ensure SkillSlotUI prefab has SkillSlotUI component attached
- Check that skill icons are assigned in SkillUIManager

If cooldowns don't work / 쿨다운이 작동하지 않는 경우:
- Verify PlayerController calls `TriggerCooldown()` when skills are cast
- Check that CooldownOverlay Image has Fill Type set to "Filled" and Fill Method set to "Radial 360"

---

## Expected Hierarchy Structure / 예상 계층 구조

```
Canvas
├── SkillUIManager (Empty GameObject with SkillUIManager script)
└── SkillContainer (Empty GameObject, RectTransform anchored bottom-right)
    └── [Skill slots instantiated here at runtime]
        ├── SkillSlotUI (Clone) - First acquired skill (rightmost)
        ├── SkillSlotUI (Clone) - Second acquired skill
        └── SkillSlotUI (Clone) - Third acquired skill (leftmost)
```

---

## Skill Acquisition Flow / 스킬 획득 흐름

1. **Player picks up skill item** (e.g., Wizard Spellbook)
   - 플레이어가 스킬 아이템을 획득합니다

2. **Skill item calls PlayerController.UnlockFireballSkill()**
   - 스킬 아이템이 PlayerController의 언락 메서드를 호출합니다

3. **PlayerController calls SkillUIManager.Instance.AddSkill(SkillType.Fireball, cooldown)**
   - PlayerController가 SkillUIManager에 스킬을 추가합니다

4. **SkillUIManager instantiates SkillSlotUI prefab**
   - SkillUIManager가 스킬 슬롯 프리팹을 생성합니다

5. **SkillSlotUI displays with icon, key binding, and cooldown ready**
   - 스킬 슬롯이 아이콘, 키 바인딩과 함께 표시됩니다

6. **When skill is used, PlayerController calls SkillUIManager.Instance.TriggerCooldown()**
   - 스킬 사용 시 PlayerController가 쿨다운을 트리거합니다

7. **SkillSlotUI animates cooldown overlay and text**
   - 스킬 슬롯이 쿨다운 오버레이와 텍스트를 애니메이션합니다

---

## Customization Tips / 커스터마이징 팁

### Change Slot Size / 슬롯 크기 변경
- Modify SkillSlotUI prefab Width/Height (default 60x60)
- Adjust `slotSpacing` in SkillUIManager (default 70)

### Change Position / 위치 변경
- Modify SkillContainer's Anchored Position (default -20, 20 from bottom-right)
- To move to bottom-left: Set Anchor to Bottom-Left, change X to positive value

### Add Visual Effects / 시각 효과 추가
- Add animations to SkillSlotUI prefab (e.g., pulse when skill is ready)
- Add particle effects on skill acquisition
- Add sound effects in SkillUIManager.AddSkill()

### Custom Fonts / 커스텀 폰트
- Import your font
- Assign to CooldownText and KeyBindingText in prefab

---

## Troubleshooting / 문제 해결

### Problem: Skill icons are stretched / 아이콘이 늘어남
**Solution:** Enable "Preserve Aspect" on SkillIcon Image component

### Problem: Cooldown overlay doesn't animate / 쿨다운 오버레이가 애니메이션되지 않음
**Solution:**
- Check Image Type is set to "Filled"
- Check Fill Method is "Radial 360"
- Verify SkillSlotUI.Update() is running (script enabled)

### Problem: Text is blurry / 텍스트가 흐림
**Solution:**
- Increase Canvas Scaler reference resolution
- Use higher font size and scale down object instead

### Problem: Skills appear in wrong order / 스킬이 잘못된 순서로 나타남
**Solution:**
- Check SkillUIManager.RepositionSlots() logic
- Verify skillSlots list order matches acquisition order

---

## Integration Complete / 통합 완료

Your skill UI system is now ready! Skills will automatically appear as the player acquires them, with working cooldown displays.

스킬 UI 시스템이 준비되었습니다! 플레이어가 스킬을 획득하면 자동으로 쿨다운 표시와 함께 나타납니다.

**Key Features:**
- ✓ Dynamic skill acquisition
- ✓ Right-to-left positioning
- ✓ Cooldown visualization (radial fill + timer)
- ✓ Key binding display
- ✓ All 4 skills supported (Fireball, Explosion, Defense, Dash)

**주요 기능:**
- ✓ 동적 스킬 획득
- ✓ 오른쪽에서 왼쪽으로 배치
- ✓ 쿨다운 시각화 (원형 채우기 + 타이머)
- ✓ 키 바인딩 표시
- ✓ 4가지 스킬 모두 지원 (화염구, 폭발, 방어, 대쉬)
