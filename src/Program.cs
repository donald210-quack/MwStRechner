using System;
using System.Drawing;
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
    readonly CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
    decimal acc;
    string op = "";
    bool fresh = true;

    public CalcForm()
    {
        Text = "MwSt-Rechner 0.1 Beta";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        KeyPreview = true;
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(295, 435);

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

        Controls.Add(info);
        Controls.Add(display);
        Controls.Add(rateLabel);
        Controls.Add(rate);

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
        info.Text = Fmt(acc) + " " + op + " " + display.Text + " =";
        Calc();
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
        fresh = true;
    }
}
