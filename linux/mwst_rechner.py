import subprocess
import tempfile
import tkinter as tk
from datetime import datetime
from tkinter import ttk, messagebox
from decimal import Decimal, ROUND_HALF_UP

CENT = Decimal("0.01")


def fmt(v):
    return format(v.normalize(), "f").replace(".", ",")


class CalcApp:
    def __init__(self, root):
        self.root = root
        root.title("MwSt-Rechner 0.22")
        root.resizable(False, False)

        self.display = tk.StringVar(value="0")
        self.info = tk.StringVar(value="")
        self.acc = Decimal(0)
        self.op = ""
        self.fresh = True
        self.history = []

        tk.Label(root, textvariable=self.info, fg="gray").grid(
            row=0, column=0, columnspan=4, sticky="e", padx=10, pady=(10, 0))
        tk.Entry(root, textvariable=self.display, font=("Sans", 20), justify="right",
                 state="readonly", readonlybackground="white").grid(
            row=1, column=0, columnspan=4, sticky="ew", padx=10, pady=5, ipady=5)

        tk.Label(root, text="MwSt-Satz:").grid(row=2, column=0, columnspan=2, sticky="w", padx=10)
        self.rate = ttk.Combobox(root, state="readonly", values=["19 %", "7 %"], width=8)
        self.rate.current(0)
        self.rate.grid(row=2, column=2, sticky="w", padx=10)
        self.hist_button = tk.Button(root, text="Verlauf >>", command=self.toggle_history)
        self.hist_button.grid(row=2, column=3, sticky="e", padx=10)

        buttons = [
            ("C", 0, 0), ("\u2190", 1, 0), ("+MwSt", 2, 0), ("-MwSt", 3, 0),
            ("7", 0, 1), ("8", 1, 1), ("9", 2, 1), ("+", 3, 1),
            ("4", 0, 2), ("5", 1, 2), ("6", 2, 2), ("-", 3, 2),
            ("1", 0, 3), ("2", 1, 3), ("3", 2, 3),
            ("0", 0, 4), (",", 2, 4),
        ]
        for text, col, row in buttons:
            span = 2 if text == "0" else 1
            tk.Button(root, text=text, width=6, height=2,
                      command=lambda t=text: self.press(t)).grid(
                row=row + 3, column=col, columnspan=span, padx=5, pady=5, sticky="ew")
        tk.Button(root, text="=", width=6, height=5,
                  command=lambda: self.press("=")).grid(
            row=6, column=3, rowspan=2, padx=5, pady=5, sticky="nsew")

        tk.Label(root, text="Powered by Donald2_", fg="gray").grid(
            row=8, column=0, columnspan=4, pady=(0, 10))

        self.hist_frame = tk.Frame(root)
        self.hist_text = tk.Text(self.hist_frame, width=30, height=22, font=("Monospace", 9),
                                 state="disabled", bg="white", wrap="none")
        self.hist_text.grid(row=0, column=0, columnspan=2, padx=5, pady=5, sticky="nsew")
        tk.Button(self.hist_frame, text="Drucken", command=self.print_history).grid(
            row=1, column=0, sticky="ew", padx=5, pady=5)
        tk.Button(self.hist_frame, text="Leeren", command=self.clear_history).grid(
            row=1, column=1, sticky="ew", padx=5, pady=5)
        self.hist_frame.grid(row=0, column=4, rowspan=9, sticky="ns", padx=(0, 10), pady=10)
        self.hist_frame.grid_remove()

        root.bind("<Key>", self.on_key)
        root.bind("<Return>", lambda e: self.press("="))
        root.bind("<BackSpace>", lambda e: self.press("\u2190"))
        root.bind("<Escape>", lambda e: self.press("C"))

    def toggle_history(self):
        if self.hist_frame.winfo_ismapped():
            self.hist_frame.grid_remove()
            self.hist_button.config(text="Verlauf >>")
        else:
            self.hist_frame.grid()
            self.hist_button.config(text="<< Verlauf")

    def log(self, line):
        self.history.append(line)
        self.hist_text.config(state="normal")
        self.hist_text.insert("end", line + "\n")
        self.hist_text.config(state="disabled")
        self.hist_text.see("end")

    def clear_history(self):
        self.history = []
        self.hist_text.config(state="normal")
        self.hist_text.delete("1.0", "end")
        self.hist_text.config(state="disabled")

    def print_history(self):
        if not self.history:
            messagebox.showinfo("Drucken", "Der Verlauf ist leer.")
            return
        lines = [
            "MwSt-Rechner 0.22",
            datetime.now().strftime("%d.%m.%Y %H:%M"),
            "-" * 32,
            *self.history,
            "-" * 32,
            "Powered by Donald2_",
        ]
        try:
            with tempfile.NamedTemporaryFile("w", suffix=".txt", delete=False, encoding="utf-8") as f:
                f.write("\n".join(lines) + "\n")
                path = f.name
            subprocess.run(["lp", path], check=True)
        except FileNotFoundError:
            messagebox.showerror("Drucken", "Das Programm 'lp' wurde nicht gefunden. Bitte CUPS installieren.")
        except subprocess.CalledProcessError:
            messagebox.showerror("Drucken", "Der Druckauftrag konnte nicht gesendet werden.")

    def on_key(self, event):
        c = event.char
        if c.isdigit():
            self.press(c)
        elif c in ",.":
            self.press(",")
        elif c in "+-":
            self.press(c)
        elif c in "cC":
            self.press("C")

    def press(self, k):
        text = self.display.get()
        if k == "C":
            self.display.set("0")
            self.acc = Decimal(0)
            self.op = ""
            self.info.set("")
            self.fresh = True
        elif k == "\u2190":
            if self.fresh:
                return
            self.display.set("0" if len(text) <= 1 else text[:-1])
        elif k in ("+MwSt", "-MwSt"):
            self.vat(k == "+MwSt")
        elif k in ("+", "-"):
            self.operator(k)
        elif k == "=":
            self.equals()
        elif k == ",":
            if self.fresh:
                self.display.set("0,")
                self.fresh = False
            elif "," not in text:
                self.display.set(text + ",")
        else:
            if self.fresh:
                self.display.set(k)
                self.fresh = False
            elif text == "0":
                self.display.set(k)
            elif len(text) < 15:
                self.display.set(text + k)

    def value(self):
        return Decimal(self.display.get().replace(",", "."))

    def calc(self):
        if not self.op:
            return
        v = self.value()
        self.acc = self.acc + v if self.op == "+" else self.acc - v
        self.op = ""
        self.display.set(fmt(self.acc))

    def operator(self, o):
        if self.op and not self.fresh:
            self.calc()
        elif not self.op:
            self.acc = self.value()
        self.op = o
        self.fresh = True
        self.info.set(f"{fmt(self.acc)} {o}")

    def equals(self):
        if not self.op:
            return
        left = fmt(self.acc)
        o = self.op
        right = self.display.get()
        self.info.set(f"{left} {o} {right} =")
        self.calc()
        self.log(f"{left} {o} {right} = {self.display.get()}")
        self.fresh = True

    def vat(self, add):
        v = self.value()
        r = Decimal("0.19") if self.rate.current() == 0 else Decimal("0.07")
        res = v * (1 + r) if add else v / (1 + r)
        res = res.quantize(CENT, rounding=ROUND_HALF_UP)
        tax = (res - v) if add else (v - res)
        tax = tax.quantize(CENT, rounding=ROUND_HALF_UP)
        self.display.set(fmt(res))
        sign = "+" if add else "-"
        self.info.set(f"{sign} MwSt {self.rate.get()}: {fmt(tax)}")
        self.log(f"{sign}MwSt {self.rate.get()}: {fmt(v)} -> {fmt(res)}")
        self.fresh = True


if __name__ == "__main__":
    root = tk.Tk()
    CalcApp(root)
    root.mainloop()
