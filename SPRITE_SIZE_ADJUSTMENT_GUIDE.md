# Unity 에디터에서 스프라이트 크기 조정 가이드

ArcaneBurst의 폭발 이펙트와 경고 마커의 크기를 Unity 에디터에서 직접 조정하는 방법입니다.

---

## 방법 1: Prefab에서 직접 조정 (가장 간단)

### A. Prefab 열기

1. **Project View에서 ArcaneBurst1 Prefab 찾기**
   - 경로: `Assets/GE_FinalProject/Prefabs/ArcaneBurst1`

2. **Prefab 편집 모드로 진입**
   - Prefab을 더블클릭
   - 또는 우클릭 → Open Prefab

---

### B. 경고 마커(Warning Marker) 크기 조정

1. **Hierarchy에서 WarningMarker 선택**
   - ArcaneBurst1 > WarningMarker

2. **Inspector에서 Transform 확인**
   - Scale: (1, 1, 1)

3. **크기 조정**
   - **더 크게**: Scale을 (1.5, 1.5, 1) 또는 (2, 2, 1)
   - **더 작게**: Scale을 (0.8, 0.8, 1) 또는 (0.5, 0.5, 1)

4. **Scene View에서 실시간 확인**
   - Rect Tool (T 키) 사용하여 시각적으로 조정 가능

**참고**: 스크립트가 이 크기에 `explosionRadius`를 곱하므로, 여기서는 기본 비율만 설정합니다.

---

### C. 폭발 이펙트(Explosion Effect) 크기 조정

1. **Hierarchy에서 ExplosionEffect 선택**
   - ArcaneBurst1 > ExplosionEffect

2. **Inspector에서 Transform 확인**
   - Scale: (1, 1, 1)

3. **크기 조정**
   - **경고 마커와 같은 크기로**: WarningMarker와 동일한 Scale 값 사용
   - **더 작게**: (0.8, 0.8, 1) 또는 (0.9, 0.9, 1)
   - **더 크게**: (1.2, 1.2, 1) 또는 (1.5, 1.5, 1)

4. **Scene View에서 비교**
   - WarningMarker와 ExplosionEffect를 번갈아 선택하며 크기 비교

---

## 방법 2: 스프라이트 Import 설정 변경

### A. 스프라이트 파일 찾기

1. **Project View에서 스프라이트 이미지 찾기**
   - 예: `Assets/Effect and FX Pixel Part 1 Free/Free/03_0.png`

2. **스프라이트 클릭하여 선택**

---

### B. Import Settings 조정

1. **Inspector에서 설정 확인**

2. **Pixels Per Unit 변경**
   - **현재값 확인**: 기본값 100
   - **더 크게 보이게**: 값을 낮춤 (예: 50)
     - 50 = 원래 크기의 2배
   - **더 작게 보이게**: 값을 높임 (예: 200)
     - 200 = 원래 크기의 0.5배

3. **Apply 버튼 클릭**

4. **확인**
   - Prefab을 다시 열어서 변경 사항 확인

**참고**: 이 방법은 해당 스프라이트를 사용하는 모든 곳에 영향을 줍니다.

---

## 방법 3: Scene에서 테스트하며 조정

### A. 씬에 배치하여 테스트

1. **테스트 씬 열기**
   - Tutorial 또는 Stage1 씬

2. **ArcaneBurst1 Prefab을 Hierarchy로 드래그**
   - 씬에 임시로 배치

3. **WarningMarker와 ExplosionEffect 크기 조정**
   - Inspector에서 Transform > Scale 조정
   - Scene View에서 실시간으로 확인

4. **만족하는 크기 찾으면**
   - Scale 값 기록 (예: 1.2, 1.2, 1)
   - Prefab에 적용 (아래 참조)

---

### B. 변경사항을 Prefab에 적용

1. **Hierarchy에서 ArcaneBurst1 선택**

2. **Inspector 상단의 Overrides 드롭다운 클릭**

3. **Apply All 클릭**
   - 또는 개별 변경사항만 선택하여 Apply

4. **씬의 임시 오브젝트 삭제**

---

## 권장 크기 설정

### 경고 마커와 폭발 이펙트가 같은 크기로 보이게 하려면:

#### 옵션 1: Transform Scale 조정
```
WarningMarker Transform Scale: (1, 1, 1)
ExplosionEffect Transform Scale: (1, 1, 1)

스크립트 코드:
- WarningMarker: explosionRadius * scale (펄스 0.8~1.2)
- ExplosionEffect: explosionRadius * 0.85
```

#### 옵션 2: 폭발 이펙트 Scale 줄이기
```
WarningMarker Transform Scale: (1, 1, 1)
ExplosionEffect Transform Scale: (0.85, 0.85, 1)

스크립트 코드:
- ExplosionEffect: explosionRadius * 1.0 (코드에서 0.85 제거)
```

