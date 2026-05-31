# 댕댕이 서바이벌 (Pup Survivors)
> 써드파티 에셋 라이센스로 인해 전체 프로젝트가 아닌 C# 스크립트만 포함

> 로그라이트 / 뱀파이어 서바이벌 류 | Unity | 1인 개발 | PC · 모바일 | 미출시

### 플레이 영상
[![플레이 영상](https://img.youtube.com/vi/uemJe7yG4UM/0.jpg)](https://youtu.be/uemJe7yG4UM)

---

## 기술 스택

- **Unity, C#**
- **Unity Job System / Burst Compiler** — 멀티스레딩 최적화

---

## 핵심 구현

| 파일 | 역할 |
|------|------|
| [PathFinder.cs](v2/Assets/03_Stage/System/PathFind/PathFinder.cs) | FlowField 관리 및 갱신 진입점 |
| [CostField.cs](v2/Assets/03_Stage/System/PathFind/CostField.cs) | 장애물 정보 저장 (타일 비용) |
| [FlowField.cs](v2/Assets/03_Stage/System/PathFind/FlowField.cs) | Job 스케줄링 및 방향 데이터 보유 |
| [ResetFlowFieldJob.cs](v2/Assets/03_Stage/System/PathFind/ResetFlowFieldJob.cs) | FlowField 초기화 (`IJobParallelFor`) |
| [UpdateFlowFieldJob.cs](v2/Assets/03_Stage/System/PathFind/UpdateFlowFieldJob.cs) | BFS 탐색으로 방향 계산 (`IJob`) |

### Flow Field 기반 길찾기

처음엔 BFS에서 영감을 얻어 자체 재귀 구현을 시도했지만, 성능 한계로 커뮤니티에서 Flow Field라는 키워드를 찾았고 GDC 자료를 참고해 재구현했습니다.

수백 마리의 몬스터가 각자 A*를 연산하는 대신, **타일마다 플레이어 방향을 BFS로 사전 계산**합니다. 몬스터 수가 늘어도 연산량은 타일 영역 크기에만 비례합니다.

### Unity Job System 멀티스레딩

지상/공중 2가지 이동 타입 × 4인 플레이 조합으로 연산량이 늘어나면서 최적화가 필요했습니다.

```csharp
// FlowField.cs — Ground/Air 두 FlowField를 병렬로 갱신
public JobHandle UpdateFlowField()
{
    // Reset: 각 셀이 독립적 → IJobParallelFor (병렬)
    var resetGroundJob = new ResetFlowFieldJob { ... };
    var resetAirJob    = new ResetFlowFieldJob { ... };
    var resetGroundHandle = resetGroundJob.Schedule(RowSize * ColSize, 32);
    var resetAirHandle    = resetAirJob.Schedule(RowSize * ColSize, 32);

    // BFS: 이전 셀 결과에 의존 → IJob (순차), Reset 완료 후 시작
    var updateGroundJob = new UpdateFlowFieldJob { FlowField = GroundCells, ... };
    var updateAirJob    = new UpdateFlowFieldJob { FlowField = AirCells,    ... };
    var updateGroundHandle = updateGroundJob.Schedule(resetGroundHandle);
    var updateAirHandle    = updateAirJob.Schedule(resetAirHandle);

    return JobHandle.CombineDependencies(updateGroundHandle, updateAirHandle);
}
```

초기화 단계는 셀 간 의존이 없어 `IJobParallelFor`로 병렬 처리하고, BFS 탐색은 이전 셀 결과에 의존하기 때문에 `IJob`으로 분리했습니다. Reset → BFS 순서는 Job 의존성 체인으로 보장합니다.

지상/공중 타입 × 4인 플레이 기준 **4ms → 0.5ms (약 8배 향상)**.

---

## 핵심 코드 구조

### v2 — Job System 적용

```
v2/Assets/
└── 03_Stage/System/PathFind/
    ├── CostField.cs             # 장애물 정보 (타일 비용)
    ├── FlowField.cs             # Job 스케줄링 및 방향 데이터
    ├── PathFinder.cs            # FlowField 관리 및 갱신 진입점
    ├── ResetFlowFieldJob.cs     # 초기화 (IJobParallelFor)
    └── UpdateFlowFieldJob.cs    # BFS 탐색 (IJob)
```

### v1 — 전체 게임 구조

```
v1/Assets/
├── 01_Player/
│   ├── Equipment/
│   │   ├── Weapon/              # 활·방패·클로·유성 등 (상속 구조)
│   │   └── Accessory/           # 15종 악세서리
│   └── PlayerController*.cs     # 상태머신, 입력, 스킬 (partial)
├── 02_Enemy/
│   ├── EnemyBase.cs             # 적 공통 베이스
│   └── CommonEnemy.cs
└── 03_Stage/
      ├── 00_Manager/StageManager*.cs   # 스테이지 서브시스템 (partial)
      ├── 99_PathFinder/           # Flow Field v1 (Job System 적용 전) ★
      ├── Object/Item/             # 경험치·회복 아이템
      └── EnemySpawner.cs
```
