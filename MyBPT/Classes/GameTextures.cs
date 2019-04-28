
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes
{
    /// <summary>
    /// MonoGame textúrákat tartalmazó elem.
    /// </summary>
    public class GameTextures
    {
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        Dictionary<string, int> isbuildings = new Dictionary<string, int>();
        Dictionary<string, int> types = new Dictionary<string, int>();
        Dictionary<string, int> levels = new Dictionary<string, int>();
        /// <summary>
        /// Visszatér a felvett textúrák gyüjteményével
        /// </summary>
        public Dictionary<string, Texture2D> GetTextures()
        {
            return textures;
        }

        /// <summary>
        /// A gyüjteményhez hozzáad egy új textúrát a megadott értékek alapján
        /// </summary>
        /// <param name="newtexture">Később felhasználandó textúra</param>
        /// <param name="newtextureid">Ezzel a névvel hivatkozik majd a gyüjtemény az új textúrára</param>
        public void AddTexture(string newtextureid, Texture2D newtexture, int isbuilding,int level, int type)
        {
            textures.Add(newtextureid,newtexture);
            isbuildings.Add(newtextureid, isbuilding);
            types.Add(newtextureid, type);
            levels.Add(newtextureid, level);
        }

        /// <summary>
        /// Kikeres a gyüjteményből egy megadott szintnek és épülettípusnak megfelelő textúrát
        /// </summary>
        /// <param name="findlevel">Az épület szintje (1-2)</param>
        /// <param name="findtype">Az épület típusa (1-3)</param>
        public Texture2D FindBuildingTexture(int findlevel,int findtype)
        {
            foreach (var texture in textures)
            {
                if (isbuildings[texture.Key]==1 && types[texture.Key]==findtype && levels[texture.Key]==findlevel)
                {
                    return texture.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Kikeres a gyüjteményből egy megadott szintnek és az állomás típusának megfelelő textúrát
        /// </summary>
        /// <param name="findlevel">Az állomás szintje (1-2)</param>
        /// <param name="findtype">Az állomás típusa (1-2)</param>
        public Texture2D FindStationTexture(int findlevel, int findtype)
        {
            foreach (var texture in textures)
            {
                if (isbuildings[texture.Key] == 2 && types[texture.Key] == findtype && levels[texture.Key] == findlevel)
                {
                    return texture.Value;
                }
            }
            return null;
        }

    }
}