using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Craft.Net.Data.NbtSerialization;
using System.Reflection;
using fNbt;

namespace Craft.Net.Data
{
    /// <summary>
    /// Used to represent a 16x256x16 chunk of blocks in
    /// a <see cref="Craft.Net.Data.World"/>.
    /// </summary>
    public partial class Chunk
    {
        public const int Width = 16, Height = 256, Depth = 16;
        internal bool IsModified;

        /// <summary>
        /// Creates a new Chunk within the specified <see cref="Craft.Net.Data.Region"/>
        /// at the specified location.
        /// </summary>
        public Chunk(Vector3 relativePosition, Region parentRegion) : this(relativePosition)
        {
            ParentRegion = parentRegion;
            IsModified = false;
        }

        /// <summary>
        /// Creates a new chunk at the specified location.
        /// </summary>
        public Chunk(Vector3 relativePosition)
        {
            Sections = new Section[16];
            for (int i = 0; i < Sections.Length; i++)
                Sections[i] = new Section((byte)i);
            RelativePosition = relativePosition;
            Biomes = new byte[Width * Depth];
            HeightMap = new int[Width * Depth];
            TileEntities = new Dictionary<Vector3, TileEntity>();
        }

        /// <summary>
        /// The location of this chunk in global,
        /// world-wide coordinates.
        /// </summary>
        public Vector3 AbsolutePosition 
        {
            get { return (ParentRegion.Position * new Vector3(Region.Width, 0, Region.Depth)) + RelativePosition; }
        }

        /// <summary>
        /// The biome data for this chunk.
        /// </summary>
        public byte[] Biomes { get; set; }

        /// <summary>
        /// Each byte corresponds to the height of the given 1x256x1
        /// column of blocks.
        /// </summary>
        public int[] HeightMap { get; set; }

        /// <summary>
        /// The region this chunk is contained in.
        /// </summary>
        public Region ParentRegion { get; set; }

        /// <summary>
        /// The position of this chunk within the parent region.
        /// </summary>
        public Vector3 RelativePosition { get; set; }

        /// <summary>
        /// The 16x16x16 sections that make up this chunk.
        /// </summary>
        public Section[] Sections { get; set; }

        public Dictionary<Vector3, TileEntity> TileEntities { get; set; }

        /// <summary>
        /// Sets the value of the block at the given position, relative to this chunk.
        /// </summary>
        public void SetBlock(Vector3 position, Block value)
        {
            var y = GetSectionNumber(position.Y);
            position.Y = GetPositionInSection(position.Y);

            Sections[y].SetBlock(position, value);
            var heightIndex = (byte)(position.Z * Depth) + (byte)position.X;
            if (HeightMap[heightIndex] < position.Y)
                HeightMap[heightIndex] = (byte)position.Y;
            if (TileEntities.ContainsKey(position) && value.TileEntity == null)
                TileEntities.Remove(position);
            if (value.TileEntity != null)
                TileEntities[position] = value.TileEntity;
            IsModified = true;
        }

        /// <summary>
        /// Gets the block at the given position, relative to this chunk.
        /// </summary>
        public Block GetBlock(Vector3 position)
        {
            var y = GetSectionNumber(position.Y);
            position.Y = GetPositionInSection(position.Y);

            var block = Sections[y].GetBlock(position);
            if (TileEntities.ContainsKey(position))
                block.TileEntity = TileEntities[position];
            return block;
        }

        public short GetBlockId(int x, int y, int z)
        {
            int section = GetSectionNumber(y);
            y = GetPositionInSection(y);
            return Sections[section].GetBlockId(x, y, z);
        }

        public byte GetSkyLight(int x, int y, int z)
        {
            int section = GetSectionNumber(y);
            y = GetPositionInSection(y);
            return Sections[section].GetSkyLight(x, y, z);
        }

        public byte GetBlockLight(int x, int y, int z)
        {
            int section = GetSectionNumber(y);
            y = GetPositionInSection(y);
            return Sections[section].GetBlockLight(x, y, z);
        }

        public void SetSkyLight(int x, int y, int z, byte value)
        {
            int section = GetSectionNumber(y);
            y = GetPositionInSection(y);
            Sections[section].SetSkyLight(x, y, z, value);
        }

        public void SetBlockLight(int x, int y, int z, byte value)
        {
            int section = GetSectionNumber(y);
            y = GetPositionInSection(y);
            Sections[section].SetBlockLight(x, y, z, value);
        }

        /// <summary>
        /// Gets section number from the position.
        /// </summary>
        private static int GetSectionNumber(double yPos)
        {
             return ((int)yPos) >> 4; //divide by 16 ;)
        }

