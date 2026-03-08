using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class TileSuperposition : ISerializationCallbackReceiver // TODO: Make this a struct. Figure out marshalling/Native for the bit array
{
    public Tileset tileset;
    [ShowInInspector, TableList(DrawScrollView = true, IsReadOnly = true), Searchable]
    private List<TileSetup> TileFlags
    {
        get
        {
            if (tileset == null) tileset = Tileset.defaultTileset;
            if (tileset == null || tileset.tiles == null || tileset.tiles.Count == 0) return new();
            List<TileSetup> tempList = new(tileset.tiles.Count);
            if (options.Count != tileset.tiles.Count)
            {
                BitArray optionsNew = new BitArray(tileset.tiles.Count);
                for (int i = 0; i < options.Count; i++)
                {
                    if (optionsNew.Count == i) break;
                    optionsNew[i] = options[i];
                }
                options = optionsNew;
            }
            for (int i = 0; i < tileset.tiles.Count; i++)
            {
                if (options[i]) tempList.Add(new(tileset.tiles[i].name, true, i, this));
                else tempList.Add(new(tileset.tiles[i].name, false, i, this));
            }
            return tempList;
        }
        set { }
    }
    [Serializable]
    public struct TileSetup
    {
        public string Name;
        private bool _enabled;
        [ShowInInspector]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnChange();
            }
        }
        private int index;
        private TileSuperposition tile;

        public TileSetup(string name, bool enabled, int index, TileSuperposition tile)
        {
            this.Name = name;
            this._enabled = enabled;
            this.index = index;
            this.tile = tile;
        }

        private void OnChange()
        {
            tile.options[index] = _enabled;
        }
    }

    private BitArray options; // TODO: At this point just make your own bit array implementation
    [SerializeField, HideInInspector]
    private bool[] optionsSerializer;
    [HideInInspector]
    public bool generated;

    public int Entropy
    {
        get
        {
            int entropy = 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i]) entropy++;
            }
            return entropy;
        }
    }

    public bool Solved
    {
        get
        {
            bool solved = false;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i])
                {
                    if (solved) return false;
                    solved = true;
                }
            }
            return solved;
        }
    }

    public TileSuperposition() 
    {
        if (tileset == null) tileset = Tileset.defaultTileset;
        if (tileset == null) options = new(0);
        else options = new(tileset.tiles.Count);
    }

    public TileSuperposition(Tileset tileset)
    {
        this.tileset = tileset;
        options = new(tileset.tiles.Count);
    }

    public TileSuperposition(int length)
    {
        options = new(length);
    }

    public TileSuperposition(params bool[] flags)
    {
        options = new BitArray(flags);
    }

    public TileSuperposition(BitArray flags)
    {
        options = new BitArray(flags);
    }

    public TileSuperposition(TileSuperposition source)
    {
        tileset = source.tileset;
        options = new BitArray(source.options);
    }

    public List<GenerationTile> GetTiles()
    {
        List<GenerationTile> tiles = new();
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i])
            {
                tiles.Add(tileset.tiles[i]);
            }
        }
        return tiles;
    }

    /// <summary>
    /// Returns true if this contains the subset
    /// </summary>
    public bool ContainsSubset(TileSuperposition other)
    {
        for (int i = 0; i < other.options.Length; i++)
        {
            if (i >= options.Length) break;
            if (other.options[i] && !options[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns true if this contains all the same set bits as other
    /// </summary>
    public bool ContainsAll(TileSuperposition other)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (other.options.Length == i) break;
            if (other.options[i])
            {
                if (!options[i]) return false;
            }
        }
        return true;
    }

    public void OnBeforeSerialize()
    {
        if (tileset == null) return;
        optionsSerializer = new bool[tileset.tiles.Count];
        for (int i = 0; i < options.Length; i++)
        {
            if (optionsSerializer.Length == i) break;
            optionsSerializer[i] = options[i];
        }
    }

    public void OnAfterDeserialize()
    {
        if (optionsSerializer == null) return;
        options = new BitArray(optionsSerializer);
        if (tileset == null) tileset = Tileset.defaultTileset;
    }

    // Override equals as well? Consider consiquences
    /*public override bool Equals(object obj)
    {
        if (options.Length != other.options.Length) return false;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != other.options[i]) return false;
        }
        return true;
    }*/

    public TileSuperposition SetBit(int bit, bool value)
    {
        options.Set(bit, false);
        return this;
    }

    public TileSuperposition SetAll(bool value)
    {
        options.SetAll(false);
        return this;
    }

    public TileSuperposition Or(TileSuperposition other)
    {
        options.Or(other.options);
        return this;
    }

    public TileSuperposition And(TileSuperposition other)
    {
        options.And(other.options);
        return this;
    }

    public TileSuperposition Not()
    {
        options.Not();
        return this;
    }

    public bool IsZero()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i]) return false;
        }
        return true;
    }
}