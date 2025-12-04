# Dash Effect 설정 가이드

## 방법 1: 간단한 Trail Effect (추천)

### 1. Trail Renderer를 사용한 잔상 이펙트

1. **빈 GameObject 생성**
   - Hierarchy에서 우클릭 → Create Empty
   - 이름: `DashTrailEffect`

2. **Trail Renderer 추가**
   - DashTrailEffect GameObject 선택
   - Add Component → Effects → Trail Renderer

3. **Trail Renderer 설정**
   - **Time**: 0.3 (잔상 지속 시간)
   - **Width**: 0.5 ~ 0.1 (시작 → 끝 폭, Curve로 조정)
   - **Color**: 흰색 또는 청색 계열
     - Gradient에서 시작: 불투명, 끝: 투명
   - **Material**:
     - Default-Particle 또는
     - Sprites/Default 사용

4. **Auto Destruct Script 추가**
   ```csharp
   // Assets/GE_FinalProject/Scripts/Effects/AutoDestroy.cs
   using UnityEngine;

   public class AutoDestroy : MonoBehaviour
   {
       [SerializeField] private float lifetime = 1f;

       private void Start()
       {
           Destroy(gameObject, lifetime);
       }
   }
   ```

5. **DashTrailEffect에 AutoDestroy 추가**
   - Lifetime: 0.5초

6. **Prefab으로 저장**
   - DashTrailEffect를 `Assets/GE_FinalProject/Prefabs/` 폴더로 드래그

---

## 방법 2: Sprite 기반 간단한 이펙트

### 1. 대쉬 이펙트 GameObject 생성

1. **빈 GameObject 생성**
   - 이름: `DashEffect`

2. **Sprite Renderer 추가**
   - Add Component → Rendering → Sprite Renderer
   - Sprite: 원형 또는 별 모양 스프라이트 선택
   - Color: 하얀색 또는 청색 (Alpha: 0.5)
   - Sorting Layer: Effects
   - Order in Layer: 10

3. **Scale 애니메이션 추가 (선택사항)**
   ```csharp
   // Assets/GE_FinalProject/Scripts/Effects/DashEffectAnimation.cs
   using UnityEngine;

   public class DashEffectAnimation : MonoBehaviour
   {
       [SerializeField] private float duration = 0.2f;
       [SerializeField] private float startScale = 0.5f;
       [SerializeField] private float endScale = 2f;

       private float timer = 0f;
       private SpriteRenderer spriteRenderer;

       private void Start()
       {
           spriteRenderer = GetComponent<SpriteRenderer>();
           Destroy(gameObject, duration);
       }

       private void Update()
       {
           timer += Time.deltaTime;
           float progress = timer / duration;

           // Scale up
           float scale = Mathf.Lerp(startScale, endScale, progress);
           transform.localScale = Vector3.one * scale;

           // Fade out
           if (spriteRenderer != null)
           {
               Color color = spriteRenderer.color;
               color.a = 1f - progress;
               spriteRenderer.color = color;
           }
       }
   }
   ```

4. **DashEffect에 Script 추가**
   - DashEffectAnimation 컴포넌트 추가

5. **Prefab으로 저장**

---

## 방법 3: Particle System (고급)

### 1. Particle System GameObject 생성

1. **Particle System 생성**
   - Hierarchy에서 우클릭 → Effects → Particle System
   - 이름: `DashParticleEffect`

2. **Main Module 설정**
   - Duration: 0.3
   - Start Lifetime: 0.2 ~ 0.3
   - Start Speed: 0
   - Start Size: 0.2 ~ 0.5
   - Start Color: 흰색 또는 청색 (Gradient로 Fade Out)
   - Max Particles: 20

3. **Emission Module**
   - Rate over Time: 0
   - Bursts:
     - Time: 0
     - Count: 10~15
     - Cycles: 1

4. **Shape Module**
   - Shape: Circle
   - Radius: 0.5
   - Emit from: Edge

5. **Color over Lifetime**
   - 활성화
   - Gradient: 시작 불투명 → 끝 투명

6. **Size over Lifetime**
   - 활성화
   - Curve: 작게 시작 → 크게 끝

7. **Renderer**
   - Material: Default-Particle
   - Sorting Layer: Effects
   - Order in Layer: 10

8. **Stop Action**
   - Destroy

9. **Prefab으로 저장**

---

## PlayerController에 이펙트 연결

### Unity Editor에서:

1. **Player GameObject 선택**

2. **Inspector에서 PlayerController 컴포넌트 찾기**

3. **Dash Skill Settings 섹션**
   - `Dash Effect Prefab` 슬롯에 위에서 만든 Prefab 드래그 앤 드롭

4. **테스트**
   - 플레이 모드로 들어가기
   - WereWolf를 죽여서 Dash Skill 아이템 획득
   - Shift + 이동 키로 대쉬 테스트
   - 이펙트가 플레이어 위치에 생성되는지 확인

---

## 팁: 기존 에셋 활용

프로젝트에 이미 이펙트 에셋이 있다면:

1. **Effect and FX Pixel Part 1 Free** 폴더 확인
2. 적절한 이펙트 스프라이트 찾기
3. 그 스프라이트로 Animator를 만들어 애니메이션 재생
4. AutoDestroy 스크립트로 자동 삭제

예시:
```
Assets/Effect and FX Pixel Part 1 Free/Free/
- 폭발, 섬광, 연기 등 다양한 이펙트 스프라이트 있음
- 이 중 하나를 선택해서 Dash Effect로 사용 가능
```

---

## 문제 해결

### 이펙트가 보이지 않음
- Sorting Layer 확인 (Effects > Player보다 높게)
- Z Position 확인 (0으로 설정)
- Camera Culling Mask 확인

### 이펙트가 사라지지 않음
- AutoDestroy 스크립트 확인
- Particle System의 Stop Action = Destroy 확인
- Trail Renderer의 Time 값 확인

### 이펙트가 너무 약함
- Particle Count 증가
- Color Alpha 값 증가
- Scale 크기 증가
- Emission Rate 증가
