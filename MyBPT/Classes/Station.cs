﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes {
    /// <summary>
    /// Egy megjeleníthető buszállomás / végállomás objektum. A játékos létrehozhatja ezeket. és segítségükkel gyüjthet bevételt.
    /// </summary>
    public class Station {
        //adatok
        int cost;
        int type;
        int level;
        int maxlevel;
        int income;
        int effectradius;
        bool visible = true;
        bool highlighted;
        bool moving;
        bool isterminus;
        Texture2D texture;

        //pozícionálás
        Point coordinates;
        Point safecoordinates;
        Vector2 safeposition;
        Vector2 tileposition;
        Tile highlighttile;

        //gombok és megjelenítes
        Button movebutton;
        Button sellbutton;
        Button upgradebutton;
        Button acceptbutton;
        Button cancelbutton;
        Texture2D gameplaystats_influence;
        Texture2D gameplaystats_clock;
        Texture2D gameplaystats_coins;
        Texture2D background;

        //Tulajdonságok
        public bool Highlighted { get => highlighted; set => highlighted = value; }
        public bool Isterminus { get => isterminus; set => isterminus = value; }
        public int Effectradius { get => effectradius; set => effectradius = value; }
        public int Type { get => type; set => type = value; }
        public int Cost { get => cost; set => cost = value; }
        public int Level { get => level; set => level = value; }
        public int Maxlevel { get => maxlevel; set => maxlevel = value; }
        public int Income { get => income; set => income = value; }
        public Button MoveButton { get => movebutton; set => movebutton = value; }
        public Button SellButton { get => sellbutton; set => sellbutton = value; }
        public Button UpgradeButton { get => upgradebutton; set => upgradebutton = value; }
        public Button AcceptButton { get => acceptbutton; set => acceptbutton = value; }
        public Button CancelButton { get => cancelbutton; set => cancelbutton = value; }
        public Point Coordinates { get => coordinates; set => coordinates = value; }
        public bool Moving { get => moving; set => moving = value; }

        //Leírások
        public string Description_LevelAndType
        {
            get
            {
                string stationtype = "";
                if (isterminus)
                {
                    stationtype = "Terminus";
                }
                else
                {
                    stationtype = "Station";
                }
                return "LVL " + level + " " + stationtype;
            }
        }
        public string Description_Influence_Worth
        {
            get
            {
                return "Income: " + income + " cash/month";
            }
        }
        public string Description_Influence_Tiles
        {
            get
            {
                return "Influence: " + effectradius + " tiles";
            }
        }
        public string Description_UpgradeCost
        {
            get
            {
                return "Upgrade Cost: " + UpgradeCost + " cash";
            }
        }
        public string Description_SellPrice
        {
            get
            {
                return "Sell Price: " + SellPrice + " cash"; ;
            }
        }
        public string Description_Cost
        {
            get
            {
                return "Cost :" + cost + " cash"; ;

            }

        }

        /// <summary>
        /// Létrehoz egy állomás objektumot. Létrehozza a HUD gombokat, átveszi az adatokat és inicializálja a megjelenítést.
        /// </summary>
        /// <param name="graphicsdevice">MonoGame-hez tartozó grafikai készlet</param>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        /// <param name="preferredscreensize">Ajánlott képernyőméret</param>
        /// <param name="coordinates">Ahol az épület található</param>
        /// <param name="hudmargin">Pixelben megadott távolság a HUD elemek és a képernyő között</param>
        /// <param name="isterminus">Végállomás-e?</param>
        public Station(GraphicsDevice graphicsdevice, Point preferredscreensize, GameTextures texturecollection, int hudmargin, GameWorld gameWorld, Point coordinates, bool isterminus)
        {
            Color[] data = new Color[preferredscreensize.X * preferredscreensize.Y];
            background = new Texture2D(graphicsdevice, preferredscreensize.X, preferredscreensize.Y);
            for (int i = 0; i < data.Length; ++i)
                data[i] = Color.White;
            background.SetData(data);

            Vector2 initialpos = new Vector2(0, 0);
            movebutton = new Button(initialpos, texturecollection.GetTextures()["hud_button_station_move"]);
            sellbutton = new Button(initialpos, texturecollection.GetTextures()["hud_button_station_sell"]);
            upgradebutton = new Button(initialpos, texturecollection.GetTextures()["hud_button_station_upgrade"]);
            acceptbutton = new Button(initialpos, texturecollection.GetTextures()["hud_button_apply"]);
            cancelbutton = new Button(initialpos, texturecollection.GetTextures()["hud_button_cancel"]);

            movebutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 - movebutton.Texture.Width / 2 - movebutton.Texture.Width - hudmargin, preferredscreensize.Y - 300));
            sellbutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 - movebutton.Texture.Width / 2, preferredscreensize.Y - 300));
            upgradebutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 + movebutton.Texture.Width / 2 + hudmargin, preferredscreensize.Y - 300));
            acceptbutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 - movebutton.Texture.Width - hudmargin / 2, preferredscreensize.Y - 300));
            cancelbutton.UpdatePosition(new Vector2(preferredscreensize.X / 2 + hudmargin / 2, preferredscreensize.Y - 300));

            gameplaystats_clock = texturecollection.GetTextures()["hud_gameplaystats_clock"];
            gameplaystats_coins = texturecollection.GetTextures()["hud_gameplaystats_coins"];
            gameplaystats_influence = texturecollection.GetTextures()["hud_gameplaystats_influence"];

            this.tileposition = gameWorld.MapData[coordinates.X, coordinates.Y].Position;
            this.isterminus = isterminus;
            this.coordinates = coordinates;
            level = 0;
            maxlevel = 2;
            LevelStationUp(texturecollection, gameWorld);
            ApplyIncome(gameWorld);
            highlighttile = new Tile(0, texturecollection.GetTextures()["world_highlight_lightblue"], tileposition, new Rectangle(tileposition.ToPoint(), new Point(200, 100)));
            ButtonVisibility_NoSelection();
        }

        //MEGJELENÍTÉS
    
        /// <summary>
        /// Az állomások felrajzolása a játéktérre
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
            {
                if (moving)
                {
                    ButtonVisibility_Moving();
                }
                else
                {
                    ButtonVisibility_Highlighted();
                }
            }
            if (visible)
            {
                spriteBatch.Draw(texture, DisplayPosition(), Color.White);
            }
        }

        /// <summary>
        /// Bevételi zóna megjelenítése, ha a kurzor állomáson tartózkodik.
        /// </summary>
        /// <param name="spriteBatch">MonoGame spritegyüjtemény, amely lerajzolja az objektumot</param>
        public void DrawAffectionOfArea(SpriteBatch spriteBatch)
        {
            if (highlighted)
                for (int y = -effectradius; y < effectradius + 1; y++)
                {
                    for (int x = -effectradius; x < effectradius + 1; x++)
                    {
                        Vector2 arealocationisntance = new Vector2(highlighttile.Position.X + y * 100 - x * 100, (highlighttile.Position.Y + y * 50 + x * 50));
                        spriteBatch.Draw(highlighttile.Texture, arealocationisntance, Color.White);
                    }
                }

        }

        /// <summary>
        /// Az állomáshoz tartozó információk HUD-ra történő megjelenítése
        /// </summary>
        /// <param name="spriteBatchHud">MonoGame spritegyüjtemény, amely lerajzolja a HUD elemeket</param>
        /// <param name="font">MonoGame betűgyüjtemény, amelyet beállításai alapján rajzolódik a funckióban megadott szöveg</param>
        /// <param name="hudmargin">Pixelben megadott távolság a HUD elemek és a képernyő között</param>
        public void DrawInfo(SpriteBatch spriteBatchHud, SpriteFont font, int hudmargin)
        {
            spriteBatchHud.Draw(gameplaystats_influence, new Vector2(hudmargin, hudmargin + 50), Color.White);
            spriteBatchHud.Draw(gameplaystats_clock, new Vector2(hudmargin, hudmargin + 100), Color.White);
            spriteBatchHud.Draw(gameplaystats_coins, new Vector2(hudmargin, hudmargin + 150), Color.White);
            spriteBatchHud.Draw(gameplaystats_coins, new Vector2(hudmargin, hudmargin + 200), Color.White);

            spriteBatchHud.DrawString(font, Description_LevelAndType, new Vector2(hudmargin, hudmargin), Color.White);

            spriteBatchHud.DrawString(font, Description_Influence_Tiles, new Vector2(hudmargin * 2, hudmargin + 50), Color.White);
            spriteBatchHud.DrawString(font, Description_Influence_Worth, new Vector2(hudmargin * 2, hudmargin + 100), Color.White);
            spriteBatchHud.DrawString(font, Description_UpgradeCost, new Vector2(hudmargin * 2, hudmargin + 150), Color.White);
            spriteBatchHud.DrawString(font, Description_SellPrice, new Vector2(hudmargin * 2, hudmargin + 200), Color.White);
        }

        /// <summary>
        /// Az állomáshoz tartozó gombok HUD-ra történő megjelenítése
        /// </summary>
        /// <param name="spriteBatchHud">MonoGame spritegyüjtemény, amely lerajzolja a HUD elemeket</param>
        public void DrawButtons(SpriteBatch spriteBatchHud)
        {
            spriteBatchHud.Draw(background, new Vector2(-750, -365), new Color(0, 0, 0, 85));
            if (moving)
            {
                AcceptButton.Draw(spriteBatchHud);
                CancelButton.Draw(spriteBatchHud);
            }
            else
            {
                MoveButton.Draw(spriteBatchHud);
                SellButton.Draw(spriteBatchHud);
                UpgradeButton.Draw(spriteBatchHud);
            }

        }

        //MŰVELETEK
       
        /// <summary>
        /// Az állomást szintjének növelése 1-el. Ez a textúráját is megváltoztatja
        /// </summary>
        /// <param name="texturecollection">A megjelenítéshez szükséges textúragyüjtemény</param>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        public void LevelStationUp(GameTextures texturecollection, GameWorld gameworld)
        {
            int type = 0;
            level += 1;
            if (level > maxlevel)
            {
                level = maxlevel;
            }
            if (isterminus)
            {
                type = 2;
            }
            else
            {
                type = 1;
            }
            this.texture = texturecollection.FindStationTexture(level, type);
            ApplyEffectRadius();
            ApplyCost();
            ApplyIncome(gameworld);
        }

        /// <summary>
        /// Az állomás bevételi zónájának frissítése
        /// </summary>
        public void ApplyEffectRadius()
        {
            effectradius = 1;
            if (isterminus)
            {
                effectradius += 1;
            }
            effectradius += level;
        }

        /// <summary>
        /// Az állomás teljes bevételének frissítése
        /// </summary>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        public void ApplyIncome(GameWorld gameworld)
        {
            income = 100;
            for (int y = -effectradius; y < effectradius + 1; y++)
            {
                for (int x = -effectradius; x < effectradius + 1; x++)
                {
                    if (gameworld.IsThereABuildingAt(new Point(coordinates.X+x,coordinates.Y+y)))
                    {
                        for (int i = 0; i < gameworld.Buildings.Count; i++)
                        {
                            if (gameworld.Buildings[i].Coordinates== (new Point(coordinates.X + x, coordinates.Y + y)))
                            {
                                income= income + gameworld.Buildings[i].Influenceamount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Az állomás megvásárási árának incializálása függően attól, végállomás-e
        /// </summary>
        public void ApplyCost()
        {
            if (isterminus)
            {
                cost = 1000;
            }
            else
            {
                cost = 350;
            }
            cost = cost * level;
        }

        /// <summary>
        /// Frissíti azt, hogy a kurzor a jelenlegi objektumon tartózkodik-e.
        /// </summary>
        /// <param name="coordinatetocheck">Ellenőrizendő koordináta</param>
        public void CheckIfHighlighted(Point coordinatetocheck)
        {
            if (coordinatetocheck == coordinates)
            {
                highlighted = true;
            }
            else
            {
                highlighted = false;
            }
        }

        //MOZGATÁSI FOLYAMAT

        /// <summary>
        /// A mozgatási folyamat megkezdése
        /// </summary>
        public void InitiateMove()
        {
            moving = true;
            safeposition = tileposition;
            safecoordinates = coordinates;
            ButtonVisibility_Moving();
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }

        /// <summary>
        /// A mozgatási folyamat véglegesítése
        /// </summary>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        public void FinalizeMove(GameWorld gameworld)
        {
            moving = false;
            safeposition = tileposition;
            safecoordinates = coordinates;
            highlighttile.Position = tileposition;
            ButtonVisibility_NoSelection();
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }

        /// <summary>
        /// Az állomás új koordinátára történ helyezése
        /// </summary>
        /// <param name="gameWorld">A már legalább részlegesen legenerált játékvilág</param>
        /// <param name="newcoordinates">Erre a koordinátára helyeződik</param>
        public void MoveStationTo(Point newcoordinates,GameWorld gameworld)
        {
            if (newcoordinates.X<0)
            {
                newcoordinates = new Point(0, newcoordinates.Y);
            }
            if (newcoordinates.Y < 0)
            {
                newcoordinates = new Point(newcoordinates.X, 0);
            }
            this.tileposition = gameworld.MapData[newcoordinates.X, newcoordinates.Y].Position;
            highlighttile.Position = tileposition;
            coordinates = newcoordinates;
            ButtonVisibility_Moving();
        }

        /// <summary>
        /// Jelenleg aktív mozgatási folyamat megszakítása és visszavonása
        /// </summary>
        public void RollbackMove()
        {
            moving = false;
            coordinates = safecoordinates;
            tileposition = safeposition;
            highlighttile.Position = tileposition;
            ButtonVisibility_NoSelection();
        }

        //VÁLTOZÓ MŰVELETEK

        /// <summary>
        /// Visszatér azzal az összeggel amelyet a játékos kap, ha eladja az állomást
        /// </summary>
        public int SellPrice
        {
            get
            {
                return cost / 2;
            }
        }

        /// <summary>
        /// Visszatér az állomás felújításához szükséges pénzösszeggel
        /// </summary>
        public int UpgradeCost
        {
            get
            {
                return cost * 2;
            }
        }

        /// <summary>
        /// Az épületobjektumot a pozíciója alapján az alatta lévő "csempére" igazítja
        /// </summary>
        private Vector2 DisplayPosition()
        {
            return new Vector2((tileposition.X + 200) - (texture.Width), (tileposition.Y + 150) - (texture.Height));
        }

        //Láthatóságot irányító funkciók
        private void ButtonVisibility_NoSelection()
        {
            movebutton.Visible = false;
            sellbutton.Visible = false;
            upgradebutton.Visible = false;
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;

        }
        private void ButtonVisibility_Highlighted()
        {
            movebutton.Visible = true;
            sellbutton.Visible = true;
            upgradebutton.Visible = true;
            acceptbutton.Visible = false;
            cancelbutton.Visible = false;
        }
        private void ButtonVisibility_Moving()
        {
            movebutton.Visible = false;
            sellbutton.Visible = false;
            upgradebutton.Visible = false;
            acceptbutton.Visible = true;
            cancelbutton.Visible = true;
        }
    }
}