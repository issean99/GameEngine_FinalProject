# Enemy Sound Effects Setup Guide

이 가이드는 모든 적 캐릭터에 소리 효과를 추가하는 방법을 설명합니다.

## 개요

모든 적 캐릭터(Skeleton, WereWolf, Slime, Archer, WizardBoss, FinalBoss)는 이제 `EnemySoundEffects` 컴포넌트를 사용하여 피격/죽음/공격 소리를 재생합니다.

## Unity에서 설정하는 방법

### 1단계: 적 프리팹에 EnemySoundEffects 컴포넌트 추가

각 적 프리팹 (Skeleton, WereWolf, Slime, SkeletonArcher, WizardBoss, FinalBoss)에 다음을 수행하세요:

1. **프리팹 열기**
   - Project 창에서 적 프리팹을 찾습니다
   - 프리팹을 더블클릭하여 Prefab 모드로 엽니다

2. **EnemySoundEffects 컴포넌트 추가**
   - Inspector 창에서 "Add Component" 클릭
   - "EnemySoundEffects" 검색 후 추가
   - (또는) 스크립트 파일을 적 오브젝트로 드래그

3. **AudioSource 자동 생성 확인**
   - EnemySoundEffects는 자동으로 AudioSource를 생성합니다
   - 수동으로 추가할 필요 없습니다

### 2단계: 사운드 클립 할당

Inspector 창에서 EnemySoundEffects 컴포넌트의 필드에 사운드 파일을 할당하세요:

#### **Hit Sounds (피격 소리)** - 배열
- Size를 설정 (예: 3개의 다양한 피격 소리)
- 각 Element에 피격 사운드 클립을 드래그
- 랜덤으로 재생됩니다

#### **Death Sound (죽음 소리)** - 단일 클립
- 적이 죽을 때 재생될 사운드 클립

#### **Attack Sound (공격 소리)** - 단일 클립
- 적이 공격할 때 재생될 사운드 클립

#### **Volume (볼륨)**
- 0.0 ~ 1.0 사이의 값 (기본값: 0.7)

### 3단계: 각 적 타입별 권장 사운드

#### Skeleton (스켈레톤)
- Hit Sounds: 뼈 부딪히는 소리, 금속 부딪히는 소리
- Death Sound: 뼈가 부서지는 소리
- Attack Sound: 검 휘두르는 소리

#### WereWolf (늑대인간)
- Hit Sounds: 짐승 신음 소리, 울부짖는 소리
- Death Sound: 마지막 울부짖음
- Attack Sound: 으르렁거리는 소리

#### Slime (슬라임)
- Hit Sounds: 젤리 같은 소리, 물컹거리는 소리
- Death Sound: 터지는 소리
- Attack Sound: 점프/공격 소리

#### SkeletonArcher (스켈레톤 궁수)
- Hit Sounds: 뼈 부딪히는 소리
- Death Sound: 뼈가 부서지는 소리
- Attack Sound: 활시위 당기는 소리

#### WizardBoss (마법사 보스)
- Hit Sounds: 마법 충격 소리
- Death Sound: 강력한 마법 폭발 소리
- Attack Sound: 마법 시전 소리 (또는 투사체 자체에서 재생)

#### FinalBoss (최종 보스)
- Hit Sounds: 강렬한 충격음
- Death Sound: 장대한 죽음의 소리
- Attack Sound: 강력한 공격음

## 투사체 및 이펙트 사운드

### Fireball (FireballProjectile.cs)
이미 구현됨:
- **Launch Sound**: 발사될 때
- **Hit Sound**: 맞았을 때

Unity 설정:
1. Fireball 프리팹 열기
2. Inspector에서 Sound Effects 섹션 찾기
3. Launch Sound와 Hit Sound 클립 할당

### Arcane Burst (ArcaneBurstEffect.cs)
이미 구현됨:
- **Explosion Sound**: 폭발할 때

