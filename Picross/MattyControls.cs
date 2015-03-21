using System;
using System.Drawing;
using System.Windows.Forms;

//
// Some personal often used controls, to make the positioning of controls easier to do using just code.
//
namespace Picross
{
    class Btn : Button
    {
        public const int distance = 10;
        public const int labelWidth = 100;
        public enum Horizontal { Left, CopyLeft, Center, CopyRight /* Pun intended */, Right };
        public enum Vertical { Top, CopyTop, Middle, CopyBottom, Bottom };

        public Lbl Label;

        public Btn(string text, Control parent) {
            // Some default things about buttons
            this.Text = text;
            parent.Controls.Add(this);
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }

        /// <summary>
        /// Add a label to this control
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="d">The distance between the label and the control</param>
        /// <param name="moveCtrl">Whether the control should be moved or not</param>
        /// <param name="labelWidth">The width of the label. Set to 0 to keep the original width</param>
        public void AddLabel(string text, int d = Btn.distance, bool moveCtrl = true, int labelWidth = 0) {
            if (this.Label != null)
                this.Parent.Controls.Remove(this.Label);
            this.Label = Btn.AddLabel(this, text, d, moveCtrl, labelWidth);
        }

        // The static methods that actually do the work
        public static void LocateInside(Control ctrl, Control c, Btn.Horizontal h, Btn.Vertical v, int d) {
            int x = 0;
            int y = 0;

            if (h == Horizontal.Left)
                x = d;
            if (h == Horizontal.CopyLeft)
                x = c.Location.X;
            if (h == Horizontal.Center)
                x = (c.ClientSize.Width - ctrl.Size.Width) / 2;
            if (h == Horizontal.CopyRight)
                x = c.Location.X + c.Size.Width - ctrl.Size.Width;
            if (h == Horizontal.Right)
                x = c.ClientSize.Width - ctrl.Size.Width - d;

            if (v == Vertical.Top)
                y = d;
            if (v == Vertical.CopyTop)
                y = c.Location.Y;
            if (v == Vertical.Middle)
                y = (c.ClientSize.Height - ctrl.Size.Height) / 2;
            if (v == Vertical.CopyBottom)
                y = c.Location.Y + c.ClientSize.Height - ctrl.Size.Height;
            if (v == Vertical.Bottom)
                y = c.ClientSize.Height - ctrl.Size.Height - d;

            ctrl.Location = new Point(x, y);
        }

        public static void LocateFrom(Control ctrl, Control c, Btn.Horizontal h, Btn.Vertical v, int d) {
            int x = 0;
            int y = 0;

            if (h == Horizontal.Left)
                x = c.Location.X - ctrl.Size.Width - d;
            if (h == Horizontal.CopyLeft)
                x = c.Location.X;
            if (h == Horizontal.Center)
                x = c.Location.X + (c.Size.Width - ctrl.Size.Width) / 2;
            if (h == Horizontal.CopyRight)
                x = c.Location.X + c.Size.Width - ctrl.Size.Width;
            if (h == Horizontal.Right)
                x = c.Location.X + c.Size.Width + d;

            if (v == Vertical.Top)
                y = c.Location.Y - ctrl.Size.Height - d;
            if (v == Vertical.CopyTop)
                y = c.Location.Y;
            if (v == Vertical.Middle)
                y = c.Location.Y + (c.Size.Height - ctrl.Size.Height) / 2;
            if (v == Vertical.CopyBottom)
                y = c.Location.Y + c.Size.Height - ctrl.Size.Height;
            if (v == Vertical.Bottom)
                y = c.Location.Y + c.Size.Height + d;

            ctrl.Location = new Point(x, y);
        }

        public static Lbl AddLabel(Control ctrl, string text, int d, bool moveCtrl, int labelWidth) {
            // Create a new label
            Lbl label = new Lbl(text, ctrl.Parent);

            // Set its width
            if (labelWidth != 0)
                label.Size = new Size(labelWidth, label.Height);

            // Give it the right position
            if (moveCtrl) {
                label.LocateFrom(ctrl, Horizontal.CopyLeft, Vertical.Middle, d);
                LocateFrom(ctrl, label, Horizontal.Right, Vertical.Middle, d);
            }
            else {
                label.LocateFrom(ctrl, Horizontal.Left, Vertical.Middle, d);
            }

            // Return the label, for the sake of easyness
            return label;
        }
    }

