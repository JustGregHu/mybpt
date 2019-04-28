namespace MyBPT.Classes {
    /// <summary>
    /// Játékos adatait tároló osztály.
    /// </summary>
    public class Player {
        //Változók
        string name;
        int money;
        int level;

        //Tulajdonságok
        public string Name { get => name; set => name = value; }
        public int Money { get => money; set => money = value; }
        public int Level { get => level; set => level = value; }

        /// <summary>
        /// Létrehoz egy új játékos objektumot.
        /// </summary>
        /// <param name="name">Játékos neve. Ajánlott üresen hagyni, jelenlegi esetben később automatikusan felülíródik.</param>
        public Player(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Létrehoz egy új játékos objektumot.
        /// </summary>
        /// <param name="name">Játékos neve. Ajánlott üresen hagyni, jelenlegi esetben később automatikusan felülíródik.</param>
        /// <param name="level">A játékos szintje. Ez később felülíródik és megegyezik a megépített végállomások számával</param>
        /// <param name="money">A játékosnak szánt kezdő pénzmennyiség</param>
        public Player(string name, int money, int level) : this(name)
        {
            this.money = money;
            this.level = level;
        }

        /// <summary>
        /// Frissíti a játékos szintjét. Ez mindig megyezik a végállomások számával
        /// </summary>
        /// <param name="terminuscount">A játékos által megépített végállomások száma</param>
        public void UpdateLevel(int terminuscount)
        {
            if (terminuscount>0)
            {
                level = terminuscount;
            }
        }

        /// <summary>
        /// Pénz hozzáadása, illetve negatív érték megadásakor elvonása a játékostól. Nem engedi, hogy a pénz értéke 0 alá csökkenjen.
        /// </summary>
        /// <param name="terminuscount">A játékosnak szánt pénzmennyiség</param>
        public void AddMoney(int newmoney)
        {
            money += newmoney;
            if (money<0)
            {
                money = 0;
            }
        }

        /// <summary>
        /// Igazzal tér vissza, ha a játékos rendelkezik elegendő pénzmennyiséggel, hogy a megadott értéket ki tudja fizetni
        /// </summary>
        /// <param name="cost">A játékos pénzével összehasonlítandó érték</param>
        public bool CanAfford(int cost)
        {
            if (cost <= Money)
            {
                return true;
            }
            return false;
        }
    }
}