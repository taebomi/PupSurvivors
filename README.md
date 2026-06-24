# 댕댕이 서바이벌 (Pup Survivors)
> 써드파티 에셋 라이선스로 인해 전체 프로젝트가 아닌 C# 스크립트만 포함

> 로그라이트 / 뱀파이어 서바이벌 류 | Unity, C# | 1인 개발 | PC · 모바일 | 미출시

<img src="https://github.com/user-attachments/assets/54895e1f-69ce-4bba-946b-63bf4422ca5e" width="440" />

### 플레이 영상
[![플레이 영상](https://img.youtube.com/vi/uemJe7yG4UM/0.jpg)](https://youtu.be/uemJe7yG4UM)

---

## 핵심 — 대규모 몬스터 길찾기 최적화

지형(장애물)이 존재하는 뱀서류에서 **수백 마리 몬스터의 길찾기**를 어떻게
저비용으로 푸는가. "각자 A\*로 연산"하는 방식을 버리고, 두 단계로 최적화했습니다.

### 1. Flow Field 기반 시스템 구축

| 파일 | 역할 |
|------|------|
| [PathFinder.cs](v2/Assets/03_Stage/System/PathFind/PathFinder.cs) | Flow Field 관리·갱신 진입점 |
| [CostField.cs](v2/Assets/03_Stage/System/PathFind/CostField.cs) | 장애물 정보 저장 (타일 비용) |
| [FlowField.cs](v2/Assets/03_Stage/System/PathFind/FlowField.cs) | 방향 데이터 보유 및 Job 스케줄링 |
| [PathFinderDebugger.cs](v2/Assets/03_Stage/System/PathFind/PathFinderDebugger.cs) | 방향·BFS 전파 시각화 디버거 |

**착안** — 모든 몬스터의 목적지는 플레이어 하나뿐. 타일마다 플레이어 방향을
미리 담아두면, 몬스터 수와 무관하게 **타일 개수만큼만** 연산하면 됩니다.

**한계** — 처음엔 플레이어 기준으로 인접 타일 방향을 합성해 나가는 반복문으로
구현했으나, 탐색 순서에 의존하는 탓에 **장애물 뒤편에 사각지대(Blind Spot)** 가
생겼습니다.

<img src="https://github.com/user-attachments/assets/c7b84ffe-33dc-4827-af3a-20d5d8b0055f" width="560" />

*△ 화살표: 타일에서 플레이어로 향하는 방향 · 사각형: 영벡터 · 원: 방향 계산 불가 — 자체 구현 단계에서 발생한 사각지대*

**해결** — 커뮤니티 피드백으로 이 방식이 이미 정립된 **Flow Field** 임을 알게 됐고,
[GDC 2022 'Age of Empires IV' 길찾기 발표](https://www.gdcvault.com/play/1027659/Pathing-in-Age-of-Empires)를
연구했습니다. 핵심은 '전파 순서'. 플레이어로부터 **BFS로 거리순 전파**하면 계산
시점에 항상 계산된 인접 타일이 존재해 사각지대가 원천 차단됩니다. 이동 비용이
균일한 게임 특성상 Integration Field(비용 가중) 단계는 생략하고, 거리·방향을
함께 구하는 형태로 단순화했습니다.

<img src="https://github.com/user-attachments/assets/0804765f-dab2-448f-bf24-ceef97d121b4" width="560" />

*△ 플레이어 중심 일정 영역의 BFS 전파 (경로 탐색 시각화)*

**성과** — 사각지대 제거 + 중복 탐색 제거 + 전파 로직 간결화. 에디터 프로파일러
기준 1회 연산(1만 타일) **1.2ms → 0.8ms (약 33% 단축)**, 부하가 타일 영역에
고정돼 적의 수와 무관하게 일정합니다.

### 2. Job System · Burst 기반 병렬화

| 파일 | 역할 |
|------|------|
| [FlowField.cs](v2/Assets/03_Stage/System/PathFind/FlowField.cs) | 의존성에 맞춘 Job 스케줄링 |
| [ResetFlowFieldJob.cs](v2/Assets/03_Stage/System/PathFind/ResetFlowFieldJob.cs) | 셀별 초기화 (`IJobParallelFor`) |
| [UpdateFlowFieldJob.cs](v2/Assets/03_Stage/System/PathFind/UpdateFlowFieldJob.cs) | BFS 경로 탐색 (`IJob`) |

**동기** — 4인 코옵 + 지상/공중 타입이 더해지자 매 프레임 갱신할 Flow Field가
8개로 늘어 메인 스레드가 병목이 됐습니다.

<img src="https://github.com/user-attachments/assets/ad61998e-e75c-4a65-ad65-199fd78f61bf" width="440" />

*△ 4인 플레이 + 지상/공중 타입 길찾기 (분홍: 방향 · 노랑: 장애물 · 하늘: 경로 없음)*

**한계** — 길찾기는 현재 타일이 이전 타일 결과에 의존하므로, 단순 분할
병렬화(`IJobParallelFor`)가 불가능했습니다.

**해결** — 연산을 **의존성 유무로 분리해 스케줄링**했습니다.

```csharp
// FlowField.cs — 초기화(병렬) → BFS(순차)를 Job 의존성 체인으로 연결
// Reset: 셀 간 독립 → IJobParallelFor 로 스레드 풀에 분산
var resetGroundHandle = resetGroundJob.Schedule(RowSize * ColSize, 32);
var resetAirHandle    = resetAirJob.Schedule(RowSize * ColSize, 32);

// BFS: 이전 셀에 의존 → IJob(순차). 단, 8개 FlowField를 서로 다른 스레드에 동시 스케줄
var updateGroundHandle = updateGroundJob.Schedule(resetGroundHandle);
var updateAirHandle    = updateAirJob.Schedule(resetAirHandle);

return JobHandle.CombineDependencies(updateGroundHandle, updateAirHandle);
```

- **공통 데이터** — Cost Field는 Physics2D 참조 + 전 플레이어 공유라 메인 스레드에서 1회만 계산 (Job 제외)
- **독립 데이터** — 셀 초기화는 `IJobParallelFor`로 100% 병렬
- **종속 데이터** — BFS 탐색은 `IJob`(순차)이되, FlowField별로 다른 스레드에 동시 스케줄
- **Burst** — 수학 연산이 잦은 Job에 `[BurstCompile]` 적용

**성과** — 에디터 프로파일러 기준 1회 연산(약 7천 타일 × 8개 Flow Field)
**4ms → 0.5ms (약 1/8)**. 무작정 병렬화가 아니라 *데이터 흐름·의존성을 통제*하는
것이 멀티스레딩 최적화의 핵심임을 확인했습니다.

| 기존 (싱글스레딩) | 개선 (Job System + Burst) |
|:--:|:--:|
| <img src="https://github.com/user-attachments/assets/865eed2a-e3dc-47e2-b308-9af146f696e1" width="380" /> | <img src="https://github.com/user-attachments/assets/9e0a4b36-8529-40c5-87e1-c9a943ef0feb" width="380" /> |

---

## 핵심 코드 구조

### v2 — Job System 적용 (최적화 버전)
<pre>
v2/Assets/03_Stage/System/PathFind/
├── <a href="v2/Assets/03_Stage/System/PathFind/PathFinder.cs">PathFinder.cs</a>            # 관리·갱신 진입점
├── <a href="v2/Assets/03_Stage/System/PathFind/CostField.cs">CostField.cs</a>             # 장애물 정보 (타일 비용)
├── <a href="v2/Assets/03_Stage/System/PathFind/FlowField.cs">FlowField.cs</a>             # 방향 데이터 + Job 스케줄링
├── <a href="v2/Assets/03_Stage/System/PathFind/ResetFlowFieldJob.cs">ResetFlowFieldJob.cs</a>     # 초기화 (IJobParallelFor)
├── <a href="v2/Assets/03_Stage/System/PathFind/UpdateFlowFieldJob.cs">UpdateFlowFieldJob.cs</a>    # BFS 탐색 (IJob)
└── <a href="v2/Assets/03_Stage/System/PathFind/PathFinderDebugger.cs">PathFinderDebugger.cs</a>    # 방향·BFS 전파 시각화
</pre>

### v1 — 전체 게임 구조
<pre>
v1/Assets/
├── <a href="v1/Assets/01_Player">01_Player/</a>
│   ├── <a href="v1/Assets/01_Player/Equipment">Equipment/{Weapon, Accessory}/</a>   # 무기·악세서리 상속 구조
│   └── <a href="v1/Assets/01_Player">PlayerController*.cs</a>              # 상태머신·입력·스킬 (partial 분리)
├── <a href="v1/Assets/02_Enemy">02_Enemy/</a>                            # 적 베이스·매니저
└── 03_Stage/
    ├── <a href="v1/Assets/03_Stage/00_Manager">00_Manager/StageManager*.cs</a>      # 스테이지 서브시스템 (partial 분리)
    ├── <a href="v1/Assets/03_Stage/99_PathFinder/TempPathFinder.cs">99_PathFinder/TempPathFinder.cs</a>  # BFS Flow Field 본체 — 적 이동·스폰 전체가 사용 (단일 스레드) ★
    └── <a href="v1/Assets/03_Stage/Object/Item">Object/Item/</a>                     # 경험치·회복 아이템
</pre>
