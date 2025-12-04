# 🔥 Fireball Skill System - Setup Guide
## Wizard Boss Item Drop & Player Skill System

---

## 📋 System Overview

```
Wizard Boss Defeated
        ↓
FireballSkillItem Drops
        ↓
Player Walks Over Item
        ↓
Fireball Skill Unlocked
        ↓
Press Right Mouse Button → Cast Fireball!
```

---

## 📂 Created Files

### 1. **SkillItem.cs** (Base Class)
**Location:** `Assets/GE_FinalProject/Scripts/Items/SkillItem.cs`

**Features:**
- Floating animation (sine wave)
- Rotation animation
- Pulse effect (scale animation)
- Auto-pickup on player collision
- Extensible for different skill types

### 2. **FireballSkillItem.cs** (Fireball Skill)
**Location:** `Assets/GE_FinalProject/Scripts/Items/FireballSkillItem.cs`

**Features:**
- Inherits from SkillItem
- Grants fireball skill to player
- Called on pickup

### 3. **FireballProjectile.cs** (Projectile)
**Location:** `Assets/GE_FinalProject/Scripts/Player/FireballProjectile.cs`

**Features:**
- **Damage:** 30 (configurable)
- **Speed:** 12 m/s (configurable)
- **Lifetime:** 5 seconds
- **Rotation:** Spins while flying
- **Hits:** All enemy types + bosses
- **Effects:** Trail and hit effects support

### 4. **Updated PlayerController.cs**
**Location:** `Assets/GE_FinalProject/Scripts/Player/PlayerController.cs`

**New Features:**
- Fireball skill toggle (hasFireballSkill)
- Fireball cooldown (1.5s default)
- Right-click to cast fireball
- UnlockFireballSkill() public method
- Mana system ready (currently disabled)

### 5. **Updated WizardBoss.cs**
**Location:** `Assets/GE_FinalProject/Scripts/Wizard/WizardBoss.cs`

**New Features:**
- Drops fireballSkillItemPrefab on death
- Item spawns 0.5 units above boss
- Drops immediately when die animation starts

---

## 🛠️ Unity Setup Instructions

### Step 1: Create Fireball Projectile Prefab

1. **Create Empty GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name: `Fireball`

2. **Add Components:**
   - Add Component → **Rigidbody2D**
     - Body Type: Dynamic
     - Gravity Scale: 0
     - Linear Drag: 0
     - Angular Drag: 0

   - Add Component → **CircleCollider2D**
     - Is Trigger: ✅ (Checked)
     - Radius: 0.3

   - Add Component → **FireballProjectile** (script)

3. **Add Visual (Sprite):**
   - Right-click Fireball → 2D Object → Sprite
   - Name: `FireballSprite`
   - Select a fire/magic sprite from your assets
   - **Suggested:** Use a red/orange magic circle or fire effect
   - Scale: (0.5, 0.5, 1) or adjust to fit

4. **Configure FireballProjectile Script:**
   - Damage: 30
   - Speed: 12
   - Lifetime: 5
   - Rotation Speed: 360
   - Hit Effect Prefab: (Optional - assign explosion effect)
   - Trail Effect Prefab: (Optional - assign trail effect)

5. **Layer & Tag:**
   - Layer: Projectile (or default)
   - Tag: Untagged

6. **Save as Prefab:**
   - Drag Fireball from Hierarchy to `Assets/GE_FinalProject/Prefabs/`
   - Delete from Hierarchy

---

### Step 2: Create Fireball Skill Item Prefab

1. **Create Empty GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name: `FireballSkillItem`

2. **Add Components:**
   - Add Component → **CircleCollider2D**
     - Is Trigger: ✅ (Checked)
     - Radius: 1.0

   - Add Component → **FireballSkillItem** (script)

3. **Add Visual (Sprite):**
   - Right-click FireballSkillItem → 2D Object → Sprite
   - Name: `ItemSprite`
   - **Suggested Sprite:**
     - Use a glowing orb, magic tome, or spell scroll
     - Color: Red/Orange tint for fire theme
   - Position: (0, 0, 0)
   - Scale: (0.8, 0.8, 1)

4. **Configure FireballSkillItem Script:**
   - Skill Name: "Fireball"
   - Float Height: 0.5
   - Float Speed: 2
   - Rotation Speed: 90
   - Pulse Effect: ✅ (Checked)
   - Pulse Speed: 3
   - Pulse Min: 0.8
   - Pulse Max: 1.2
   - Pickup Effect Prefab: (Optional - assign particle effect)