#### 옵션 3: 둘 다 동일하게 설정
```
WarningMarker Transform Scale: (1.2, 1.2, 1)
ExplosionEffect Transform Scale: (1.2, 1.2, 1)

스크립트에서 추가 배율 제거하고 동일하게 처리
```

---

## 실시간 비교 방법

### Scene View에서 두 이펙트 동시에 보기

1. **Prefab 편집 모드**

2. **WarningMarker 활성화**
   - WarningMarker GameObject 체크박스 ✅

3. **ExplosionEffect도 활성화**
   - ExplosionEffect GameObject 체크박스 ✅
   - Inspector > Sprite Renderer > Enabled ✅

4. **Scene View에서 겹쳐서 확인**
   - 두 스프라이트가 겹쳐져 보이므로 크기 비교 가능
   - 색상을 다르게 하면 더 쉽게 비교 가능

5. **크기 조정 후 다시 비활성화**
   - 게임에서는 스크립트가 자동으로 켜고 끔

---

## 세밀한 조정 팁

### 시각적으로 정확히 맞추려면:

1. **Sprite Renderer의 Draw Mode 확인**
   - Simple (기본): Scale에 따라 늘어남
   - Sliced/Tiled: 경계만 보존하며 크기 조정

2. **정확한 픽셀 크기 계산**
   ```
   실제 크기 = 스프라이트 픽셀 크기 / Pixels Per Unit * Transform Scale

   예:
   - 스프라이트: 64x64 픽셀
   - Pixels Per Unit: 100
   - Transform Scale: (1, 1, 1)
   - 실제 Unity 크기: 0.64 x 0.64 units
   ```

3. **Scene View에서 Grid 활성화**
   - 상단 메뉴: Show Grid
   - 크기를 Grid에 맞춰 조정

---

## 문제 해결

### 문제: 크기를 조정해도 게임에서 안 바뀜
**해결**:
- Prefab Override를 Apply 했는지 확인
- 씬에 이미 배치된 오브젝트는 Prefab에서 새로 드래그
- Play Mode를 껐다 켜기

### 문제: 두 이펙트 크기가 계속 안 맞음
**해결**:
- 스프라이트 자체의 크기가 다를 수 있음
- Sprite Renderer의 Pixels Per Unit 확인
- 또는 스크립트의 배율 조정 (0.85f → 0.9f 등)

### 문제: Prefab 변경이 저장 안 됨
**해결**:
- Prefab 편집 모드에서 상단의 "Auto Save" 확인
- 수동 저장: Ctrl+S (또는 Cmd+S)
- Prefab 편집 모드 종료 시 저장 확인

---

## 추천 작업 순서

1. **Prefab 편집 모드에서 시작**
2. **WarningMarker와 ExplosionEffect를 둘 다 활성화**
3. **Scene View에서 겹쳐서 확인**
4. **Transform Scale 조정**
   - 먼저 WarningMarker 크기 확정
   - 그 다음 ExplosionEffect를 맞춤
5. **변경사항 저장 (Ctrl+S)**
6. **Play Mode에서 테스트**
7. **필요시 미세 조정 반복**

---

## 최종 체크리스트

- [ ] ArcaneBurst1 Prefab 열기
- [ ] WarningMarker Transform Scale 확인/조정
- [ ] ExplosionEffect Transform Scale 확인/조정
- [ ] Scene View에서 두 이펙트 겹쳐서 크기 비교
- [ ] 만족스러운 크기로 조정
- [ ] Prefab 저장 (Ctrl+S)
- [ ] Play Mode에서 실제 게임 테스트
- [ ] 필요시 스크립트의 배율(0.85f) 조정

---

## 예시 설정

### 설정 A: 완전히 같은 크기
```
WarningMarker:
- Transform Scale: (1, 1, 1)

ExplosionEffect:
- Transform Scale: (1, 1, 1)

스크립트 수정:
explosionEffect.transform.localScale = Vector3.one * explosionRadius * 1.0f;
```

### 설정 B: 폭발이 약간 작게
```
WarningMarker:
- Transform Scale: (1, 1, 1)

ExplosionEffect:
- Transform Scale: (0.85, 0.85, 1)

스크립트:
explosionEffect.transform.localScale = Vector3.one * explosionRadius * 1.0f;
```

### 설정 C: 둘 다 크게
```
WarningMarker:
- Transform Scale: (1.5, 1.5, 1)

ExplosionEffect:
- Transform Scale: (1.5, 1.5, 1)

스크립트:
explosionEffect.transform.localScale = Vector3.one * explosionRadius * 1.0f;
```
