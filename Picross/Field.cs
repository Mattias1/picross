namespace Picross
{
    struct Field
    {
        // The fields
        public static Field Decoration => new Field(-2);    // An empty square with a decoration colour
        public static Field Empty => new Field(-1);         // A square you are sure it's empty
        public static Field Unknown => new Field(0);        // A square you don't know anything about
        public static Field Black => new Field(1);          // A black square
        public static Field Red => new Field(2);            // A red square

        public static Field[] All => new Field[] { Decoration, Empty, Unknown, Black, Red };

        // Logic methods
        public bool IsOn() => this.value > 0;
        public bool IsNotOn() => !this.IsOn();
        public bool IsOff() => this.value < 0;
        public bool IsNotOff() => !this.IsOff();

        // Struct management
        private int value;
        public int Index => this.value + 2;

        public Field(int value) {
            this.value = value;
        }

        public override string ToString() {
            return this.value.ToString();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public bool Equals(Field other) {
            if ((object)other == null)
                return false;
            return this.value == other.value;
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(Field a, Field b) => a.value == b.value;

        public static bool operator !=(Field a, Field b) => a.value != b.value;

        public static Field Parse(string s) => new Field(int.Parse(s));
    }
}