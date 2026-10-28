# MwSt-Rechner

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-donald210-yellow?logo=buymeacoffee&logoColor=black)](https://buymeacoffee.com/donald210)

Ein einfacher Windows-Taschenrechner für Mehrwertsteuer.

## Funktionen

- **+MwSt**: Netto → Brutto
- **-MwSt**: Brutto → Netto
- MwSt-Satz wählbar: 19 % oder 7 %
- Grundrechnen mit **+** und **-**
- Tastatureingabe möglich (Zahlen, +, -, Enter, Backspace, Esc)

## Windows

Voraussetzung: Windows mit .NET Framework 4 (bei Windows 10/11 bereits vorhanden).

```
build.bat
```

Die EXE landet unter `build\MwStRechner.exe`.

## Linux (Debian, Fedora)

Voraussetzung: Python 3 mit tkinter.

- Debian: `sudo apt install python3-tk`
- Fedora: `sudo dnf install python3-tkinter`

Starten:

```
python3 linux/mwst_rechner.py
```

## Version

0.1 Beta
