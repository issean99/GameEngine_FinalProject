# 튜토리얼 씬 이펙트 검정색 문제 해결 가이드

튜토리얼 씬에서만 이펙트(폭발, 화염구, 대쉬 등)가 검정색으로 보이는 문제를 해결하는 방법입니다.

---

## 원인

이펙트가 검정색으로 보이는 주요 원인:
1. **2D Light 설정 문제** - 튜토리얼 씬에만 다른 라이팅 설정
2. **카메라 Culling Mask 문제** - 특정 레이어가 보이지 않음
3. **Material Shader 문제** - 튜토리얼 씬의 렌더링 설정이 다름
4. **Global Illumination 설정** - 씬별 라이팅 베이크 설정 차이

---

## 해결 방법 1: 2D Light 설정 확인 (가장 흔한 원인)

### A. Global Light 2D 확인

1. **Tutorial 씬 열기**
   - Hierarchy에서 Scene 더블클릭

2. **Directional Light 2D 또는 Global Light 2D 찾기**
   - Hierarchy 검색: "Light" 또는 "Directional"

3. **Light 2D 컴포넌트 설정 확인**
   - **Intensity**: 1 (너무 낮으면 어두움)
   - **Color**: 흰색 (R:255, G:255, B:255)
   - **Target Sorting Layers**: Effects 레이어 포함되어 있는지 확인

4. **Light 2D가 없다면 생성**
   - Hierarchy 우클릭 → Light → Global Light 2D
   - Intensity: 1
   - Color: 흰색

---

### B. Sprite Renderer Material 확인

1. **이펙트 Prefab 열기** (예: ArcaneBurst1, Fireball)

2. **Sprite Renderer 컴포넌트 확인**
   - Material: **Sprites-Default** 또는 **Sprites-Lit** 사용
   - Material이 Custom이면 Shader 확인 필요

3. **Material 변경 (필요시)**
   - Material 드롭다운 → Sprites-Default
   - 또는 Sprites/Default shader 사용

---

## 해결 방법 2: Render Pipeline 설정

### A. 프로젝트가 URP를 사용하는 경우

1. **Edit → Project Settings → Graphics**

2. **Scriptable Render Pipeline Settings 확인**
   - URP Asset이 올바르게 할당되어 있는지 확인

3. **Window → Rendering → Lighting**
   - Tutorial 씬에서 열기
   - **Environment** 탭
     - Skybox Material: None (2D 게임)
     - Environment Lighting Source: Color
     - Ambient Color: 흰색 또는 밝은 회색

---

### B. Built-in Render Pipeline 사용 시

1. **Window → Rendering → Lighting**
   - Tutorial 씬에서 열기

2. **Environment 탭**
   - Skybox Material: None
   - Environment Lighting:
     - Source: Color
     - Ambient Color: (200, 200, 200) 또는 흰색

3. **Auto Generate 체크 해제**
   - 2D 게임은 베이킹 불필요

---

## 해결 방법 3: 카메라 설정 확인

### Tutorial 씬의 Main Camera 설정

1. **Main Camera 선택**

2. **Camera 컴포넌트 확인**
   - **Clear Flags**: Solid Color (2D 게임)
   - **Background**: 원하는 배경색
   - **Culling Mask**: Everything 체크 (또는 Effects 레이어 포함)
   - **Projection**: Orthographic
   - **Depth**: 0

3. **Universal Additional Camera Data (URP 사용 시)**
   - Render Type: Base
   - Post Processing: 체크 (필요시)

---

## 해결 방법 4: 레이어 및 Sorting Layer 확인

### A. Effects Sorting Layer 존재 확인

1. **Edit → Project Settings → Tags and Layers**

2. **Sorting Layers 확인**
   - "Effects" 레이어가 있는지 확인
   - 없으면 추가: + 버튼 클릭

3. **이펙트 Prefab의 Sorting Layer**
   - 모든 이펙트가 "Effects" 레이어 사용하는지 확인
   - Order in Layer: 10 이상 (플레이어/적보다 위)

---

### B. Camera Culling Mask

1. **Main Camera 선택**

2. **Culling Mask 확인**
   - Effects 레이어 체크되어 있는지 확인
   - 체크 안 되어 있으면 활성화

---

## 해결 방법 5: 다른 씬과 설정 비교

### 작동하는 씬(Stage1)의 설정을 Tutorial로 복사

1. **Stage1 씬 열기**

2. **Main Camera 선택**
   - Inspector에서 Camera 컴포넌트 우클릭
   - Copy Component

3. **Tutorial 씬 열기**

4. **Main Camera 선택**
   - Inspector에서 Camera 컴포넌트 우클릭
   - Paste Component Values

