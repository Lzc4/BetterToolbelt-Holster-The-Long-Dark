# PermanentQuestRewards

*A lightweight gameplay overhaul mod for* **The Long Dark** *that transforms late-game quest rewards into true permanent progression rewards — similar to the Technical Backpack.*

Instead of permanently occupying valuable clothing/accessory slots, special quest rewards now unlock passive bonuses permanently after being discovered once.

---

# Features

## Security Chief's Rifle Holster

Once obtained, the holster is consumed permanently and grants:

* **50% weight reduction** for the **single heaviest rifle** currently in your inventory
* Automatically updates in real time when inventory changes
* Functions as a permanent passive perk
* No equip slot required anymore



---

## Foreman's Tool Belt

Once obtained, the belt is consumed permanently and grants:

* **50% weight reduction** for the **3 heaviest supported tools** currently carried
* Dynamically recalculates whenever inventory changes
* No equip slot required anymore


---

# Design Goals

This mod exists to solve a major gameplay issue:

Late-game rewards in vanilla compete with essential equipment slots, making them difficult to justify despite being hard-earned quest rewards.

This mod redesigns those rewards into meaningful permanent progression unlocks while preserving balance:

* Only the heaviest rifle receives the bonus
* Only the 3 heaviest tools receive the bonus
* Bonuses update dynamically
* Progress is stored per savegame

---

# Technical Notes

* Built for MelonLoader 0.7+
* Runtime `ItemWeight` patching
* Save-scoped progression system
* Optimized inventory caching to avoid FPS loss
* Includes optional lightweight debug logging

---

# Credits

Created by **Lzc4**

Made because **RaverLP** insisted this mechanic deserved to exist.

Special thanks to the Hinterland modding community for collectively suffering through undocumented IL2CPP behavior together.

---

# Personal Note

I am neither an active *The Long Dark* player nor a dedicated mod developer in general.

I currently see myself primarily as a web developer — this project only exists because RaverLP kept insisting the mechanic deserved to exist.

If you're interested in my other projects, feel free to check out **Sidequest-Arcade**.

---

# Disclaimer

This mod is not affiliated with or endorsed by Hinterland Studio.

---

# Download

[![Download](https://img.shields.io/badge/Download-v1.6.1-blue?style=for-the-badge)](https://github.com/Lzc4/BetterToolbelt-Holster-The-Long-Dark/releases/download/v1.6.1/PermanentQuestRewards.dll)
