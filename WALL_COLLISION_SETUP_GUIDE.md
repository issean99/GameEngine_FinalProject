# 벽 충돌 설정 가이드

캐릭터가 벽을 뚫고 지나가지 못하도록 설정하는 방법입니다.

## 문제 원인

캐릭터가 벽을 뚫고 지나가는 이유:
1. Collider가 없거나 비활성화됨
2. Rigidbody2D Body Type이 Kinematic으로 설정됨
3. 벽에 Collider가 없음
4. Layer Collision Matrix 설정 문제

---

## 해결 방법

### 1. Player GameObject 설정

#### A. Collider 확인
1. **Player GameObject 선택**
2. **Inspector에서 Collider 확인**
   - `CapsuleCollider2D` 또는 `BoxCollider2D`가 있어야 함
   - 없다면 추가: Add Component → Physics 2D → Capsule Collider 2D

3. **Collider 설정**
   - **Is Trigger**: ❌ 체크 해제 (중요!)
   - **Size**: 캐릭터 크기에 맞게 조정
   - **Offset**: 필요시 조정

#### B. Rigidbody2D 설정
1. **Player GameObject의 Rigidbody2D 확인**
2. **설정 값**:
   - **Body Type**: `Dynamic` (Kinematic이 아님!)
   - **Simulated**: ✅ 체크
   - **Collision Detection**: `Continuous` (빠른 이동 시 권장)
   - **Sleeping Mode**: Never Sleep
   - **Interpolate**: Interpolate (부드러운 움직임)
   - **Constraints**:
     - Freeze Position Z: ✅ 체크 (2D 게임)
     - Freeze Rotation Z: ✅ 체크 (회전 방지)
   - **Gravity Scale**: `0` (탑다운 게임이므로)
   - **Linear Drag**: `0` (마찰 없음)
   - **Angular Drag**: `0.05`

---

### 2. Wall/Tile GameObject 설정

#### A. Wall Prefab 설정
1. **Wall GameObject 선택** (또는 Tilemap)
2. **Collider 추가**
   - Tilemap인 경우: Add Component → Tilemap Collider 2D
   - 개별 오브젝트인 경우: Add Component → Box Collider 2D

3. **Collider 설정**
   - **Is Trigger**: ❌ 체크 해제
   - **Used By Composite** (Tilemap만): 여러 타일을 하나로 합치려면 체크

#### B. Composite Collider (Tilemap 최적화)
1. **Tilemap에 Composite Collider 2D 추가** (선택사항)
   - Add Component → Composite Collider 2D
   - 여러 타일의 Collider를 하나로 합쳐 성능 향상

2. **Rigidbody2D 자동 추가됨**
   - **Body Type**: `Static` (벽은 움직이지 않음!)
   - **Simulated**: ✅ 체크

---

### 3. Layer 설정 (권장)

#### A. Layer 생성
1. **Edit → Project Settings → Tags and Layers**
2. **User Layer에 추가**:
   - Layer 6: `Player`
   - Layer 7: `Enemy`
   - Layer 8: `Wall`
   - Layer 9: `Ground`

#### B. GameObject에 Layer 할당
1. **Player GameObject** → Layer: `Player`
2. **Enemy GameObject들** → Layer: `Enemy`
3. **Wall/Tile GameObject** → Layer: `Wall`

#### C. Collision Matrix 설정
1. **Edit → Project Settings → Physics 2D**
2. **Layer Collision Matrix 스크롤**
3. **체크 설정**:
   - Player ↔ Wall: ✅ (충돌함)
   - Player ↔ Ground: ✅ (충돌함)
   - Player ↔ Enemy: ❌ (스크립트로 처리)
   - Enemy ↔ Wall: ✅ (충돌함)
   - Wall ↔ Wall: ❌ (필요 없음)

---

### 4. 스크립트 확인

PlayerController.cs에서 Rigidbody2D를 올바르게 사용하는지 확인:

```csharp
// FixedUpdate에서 이동 처리 (이미 구현됨)
private void FixedUpdate()
{
    if (isStunned)
    {
        rb.linearVelocity = Vector2.zero;
        return;
    }

    if (isDashing)
    {
        float dashSpeed = dashDistance / dashDuration;
        rb.linearVelocity = dashDirection * dashSpeed;
        return;
    }

    HandleMovement();
}

// linearVelocity 사용 (Transform 직접 변경 ❌)
private void HandleMovement()
{
    float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
    rb.linearVelocity = moveInput * currentSpeed; // ✅ 올바름

    // transform.position += ... // ❌ 이렇게 하면 안 됨!
}
```

---

## 문제 해결

### 문제 1: 여전히 벽을 뚫고 지나감

**확인 사항**:
1. Player의 Collider가 `Is Trigger = false`인지 확인
2. Player의 Rigidbody2D가 `Dynamic`인지 확인
3. Wall의 Collider가 존재하고 `Is Trigger = false`인지 확인
4. 스크립트에서 `transform.position`을 직접 변경하지 않는지 확인

### 문제 2: 캐릭터가 벽에서 튕김

**해결**:
1. **Physics Material 2D 생성**
   - Assets 우클릭 → Create → Physics Material 2D
   - 이름: `NoFriction`
   - **Friction**: `0`
   - **Bounciness**: `0`

2. **Player와 Wall의 Collider에 적용**
   - Collider → Material: NoFriction 할당

### 문제 3: 캐릭터가 벽 모서리에 걸림

**해결**:
1. **Composite Collider 사용** (Tilemap)
2. **Collider 모서리 둥글게**:
   - Edge Radius 값 증가 (CapsuleCollider2D)

### 문제 4: 빠르게 움직일 때 뚫고 지나감

**해결**:
1. **Rigidbody2D 설정 변경**:
   - Collision Detection: `Continuous`
2. **FixedUpdate 사용**:
   - 이동 로직이 FixedUpdate에 있는지 확인 (이미 구현됨)

---

## 빠른 체크리스트

- [ ] Player에 Collider 있음 (Is Trigger = false)
- [ ] Player에 Rigidbody2D 있음 (Body Type = Dynamic)
- [ ] Wall/Tilemap에 Collider 있음 (Is Trigger = false)
- [ ] Wall에 Rigidbody2D 있음 (Body Type = Static)
- [ ] Rigidbody2D Constraints에서 Freeze Rotation Z 체크
- [ ] 이동 로직이 FixedUpdate에서 linearVelocity 사용
- [ ] Physics Material 2D로 Friction = 0 설정
- [ ] Collision Detection = Continuous

---

## Unity 에디터에서 실시간 테스트

1. **Play Mode로 진입**
2. **Window → Analysis → Physics Debugger 열기**
3. **Show Colliders 체크**
4. **충돌이 감지되는지 확인** (충돌 시 색상 변경)

또는

1. **Scene View에서 Gizmos 버튼 클릭**
2. **Physics 2D → Show Collider 체크**
3. **플레이 중 Collider가 보이는지 확인**
