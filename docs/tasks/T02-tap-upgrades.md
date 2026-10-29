# T02: Прокачка урона за тап (UpgradeService)

## Цель
Покупка улучшений «урон за тап» за монеты по формулам ГДД, с сохранением и обновлением боевого урона.

## Контекст
- Проект: `F:/Unity/Tap-Tap Battle` (Unity 6, 2D URP, Zenject, C# events). R3 удалён — ТОЛЬКО обычные C# `event`.
- Урон тапа живёт в `DamageInfo` (Assets/_Project/Scripts/Core/DamageInfo.cs): `SetDamageOnTap(float)` (НЕ менять этот файл). Сейчас тап = 1.
- Валюта: `ICurrencyController` (Assets/_Project/Scripts/CurrencyController/ICurrencyController.cs): `HasEnough(amount, type)`, `TryRemoveCurrency(amount, type)`, `AddCurrency`. `CurrencyType.SoftCurrency`.
- Паттерн событий: static class, `public static event Action ...;` + `Emit()` — см. Events/TapEvent.cs.
- Формулы ГДД (точные): Cost = 50 * 1.07^Level; Damage = 10 * Level, где Level — число купленных уровней (0 на старте → урон 1, после 1-й покупки (50 монет) → урон 10).

## Что сделать
1. `Assets/_Project/Scripts/Upgrades/UpgradeMath.cs` — статический класс: `TapDamageCost(int purchasedLevels)` (стоимость СЛЕДУЮЩЕЙ покупки), `TapDamage(int purchasedLevels)`.
2. `Assets/_Project/Scripts/Upgrades/TapDamageUpgradeService.cs` — Zenject-сервис (plain C#, конструктор-инжект `DamageInfo` и `ICurrencyController`):
   - `int Level { get; }` (загружается из PlayerPrefs "Upgrades.TapDamage.Level");
   - `float NextCost`, `float CurrentDamage`, `bool CanBuy`;
   - `bool TryBuy()` → проверка и списание монет, Level++, сохранение, `DamageInfo.SetDamageOnTap(CurrentDamage)`, `UpgradeChangedSignal.Emit()`;
   - в конструкторе сразу применяет `SetDamageOnTap(CurrentDamage)` (при загрузке сейва).
3. `Assets/_Project/Scripts/Events/UpgradeChangedSignal.cs` — `event Action` + Emit.

## Ограничения
- Файлы: только новые Upgrades/UpgradeMath.cs, Upgrades/TapDamageUpgradeService.cs, Events/UpgradeChangedSignal.cs. Больше НИЧЕГО не трогать (особенно LevelInstaller.cs, DamageInfo.cs, CurrencyController).
- ЗАПРЕЩЕНО: R3/UniRx/UniTask; MonoBehaviour (это pure C# сервис); git commit; mcp__UnityMCP__*; запуск Unity; сцены/префабы/.asset.
- Стиль: 4 пробела, `_camelCase`, readonly, namespace `_Project.Scripts.Upgrades`, комментарии по-английски минимально.

## Критерии готовности
- [ ] Level 0: Cost=50, Damage=1; после покупки Level 1: Cost=53.5 (50*1.07), Damage=10
- [ ] Level 10: Cost≈98.3, Damage=100
- [ ] TryBuy=false при нехватке монет (валюта не списывается)
- [ ] Покупка: монета списалась, уровень сохранён, DamageInfo обновлён, сигнал эмитнут
- [ ] Рестарт домена: уровень восстановлен, DamageInfo применён при старте

## Проверка
Не опирайся на скриншоты — проверяй кодом и числами (просчитай формулы вручную для Level 0/1/10). Застрял >20 мин — остановись и отчитайся.

## Отчёт в конце
Файлы; что сделано; строка биндинга Zenject для LevelInstaller; что не вышло.
