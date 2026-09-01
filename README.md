# 천도컴퍼니 · 1000-Company

**의뢰를 받아 심령 현상을 조사하고 귀신을 천도하는 협동 공포 게임**

![Unity](https://img.shields.io/badge/Unity_6-000000?style=flat-square&logo=unity&logoColor=white)
![C Sharp](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=csharp&logoColor=white)
![Photon Fusion](https://img.shields.io/badge/Photon_Fusion-004480?style=flat-square)
![Vivox](https://img.shields.io/badge/Vivox-1a1a1a?style=flat-square)
![URP](https://img.shields.io/badge/URP-0f172a?style=flat-square)

<!-- TODO: 플레이 GIF 또는 스크린샷 2~3장 -->

## 프로젝트 소개

플레이어는 천도컴퍼니 소속 퇴마사가 되어 의뢰를 받고 현장에 들어갑니다. 부적과 무당방울, 가야금 같은 도구로 심령 현상에 대응하고, 꽃신과 꽹과리로 귀신의 흔적을 탐지합니다. 모은 단서로 귀신의 정체를 추리한 뒤 마법진을 그려 천도하는 것이 목표입니다.

한국 무속을 소재로 삼아 도구와 연출을 구성했고, 위치 기반 음성 채팅을 붙여 거리에 따라 팀원의 목소리가 달라지도록 만들었습니다.

## 프로젝트 상태

> **프로토타입.** 플레이 가능한 구조까지 구현했으며 출시하지 않았습니다. 실행에는 Unity 프로젝트와 Photon 설정이 필요합니다.

## 주요 시스템

| 영역 | 구현 내용 |
|---|---|
| 귀신 | 상태 기계 기반 행동(대기·순찰·추격·사냥·공격·피격·퇴마·사망), 방 배치와 재배치 |
| 탐지 | 꽃신, 꽹과리, 촛대, 복숭아나무가지, 라디오, 온도계 등 도구별 탐지 방식 |
| 퇴마 | 부적, 무당방울, 향로, 가야금, 혼례복, 마법진 그리기와 게이지 |
| 심령 현상 | 천우인, 다크오라, 어둠 등 현장에서 발생하는 이상 현상 |
| 협동 | 위치 기반 음성 채팅, 텍스트 채팅, 동료 구조와 시체 운반 |
| 진행 | 의뢰 접수와 모니터 조회, 상점·키오스크, 도감, 귀신 추리 UI, 관전 |

## 기술 스택

Unity 6000.0.59f2 · C# · Photon Fusion · Unity Vivox · URP · Cinemachine 3 · AI Navigation · Input System · Shader Graph · Post Processing · ParrelSync

## 개인 기여 — 최서영

아이템 시스템과 심령 현상을 담당했습니다.

- 자율 이동 탐지 아이템 「꽃신」 구현 — 호스트 권한 기반 상태 기계와 NavMesh 이동
- 오브젝트 풀 매니저와 네트워크 연동 서브 몬스터 스폰
- 데이터 지향 심령 현상 스폰 매니저 설계
- Tick 기반 연출 동기화와 렌더링 분리 (천우인, 다크오라)
- 거리 기반 탐지 판정과 사운드 피드백
- 인벤토리 상호작용, 무당방울 멀티 동기화, 시체 운반
- 가이드북 UI, 복숭아나무가지

## 프로젝트 구조

```text
Assets/
├── 01.Scenes/           # 사무실, 의뢰 현장
├── 02.Scripts/
│   ├── Common/          # 씬 관리, 오브젝트 풀, 사운드, 스폰
│   ├── Ghost/           # 귀신 상태 기계와 데이터
│   ├── Interaction/     # 문, 엘리베이터, 모니터, 문서
│   ├── MagicCircle/     # 마법진 판정과 연출
│   ├── Paranormal Phenomena/  # 심령 현상
│   ├── Network/         # 세션, 미션, Vivox
│   ├── Player/          # 조작, 인벤토리, 카메라, 관전
│   └── UI/              # 상점, 키오스크, 도감, 귀신 추리, 설정
└── 03.Prefabs/          # 아이템별 스크립트와 프리팹
```
