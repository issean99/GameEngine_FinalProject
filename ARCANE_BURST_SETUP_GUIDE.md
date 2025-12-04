# Wizard 보스 ArcaneBurst 이펙트 만들기 가이드

Wizard Boss와 Player가 사용하는 ArcaneBurst(폭발 스킬) 이펙트를 만드는 방법입니다.

---

## ArcaneBurst란?

- **기능**: 마우스 클릭 위치 또는 보스 주변에 폭발 범위 생성
- **동작 순서**:
  1. 경고 마커가 깜빡이며 표시됨 (1초)
  2. 폭발 이펙트 발생
  3. 범위 내 적/플레이어에게 데미지
  4. 이펙트 사라짐

---

## 1. Prefab 구조 만들기

### A. 기본 GameObject 생성

1. **Hierarchy에서 빈 GameObject 생성**
   - 우클릭 → Create Empty
   - 이름: `ArcaneBurst1`

2. **Transform 초기화**
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

---

### B. 경고 마커 (Warning Marker) 추가

1. **ArcaneBurst1의 자식으로 빈 GameObject 생성**
   - ArcaneBurst1 우클릭 → Create Empty
   - 이름: `WarningMarker`

2. **Sprite Renderer 추가**
   - Add Component → Rendering → Sprite Renderer

3. **Sprite Renderer 설정**
   - **Sprite**: 원형 스프라이트 선택
     - Unity 기본: Circle (또는 프로젝트의 원형 스프라이트)
   - **Color**: 빨간색 또는 주황색
     - 예: R:255, G:100, B:100, A:180
   - **Sorting Layer**: Effects
   - **Order in Layer**: 5

4. **Transform 설정**
   - Position: (0, 0, 0)
   - Scale: (1, 1, 1) - 스크립트가 자동 조정

---

### C. 폭발 이펙트 (Explosion Effect) 추가

1. **ArcaneBurst1의 자식으로 빈 GameObject 생성**
   - ArcaneBurst1 우클릭 → Create Empty
   - 이름: `ExplosionEffect`

2. **Sprite Renderer 추가**
   - Add Component → Rendering → Sprite Renderer

3. **Sprite Renderer 설정**
   - **Sprite**: 폭발 이펙트 스프라이트
     - 프로젝트의 폭발/마법 이펙트 스프라이트 사용
     - 또는 밝은 원형 스프라이트
   - **Color**: 흰색 또는 보라색
     - 예: R:200, G:150, B:255, A:255
   - **Sorting Layer**: Effects
   - **Order in Layer**: 10 (경고 마커보다 위)

4. **Transform 설정**
   - Position: (0, 0, 0)
   - Scale: (1, 1, 1) - 스크립트가 자동 조정

---

## 2. 스크립트 추가

### A. ArcaneBurstEffect 스크립트 부착

1. **ArcaneBurst1 GameObject 선택**
2. **Add Component → ArcaneBurstEffect**
3. 스크립트가 이미 있으므로 자동으로 찾아짐

---

### B. Inspector 설정

#### Explosion Settings (폭발 설정)

**Delay Time**
- **의미**: 경고 후 폭발까지 시간
- **기본값**: 1초
- **설명**: 빨간 원이 깜빡이는 시간
- **추천값**: 0.8~1.5초
- **용도**: 플레이어가 회피할 시간

**Explosion Radius**
- **의미**: 폭발 반경
- **기본값**: 3
- **설명**: 폭발 범위 크기
- **추천값**: 2.5~4
- **Scene View**: 빨간 원으로 표시됨

**Damage**
- **의미**: 폭발 데미지
- **기본값**: 20
- **보스 사용**: 20~30
- **플레이어 사용**: 30~50

**Is Player Cast**
- **의미**: 플레이어가 시전하는지 여부
- **false (체크 해제)**: 보스가 사용 → 플레이어에게 데미지
- **true (체크)**: 플레이어가 사용 → 적에게 데미지

---

#### Visual Settings (비주얼 설정)

**Warning Marker**
- **의미**: 경고 표시 Sprite Renderer
- **할당**: WarningMarker GameObject의 Sprite Renderer 드래그

**Explosion Effect**
- **의미**: 폭발 이펙트 Sprite Renderer
- **할당**: ExplosionEffect GameObject의 Sprite Renderer 드래그

**Warning Pulse Speed**
- **의미**: 경고 깜빡임 속도
- **기본값**: 3
- **설명**: 높을수록 빠르게 깜빡임
- **추천값**: 2~4

---

## 3. 자동 할당 기능

스크립트가 자동으로 Sprite Renderer를 찾습니다:
- **첫 번째 Sprite Renderer**: Warning Marker
- **두 번째 Sprite Renderer**: Explosion Effect

수동 할당을 원하면 Inspector에서 직접 드래그하세요.

---

## 4. Prefab으로 저장

1. **ArcaneBurst1 GameObject를 Project View로 드래그**
   - 경로: `Assets/GE_FinalProject/Prefabs/`
   - 이름: `ArcaneBurst1`

2. **Hierarchy에서 원본 삭제** (선택사항)

---

## 5. 사용 방법

### A. Wizard Boss에서 사용

1. **WizardBoss GameObject 선택**
2. **Inspector → Wizard Boss 스크립트**
3. **Arcane Burst Prefab** 슬롯에 ArcaneBurst1 드래그