5. **Optional - Add Glow Effect:**
   - Right-click FireballSkillItem → 2D Object → Sprite
   - Name: `GlowEffect`
   - Sprite: Circle or soft glow
   - Color: Orange with low alpha (1, 0.6, 0.2, 0.5)
   - Scale: (1.2, 1.2, 1)
   - Move behind item sprite in hierarchy

6. **Layer & Tag:**
   - Layer: Default
   - Tag: Untagged

7. **Save as Prefab:**
   - Drag FireballSkillItem to `Assets/GE_FinalProject/Prefabs/Items/`
   - Delete from Hierarchy

---

### Step 3: Setup Player Controller

1. **Select Player GameObject in scene**

2. **Find PlayerController Component**

3. **Configure Fireball Settings:**
   - **Fireball Prefab:** Drag `Fireball` prefab here
   - **Fireball Cooldown:** 1.5
   - **Fireball Mana Cost:** 0 (for now)
   - **Has Fireball Skill:** ❌ (Unchecked) - Will be unlocked by item!

> **Important:** Leave "Has Fireball Skill" UNCHECKED! It will be automatically checked when player picks up the item.

---

### Step 4: Setup Wizard Boss

1. **Open boss1.unity scene**
   - Or wherever your Wizard Boss is located

2. **Select WizardBoss GameObject**

3. **Find WizardBoss Component**

4. **Configure Item Drop:**
   - Scroll to bottom: **Item Drop** section
   - **Fireball Skill Item Prefab:** Drag `FireballSkillItem` prefab here

5. **Save Scene**

---

## 🎮 How to Test

### Test 1: Item Drop
1. Play boss1.unity scene
2. Defeat Wizard Boss
3. ✅ FireballSkillItem should drop above the boss corpse
4. ✅ Item should float and rotate

### Test 2: Item Pickup
1. Walk player over the dropped item
2. ✅ Item should disappear
3. ✅ Console should show: "Player picked up: Fireball"
4. ✅ Console should show: "Fireball skill unlocked! Press Right Mouse Button to cast."

### Test 3: Fireball Casting
1. After picking up item
2. Press **Right Mouse Button** towards an enemy
3. ✅ Fireball should spawn and fly towards mouse cursor
4. ✅ Fireball should rotate while flying
5. ✅ Fireball should damage enemies on hit
6. ✅ 1.5 second cooldown between casts

---

## ⚙️ Customization Options

### Fireball Damage & Speed
Edit **FireballProjectile** component on prefab:
- **More Powerful:** Increase Damage (30 → 40)
- **Faster:** Increase Speed (12 → 15)
- **Longer Range:** Increase Lifetime (5 → 7)

### Fireball Cooldown
Edit **PlayerController** on Player:
- **Spam Cast:** Decrease Cooldown (1.5 → 0.5)
- **Limited Use:** Increase Cooldown (1.5 → 3.0)

### Item Float Animation
Edit **FireballSkillItem** component on prefab:
- **Higher Float:** Increase Float Height (0.5 → 1.0)
- **Faster Float:** Increase Float Speed (2 → 4)
- **Faster Spin:** Increase Rotation Speed (90 → 180)

### Item Pulse Effect
Edit **FireballSkillItem** component:
- **Disable Pulse:** Uncheck Pulse Effect
- **Stronger Pulse:** Increase Pulse Max (1.2 → 1.5)
- **Faster Pulse:** Increase Pulse Speed (3 → 5)

---

## 🔧 Advanced Features (Optional)

### Add Mana System

**In PlayerController.cs:**
```csharp
[Header("Mana System")]
[SerializeField] private int maxMana = 100;
[SerializeField] private int currentMana;
[SerializeField] private float manaRegenRate = 5f; // per second
```

**Uncomment in TryFireball():**
```csharp
if (currentMana < fireballManaCost)
{
    Debug.Log("Not enough mana!");
    return;
}
```

**Uncomment in CastFireball():**
```csharp
currentMana -= fireballManaCost;
```

### Add Cast Animation

**In PlayerController.cs CastFireball():**
```csharp
// Uncomment this line:
animator.SetTrigger("Cast");
```

**In Animator:**
- Add "Cast" trigger parameter
- Create casting animation
- Add transition from Any State → Cast

### Add Visual/Audio Effects

**Fireball Spawn Effect:**
1. Create particle effect prefab (fire burst)
2. In CastFireball(), spawn at player position:
```csharp
Instantiate(castEffectPrefab, transform.position, Quaternion.identity);
```

**Fireball Trail:**
1. Assign trail effect prefab to FireballProjectile
2. Automatically spawns as child

