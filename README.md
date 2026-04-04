# 댕댕이 서바이벌 (Pup Survivors)

> 로그라이트 / 뱀파이어 서바이벌 류 | Unity, C# | 1인 개발 | PC · 모바일

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

수백 마리의 몬스터가 존재하는 환경에서 각자 A*를 연산하는 대신, 타일마다 플레이어 방향을 BFS로 사전 계산하여 몬스터 수와 무관하게 **타일 영역 크기만큼만** 연산합니다.

### Unity Job System 멀티스레딩

초기화(셀 독립) → `IJobParallelFor`, BFS 탐색(이전 셀 의존) → `IJob` 으로 분리 스케줄링.
지상/공중 타입 × 4인 플레이 기준 **4ms → 0.5ms (약 8배 향상)**.

---

## 핵심 코드 구조

### v2 — Job System 적용 버전

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
│   │   ├── Weapon/              # 활·방패·클로·유성·하키 등 (상속 구조)
│   │   └── Accessory/           # 15종 악세서리
│   └── PlayerController*.cs     # 상태머신, 입력, 스킬 (partial)
├── 02_Enemy/
│   ├── EnemyBase.cs             # 적 공통 베이스
│   └── CommonEnemy.cs
└── 03_Stage/
      ├── 00_Manager/StageManager*.cs   # 스테이지 서브시스템 (partial)
      ├── 99_PathFinder/           # ★ Flow Field v1 (Job System 적용 전)
      ├── Object/Item/             # 경험치·회복 아이템
      └── EnemySpawner.cs
```
