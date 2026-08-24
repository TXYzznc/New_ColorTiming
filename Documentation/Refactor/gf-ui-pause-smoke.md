# GF.UI Pause Form Smoke Evidence

- Date: 2026-08-24
- Entry path: `Launch → StartMenu → Boss1`
- Navigation used the real serialized menu button listeners.

## Runtime result

- Boss1 loaded active with zero Console errors.
- Calling the same `UI_HeroInfo.TogglePause` path used by semantic pause input opened exactly one active `Esc(Clone)` in the GF.UI Dialog group.
- The authored scene `Esc` object remained inactive and did not handle runtime pause UI.
- Persistent listeners were present and resolved for `OffKeyTip`, `OpenKeyTip`, `GoNext`, `GoLast`, and `BackMenu`.
- Invoking `OffKeyTip` changed the two toggle objects to OffTip inactive / OpenTip active; invoking `OpenKeyTip` restored the setting.
- Closing and reopening the pause form reused the framework-pooled form instead of creating another active form.
- Invoking the real `BackMenu` button loaded `StartMenu`, closed the active pause form, opened exactly one active `MainMenu(Clone)`, and produced zero Console errors.

Inactive `Esc(Clone)` remains under the framework UI pool after close by design; it is not registered as an open form and is reused on the next open.
