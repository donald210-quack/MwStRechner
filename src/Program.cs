using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new CalcForm());
    }
}

class CalcForm : Form
{
    readonly TextBox display = new TextBox();
    readonly Label info = new Label();
    readonly ComboBox rate = new ComboBox();
    readonly Button histButton = new Button();
    readonly Panel histPanel = new Panel();
    readonly TextBox histBox = new TextBox();
    readonly List<string> history = new List<string>();
    readonly CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
    decimal acc;
    string op = "";
    bool fresh = true;

    const int CalcWidth = 295;
    const int FullWidth = 565;
    const int FormHeight = 435;

    public CalcForm()
    {
        Text = "MwSt-Rechner 0.1 Beta";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        KeyPreview = true;
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(CalcWidth, FormHeight);

        info.SetBounds(10, 10, 275, 20);
        info.TextAlign = ContentAlignment.MiddleRight;
        info.ForeColor = Color.DimGray;

        display.SetBounds(10, 35, 275, 40);
        display.Font = new Font("Segoe UI", 20);
        display.TextAlign = HorizontalAlignment.Right;
        display.ReadOnly = true;
        display.BackColor = Color.White;
        display.TabStop = false;
        display.Text = "0";

        var rateLabel = new Label { Text = "MwSt-Satz:" };
        rateLabel.SetBounds(10, 89, 90, 20);

        rate.DropDownStyle = ComboBoxStyle.DropDownList;
        rate.Items.AddRange(new object[] { "19 %", "7 %" });
        rate.SelectedIndex = 0;
        rate.SetBounds(105, 85, 80, 25);
        rate.TabStop = false;

        histButton.Text = "Verlauf >>";
        histButton.SetBounds(195, 84, 90, 27);
        histButton.TabStop = false;
        histButton.Click += (s, e) => ToggleHistory();

        Controls.Add(info);
        Controls.Add(display);
        Controls.Add(rateLabel);
        Controls.Add(rate);
        Controls.Add(histButton);

        AddButton("C", 0, 0);
        AddButton("\u2190", 1, 0);
        AddButton("+MwSt", 2, 0);
        AddButton("-MwSt", 3, 0);

        AddButton("7", 0, 1);
        AddButton("8", 1, 1);
        AddButton("9", 2, 1);
        AddButton("+", 3, 1);

        AddButton("4", 0, 2);
        AddButton("5", 1, 2);
        AddButton("6", 2, 2);
        AddButton("-", 3, 2);

        AddButton("1", 0, 3);
        AddButton("2", 1, 3);
        AddButton("3", 2, 3);
        AddButton("=", 3, 3, 1, 2);

        AddButton("0", 0, 4, 2, 1);
        AddButton(",", 2, 4);

        var credit = new Label { Text = "Powered by Donald2_", ForeColor = Color.Gray };
        credit.SetBounds(10, 408, 275, 20);
        credit.TextAlign = ContentAlignment.MiddleCenter;
        Controls.Add(credit);

        histPanel.SetBounds(305, 10, 250, 415);
        histPanel.Visible = false;

        histBox.SetBounds(5, 5, 240, 340);
        histBox.Multiline = true;
        histBox.ReadOnly = true;
        histBox.ScrollBars = ScrollBars.Vertical;
        histBox.BackColor = Color.White;
        histBox.Font = new Font("Consolas", 9);
        histBox.TabStop = false;

        var printButton = new Button { Text = "Drucken" };
        printButton.SetBounds(5, 352, 115, 30);
        printButton.TabStop = false;
        printButton.Click += (s, e) => Print();

        var clearButton = new Button { Text = "Leeren" };
        clearButton.SetBounds(130, 352, 115, 30);
        clearButton.TabStop = false;
        clearButton.Click += (s, e) => ClearHistory();

        histPanel.Controls.Add(histBox);
        histPanel.Controls.Add(printButton);
        histPanel.Controls.Add(clearButton);
        Controls.Add(histPanel);
    }

    Button AddButton(string text, int col, int row, int colSpan = 1, int rowSpan = 1)
    {
        var b = new Button
        {
            Text = text,
            Location = new Point(10 + col * 70, 125 + row * 55),
            Size = new Size(65 + (colSpan - 1) * 70, 50 + (rowSpan - 1) * 55),
            TabStop = false
        };
        b.Click += (s, e) => Press(text);
        Controls.Add(b);
        return b;
    }

