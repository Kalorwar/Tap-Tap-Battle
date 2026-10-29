# T01: Прогрессия уровней, боссы и награды

## Цель
Уровень = 5 врагов; каждый 10-й уровень — бой с боссом (x5 HP, таймер 30 сек, провал → босс респавнится с полным HP и таймером заново); награда начисляется за каждого убитого врага, а не за уровень.

## Контекст
- Проект: `F:/Unity/Tap-Tap Battle` (Unity 6, 2D URP, Zenject, C# events, DOTween). R3 удалён — использовать ТОЛЬКО обычные C# `event`.
- Сейчас: `LevelCountService` (Assets/_Project/Scripts/Core/LevelCountService.cs) повышает уровень на КАЖДУЮ смерть врага (`EnemyDiedEvent`). `EnemySpawner` спавнит врага на старте и по `LevelCompletedSignal`. `RewardService` даёт монеты за уровень. `EnemyLibrary` (SO) считает HP: `DefaultHealth * 1.15^(level-1)`, DefaultHealth=10, DefaultReward=10; босс-данные: DefaultHealth=500, DefaultReward=10.
- Событийная шина (паттерн строго соблюдать): static class с `public static event Action<T> Xxx;` и `public static void Emit(T x) => Xxx?.Invoke(x);` — см. Events/TapEvent.cs, Events/EnemySpawnedSignal.cs (уже есть: `EnemySpawnedInfo{Enemy Enemy, bool IsBoss}`), Events/BossDefeatedSignal.cs (уже есть: `event Action<int>`, 1-based индекс босса).
- `Enemy` (Enemies/Enemy.cs): `Configure(float health)`, `TakeDamage(float)`, эмитит `EnemyHitSignal` при ударе и `EnemyDiedEvent` при смерти, сам `Destroy(gameObject)`. Есть `IsAlive`, `MaxHealth`.

## Что сделать
1. **5 врагов на уровень.** Новый сервис `LevelProgressionService` (или перепиши LevelCountService — имя оставь LevelCountService, чтобы не ломать биндинги): хранит текущий уровень и счётчик убийств на уровне (0..4). Смерть обычного врага → kills+1; kills==5 → LevelCount++, kills=0, `LevelCompletedSignal.Emit(level)`.
2. **Босс-уровни.** Если `level % 10 == 0` — на уровне ОДИН босс вместо 5 врагов. HP босса = `EnemyLibrary.CalculateHealth(level) * 5f` (множитель — `private const float BossHealthMultiplier = 5f;`). Спавн из `GetRandomBossPrefab` (ИСПРАВИТЬ баг: индекс сейчас берётся по `_enemiesPrefab.Count` вместо `_bossesPrefabs.Count`). При спавне босса — `EnemySpawnedSignal.Emit(new EnemySpawnedInfo(enemy, true))`, обычного — `(..., false)`.
3. **Таймер босса 30 сек.** Новый `BossFightService` (plain C# сервис; тики ему даёт MonoBehaviour-обёртка? НЕТ — сделай сам сервис с методом `Tick(float deltaTime)`, а вызовы добавит интегратор, ИЛИ сделай простую корутину на самом EnemySpawner — выбери сам, но без R3/UniTask). Таймер стартует при спавне босса. Каждую секунду (и при старте/стопе) эмитит `BossTimerSignal` (новый Events/BossTimerSignal.cs): `event Action<BossTimerInfo>`; `BossTimerInfo{bool IsRunning; float RemainingSeconds;}`. Когда время вышло: уничтожить текущего босса, заспавнить нового с полным HP, таймер заново (уровень не меняется). Если босс убит — таймер стоп, `BossDefeatedSignal.Emit(bossIndex)`, `LevelCompletedSignal`, переход на следующий уровень.
4. **Счётчик боссов.** `bossIndex` — 1-based количество побеждённых боссов всего (сохранять PlayerPrefs "Progression.BossesDefeated"). Награда за босса: софт x5 от обычной + хард = `BossLibraryData.DefaultReward`.
5. **Награда за каждого врага.** Новый Events/EnemyKilledSignal.cs: `event Action<EnemyKilledInfo>`; `EnemyKilledInfo{bool IsBoss; int Level;}`. EnemySpawner (или прогрессион-сервис) эмитит его при смерти врага с контекстом. `RewardService` подписывается на него: софт = `DefaultReward * 1.12^(level-1)` (босс: `* 5`), хард только за босса. Уровневую подписку RewardService убрать.
6. **Сохранение.** PlayerPrefs: уровень "Progression.Level", убийства на уровне "Progression.KillsOnLevel", боссы "Progression.BossesDefeated". Старые ключи не использовать. При старте — восстановление состояния (уровень, kills).
7. `SeedService` не трогать (сид инкрементится по `LevelCompletedSignal` — это ок).

## Ограничения
- Файлы, которые можно менять/создавать: Core/LevelCountService.cs, Core/EnemySpawner.cs, Core/EnemyLibrary.cs, Core/RewardService.cs, Core/BossFightService.cs (новый), Enemies/Enemy.cs, Events/*.cs (новые: EnemyKilledSignal.cs, BossTimerSignal.cs). 
- ЗАПРЕЩЕНО: трогать LevelInstaller.cs, DamageInfo.cs, DamageController.cs, SeedService.cs, сцену, префабы, .asset, Packages/manifest.json; менять сериализуемые поля EnemyLibraryData/BossLibraryData (существующие .asset сломаются); использовать R3/UniRx/UniTask; git commit; инструменты mcp__UnityMCP__*; запуск Unity.
- Стиль: как окружающий код — 4 пробела, `_camelCase` приватные поля, readonly где возможно, комментарии по-английски только где не очевидно, namespace `_Project.Scripts.Core` / `_Project.Scripts.Events`.

## Критерии готовности
- [ ] Уровень 1: убил 5 врагов → LevelCompletedSignal(2), счётчик kills сброшен
- [ ] Уровень 10: спавнится босс (IsBoss=true), HP = CalculateHealth(10)*5, таймер 30→0 за 30 тиков-секунд
- [ ] Таймер вышел → босс уничтожен и заспавнен заново с полным HP, таймер снова 30
- [ ] Босс убит → BossDefeatedSignal(N), LevelCompletedSignal, уровень+1
- [ ] За каждого врага: AddCurrency(10 * 1.12^(level-1), Soft); за босса: x5 софт + 10 хард
- [ ] Перезапуск домена: уровень/kills/боссы восстановлены из PlayerPrefs
- [ ] Код компилируется логически: все типы/события согласованы, нет orphan-подписок (отписка в Dispose/OnDisable)

## Проверка
Не опирайся на скриншоты — проверяй кодом, числами, трассировкой вызовов. Застрял >20 мин или 2 гипотезы не подтвердились — остановись и отчитайся.

## Отчёт в конце
Изменённые/созданные файлы; что сделано; точные строки для биндингов Zenject в LevelInstaller (какие сервисы забиндить); что не вышло.
