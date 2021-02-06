namespace SourceUtils
{
    partial class ValveBspFile
    {
        public interface ILump
        {
            LumpType LumpType { get; }
        }

        public enum LumpType : int
        {
            ENTITIES,
            PLANES,
            TEXDATA,
            VERTEXES,
            VISIBILITY,
            NODES,
            TEXINFO,
            FACES,
            LIGHTING,
            OCCLUSION,
            LEAFS,
            FACEIDS,
            EDGES,
            SURFEDGES,
            MODELS,
            WORLDLIGHTS,
            LEAFFACES,
            LEAFBRUSHES,
            BRUSHES,
            BRUSHSIDES,
            AREAS,
            AREAPORTALS,
            PROPCOLLISION,
            PROPHULLS,
            PROPHULLVERTS,
            PROPTRIS,
            DISPINFO,
            ORIGINALFACES,
            PHYSDISP,
            PHYSCOLLIDE,
            VERTNORMALS,
            VERTNORMALINDICES,
            DISP_LIGHTMAP_ALPHAS,
            DISP_VERTS,
            DISP_LIGHTMAP_SAMPLE_POSITIONS,
            GAME_LUMP,
            LEAFWATERDATA,
            PRIMITIVES,
            PRIMVERTS,
            PRIMINDICES,
            PAKFILE,
            CLIPPORTALVERTS,
            CUBEMAPS,
            TEXDATA_STRING_DATA,
            TEXDATA_STRING_TABLE,
            OVERLAYS,
            LEAFMINDISTTOWATER,
            FACE_MACRO_TEXTURE_INFO,
            DISP_TRIS,
            PROP_BLOB,
            WATEROVERLAYS,
            LEAF_AMBIENT_INDEX_HDR,
            LEAF_AMBIENT_INDEX,
            LIGHTING_HDR,
            WORLDLIGHTS_HDR,
            LEAF_AMBIENT_LIGHTING_HDR,
            LEAF_AMBIENT_LIGHTING,
            XZIPPAKFILE,
            FACES_HDR,
            MAP_FLAGS,
            OVERLAY_FADES,
            OVERLAY_SYSTEM_LEVELS,
            PHYSLEVEL,
            DISP_MULTIBLEND
        }
    }
}
