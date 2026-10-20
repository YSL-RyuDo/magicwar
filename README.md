# Arcana Duel

[English](README.md) · [Korean](README.ko.md) · [Japanese](README.ja.md)

A multiplayer magic-combat game, built by one programmer and one graphics teammate.

**C++ · IOCP · DB worker · Unity** · Development archive

**Aug 2025 - Feb 2026 · 2 members: 1 programmer, 1 graphics**

Programming: C++ server & Unity game integration

[Explore the case](https://ysl-ryudo.github.io/en/work/magicwar/) · [Portfolio](https://ysl-ryudo.github.io/en/)

[magicwarserver](https://github.com/YSL-RyuDo/magicwarserver) (Private · access required) · [magicwar](https://github.com/YSL-RyuDo/magicwar) (Private · access required)

## System responsibilities

| Area | Implementation |
|---|---|
| Network work | Accept connections; complete receives and sends through IOCP workers |
| Resource lifetime | Reusable session, buffer and overlapped-I/O pools |
| Database work | Database requests handled by a separate DB worker |
| Game integration | Magic, cooldowns, matches, profiles and ranks paired with Unity |

## Development videos

- [Arcana Duel](https://www.youtube.com/watch?v=MJo1y09kzd4)
- [Early development demo](https://www.youtube.com/watch?v=8sKpA2H9Ib4)

## My contribution

- Worked on the game/network integration and the client-side magic-combat systems.
- The source includes damage/heal, blink, invisibility, barriers, walls and slow effects; the companion server handles the corresponding game services.
- Original audio/UI contributions recorded in source history and third-party asset credits are retained.

## Contribution evidence

- [Development history / 개발 이력 / 開発履歴](https://github.com/YSL-RyuDo/magicwar/commits/main/?author=YSL-RyuDo)

Commits are evidence of the stated changes, not a percentage-of-authorship score. Third-party assets and teammates' work remain separately credited.

## Open and inspect

Open the project with the Unity version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). Keep its package lock and serialized assets intact. Select the documented branch before opening Unity.

- Unity scene: [Assets/Scenes/LoginScene.unity](Assets/Scenes/LoginScene.unity)

[magicwarserver](https://github.com/YSL-RyuDo/magicwarserver)

## Development and verification scope

Historical development snapshot. The documentation identifies implementation areas, not a new gameplay test or a production-capacity result. Use the client and server as a matched pair.
