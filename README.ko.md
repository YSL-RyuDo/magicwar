# Magicwar — Multiplayer Magic Combat

[English](README.md) · [Korean](README.ko.md) · [Japanese](README.ja.md)

C++ IOCP 서버와 연결되는 Unity 멀티플레이 마법 전투 프로젝트입니다.

## 내 기여

- 게임·네트워크 연동과 클라이언트 마법 전투 시스템을 작업했습니다.
- 피해·회복, 순간이동, 투명화, 보호막, 벽과 둔화 효과가 소스에 있으며, 대응하는 게임 서비스는 별도 서버에서 처리합니다.
- 오디오·UI 등 다른 개발자의 기여가 있는 프로젝트입니다. 저장소 소유가 모든 에셋·코드의 단독 제작을 의미하지 않습니다.

## 기여 근거

- [Development history / 개발 이력 / 開発履歴](https://github.com/YSL-RyuDo/magicwar/commits/main/?author=YSL-RyuDo)

커밋은 설명한 변경의 근거이며 기여율 점수가 아닙니다. 외부 에셋과 팀원 작업의 크레딧은 구분합니다.

## 실행·코드 확인

[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)의 Unity 버전으로 프로젝트를 여세요. 패키지 잠금 파일과 직렬화된 에셋을 유지하고, Unity를 열기 전에 문서에서 설명한 브랜치를 선택하세요.

- Unity scene: [Assets/Scenes/LoginScene.unity](Assets/Scenes/LoginScene.unity)

[magicwarserver](https://github.com/YSL-RyuDo/magicwarserver)

## 개발·검증 범위

과거 개발 상태를 정리합니다. 문서는 구현 범위를 설명하며, 새 플레이 테스트나 상용 수용량 검증 결과가 아닙니다. 클라이언트와 서버 버전을 맞춰 사용하세요.