**보스 설정 예시**:
```
Arcane Burst Prefab: ArcaneBurst1
Arcane Burst Cooldown: 5
Arcane Burst Delay: 1
Arcane Burst Radius: 3
Arcane Burst Damage: 25
```

---

### B. Player에서 사용 (Q 키 스킬)

1. **Player GameObject 선택**
2. **Inspector → Player Controller 스크립트**
3. **Explosion Skill Settings 섹션**
4. **Explosion Prefab** 슬롯에 ArcaneBurst1 드래그
5. **Has Explosion Skill** 체크 (또는 아이템으로 활성화)

**플레이어 설정 예시**:
```
Explosion Prefab: ArcaneBurst1
Explosion Cooldown: 3
Explosion Delay: 1
Explosion Radius: 3
Explosion Damage: 40
```

---

## 6. 고급 설정

### 다양한 이펙트 만들기

#### 작은 폭발 (Small Burst)
```
Delay Time: 0.5초
Explosion Radius: 2
Damage: 15
Warning Pulse Speed: 4
```

#### 큰 폭발 (Large Burst)
```
Delay Time: 1.5초
Explosion Radius: 5
Damage: 50
Warning Pulse Speed: 2
```

#### 빠른 폭발 (Quick Burst)
```
Delay Time: 0.3초
Explosion Radius: 2.5
Damage: 20
Warning Pulse Speed: 5
```

---

## 7. 애니메이션 추가 (선택사항)

### A. Animator로 회전 애니메이션

1. **ExplosionEffect에 Animator 추가**
2. **Animation Clip 생성**:
   - Rotation Z: 0 → 360도 (1초)
3. **Loop 설정**

### B. Scale 애니메이션

1. **WarningMarker에 Animation 추가**
2. **Animation Clip**:
   - Scale: (0.8, 0.8, 1) → (1.2, 1.2, 1) → (0.8, 0.8, 1)
   - Duration: 0.3초
   - Loop

---

## 8. 프로젝트 에셋 활용

프로젝트에 이미 이펙트 스프라이트가 있다면:

### 사용 가능한 스프라이트
```
Assets/Effect and FX Pixel Part 1 Free/Free/
- 폭발, 마법진, 빛 효과 등

Assets/Free 8bit Spells/
- 16x16.png, 64x64.png
- 다양한 마법 이펙트
```

### 추천 조합
1. **Warning Marker**: 빨간 원형 스프라이트
2. **Explosion Effect**: 폭발 또는 빛 이펙트

---

## 9. 문제 해결

### 문제: 이펙트가 보이지 않음
**해결**:
- Sorting Layer가 "Effects"로 설정되어 있는지 확인
- Order in Layer가 충분히 높은지 확인
- Sprite Renderer가 할당되어 있는지 확인

### 문제: 경고 마커가 깜빡이지 않음
**해결**:
- Warning Marker가 올바르게 할당되어 있는지 확인
- Warning Pulse Speed 값 확인 (0이 아닌지)

### 문제: 폭발 이펙트가 안 보임
**해결**:
- Explosion Effect가 올바르게 할당되어 있는지 확인
- Sprite Renderer의 Enabled 확인 (스크립트가 자동으로 제어)

### 문제: 데미지가 안 들어감
**해결**:
- Is Player Cast 설정 확인
  - 플레이어 사용: true
  - 보스 사용: false
- Target의 Tag 확인 ("Player" 또는 "Enemy")

---

## 10. 계층 구조 요약

```
ArcaneBurst1
├─ ArcaneBurstEffect (스크립트)
├─ WarningMarker (GameObject)
│  └─ Sprite Renderer (빨간 원)
└─ ExplosionEffect (GameObject)
   └─ Sprite Renderer (폭발 이펙트)
```

---

## 11. 빠른 체크리스트

- [ ] ArcaneBurst1 GameObject 생성
- [ ] WarningMarker 자식 오브젝트 생성 + Sprite Renderer
- [ ] ExplosionEffect 자식 오브젝트 생성 + Sprite Renderer
- [ ] ArcaneBurstEffect 스크립트 부착
- [ ] Inspector에서 Sprite Renderer들 할당
- [ ] Delay Time, Radius, Damage 설정
- [ ] Is Player Cast 설정 (보스 사용: false, 플레이어 사용: true)
- [ ] Prefab으로 저장
- [ ] Wizard Boss 또는 Player에 할당

---

## 12. 실전 사용 예시

### Wizard Boss 패턴
```
1. 보스가 플레이어 주변에 ArcaneBurst 소환
2. 빨간 원이 1초간 깜빡임 (경고)
3. 폭발! 범위 내 플레이어 데미지
4. 5초 쿨타임 후 다시 사용
```

### Player 스킬 (Q 키)
```
1. 플레이어가 Q 키 누름
2. 마우스 위치에 ArcaneBurst 생성
3. 1초 후 폭발하여 범위 내 적 모두 데미지
4. 3초 쿨타임
```

---

## 참고

- Scene View에서 GameObject 선택 시 폭발 반경이 빨간 원으로 표시됩니다
- 게임 실행 중 Gizmos를 활성화하면 범위를 볼 수 있습니다
- 여러 개의 ArcaneBurst를 동시에 생성 가능합니다
