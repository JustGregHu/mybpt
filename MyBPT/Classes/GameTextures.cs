
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MyBPT.Classes
{
    /// <summary>
    /// MonoGame textúrákat tartalmazó elem.
    /// </summary>
    class GameTextures
    {
        Dictionary<string, Texture2D> collection = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Visszatér a felvett textúrák gyüjteményével
        /// </summary>
        public Dictionary<string, Texture2D> GetTextures()
        {
            return collection;
        }

        /// <summary>
        /// A gyüjteményhez hozzáad egy új textúrát a megadott értékek alapján
        /// </summary>
        /// <param name="newtexture">Később felhasználandó textúra</param>
        /// <param name="newtextureid">Ezzel a névvel hivatkozik majd a gyüjtemény az új textúrára</param>
        public void AddTexture(string newtextureid, Texture2D newtexture)
        {
            collection.Add(newtextureid,newtexture);
        }

    }
}