# 🧠 Orchid.Core (`src/Core`)

> **Windows, macOS, Android, iOS, Web** 5대 플랫폼이 100% 공유하는 **범용 C# 클라이언트 코어 비즈니스 라이브러리**

---

## 1. 프로젝트 역할 및 책임
- **역할**: 플랫폼 UI와 완전히 분리된 클립보드 도메인 엔티티, E2EE 암호화 엔진, SQLite 로컬 캐시, 동기화 상태 엔진 소유.
- **아키텍처 원칙**:
  - UI 독립성: 특정 OS의 UI 요소(WPF/MAUI/Blazor)를 참조하지 않는 순수 C# 계층.
  - 의존성 역전(DIP): 플랫폼 고유 기능(시스템 클립보드 읽기/쓰기, 파일 스토리지)은 인터페이스(`IClipboardService`, `IPlatformStorage`)로 추상화하여 상위 플랫폼 헤드가 구현.

---

## 2. 폴더 및 디렉터리 구조

```text
src/Core/
├── Domain/                      # 순수 도메인 모델 (의존성 제로)
│   ├── Entity/                  # 클립보드 아이템 엔티티 (불변식 보호)
│   │   ├── ClipboardItem.cs     # 고유 ID, 암호화 상태, 타입, 생성/수정 일시
│   │   └── DeviceSession.cs     # 페어링된 디바이스 정보
│   ├── ValueObject/             # 불변 값 객체
│   │   ├── PayloadHash.cs       # 중복 검사용 SHA-256 해시
│   │   └── EncryptionKey.cs     # 메모리 보안 처리된 키 값
│   └── Common/                  # Result 패턴 및 도메인 에러
│       ├── Result.cs            # 함수형 Result 및 Result<T>
│       └── Error.cs             # 도메인 오류 정의
├── Cryptography/                # E2EE 종단간 암호화 파이프라인
│   ├── IAesGcmCryptoService.cs  # AES-256-GCM 암호화/복호화 인터페이스
│   ├── AesGcmCryptoService.cs   # System.Security.Cryptography 기반 구현
│   └── KeyDerivationService.cs  # 사용자 패스프레이즈 기반 Argon2id 키 유도
├── Storage/                     # 로컬 영속성 및 캐시
│   ├── IClipboardRepository.cs  # 로컬 저장소 CRUD 인터페이스
│   ├── SqliteClipboardRepository.cs # SQLite-net 기반 암호화 로컬 히스토리
│   └── DatabaseMigrator.cs      # 로컬 DB 스키마 마이그레이터
├── Sync/                        # 분산 동기화 엔진
│   ├── ISyncEngine.cs           # 실시간 WebSocket 및 REST 증분 동기화 조율자
│   ├── SyncEngine.cs            # 변경 추적 및 충돌 해결 (Vector Clock / LWW)
│   └── SyncStateStore.cs        # 현재 동기화 커서 및 펜딩 큐 관리
├── Abstraction/                 # OS 네이티브 어댑터용 계약 인터페이스
│   ├── ISystemClipboard.cs      # 네이티브 클립보드 I/O 추상화
│   └── INativeNotification.cs   # 네이티브 알림 배너 추상화
├── Orchid.Core.csproj           # 프로젝트 파일 (.NET 10)
└── README.md                    # 본 설명 문서
```