Unity 설정:
1. ArcaneBurst 프리팹 열기
2. Inspector에서 Sound Effects 섹션 찾기
3. Explosion Sound 클립 할당

### Magic Missile (MagicProjectile.cs)
WizardBoss가 사용하는 마법 미사일:
- **Launch Sound**: 발사될 때
- **Hit Sound**: 충돌할 때

Unity 설정:
1. MagicProjectile 프리팹 열기
2. Inspector에서 Sound Effects 섹션 찾기
3. Launch Sound와 Hit Sound 클립 할당

### Spear (SpearProjectile.cs)
FinalBoss가 사용하는 창 투사체:
- **Launch Sound**: 발사될 때
- **Hit Sound**: 벽에 충돌할 때

Unity 설정:
1. SpearProjectile 프리팹 열기
2. Inspector에서 Sound Effects 섹션 찾기
3. Launch Sound와 Hit Sound 클립 할당

### Arrow (SkeletonArcherArrow.cs)
SkeletonArcher가 사용하는 화살:
- **Launch Sound**: 발사될 때
- **Hit Sound**: 충돌할 때

Unity 설정:
1. SkeletonArcherArrow 프리팹 열기
2. Inspector에서 Sound Effects 섹션 찾기
3. Launch Sound와 Hit Sound 클립 할당

## 플레이어 사운드 (이미 완료)

PlayerController에는 이미 다음 사운드가 구현되어 있습니다:
- Attack Sound (공격)
- Defense Sound (방어)
- Dash Sound (대쉬)
- Hit Sound (피격)

Inspector에서 PlayerController 컴포넌트의 Sound Effects 섹션에서 클립을 할당하세요.

## 테스트 방법

1. **게임 실행**
   - Play 모드로 들어갑니다

2. **적 공격**
   - 적을 공격하여 피격 소리 확인
   - 적을 죽여서 죽음 소리 확인

3. **적의 공격 받기**
   - 적이 공격할 때 공격 소리 확인

4. **볼륨 조정**
   - 소리가 너무 크거나 작으면 Volume 값 조정
   - 또는 Unity Audio Mixer 사용

## 문제 해결

### 소리가 재생되지 않는 경우:

1. **컴포넌트 확인**
   - EnemySoundEffects 컴포넌트가 적에게 추가되어 있는지 확인
   - AudioSource가 있는지 확인 (자동 생성됨)

2. **클립 할당 확인**
   - Inspector에서 사운드 클립이 올바르게 할당되어 있는지 확인

3. **볼륨 확인**
   - Volume이 0이 아닌지 확인
   - Unity의 Master Volume이 음소거되지 않았는지 확인

4. **Console 로그 확인**
   - 에러 메시지가 있는지 확인

### 소리가 너무 많이 겹치는 경우:
- Hit Sounds 배열에 여러 사운드를 추가하여 다양성 확보
- Volume 값을 낮춰서 조정

## 구현된 기능

✅ **모든 적 캐릭터**:
- Skeleton
- WereWolf
- Slime
- SkeletonArcher
- WizardBoss
- FinalBoss

✅ **모든 사운드 타입**:
- 피격 소리 (Hit)
- 죽음 소리 (Death)
- 공격 소리 (Attack)

✅ **투사체/이펙트**:
- Fireball (발사 + 충돌)
- ArcaneBurst (폭발)
- MagicMissile (발사 + 충돌)
- Spear (발사 + 충돌)
- Arrow (발사 + 충돌)

✅ **플레이어**:
- 공격, 방어, 대쉬, 피격 소리

## 참고 사항

- 모든 사운드는 2D 사운드로 설정됨 (spatialBlend = 0)
- Hit Sounds는 배열이므로 여러 소리 중 랜덤 재생
- 사운드는 PlayOneShot으로 재생되어 중복 재생 가능
- 적이 죽을 때는 GameObject가 1초 후에 파괴되므로 사운드 재생에 충분한 시간 보장
