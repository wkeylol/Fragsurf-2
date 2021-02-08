﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SourceUtils
{
    public class StudioModelFile
    {
        public static StudioModelFile FromProvider(string path, params IResourceProvider[] providers)
        {
            var provider = providers.FirstOrDefault(x => x.ContainsFile(path));
            if (provider == null) return null;

            using (var stream = provider.OpenFile(path))
            {
                return new StudioModelFile(stream);
            }
        }

        [Flags]
        public enum Flags : int
        {
            AUTOGENERATED_HITBOX = 1 << 0,
            USES_ENV_CUBEMAP = 1 << 1,
            FORCE_OPAQUE = 1 << 2,
            TRANSLUCENT_TWOPASS = 1 << 3,
            STATIC_PROP = 1 << 4,
            USES_FB_TEXTURE = 1 << 5,
            HASSHADOWLOD = 1 << 6,
            USES_BUMPMAPPING = 1 << 7,
            USE_SHADOWLOD_MATERIALS = 1 << 8,
            OBSOLETE = 1 << 9,
            UNUSED = 1 << 10,
            NO_FORCED_FADE = 1 << 11,
            FORCE_PHONEME_CROSSFADE = 1 << 12,
            CONSTANT_DIRECTIONAL_LIGHT_DOT = 1 << 13,
            FLEXES_CONVERTED = 1 << 14,
            BUILT_IN_PREVIEW_MODE = 1 << 15,
            AMBIENT_BOOST = 1 << 16,
            DO_NOT_CAST_SHADOWS = 1 << 17,
            CAST_TEXTURE_SHADOWS = 1 << 18
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Header
        {
            public const int LengthOffset = sizeof(int) * 3 + 64;
            public const int FlagsOffset = LengthOffset + sizeof(int) + sizeof(float) * 3 * 6;

            public int Id;
            public int Version;
            public int Checksum;

            private fixed byte _name[64];

            public string Name
            {
                get
                {
                    fixed ( byte* name = _name )
                    {
                        return new string( (sbyte*) name );
                    }
                }
            }

            public int Length;

            public Vector3 EyePosition;
            public Vector3 IllumPosition;
            public Vector3 HullMin;
            public Vector3 HullMax;
            public Vector3 ViewBbMin;
            public Vector3 ViewBbMax;

            public Flags Flags;

            public int NumBones;
            public int BoneIndex;

            public int NumBoneControllers;
            public int BoneControllerIndex;

            public int NumHitBoxSets;
            public int HitBoxSetIndex;

            public int NumLocalAnim;
            public int LocalAnimIndex;

            public int NumLocalSeq;
            public int LocalSeqIndex;

            public int ActivityListVersion;
            public int EventsIndexed;

            public int NumTextures;
            public int TextureIndex;

            public int NumCdTextures;
            public int CdTextureIndex;

            public int NumSkinRef;
            public int NumSkinFamilies;
            public int SkinIndex;

            public int NumBodyParts;
            public int BodyPartIndex;

            int attachment_count;
            int attachment_offset;

            // Node values appear to be single bytes, while their names are null-terminated strings.
            int localnode_count;
            int localnode_index;
            int localnode_name_index;

            // mstudioflexdesc_t
            int flexdesc_count;
            int flexdesc_index;

            // mstudioflexcontroller_t
            int flexcontroller_count;
            int flexcontroller_index;

            // mstudioflexrule_t
            int flexrules_count;
            int flexrules_index;

            // IK probably referse to inverse kinematics
            // mstudioikchain_t
            int ikchain_count;
            int ikchain_index;

            // Information about any "mouth" on the model for speech animation
            // More than one sounds pretty creepy.
            // mstudiomouth_t
            int mouths_count;
            int mouths_index;

            // mstudioposeparamdesc_t
            int localposeparam_count;
            int localposeparam_index;

            /*
             * For anyone trying to follow along, as of this writing,
             * the next "surfaceprop_index" value is at position 0x0134 (308)
             * from the start of the file.
             */

            // Surface property value (single null-terminated string)
            int surfaceprop_index;

            // Unusual: In this one index comes first, then count.
            // Key-value data is a series of strings. If you can't find
            // what you're interested in, check the associated PHY file as well.
            int keyvalue_index;
            int keyvalue_count;

            // More inverse-kinematics
            // mstudioiklock_t
            int iklock_count;
            int iklock_index;


            float mass;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xd8)]
        public unsafe struct StudioBone
        {
            public int NameIndex;
            public int Parent;

            public fixed int BoneController[6];
            public Vector3 Pos;
            public Vector4 Quat;
            public Vector3 Rot;
            public Vector3 PosScale;
            public Vector3 RotScale;
            public fixed float PoseToBone[16];
            public Vector4 Alignment;
            public int Flags;
            public int ProcType;
            public int ProcIndex;
            public int PhysicBone;
            public int SurfacePropIndex;
            public int Contents;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StudioTexture
        {
            public const int NameIndexOffset = 0;

            public int NameIndex;
            public int Flags;
            public int Used;

            private int _unused0;

            public int MaterialPtr;
            public int ClientMaterialPtr;

            public int _unused1;
            public int _unused2;
            public int _unused3;
            public int _unused4;
            public int _unused5;
            public int _unused6;
            public int _unused7;
            public int _unused8;
            public int _unused9;
            public int _unused10;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StudioBodyPart
        {
            public int NameIndex;
            public int NumModels;
            public int Base;
            public int ModelIndex;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct StudioModel
        {
            private fixed byte _name[64];

            public string Name
            {
                get
                {
                    fixed ( byte* name = _name )
                    {
                        return new string( (sbyte*) name );
                    }
                }
            }

            public int Type;
            public float BoundingRadius;
            public int NumMeshes;
            public int MeshIndex;

            public int NumVertices;
            public int VertexIndex;
            public int TangentsIndex;

            public int NumAttachments;
            public int AttachmentIndex;

            public int NumEyeBalls;
            public int EyeBallIndex;

            public StudioModelVertexData VertexData;
            private fixed int _unused[8];
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public struct StudioModelVertexData
        {
            
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct StudioMesh
        {
            public int Material;
            public int ModelIndex;
            public int NumVertices;
            public int VertexOffset;
            public int NumFlexes;
            public int FlexIndex;
            public int MaterialType;
            public int MaterialParam;
            public int MeshId;
            public Vector3 Center;
            public StudioMeshVertexData VertexData;
            private fixed int _unused[8];
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct StudioMeshVertexData
        {
            private int _modelVertexData;
            private fixed int _numLodVertices[8];
        }

        [ThreadStatic]
        private static StringBuilder _sBuilder;
        private static string ReadNullTerminatedString(Stream stream)
        {
            if (_sBuilder == null) _sBuilder = new StringBuilder();
            else _sBuilder.Remove(0, _sBuilder.Length);

            while (true)
            {
                var c = (char) stream.ReadByte();
                if (c == 0) return _sBuilder.ToString();
                _sBuilder.Append(c);
            }
        }
        
        public static StudioModelFile FromStream(Stream stream)
        {
            return new StudioModelFile(stream);
        }

        public readonly Header FileHeader;

        private readonly StudioTexture[] _materials;
        private readonly string[] _materialNames;
        private readonly string[] _materialPaths;

        private readonly string[] _cachedFullMaterialPaths;

        private readonly StudioBodyPart[] _bodyParts;
        private readonly string[] _bodyPartNames;

        private readonly StudioModel[] _models;
        private readonly StudioMesh[] _meshes;
        private readonly StudioBone[] _bones;

        private readonly string[] _boneNames;

        public int Checksum => FileHeader.Checksum;
        public int NumTextures => FileHeader.NumTextures;
        public Vector3 HullMin => FileHeader.HullMin;
        public Vector3 HullMax => FileHeader.HullMax;

        public IReadOnlyList<StudioTexture> Textures => _materials;
        public IReadOnlyList<string> TextureDirectories => _materialPaths;
        public IReadOnlyList<string> TextureNames => _materialNames;

        public int TotalVertices => _meshes.Sum( x => x.NumVertices );

        public StudioModelFile(Stream stream)
        {
            FileHeader = LumpReader<Header>.ReadSingleFromStream(stream);

            if ( FileHeader.Id != 0x54534449 ) throw new Exception( "Not a MDL file." );

            _bones = new StudioBone[FileHeader.NumBones];
            _boneNames = new string[FileHeader.NumBones];
            _materials = new StudioTexture[FileHeader.NumTextures];
            _materialNames = new string[FileHeader.NumTextures];
            _cachedFullMaterialPaths = new string[FileHeader.NumTextures];

            stream.Seek(FileHeader.BoneIndex, SeekOrigin.Begin);
            LumpReader<StudioBone>.ReadLumpFromStream(stream, FileHeader.NumBones, (index, bone) =>
            {
                _bones[index] = bone;

                stream.Seek(bone.NameIndex, SeekOrigin.Current);
                _boneNames[index] = ReadNullTerminatedString(stream);
            });

            stream.Seek(FileHeader.TextureIndex, SeekOrigin.Begin);
            LumpReader<StudioTexture>.ReadLumpFromStream(stream, FileHeader.NumTextures, (index, tex) =>
            {
                _materials[index] = tex;

                stream.Seek(tex.NameIndex, SeekOrigin.Current);
                _materialNames[index] = ReadNullTerminatedString(stream);
            });

            _materialPaths = new string[FileHeader.NumCdTextures];

            stream.Seek( FileHeader.CdTextureIndex, SeekOrigin.Begin );
            LumpReader<int>.ReadLumpFromStream( stream, FileHeader.NumCdTextures, ( index, cdTex ) =>
            {
                stream.Seek( cdTex, SeekOrigin.Begin );
                _materialPaths[index] = ReadNullTerminatedString( stream ).Replace( '\\', '/' );
            } );

            _bodyParts = new StudioBodyPart[FileHeader.NumBodyParts];
            _bodyPartNames = new string[FileHeader.NumBodyParts];

            var modelList = new List<StudioModel>();
            var meshList = new List<StudioMesh>();

            stream.Seek( FileHeader.BodyPartIndex, SeekOrigin.Begin );
            LumpReader<StudioBodyPart>.ReadLumpFromStream( stream, FileHeader.NumBodyParts, (partIndex, part) =>
            {
                var partPos = stream.Position;

                stream.Seek( partPos + part.NameIndex, SeekOrigin.Begin );
                _bodyPartNames[partIndex] = ReadNullTerminatedString( stream );

                stream.Seek( partPos + part.ModelIndex, SeekOrigin.Begin );

                // Now indexes into array of models
                part.ModelIndex = modelList.Count;

                LumpReader<StudioModel>.ReadLumpFromStream( stream, part.NumModels, ( modelIndex, model ) =>
                {
                    var modelPos = stream.Position;

                    stream.Seek( modelPos + model.MeshIndex, SeekOrigin.Begin );

                    model.MeshIndex = meshList.Count;
                    LumpReader<StudioMesh>.ReadLumpFromStream( stream, model.NumMeshes, ( meshIndex, mesh ) =>
                    {
                        mesh.ModelIndex = modelIndex;
                        meshList.Add( mesh );
                    } );

                    modelList.Add( model );
                } );

                _bodyParts[partIndex] = part;
            } );

            _models = modelList.ToArray();
            _meshes = meshList.ToArray();
        }

        public int BodyPartCount => _bodyParts.Length;

        public string GetBodyPartName( int index )
        {
            return _bodyPartNames[index];
        }

        public StudioMesh GetMesh( int bodyPartIndex, int modelIndex, int meshIndex )
        {
            return _meshes[_models[_bodyParts[bodyPartIndex].ModelIndex + modelIndex].MeshIndex + meshIndex];
        }

        public IEnumerable<StudioModel> GetModels( int bodyPartIndex )
        {
            var part = _bodyParts[bodyPartIndex];
            return Enumerable.Range( part.ModelIndex, part.NumModels ).Select( x => _models[x] );
        }
        
        public IEnumerable<StudioMesh> GetMeshes( int index, int count )
        {
            return Enumerable.Range( index, count ).Select( x => _meshes[x] );
        }

        public IEnumerable<StudioMesh> GetMeshes( ref StudioModel model )
        {
            return GetMeshes( model.MeshIndex, model.NumMeshes );
        }

        public int MaterialCount => _materials.Length;

        public string GetMaterialName(int index, params IResourceProvider[] providers)
        {
            var name = $"{_materialNames[index]}.vmt";

            if ( _cachedFullMaterialPaths[index] != null ) return _cachedFullMaterialPaths[index];
            if ( _materialPaths.Length == 0 ) return _cachedFullMaterialPaths[index] = name;

            foreach ( var path in _materialPaths )
            {
                var fullPath = (path + name).Replace( '\\', '/' );
                if ( !fullPath.StartsWith( "materials/" ) ) fullPath = $"materials/{fullPath}";

                foreach ( var provider in providers )
                {
                    if ( provider.ContainsFile( fullPath ) ) return _cachedFullMaterialPaths[index] = fullPath;
                }
            }

            return name;
        }
    }
}