**Fireball Hit Effect:**
1. Assign explosion effect prefab to FireballProjectile
2. Spawns on collision

**Audio:**
```csharp
// In CastFireball()
AudioSource.PlayClipAtPoint(fireballCastSound, transform.position);

// In FireballProjectile OnHit()
AudioSource.PlayClipAtPoint(fireballHitSound, transform.position);
```

---

## 🎨 Suggested Visual Assets

### Fireball Sprite
- **Recommended:** Glowing orange/red sphere
- **Alternative:** Fire swirl effect
- **Size:** 32x32 or 64x64 pixels
- **Color:** Red/Orange gradient

### Skill Item Sprite
- **Recommended:** Magic tome, spell scroll, or glowing orb
- **Color:** Match fireball (red/orange)
- **Size:** 64x64 or 128x128 pixels
- **Effect:** Add subtle glow

### Trail Effect
- **Type:** Particle System
- **Color:** Orange to red gradient
- **Fade:** Fade out over 0.5 seconds
- **Size:** Decrease over lifetime

### Hit Effect
- **Type:** Animated sprite or particles
- **Animation:** Explosion/burst
- **Duration:** 0.3-0.5 seconds
- **Color:** Orange/red/yellow

---

## 🐛 Troubleshooting

### Problem: Item doesn't drop
**Solution:**
- Check WizardBoss → Fireball Skill Item Prefab is assigned
- Check prefab exists in project

### Problem: Item drops but can't be picked up
**Solution:**
- Check FireballSkillItem has CircleCollider2D
- Check "Is Trigger" is checked
- Check Player has tag "Player"

### Problem: Fireball doesn't spawn
**Solution:**
- Check PlayerController → Fireball Prefab is assigned
- Check "Has Fireball Skill" is TRUE after pickup
- Check Right Mouse Button is working

### Problem: Fireball doesn't damage enemies
**Solution:**
- Check FireballProjectile has CircleCollider2D with "Is Trigger" checked
- Check enemies have correct scripts (SlimeController, etc.)
- Check Fireball damage is set > 0

### Problem: Fireball goes through walls
**Solution:**
- Check walls are NOT triggers
- Check walls have Collider2D
- Make sure Fireball's OnTriggerEnter2D checks for non-trigger collisions

### Problem: Can spam fireball with no cooldown
**Solution:**
- Check Fireball Cooldown is set (should be 1.5)
- Check Time.time is being used correctly
- Console should show "Fireball on cooldown!" if pressed too fast

---

## 📋 Checklist

- [ ] Created Fireball prefab with sprite and components
- [ ] Created FireballSkillItem prefab with floating animation
- [ ] Assigned Fireball prefab to PlayerController
- [ ] Assigned FireballSkillItem prefab to WizardBoss
- [ ] Tested boss death → item drop
- [ ] Tested item pickup → skill unlock
- [ ] Tested right-click → fireball cast
- [ ] Tested fireball damage on enemies
- [ ] Tested cooldown system
- [ ] (Optional) Added visual effects
- [ ] (Optional) Added audio effects

---

## 🎯 Next Steps

### Add More Skills
1. Create new skill item classes (inherit from SkillItem)
2. Examples:
   - IceBoltSkillItem (from Ice Boss)
   - LightningSkillItem (from Thunder Boss)
   - HealingSkillItem (from drops)

### Add Skill Wheel UI
1. Create skill slot UI (1, 2, 3, 4 keys)
2. Allow switching between multiple skills
3. Show cooldowns on UI

### Add Skill Combos
1. Cast multiple skills in sequence for bonus effects
2. Example: Fire + Ice = Steam explosion

### Add Skill Upgrade System
1. Pick up duplicate items to upgrade skill
2. Level 1: Base fireball
3. Level 2: Bigger fireball
4. Level 3: Explosive fireball (AOE)

---

## 📝 Summary

**What was created:**
- ✅ Base skill item system (SkillItem.cs)
- ✅ Fireball skill item (FireballSkillItem.cs)
- ✅ Fireball projectile (FireballProjectile.cs)
- ✅ Player skill unlock system (PlayerController update)
- ✅ Boss item drop system (WizardBoss update)

**How it works:**
1. Wizard Boss dies → Drops FireballSkillItem
2. Player touches item → Unlocks fireball skill
3. Player presses Right Mouse → Casts fireball towards cursor
4. Fireball flies and damages enemies
5. 1.5s cooldown between casts

**Result:**
A complete RPG-style skill acquisition system where defeating bosses rewards the player with new abilities!

---

**Created by:** Claude Code
**Date:** 2024-12-02
**System:** Fireball Skill & Item Drop System