5. **Lighting 설정도 복사**
   - Stage1 씬: Window → Rendering → Lighting
   - 설정 값 기록
   - Tutorial 씬에서 동일하게 설정

---

## 해결 방법 6: Sprite Material Shader 변경

### 이펙트 Sprite의 Shader 확인

1. **이펙트 Prefab 열기** (ArcaneBurst1, Fireball 등)

2. **Sprite Renderer → Material → Shader 확인**

3. **Shader 변경 시도**
   - **Sprites/Default**: 라이팅 영향 없음 (추천)
   - **Sprites/Diffuse**: 라이팅 영향 받음
   - **Universal Render Pipeline/2D/Sprite-Lit-Default**: URP 사용 시

4. **Shader 변경 방법**
   - Material 생성: Assets 우클릭 → Create → Material
   - 이름: SpriteDefault
   - Shader: Sprites/Default
   - 모든 이펙트 Sprite Renderer에 이 Material 할당

---

## 해결 방법 7: Scene 특정 문제 해결

### Tutorial 씬만의 문제라면

1. **씬 비교**
   - Tutorial 씬과 Stage1 씬을 동시에 열기
   - Hierarchy에서 Light, Camera 설정 비교

2. **Lighting 창 비교**
   - Window → Rendering → Lighting
   - 두 씬의 설정 비교

3. **차이점 발견 시**
   - Tutorial 씬 설정을 Stage1과 동일하게 변경

---

## 빠른 해결 체크리스트

**단계별로 확인하세요:**

- [ ] Tutorial 씬에 Global Light 2D 또는 Directional Light 2D 존재 확인
- [ ] Light Intensity가 1이고 Color가 흰색인지 확인
- [ ] Light Target Sorting Layers에 "Effects" 포함 확인
- [ ] Main Camera의 Culling Mask에 "Effects" 레이어 체크 확인
- [ ] 이펙트 Prefab의 Sprite Renderer Material이 Sprites-Default인지 확인
- [ ] Window → Rendering → Lighting에서 Ambient Color가 밝은지 확인
- [ ] 이펙트 Sorting Layer가 "Effects"이고 Order가 10 이상인지 확인
- [ ] Stage1 씬과 Camera/Light 설정 비교

---

## 가장 빠른 해결 방법 (추천)

### 1. Global Light 2D 추가 (99% 해결)

```
Tutorial 씬 열기
↓
Hierarchy 우클릭 → Light → Global Light 2D
↓
Light 2D 컴포넌트 설정:
- Intensity: 1
- Color: 흰색 (255, 255, 255)
- Target Sorting Layers: Everything 또는 Effects 포함
↓
저장 (Ctrl+S)
↓
Play Mode로 테스트
```

### 2. Sprite Material Shader 변경

```
모든 이펙트 Prefab 열기
↓
Sprite Renderer → Material → Shader
↓
Sprites/Default 선택
↓
Apply to Prefab
↓
테스트
```

---

## 문제가 계속되면

### 최종 해결책: Stage1 씬 복제

1. **Project View에서 Stage1.unity 찾기**

2. **Ctrl+D로 복제**
   - 이름: Tutorial_Fixed

3. **필요 없는 오브젝트 삭제**
   - 적, 아이템 등

4. **Tutorial 맵 타일 배치**

5. **씬 이름 변경**
   - Tutorial_Fixed → Tutorial (기존 Tutorial 백업 후)

---

## 예방 방법

### 새 씬 생성 시 항상 확인

1. **Global Light 2D 추가**
2. **Lighting 설정 → Ambient Color: 흰색**
3. **Camera Culling Mask: Everything**
4. **이펙트 Material: Sprites-Default**

### Template Scene 만들기

1. **정상 작동하는 씬(Stage1) 복제**
2. **이름: _SceneTemplate**
3. **모든 GameObject 삭제 (Camera, Light만 남김)**
4. **새 씬 만들 때 이것을 복제하여 사용**

---

## 참고 이미지 (확인할 설정)

### Light 2D 올바른 설정
```
Light Type: Global
Intensity: 1.0
Blend Style: Default (0)
Color: (255, 255, 255, 255)
Target Sorting Layers: All 또는 Effects 포함
```

### Sprite Renderer 올바른 설정
```
Sprite: (이펙트 스프라이트)
Color: 흰색 (255, 255, 255, 255)
Material: Sprites-Default
Sorting Layer: Effects
Order in Layer: 10
```

### Camera 올바른 설정
```
Clear Flags: Solid Color
Background: (원하는 색)
Culling Mask: Everything (또는 Effects 포함)
Projection: Orthographic
Size: 5 (또는 원하는 값)
```