    void ToggleHistory()
    {
        histPanel.Visible = !histPanel.Visible;
        ClientSize = new Size(histPanel.Visible ? FullWidth : CalcWidth, FormHeight);
        histButton.Text = histPanel.Visible ? "<< Verlauf" : "Verlauf >>";
    }

    void Log(string line)
    {
        history.Add(line);
        histBox.AppendText(line + Environment.NewLine);
    }

    void ClearHistory()
    {
        history.Clear();
        histBox.Clear();
    }

    void Print()
    {
        if (history.Count == 0)
        {
            MessageBox.Show("Der Verlauf ist leer.", "Drucken");
            return;
        }

        var lines = new List<string>();
        lines.Add("MwSt-Rechner 0.1 Beta");
        lines.Add(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        lines.Add(new string('-', 32));
        lines.AddRange(history);
        lines.Add(new string('-', 32));
        lines.Add("Powered by Donald2_");

        int idx = 0;
        var font = new Font("Courier New", 10);
        var doc = new PrintDocument();
        doc.PrintPage += (s, e) =>
        {
            float y = e.MarginBounds.Top;
            float lh = font.GetHeight(e.Graphics);
            while (idx < lines.Count && y + lh <= e.MarginBounds.Bottom)
            {
                e.Graphics.DrawString(lines[idx], font, Brushes.Black, e.MarginBounds.Left, y);
                y += lh;
                idx++;
            }
            e.HasMorePages = idx < lines.Count;
        };

        using (var dlg = new PrintDialog { Document = doc })
        {
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                idx = 0;
                doc.Print();
            }
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        char c = e.KeyChar;
        if (char.IsDigit(c)) Press(c.ToString());
        else if (c == ',' || c == '.') Press(",");
        else if (c == '+') Press("+");
        else if (c == '-') Press("-");
        else if (c == '=' || c == '\r') Press("=");
        else if (c == '\b') Press("\u2190");
        else if (c == 'c' || c == 'C') Press("C");
        else return;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) { Press("C"); e.Handled = true; }
        base.OnKeyDown(e);
    }

    void Press(string k)
    {
        switch (k)
        {
            case "C":
                display.Text = "0";
                acc = 0;
                op = "";
                info.Text = "";
                fresh = true;
                break;
            case "\u2190":
                if (fresh) break;
                if (display.Text.Length <= 1 || (display.Text.Length == 2 && display.Text[0] == '-'))
                    display.Text = "0";
                else
                    display.Text = display.Text.Substring(0, display.Text.Length - 1);
                break;
            case "+MwSt":
                Vat(true);
                break;
            case "-MwSt":
                Vat(false);
                break;
            case "+":
            case "-":
                Operator(k);
                break;
            case "=":
                Equals();
                break;
            case ",":
                if (fresh) { display.Text = "0,"; fresh = false; }
                else if (!display.Text.Contains(",")) display.Text += ",";
                break;
            default:
                if (fresh) { display.Text = k; fresh = false; }
                else if (display.Text == "0") display.Text = k;
                else if (display.Text.Length < 15) display.Text += k;
                break;
        }
    }

    decimal Value
    {
        get { return decimal.Parse(display.Text, NumberStyles.Number, de); }
    }

    string Fmt(decimal v)
    {
        return v.ToString("G29", de);
    }

    void Calc()
    {
        if (op == "") return;
        acc = op == "+" ? acc + Value : acc - Value;
        op = "";
        display.Text = Fmt(acc);
    }

    void Operator(string o)
    {
        if (op != "" && !fresh) Calc();
        else if (op == "") acc = Value;
        op = o;
        fresh = true;
        info.Text = Fmt(acc) + " " + o;
    }

    void Equals()
    {
        if (op == "") return;
        string left = Fmt(acc);
        string o = op;
        string right = display.Text;
        info.Text = left + " " + o + " " + right + " =";
        Calc();
        Log(left + " " + o + " " + right + " = " + display.Text);
        fresh = true;
    }

    void Vat(bool add)
    {
        decimal v = Value;
        decimal r = rate.SelectedIndex == 0 ? 0.19m : 0.07m;
        decimal res = add ? v * (1 + r) : v / (1 + r);
        res = Math.Round(res, 2, MidpointRounding.AwayFromZero);
        decimal tax = add ? res - v : v - res;
        display.Text = Fmt(res);
        info.Text = (add ? "+" : "-") + " MwSt " + rate.Text + ": " + Fmt(Math.Round(tax, 2, MidpointRounding.AwayFromZero));
        Log((add ? "+" : "-") + "MwSt " + rate.Text + ": " + Fmt(v) + " -> " + Fmt(res));
        fresh = true;
    }
}