        /// <summary>
        /// Gets the position inside of the section.
        /// </summary>
        private static int GetPositionInSection (double yPos)
        {
            return ((int)yPos) & (16 - 1); // http://graphics.stanford.edu/~seander/bithacks.html#
        }

        /// <summary>
        /// Gets the biome at the given column.
        /// </summary>
        public Biome GetBiome(byte x, byte z)
        {
            return (Biome)Biomes[(byte)(z * Depth) + x];
        }

        /// <summary>
        /// Sets the value of the biome at the given column.
        /// </summary>
        public void SetBiome(byte x, byte z, Biome value)
        {
            Biomes[(byte)(z * Depth) + x] = (byte)value;
            IsModified = true;
        }

        /// <summary>
        /// Gets the height of the specified column.
        /// </summary>
        public int GetHeight(byte x, byte z)
        {
            return HeightMap[(byte)(z * Depth) + x];
        }

        /// <summary>
        /// Gets the block at the top of the specified column.
        /// </summary>
        public Block GetTopBlock(byte x, byte z)
        {
            return GetBlock(new Vector3(x, GetHeight(x, z), z));
        }

        public NbtFile ToNbt() // TODO: Entities
        {
            NbtFile file = new NbtFile();
            NbtCompound level = new NbtCompound("Level");
            
            // Entities // TODO
            level.Add(new NbtList("Entities", NbtTagType.Compound));

            // Biomes
            level.Add(new NbtByteArray("Biomes", Biomes));

            // Last Update // TODO: What is this
            level.Add(new NbtLong("LastUpdate", 15));

            // Position
            level.Add(new NbtInt("xPos", (int)AbsolutePosition.X));
            level.Add(new NbtInt("zPos", (int)AbsolutePosition.Z));

            // Tile Entities
            var tileEntityList = new NbtList("TileEntities", NbtTagType.Compound);
            foreach (var tileEntity in TileEntities)
            {
                // Get properties
                tileEntityList.Add(tileEntity.Value.ToNbt(tileEntity.Key));
            }
            level.Add(tileEntityList);

            // Terrain Populated // TODO: When is this 0? Will vanilla use this?
            level.Add(new NbtByte("TerrainPopualted", 0));

            // Sections and height
            level.Add(new NbtIntArray("HeightMap", HeightMap));
            NbtList sectionList = new NbtList("Sections", NbtTagType.Compound);
            foreach (var section in Sections)
            {
                if (!section.IsAir)
                {
                    NbtCompound sectionTag = new NbtCompound();
                    sectionTag.Add(new NbtByte("Y", section.Y));
                    sectionTag.Add(new NbtByteArray("Blocks", section.Blocks));
                    sectionTag.Add(new NbtByteArray("Data", section.Metadata.Data));
                    sectionTag.Add(new NbtByteArray("SkyLight", section.SkyLight.Data));
                    sectionTag.Add(new NbtByteArray("BlockLight", section.BlockLight.Data));
                    sectionList.Add(sectionTag);
                }
            }
            level.Add(sectionList);

            var rootCompound = new NbtCompound("");
            rootCompound.Add(level);
            file.RootTag = rootCompound;

            return file;
        }

        public static Chunk FromNbt(Vector3 position, NbtFile nbt)
        {
            Chunk chunk = new Chunk(position);
            // Load data
            var root = nbt.RootTag.Get<NbtCompound>("Level");
            chunk.Biomes = root.Get<NbtByteArray>("Biomes").Value;
            chunk.HeightMap = root.Get<NbtIntArray>("HeightMap").Value;
            var sections = root.Get<NbtList>("Sections");
            foreach (var sectionTag in sections) // TODO: This might not work properly
            {
                // Load data
                var compound = (NbtCompound)sectionTag;
                byte y = compound.Get<NbtByte>("Y").Value;
                var section = new Section(y);
                section.Blocks = compound.Get<NbtByteArray>("Blocks").Value;
                section.BlockLight.Data = compound.Get<NbtByteArray>("BlockLight").Value;
                section.SkyLight.Data = compound.Get<NbtByteArray>("SkyLight").Value;
                section.Metadata.Data = compound.Get<NbtByteArray>("Data").Value;
                // Process section
                section.ProcessSection();
                chunk.Sections[y] = section;
            }
            var tileEntities = root.Get<NbtList>("TileEntities");
            if (tileEntities != null)
            {
                foreach (var tag in tileEntities)
                {
                    Vector3 tilePosition;
                    var entity = TileEntity.FromNbt(tag as NbtCompound, out tilePosition);
                    if (entity != null)
                        chunk.TileEntities.Add(tilePosition, entity);
                }
            }
            return chunk;
        }
    }
}