    class Cb : CheckBox
    {
        public Lbl Label;

        public Cb(string text, Control parent) {
            // Some default things about checkboxes
            this.Text = text;
            parent.Controls.Add(this);
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }

        /// <summary>
        /// Add a label to this control
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="d">The distance between the label and the control</param>
        /// <param name="moveCtrl">Whether the control should be moved or not</param>
        /// <param name="labelWidth">The width of the label. Set to 0 to keep the original width</param>
        public void AddLabel(string text, int d = Btn.distance, bool moveCtrl = true, int labelWidth = 0) {
            if (this.Label != null)
                this.Parent.Controls.Remove(this.Label);
            this.Label = Btn.AddLabel(this, text, d, moveCtrl, labelWidth);
        }
    }

    class Tb : TextBox
    {
        public Lbl Label;

        public Tb(Control parent) {
            // Some default things about checkboxes
            parent.Controls.Add(this);
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }

        /// <summary>
        /// Add a label to this control
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="d">The distance between the label and the control</param>
        /// <param name="moveCtrl">Whether the control should be moved or not</param>
        /// <param name="labelWidth">The width of the label. Set to 0 to keep the original width</param>
        public void AddLabel(string text, int d = Btn.distance, bool moveCtrl = true, int labelWidth = 0) {
            if (this.Label != null)
                this.Parent.Controls.Remove(this.Label);
            this.Label = Btn.AddLabel(this, text, d, moveCtrl, labelWidth);
        }
    }

    class Lb : ListBox
    {
        public Lbl Label;

        public Lb(Control parent) {
            // Some default things about checkboxes
            parent.Controls.Add(this);
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }

        /// <summary>
        /// Add a label to this control
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="d">The distance between the label and the control</param>
        /// <param name="moveCtrl">Whether the control should be moved or not</param>
        /// <param name="labelWidth">The width of the label. Set to 0 to keep the original width</param>
        public void AddLabel(string text, int d = Btn.distance, bool moveCtrl = true, int labelWidth = 0) {
            if (this.Label != null)
                this.Parent.Controls.Remove(this.Label);
            this.Label = Btn.AddLabel(this, text, d, moveCtrl, labelWidth);
        }
    }

    class Db : ComboBox
    {
        public Lbl Label;

        public Db(Control parent) {
            // Some default things about checkboxes
            parent.Controls.Add(this);
            this.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }

        /// <summary>
        /// Add a label to this control
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="d">The distance between the label and the control</param>
        /// <param name="moveCtrl">Whether the control should be moved or not</param>
        /// <param name="labelWidth">The width of the label. Set to 0 to keep the original width</param>
        public void AddLabel(string text, int d = Btn.distance, bool moveCtrl = true, int labelWidth = 0) {
            if (this.Label != null)
                this.Parent.Controls.Remove(this.Label);
            this.Label = Btn.AddLabel(this, text, d, moveCtrl, labelWidth);
        }
    }

    class Lbl : Label
    {
        public Lbl(string text, Control parent) {
            // Some default things about checkboxes
            this.Text = text;
            this.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(this);
        }

        /// <summary>
        /// Locate this control inside its parent in a specific way
        /// </summary>
        /// <param name="c">It's parent</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateInside(Control c, Btn.Horizontal h = Btn.Horizontal.Left, Btn.Vertical v = Btn.Vertical.Top, int d = Btn.distance) {
            Btn.LocateInside(this, c, h, v, d);
        }

        /// <summary>
        /// Locate this control adjacent to the other control in a specific way
        /// </summary>
        /// <param name="c">The other control</param>
        /// <param name="h">The horizontal placement</param>
        /// <param name="v">The vertical placement</param>
        /// <param name="distance">The margin</param>
        public void LocateFrom(Control c, Btn.Horizontal h = Btn.Horizontal.CopyLeft, Btn.Vertical v = Btn.Vertical.CopyTop, int d = Btn.distance) {
            Btn.LocateFrom(this, c, h, v, d);
        }
    }
}
